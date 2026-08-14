using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PackageRegistryService.API.Handlers;
using PackageRegistryService.Models;
using PackageRegistryTestHost;

using RegistryValidationPackage = PackageRegistryService.Models.ValidationPackage;

namespace APITests;

public class PackageDiscoveryTests
{
    [Fact]
    public async Task DiscoveryRoutesUseCanonicalPascalCaseContractsAndDeterministicOrdering()
    {
        using var factory = new PackageRegistryWebApplicationFactory();
        await factory.SeedPackagesAsync([
            CreatePackage("beta", 2, 0, 0),
            CreatePackage("alpha", 1, 0, 0, preRelease: "alpha.2"),
            CreatePackage("alpha", 1, 0, 0),
            CreatePackage("alpha", 1, 0, 0, buildMetadata: "z"),
            CreatePackage("alpha", 1, 0, 0, preRelease: "alpha.10")
        ]);

        using var client = factory.CreateClient();

        using var indexResponse = await client.GetAsync("/api/v1/package-index");
        indexResponse.EnsureSuccessStatusCode();
        using var index = JsonDocument.Parse(await indexResponse.Content.ReadAsStringAsync());

        var identities = index.RootElement.EnumerateArray().ToArray();
        Assert.Equal(5, identities.Length);
        Assert.Equal(
            [
                "alpha@1.0.0+z",
                "alpha@1.0.0",
                "alpha@1.0.0-alpha.10",
                "alpha@1.0.0-alpha.2",
                "beta@2.0.0"
            ],
            identities.Select(identity =>
                $"{identity.GetProperty("Name").GetString()}@{identity.GetProperty("Version").GetString()}")
        );
        Assert.All(identities, identity =>
        {
            Assert.Equal(2, identity.EnumerateObject().Count());
            Assert.False(identity.TryGetProperty("PackageContent", out _));
            Assert.False(identity.TryGetProperty("name", out _));
        });

        using var versionsResponse = await client.GetAsync("/api/v1/packages/alpha/versions");
        versionsResponse.EnsureSuccessStatusCode();
        using var versions = JsonDocument.Parse(await versionsResponse.Content.ReadAsStringAsync());
        Assert.Equal(
            ["1.0.0+z", "1.0.0", "1.0.0-alpha.10", "1.0.0-alpha.2"],
            versions.RootElement.EnumerateArray().Select(value => value.GetString())
        );

        using var metadataResponse = await client.GetAsync("/api/v1/packages/beta/2.0.0/metadata");
        metadataResponse.EnsureSuccessStatusCode();
        using var metadata = JsonDocument.Parse(await metadataResponse.Content.ReadAsStringAsync());
        var root = metadata.RootElement;
        Assert.Equal("beta", root.GetProperty("Name").GetString());
        Assert.Equal("2.0.0", root.GetProperty("Version").GetString());
        Assert.Equal("Summary for beta", root.GetProperty("Summary").GetString());
        Assert.Equal("Description for beta", root.GetProperty("Description").GetString());
        Assert.Equal("2026-08-14", root.GetProperty("ReleaseDate").GetString());
        Assert.Equal("FSharp", root.GetProperty("ProgrammingLanguage").GetString());
        Assert.Equal("verbose", root.GetProperty("Inputs")[0].GetProperty("id").GetString());
        Assert.False(root.TryGetProperty("PackageContent", out _));
        Assert.False(root.TryGetProperty("MajorVersion", out _));

        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync("/api/v1/packages/missing/versions")).StatusCode);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await client.GetAsync("/api/v1/packages/beta/9.9.9/metadata")).StatusCode);
    }

    [Fact]
    public async Task DiscoveryDoesNotValidateContentOrIncrementDownloadsButArtifactsStillDo()
    {
        using var factory = new PackageRegistryWebApplicationFactory();
        var package = CreatePackage("side-effects", 1, 2, 3);
        await factory.SeedPackageAsync(package);
        await SetHashAsync(factory, package, "deliberately-invalid");

        using var client = factory.CreateClient();
        (await client.GetAsync("/api/v1/package-index")).EnsureSuccessStatusCode();
        (await client.GetAsync("/api/v1/packages/side-effects/versions")).EnsureSuccessStatusCode();
        (await client.GetAsync("/api/v1/packages/side-effects/1.2.3/metadata")).EnsureSuccessStatusCode();
        Assert.Equal(0, await GetDownloadsAsync(factory, package));

        using var invalidArtifact = await client.GetAsync("/api/v1/packages/side-effects/1.2.3");
        Assert.Equal(HttpStatusCode.Conflict, invalidArtifact.StatusCode);
        Assert.Equal(0, await GetDownloadsAsync(factory, package));

        await SetHashAsync(factory, package, package.GetPackageContentHash());
        using var artifact = await client.GetAsync("/api/v1/packages/side-effects/1.2.3");
        artifact.EnsureSuccessStatusCode();
        Assert.Equal(1, await GetDownloadsAsync(factory, package));
    }

    [Fact]
    public void DiscoverySqlProjectionsExcludePackageContent()
    {
        var options = new DbContextOptionsBuilder<ValidationPackageDb>()
            .UseNpgsql("Host=localhost;Database=avpr_query_test;Username=test;Password=test")
            .Options;

        using var database = new ValidationPackageDb(options);
        var indexSql = PackageDiscoveryQueries.PackageVersions(database).ToQueryString();
        var metadataSql = PackageDiscoveryQueries.PackageMetadata(database).ToQueryString();

        Assert.DoesNotContain("PackageContent", indexSql, StringComparison.Ordinal);
        Assert.DoesNotContain("PackageContent", metadataSql, StringComparison.Ordinal);
        Assert.Contains("Inputs", metadataSql, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LatestArtifactExcludesPrereleaseAndBuildVersions()
    {
        using var factory = new PackageRegistryWebApplicationFactory();
        await factory.SeedPackagesAsync([
            CreatePackage("stable-only", 1, 2, 3),
            CreatePackage("stable-only", 99, 0, 0, preRelease: "rc.1"),
            CreatePackage("stable-only", 100, 0, 0, buildMetadata: "build.1")
        ]);

        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/api/v1/packages/stable-only");
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(1, document.RootElement.GetProperty("MajorVersion").GetInt32());
        Assert.Equal(2, document.RootElement.GetProperty("MinorVersion").GetInt32());
        Assert.Equal(3, document.RootElement.GetProperty("PatchVersion").GetInt32());
    }

    [Fact]
    public async Task InvalidDeclarationsAreRejectedOnWriteAndMetadataRead()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Authentication:ApiKey"] = "test-api-key"
        };
        using var factory = new PackageRegistryWebApplicationFactory(settings);
        var invalid = CreatePackage("invalid-input", 1, 0, 0);
        invalid.Inputs.Single().InputBinding.Prefix = "";

        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/packages/");
        request.Headers.Add("X-API-KEY", "test-api-key");
        request.Content = new StringContent(
            JsonSerializer.Serialize(
                invalid,
                new JsonSerializerOptions { PropertyNamingPolicy = null }),
            Encoding.UTF8,
            "application/json");

        using var rejected = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rejected.StatusCode);

        await factory.SeedPackageAsync(invalid);
        using var metadata = await client.GetAsync(
            "/api/v1/packages/invalid-input/1.0.0/metadata");
        Assert.Equal(HttpStatusCode.Conflict, metadata.StatusCode);
    }

    [Fact]
    public async Task OpenApiDeprecatesTheAllContentCollectionAndDescribesDiscoveryRoutes()
    {
        using var factory = new PackageRegistryWebApplicationFactory();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        var paths = document.RootElement.GetProperty("paths");
        Assert.True(paths.GetProperty("/api/v1/packages").GetProperty("get")
            .GetProperty("deprecated").GetBoolean());
        Assert.Equal(
            "GetPackageIndex",
            paths.GetProperty("/api/v1/package-index").GetProperty("get")
                .GetProperty("operationId").GetString());
        Assert.Equal(
            "GetPackageVersions",
            paths.GetProperty("/api/v1/packages/{name}/versions").GetProperty("get")
                .GetProperty("operationId").GetString());
        Assert.Equal(
            "GetPackageMetadata",
            paths.GetProperty("/api/v1/packages/{name}/{version}/metadata").GetProperty("get")
                .GetProperty("operationId").GetString());
        Assert.DoesNotContain(
            paths.EnumerateObject(),
            path => path.Name.StartsWith("/schemas/", StringComparison.Ordinal));
    }

    private static RegistryValidationPackage CreatePackage(
        string name,
        int major,
        int minor,
        int patch,
        string preRelease = "",
        string buildMetadata = "") => new()
    {
        Name = name,
        Summary = $"Summary for {name}",
        Description = $"Description for {name}",
        MajorVersion = major,
        MinorVersion = minor,
        PatchVersion = patch,
        PreReleaseVersionSuffix = preRelease,
        BuildMetadataVersionSuffix = buildMetadata,
        PackageContent = Encoding.UTF8.GetBytes($"printfn \"{name}\"\n"),
        ReleaseDate = new DateOnly(2026, 8, 14),
        ProgrammingLanguage = "FSharp",
        Inputs =
        [
            new CommandInputParameter
            {
                Id = "verbose",
                Type = new CommandInputType
                {
                    PrimitiveType = CwlPrimitive.Boolean,
                    IsNullable = true
                },
                InputBinding = new CommandInputBinding { Prefix = "--verbose" }
            }
        ]
    };

    private static async Task SetHashAsync(
        PackageRegistryWebApplicationFactory factory,
        RegistryValidationPackage package,
        string hash)
    {
        using var scope = factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ValidationPackageDb>();
        var stored = await database.Hashes.SingleAsync(item =>
            item.PackageName == package.Name
            && item.PackageMajorVersion == package.MajorVersion
            && item.PackageMinorVersion == package.MinorVersion
            && item.PackagePatchVersion == package.PatchVersion
            && item.PackagePreReleaseVersionSuffix == package.PreReleaseVersionSuffix
            && item.PackageBuildMetadataVersionSuffix == package.BuildMetadataVersionSuffix);
        stored.Hash = hash;
        await database.SaveChangesAsync();
    }

    private static async Task<int> GetDownloadsAsync(
        PackageRegistryWebApplicationFactory factory,
        RegistryValidationPackage package)
    {
        using var scope = factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ValidationPackageDb>();
        return await database.Downloads
            .Where(item =>
                item.PackageName == package.Name
                && item.PackageMajorVersion == package.MajorVersion
                && item.PackageMinorVersion == package.MinorVersion
                && item.PackagePatchVersion == package.PatchVersion
                && item.PackagePreReleaseVersionSuffix == package.PreReleaseVersionSuffix
                && item.PackageBuildMetadataVersionSuffix == package.BuildMetadataVersionSuffix)
            .Select(item => item.Downloads)
            .SingleAsync();
    }
}
