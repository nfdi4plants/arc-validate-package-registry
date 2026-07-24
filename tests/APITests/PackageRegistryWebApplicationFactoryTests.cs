using PackageRegistryService.Models;
using PackageRegistryTestHost;
using System.Text;
using System.Text.Json;
using static AVPRIndex.Domain;

namespace APITests;

public class PackageRegistryWebApplicationFactoryTests
{
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
