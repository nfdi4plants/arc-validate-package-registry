using System.Text.Json;
using Json.Schema;
using PackageRegistryTestHost;

namespace APITests;

public class SchemaRouteTests
{
    public static TheoryData<string, string> SchemaRoutes => new()
    {
        {
            "validation-package-frontmatter.schema.json",
            "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
        },
        {
            "validation-packages.schema.json",
            "https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json"
        }
    };

    [Theory]
    [MemberData(nameof(SchemaRoutes))]
    public async Task SchemaRoutesServeTheCommittedDraft202012Bytes(
        string fileName,
        string expectedId)
    {
        using var factory = new PackageRegistryWebApplicationFactory();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync($"/schemas/v1/{fileName}");

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/schema+json", response.Content.Headers.ContentType?.MediaType);

        var actual = await response.Content.ReadAsByteArrayAsync();
        var sourcePath = Path.Combine(FindRepositoryRoot(), "schemas", fileName);
        Assert.Equal(await File.ReadAllBytesAsync(sourcePath), actual);

        using var document = JsonDocument.Parse(actual);
        Assert.Equal(
            "https://json-schema.org/draft/2020-12/schema",
            document.RootElement.GetProperty("$schema").GetString());
        Assert.Equal(expectedId, document.RootElement.GetProperty("$id").GetString());

        var validation = MetaSchemas.Draft202012.Evaluate(document.RootElement);
        Assert.True(validation.IsValid, validation.ToString());
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "arc-validate-package-registry.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
