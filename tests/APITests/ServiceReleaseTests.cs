using System.Net;
using System.Text.Json;
using PackageRegistryTestHost;

namespace APITests;

public class ServiceReleaseTests
{
    private const string Revision = "0123456789abcdef0123456789abcdef01234567";

    private static readonly IReadOnlyDictionary<string, string?> BuildSettings =
        new Dictionary<string, string?>
        {
            ["AVPR_BUILD_REVISION"] = Revision,
            ["AVPR_BUILD_CHANNEL"] = "dev",
            ["AVPR_BUILD_CREATED"] = "2026-07-27T12:34:56Z"
        };

    [Fact]
    public async Task VersionEndpointReportsTheBuiltServiceAndReleaseIdentity()
    {
        using var factory = new PackageRegistryWebApplicationFactory(BuildSettings);
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/_version");

        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        var root = document.RootElement;
        Assert.Equal("avpr", root.GetProperty("service").GetProperty("name").GetString());
        Assert.Equal("1.2.0", root.GetProperty("service").GetProperty("version").GetString());
        Assert.Equal(
            ["v1"],
            root.GetProperty("api")
                .GetProperty("versions")
                .EnumerateArray()
                .Select(value => value.GetString())
        );

        var build = root.GetProperty("build");
        Assert.Equal(Revision, build.GetProperty("revision").GetString());
        Assert.Equal("dev", build.GetProperty("channel").GetString());
        Assert.Equal("2026-07-27T12:34:56+00:00", build.GetProperty("created").GetString());

        var release = root.GetProperty("release");
        Assert.Equal("Lightweight package discovery", release.GetProperty("name").GetString());
        Assert.EndsWith(
            "remains compatible but is deprecated.",
            release.GetProperty("summary").GetString()
        );
        Assert.Equal("/releases", release.GetProperty("notesUrl").GetString());
        Assert.False(root.TryGetProperty("Service", out _));
    }

    [Fact]
    public async Task ReleasesPageRendersTheRunningBuildAndBundledReleaseNotes()
    {
        using var factory = new PackageRegistryWebApplicationFactory(BuildSettings);
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/releases");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Currently running: 1.2.0 - Lightweight package discovery", html);
        Assert.Contains("<code>dev</code>", html);
        Assert.Contains($"/commit/{Revision}", html);
        Assert.Contains("20260724094518_AddCWLInputs", html);
        Assert.Contains("content-free package-index", html);
        Assert.Contains("Why the first release supports a scalar subset", html);
        Assert.Contains("File</code>, <code>Directory", html);
        Assert.Contains("href=\"https://avpr.nfdi4plants.org/swagger\"", html);
        Assert.Contains("href=\"../../docs/packages/cwl-inputs.md\"", html);
        Assert.Contains("aria-current=\"page\" href=\"/releases\"", html);
    }

    [Fact]
    public async Task ExistingHealthEndpointRemainsAvailable()
    {
        using var factory = new PackageRegistryWebApplicationFactory();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/_health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
