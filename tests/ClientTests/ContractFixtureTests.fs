namespace ClientContractTests

open System
open System.IO
open System.Text.Json.Nodes
open AVPR.Staging
open AVPRClient.Interop
open PackageRegistryService.Models
open PackageRegistryTestHost
open ValidationPackage.Codecs
open Xunit

module CanonicalFixture =

    let private fixturePath fileName =
        Path.Combine(
            AppContext.BaseDirectory,
            "contract-fixtures",
            "portable-validation-package",
            fileName
        )

    [<Fact>]
    let canonicalFixtureCrossesEveryPackageBoundary () = task {
        let scriptPath =
            fixturePath "canonical-contract@1.2.3-rc.1+build.7.fsx"
        let expectedMetadata =
            fixturePath "metadata.json"
            |> File.ReadAllText
            |> ValidationPackageJson.decodeOrFail
        let staged =
            StagedValidationPackage.fromFile
                scriptPath
                (DateTimeOffset(2026, 7, 31, 12, 0, 0, TimeSpan.Zero))

        Assert.Equal(expectedMetadata, staged.Metadata)
        Assert.Equal(
            "1.2.3-rc.1+build.7",
            StagedValidationPackage.getSemanticVersionString staged
        )

        let arcDirectory = staged.Metadata.Inputs[0]
        Assert.Equal("arc-directory", arcDirectory.Id)
        Assert.Equal(
            ValidationPackage.Model.CwlPrimitive.String,
            arcDirectory.Type.PrimitiveType
        )
        Assert.False(arcDirectory.Type.IsNullable)
        Assert.Equal("--arc-directory", arcDirectory.InputBinding.Prefix)

        let servicePackage =
            ValidationPackageModelMappings.ToServiceModel(staged)

        use factory = new PackageRegistryWebApplicationFactory()
        do! factory.SeedPackageAsync(servicePackage)

        use httpClient = factory.CreateClient()
        let! response =
            httpClient.GetAsync(
                "/api/v1/packages/canonical-contract/1.2.3-rc.1+build.7"
            )
        use response = response

        response.EnsureSuccessStatusCode() |> ignore
        let! rawJson = response.Content.ReadAsStringAsync()
        let expectedApiJson =
            fixturePath "api-package.json"
            |> File.ReadAllText
        let actualApiJson = JsonNode.Parse(rawJson)
        actualApiJson["PackageContent"] <-
            JsonValue.Create("__fixture-content__")

        Assert.True(
            JsonNode.DeepEquals(
                JsonNode.Parse(expectedApiJson),
                actualApiJson
            ),
            $"Raw API JSON drifted.{Environment.NewLine}Expected: {expectedApiJson}{Environment.NewLine}Actual: {rawJson}"
        )

        let client = AVPRClient.Client(httpClient)
        client.BaseUrl <- httpClient.BaseAddress.ToString()

        let! index = client.GetPackageIndexAsync()
        let generatedIdentity = Assert.Single(index)
        let modelIdentity = generatedIdentity.ToModel()
        Assert.Equal(
            ValidationPackage.Model.ValidationPackageMetadata.getIdentity expectedMetadata,
            modelIdentity
        )

        let! versions = client.GetPackageVersionsAsync("canonical-contract")
        Assert.Equal<string>([| "1.2.3-rc.1+build.7" |], versions)

        let! generatedMetadata =
            client.GetPackageMetadataAsync(
                "canonical-contract",
                "1.2.3-rc.1+build.7"
            )
        Assert.Equal(expectedMetadata, generatedMetadata.ToModel())

        let! generated =
            client.GetPackageByNameAndVersionAsync(
                "canonical-contract",
                "1.2.3-rc.1+build.7"
            )
        let roundTripped = generated.ToModel()

        Assert.Equal(expectedMetadata, roundTripped)
        Assert.True(
            NormalizedContent.fromFile scriptPath = generated.PackageContent,
            "Generated client package content drifted."
        )

        let verbose = generated.Inputs |> Seq.item 1
        Assert.Equal("--verbose", verbose.InputBinding.Prefix)
    }
