namespace ClientContractTests

open System
open System.Text
open Xunit
open AVPRClient
open PackageRegistryService.Models
open PackageRegistryTestHost
open AVPRIndex

module InProcessRegistry =

    [<Fact>]
    let ``generated client reads a package from the in-process registry`` () = task {
        use factory = new PackageRegistryWebApplicationFactory()

        let package =
            PackageRegistryService.Models.ValidationPackage(
                Name = "client-target",
                Summary = "Client target",
                Description = "Served by the in-process package registry.",
                MajorVersion = 1,
                MinorVersion = 2,
                PatchVersion = 3,
                PackageContent = Encoding.UTF8.GetBytes("printfn \"test\"\n"),
                ReleaseDate = DateOnly(2026, 7, 24),
                Inputs = ResizeArray [
                    Domain.CommandInputParameter.create(
                        "verbose",
                        Domain.CommandInputType.create(Domain.CwlPrimitive.Boolean, true),
                        Domain.CommandInputBinding(Prefix = "--verbose"),
                        Doc = "Enable verbose logging"
                    )
                ]
            )

        do! factory.SeedPackageAsync(package)

        use httpClient = factory.CreateClient()
        let client = Client(httpClient)
        client.BaseUrl <- httpClient.BaseAddress.ToString()

        let! actual = client.GetPackageByNameAndVersionAsync("client-target", "1.2.3")

        Assert.Equal("client-target", actual.Name)
        Assert.Equal(1, actual.MajorVersion)
        Assert.Equal(2, actual.MinorVersion)
        Assert.Equal(3, actual.PatchVersion)

        let input = Assert.Single(actual.Inputs)
        Assert.Equal("verbose", input.Id)
        Assert.Equal(AVPRClient.CommandInputType.Boolean_, input.Type)
        Assert.Equal("Enable verbose logging", input.Doc)
        Assert.Equal(0, input.InputBinding.Position)
        Assert.Equal("--verbose", input.InputBinding.Prefix)
        Assert.True(input.InputBinding.Separate)
    }
