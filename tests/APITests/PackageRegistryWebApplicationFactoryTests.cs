using PackageRegistryService.Models;
using PackageRegistryTestHost;
using System.Text;
using System.Text.Json;
using static AVPRIndex.Domain;

namespace APITests;

public class PackageRegistryWebApplicationFactoryTests
{
    private static readonly string[] SupportedCommandInputTypes =
    [
        "boolean",
        "boolean?",
        "int",
        "int?",
        "long",
        "long?",
        "float",
        "float?",
        "double",
        "double?",
        "string",
        "string?"
    ];

    [Fact]
    public async Task SeededPackageIsServedThroughTheRealApiPipeline()
    {
        using var factory = new PackageRegistryWebApplicationFactory();
        await factory.SeedPackageAsync(CreatePackage());

        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/api/v1/packages/test-target/1.2.3");

        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        var root = document.RootElement;
        Assert.Equal("test-target", root.GetProperty("Name").GetString());
        var input = root.GetProperty("Inputs")[0];
        Assert.Equal("verbose", input.GetProperty("id").GetString());
        Assert.Equal("boolean?", input.GetProperty("type").GetString());
    }

    [Fact]
    public async Task OpenApiDescribesTheSupportedCwlInputContract()
    {
        using var factory = new PackageRegistryWebApplicationFactory();
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/swagger/v1/swagger.json");

        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");
        var inputType = schemas.GetProperty("CommandInputType");
        Assert.Equal("string", inputType.GetProperty("type").GetString());
        Assert.Equal(
            SupportedCommandInputTypes,
            inputType.GetProperty("enum").EnumerateArray().Select(value => value.GetString()));
        Assert.False(inputType.TryGetProperty("properties", out _));
        Assert.False(schemas.TryGetProperty("CwlPrimitive", out _));

        var parameter = schemas.GetProperty("CommandInputParameter");
        var required = parameter
            .GetProperty("required")
            .EnumerateArray()
            .Select(value => value.GetString())
            .ToArray();

        Assert.Contains("id", required);
        Assert.Contains("type", required);
        Assert.Contains("inputBinding", required);

        var properties = parameter.GetProperty("properties");
        Assert.True(properties.TryGetProperty("id", out _));
        Assert.True(properties.TryGetProperty("type", out _));
        Assert.True(properties.TryGetProperty("label", out _));
        Assert.True(properties.TryGetProperty("doc", out _));
        Assert.True(properties.TryGetProperty("inputBinding", out _));
        Assert.False(properties.TryGetProperty("Id", out _));
        Assert.False(properties.TryGetProperty("Type", out _));
        Assert.False(properties.TryGetProperty("InputBinding", out _));

        foreach (var requiredProperty in new[] { "id", "type", "inputBinding" })
        {
            var property = properties.GetProperty(requiredProperty);
            Assert.False(
                property.TryGetProperty("nullable", out var nullable) && nullable.GetBoolean(),
                $"{requiredProperty} must be required and non-null: {property.GetRawText()}");
        }
    }

    internal static ValidationPackage CreatePackage() => new()
    {
        Name = "test-target",
        Summary = "Test target",
        Description = "Served by the in-process package registry.",
        MajorVersion = 1,
        MinorVersion = 2,
        PatchVersion = 3,
        PackageContent = Encoding.UTF8.GetBytes("printfn \"test\"\n"),
        ReleaseDate = new DateOnly(2026, 7, 24),
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
                InputBinding = new CommandInputBinding
                {
                    Prefix = "--verbose"
                }
            }
        ]
    };
}
