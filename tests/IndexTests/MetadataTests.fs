namespace MetadataTests

open System
open Xunit
open AVPRIndex
open AVPRIndex.Domain
open AVPRIndex.Frontmatter
open ReferenceObjects
open Utils

module FSharp =

    module InMemory =

        [<Fact>]
        let ``valid metadata is extracted from valid comment frontmatter`` () =
            Assert.All(
                [
                    Frontmatter.FSharp.Comment.validMandatoryFrontmatter, Metadata.FSharp.validMandatoryFrontmatter
                    Frontmatter.FSharp.Comment.validFullFrontmatter, Metadata.FSharp.validFullFrontmatter
                ],
                (fun (fm, expected) ->
                    let actual = ValidationPackageMetadata.extractFromString FSharpFrontmatter fm
                    Assert.Equivalent(expected, actual)
                )
            )

        [<Fact>]
        let ``valid metadata is extracted from valid binding frontmatter`` () =
            Assert.All(
                [
                    Frontmatter.FSharp.Binding.validMandatoryFrontmatter, Metadata.FSharp.validMandatoryFrontmatter
                    Frontmatter.FSharp.Binding.validFullFrontmatter, Metadata.FSharp.validFullFrontmatter
                ],
                (fun (fm, expected) ->
                    let actual = ValidationPackageMetadata.extractFromString FSharpFrontmatter fm
                    Assert.Equivalent(expected, actual)
                )
            )

    module IO =

        open System.IO

        [<Fact>]
        let ``valid metadata is extracted from valid mandatory field test file with comment frontmatter`` () =

            let actual = File.ReadAllText("fixtures/Frontmatter/Comment/valid@1.0.0.fsx") |> ValidationPackageMetadata.extractFromString FSharpFrontmatter

            Assert.MetadataValid(actual)
            Assert.Equivalent(Metadata.FSharp.validMandatoryFrontmatter, actual)

        [<Fact>]
        let ``valid metadata is extracted from all fields test file with comment frontmatter`` () =

            let actual = File.ReadAllText("fixtures/Frontmatter/Comment/valid@2.0.0.fsx") |> ValidationPackageMetadata.extractFromString FSharpFrontmatter

            Assert.MetadataValid(actual)
            Assert.Equivalent(Metadata.FSharp.validFullFrontmatter, actual)

        [<Fact>]
        let ``invalid metadata is extracted from testfile with missing fields with comment frontmatter`` () =

            let actual = File.ReadAllText("fixtures/Frontmatter/Comment/invalid@0.0.fsx") |> ValidationPackageMetadata.extractFromString FSharpFrontmatter

            Assert.ThrowsAny(fun () -> Assert.MetadataValid(actual)) |> ignore
            Assert.Equivalent(Metadata.FSharp.invalidMissingMandatoryFrontmatter, actual)

        [<Fact>]
        let ``valid metadata is extracted from valid mandatory field test file with binding frontmatter`` () =

            let actual = File.ReadAllText("fixtures/Frontmatter/Binding/valid@1.0.0.fsx") |> ValidationPackageMetadata.extractFromString FSharpFrontmatter

            Assert.MetadataValid(actual)
            Assert.Equivalent(Metadata.FSharp.validMandatoryFrontmatter, actual)

        [<Fact>]
        let ``valid metadata is extracted from all fields test file with binding frontmatter`` () =

            let actual = File.ReadAllText("fixtures/Frontmatter/Binding/valid@2.0.0.fsx") |> ValidationPackageMetadata.extractFromString FSharpFrontmatter

            Assert.MetadataValid(actual)
            Assert.Equivalent(Metadata.FSharp.validFullFrontmatter, actual)

        [<Fact>]
        let ``invalid metadata is extracted from testfile with missing fields with binding frontmatter`` () =

            let actual = File.ReadAllText("fixtures/Frontmatter/Binding/invalid@0.0.fsx") |> ValidationPackageMetadata.extractFromString FSharpFrontmatter

            Assert.ThrowsAny(fun () -> Assert.MetadataValid(actual)) |> ignore
            Assert.Equivalent(Metadata.FSharp.invalidMissingMandatoryFrontmatter, actual)


module Python =

    module InMemory =

        [<Fact>]
        let ``valid metadata is extracted from valid comment frontmatter`` () =
            Assert.All(
                [
                    Frontmatter.Python.Comment.validMandatoryFrontmatter, Metadata.Python.validMandatoryFrontmatter
                    Frontmatter.Python.Comment.validFullFrontmatter, Metadata.Python.validFullFrontmatter
                ],
                (fun (fm, expected) ->
                    let actual = ValidationPackageMetadata.extractFromString PythonFrontmatter fm
                    Assert.Equivalent(expected, actual)
                )
            )

        [<Fact>]
        let ``valid metadata is extracted from valid binding frontmatter`` () =
            Assert.All(
                [
                    Frontmatter.Python.Binding.validMandatoryFrontmatter, Metadata.Python.validMandatoryFrontmatter
                    Frontmatter.Python.Binding.validFullFrontmatter, Metadata.Python.validFullFrontmatter
                ],
                (fun (fm, expected) ->
                    let actual = ValidationPackageMetadata.extractFromString PythonFrontmatter fm
                    Assert.Equivalent(expected, actual)
                )
            )

    module IO =

        open System.IO

        [<Fact>]
        let ``valid metadata is extracted from valid mandatory field test file with comment frontmatter`` () =

            let actual = File.ReadAllText("fixtures/Frontmatter/Comment/valid@1.0.0.py") |> ValidationPackageMetadata.extractFromString PythonFrontmatter

            Assert.MetadataValid(actual)
            Assert.Equivalent(Metadata.Python.validMandatoryFrontmatter, actual)

        [<Fact>]
        let ``valid metadata is extracted from all fields test file with comment frontmatter`` () =

            let actual = File.ReadAllText("fixtures/Frontmatter/Comment/valid@2.0.0.py") |> ValidationPackageMetadata.extractFromString PythonFrontmatter

            Assert.MetadataValid(actual)
            Assert.Equivalent(Metadata.Python.validFullFrontmatter, actual)

        [<Fact>]
        let ``invalid metadata is extracted from testfile with missing fields with comment frontmatter`` () =

            let actual = File.ReadAllText("fixtures/Frontmatter/Comment/invalid@0.0.py") |> ValidationPackageMetadata.extractFromString PythonFrontmatter

            Assert.ThrowsAny(fun () -> Assert.MetadataValid(actual)) |> ignore
            Assert.Equivalent(Metadata.Python.invalidMissingMandatoryFrontmatter, actual)

        [<Fact>]
        let ``valid metadata is extracted from valid mandatory field test file with binding frontmatter`` () =

            let actual = File.ReadAllText("fixtures/Frontmatter/Binding/valid@1.0.0.py") |> ValidationPackageMetadata.extractFromString PythonFrontmatter

            Assert.MetadataValid(actual)
            Assert.Equivalent(Metadata.Python.validMandatoryFrontmatter, actual)

        [<Fact>]
        let ``valid metadata is extracted from all fields test file with binding frontmatter`` () =

            let actual = File.ReadAllText("fixtures/Frontmatter/Binding/valid@2.0.0.py") |> ValidationPackageMetadata.extractFromString PythonFrontmatter

            Assert.MetadataValid(actual)
            Assert.Equivalent(Metadata.Python.validFullFrontmatter, actual)

        [<Fact>]
        let ``invalid metadata is extracted from testfile with missing fields with binding frontmatter`` () =

            let actual = File.ReadAllText("fixtures/Frontmatter/Binding/invalid@0.0.py") |> ValidationPackageMetadata.extractFromString PythonFrontmatter

            Assert.ThrowsAny(fun () -> Assert.MetadataValid(actual)) |> ignore
            Assert.Equivalent(Metadata.Python.invalidMissingMandatoryFrontmatter, actual)