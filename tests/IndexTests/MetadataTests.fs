namespace MetadataTests

open System
open Xunit
open AVPRIndex
open AVPRIndex.Domain
open AVPRIndex.Frontmatter
open ReferenceObjects
open Utils

module InMemory =

    [<Fact>]
    let ``valid metadata is extracted from valid frontmatter`` () =
        Assert.All(
            [
                Frontmatter.validMandatoryFrontmatter, Metadata.validMandatoryFrontmatter
                Frontmatter.validFullFrontmatter, Metadata.validFullFrontmatter
            ],
            (fun (fm, expected) ->
                let actual = ValidationPackageMetadata.extractFromString fm
                Assert.Equivalent(expected, actual)
            )
        )

module IO =

    open System.IO

    [<Fact>]
    let ``valid metadata is extracted from valid mandatory field test file`` () =

        let actual = File.ReadAllText("fixtures/valid@1.0.0.fsx") |> ValidationPackageMetadata.extractFromString

        Assert.MetadataValid(actual)
        Assert.Equivalent(Metadata.validMandatoryFrontmatter, actual)

    [<Fact>]
    let ``valid metadata is extracted from all fields test file`` () =

        let actual = File.ReadAllText("fixtures/valid@2.0.0.fsx") |> ValidationPackageMetadata.extractFromString

        Assert.MetadataValid(actual)
        Assert.Equivalent(Metadata.validFullFrontmatter, actual)

    [<Fact>]
    let ``invalid metadata is extracted from testfile with missing fields`` () =

        let actual = File.ReadAllText("fixtures/invalid@0.0.fsx") |> ValidationPackageMetadata.extractFromString

        Assert.ThrowsAny(fun () -> Assert.MetadataValid(actual)) |> ignore
        Assert.Equivalent(Metadata.invalidMissingMandatoryFrontmatter, actual)