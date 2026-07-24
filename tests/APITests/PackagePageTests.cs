using System.Net;
using System.Text;
using PackageRegistryService.Models;
using PackageRegistryTestHost;
using static AVPRIndex.Domain;

namespace APITests;

public class PackagePageTests
{
    [Fact]
    public async Task PackagePageRendersCwlInputsAndEscapesDeclaredValues()
    {
        using var factory = new PackageRegistryWebApplicationFactory();
        var package = CreatePackage("page-with-inputs");
        package.Inputs =
        [
            new CommandInputParameter
            {
                Id = "output<script>",
                Type = new CommandInputType { PrimitiveType = CwlPrimitive.String },
                Label = "Output & file",
                Doc = "Write <script>alert('x')</script> here",
                InputBinding = new CommandInputBinding
                {
                    Position = 2,
                    Prefix = "--output=<",
                    Separate = false
                }
            },
            new CommandInputParameter
            {
                Id = "threads",
                Type = new CommandInputType
                {
                    PrimitiveType = CwlPrimitive.Int,
                    IsNullable = true
                },
                InputBinding = new CommandInputBinding()
            }
        ];

        await factory.SeedPackageAsync(package);

        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/package/page-with-inputs/1.2.3");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("<h2>Available Commands</h2>", html);
        Assert.Contains("<code>output&lt;script&gt;</code>", html);
        Assert.Contains("<code>string</code>", html);
        Assert.Contains("<code>--output=&lt;</code>", html);
        Assert.Contains("position: 2; separate: false", html);
        Assert.Contains("<strong>Output &amp; file</strong>", html);
        Assert.Contains("Write &lt;script&gt;alert(&#x27;x&#x27;)&lt;/script&gt; here", html);
        Assert.Contains("<code>int?</code>", html);
        Assert.Contains("<em>positional</em>", html);
        Assert.DoesNotContain("<script>alert", html);
    }

    [Fact]
    public async Task LatestPackagePageOmitsTheInputsSectionWhenNoInputsAreDeclared()
    {
        using var factory = new PackageRegistryWebApplicationFactory();
        await factory.SeedPackageAsync(CreatePackage("page-without-inputs"));

        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/package/page-without-inputs");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("<h2>Available Commands</h2>", html);
        Assert.DoesNotContain("<th scope=\"col\">Binding</th>", html);
    }

    private static ValidationPackage CreatePackage(string name) => new()
    {
        Name = name,
        Summary = "Page test",
        Description = "Served by the in-process package registry.",
        MajorVersion = 1,
        MinorVersion = 2,
        PatchVersion = 3,
        ProgrammingLanguage = "FSharp",
        PackageContent = Encoding.UTF8.GetBytes("printfn \"test\"\n"),
        ReleaseDate = new DateOnly(2026, 7, 24)
    };
}
