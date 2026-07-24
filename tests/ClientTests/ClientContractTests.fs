namespace ClientContractTests

open System
open System.Text
open Xunit
open AVPRClient
open PackageRegistryService.Models
open PackageRegistryTestHost

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
                ReleaseDate = DateOnly(2026, 7, 24)
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
    }
