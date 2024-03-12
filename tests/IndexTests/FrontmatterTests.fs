namespace FrontmatterTests

open System
open Xunit
open AVPRIndex
open ReferenceObjects

module InMemory =

    [<Fact>]
    let ``valid frontmatter capture guides lead to results`` () =
        Assert.All(
            [Frontmatter.validMandatoryFrontmatter; Frontmatter.validFullFrontmatter; Frontmatter.invalidMissingMandatoryFrontmatter],
            (fun fm ->
                Assert.True ((Frontmatter.tryExtractFromString fm).IsSome)
            )
        )

    [<Fact>]
    let ``valid frontmatter capture guides lead to correctly extracted substrings`` () =
        Assert.All(
            [
                Frontmatter.validMandatoryFrontmatter, Frontmatter.validMandatoryFrontmatterExtracted 
                Frontmatter.validFullFrontmatter, Frontmatter.validFullFrontmatterExtracted
                Frontmatter.invalidMissingMandatoryFrontmatter, Frontmatter.invalidMissingMandatoryFrontmatterExtracted
            ],
            (fun (fm, expected) ->
                let actual = Frontmatter.extractFromString fm
                Assert.Equal(expected, actual)
            )
        )

    [<Fact>]
    let ``invalid frontmatter capture substrings are leading to None`` () =
        Assert.All(
            [Frontmatter.invalidEndFrontmatter; Frontmatter.invalidStartFrontmatter],
            (fun fm ->
                Assert.True ((Frontmatter.tryExtractFromString fm).IsNone)
            )
        )

module IO =

    open System.IO

    [<Fact>]
    let ``valid frontmatter substring is extracted from valid mandatory field test file`` () =

        let actual = File.ReadAllText("fixtures/valid@1.0.0.fsx") |> Frontmatter.tryExtractFromString

        Assert.True actual.IsSome
        Assert.Equal (Frontmatter.validMandatoryFrontmatterExtracted, actual.Value)

    [<Fact>]
    let ``valid frontmatter substring is correctly from all fields test file`` () =

        let actual = File.ReadAllText("fixtures/valid@2.0.0.fsx") |> Frontmatter.tryExtractFromString

        Assert.True actual.IsSome
        Assert.Equal (Frontmatter.validFullFrontmatterExtracted, actual.Value)

    [<Fact>]
    let ``frontmatter substring is extracted although metadata is missing fields`` () =

        let actual = File.ReadAllText("fixtures/invalid@0.0.fsx") |> Frontmatter.tryExtractFromString

        Assert.True actual.IsSome
        Assert.Equal (Frontmatter.invalidMissingMandatoryFrontmatterExtracted, actual.Value)
