namespace FrontmatterTests

open System
open Xunit
open AVPRIndex
open ReferenceObjects

module FSharp = 
    module Comment =

        module InMemory =

            [<Fact>]
            let ``valid frontmatter capture guides lead to results`` () =
                Assert.All(
                    [Frontmatter.FSharp.Comment.validMandatoryFrontmatter; Frontmatter.FSharp.Comment.validFullFrontmatter; Frontmatter.FSharp.Comment.invalidMissingMandatoryFrontmatter],
                    (fun fm ->
                        Assert.True ((Frontmatter.tryExtractFromString FSharpFrontmatter fm).IsSome)
                    )
                )

            [<Fact>]
            let ``valid frontmatter capture guides lead to correctly extracted substrings`` () =
                Assert.All(
                    [
                        Frontmatter.FSharp.Comment.validMandatoryFrontmatter, Frontmatter.FSharp.Comment.validMandatoryFrontmatterExtracted 
                        Frontmatter.FSharp.Comment.validFullFrontmatter, Frontmatter.FSharp.Comment.validFullFrontmatterExtracted
                        Frontmatter.FSharp.Comment.invalidMissingMandatoryFrontmatter, Frontmatter.FSharp.Comment.invalidMissingMandatoryFrontmatterExtracted
                    ],
                    (fun (fm, expected) ->
                        let actual = Frontmatter.extractFromString FSharpFrontmatter  fm
                        Assert.Equal(expected, actual)
                    )
                )

            [<Fact>]
            let ``invalid frontmatter capture substrings are leading to None`` () =
                Assert.All(
                    [Frontmatter.FSharp.Comment.invalidEndFrontmatter; Frontmatter.FSharp.Comment.invalidStartFrontmatter],
                    (fun fm ->
                        Assert.True ((Frontmatter.tryExtractFromString FSharpFrontmatter fm).IsNone)
                    )
                )

        module IO =

            open System.IO

            [<Fact>]
            let ``valid frontmatter substring is extracted from valid mandatory field test file`` () =

                let actual = File.ReadAllText("fixtures/Frontmatter/Comment/valid@1.0.0.fsx") |> Frontmatter.tryExtractFromString FSharpFrontmatter

                Assert.True actual.IsSome
                Assert.Equal (Frontmatter.FSharp.Comment.validMandatoryFrontmatterExtracted, actual.Value)

            [<Fact>]
            let ``valid frontmatter substring is correctly from all fields test file`` () =

                let actual = File.ReadAllText("fixtures/Frontmatter/Comment//valid@2.0.0.fsx") |> Frontmatter.tryExtractFromString FSharpFrontmatter

                Assert.True actual.IsSome
                Assert.Equal (Frontmatter.FSharp.Comment.validFullFrontmatterExtracted, actual.Value)

            [<Fact>]
            let ``frontmatter substring is extracted although metadata is missing fields`` () =

                let actual = File.ReadAllText("fixtures/Frontmatter/Comment//invalid@0.0.fsx") |> Frontmatter.tryExtractFromString FSharpFrontmatter

                Assert.True actual.IsSome
                Assert.Equal (Frontmatter.FSharp.Comment.invalidMissingMandatoryFrontmatterExtracted, actual.Value)

    module Binding =

        module InMemory =

            [<Fact>]
            let ``valid frontmatter capture guides lead to results`` () =
                Assert.All(
                    [Frontmatter.FSharp.Binding.validMandatoryFrontmatter; Frontmatter.FSharp.Binding.validFullFrontmatter; Frontmatter.FSharp.Binding.invalidMissingMandatoryFrontmatter],
                    (fun fm ->
                        Assert.True ((Frontmatter.tryExtractFromString FSharpFrontmatter fm).IsSome)
                    )
                )

            [<Fact>]
            let ``valid frontmatter capture guides lead to correctly extracted substrings`` () =
                Assert.All(
                    [
                        Frontmatter.FSharp.Binding.validMandatoryFrontmatter, Frontmatter.FSharp.Binding.validMandatoryFrontmatterExtracted 
                        Frontmatter.FSharp.Binding.validFullFrontmatter, Frontmatter.FSharp.Binding.validFullFrontmatterExtracted
                        Frontmatter.FSharp.Binding.invalidMissingMandatoryFrontmatter, Frontmatter.FSharp.Binding.invalidMissingMandatoryFrontmatterExtracted
                    ],
                    (fun (fm, expected) ->
                        let actual = Frontmatter.extractFromString FSharpFrontmatter fm
                        Assert.Equal(expected, actual)
                    )
                )

            [<Fact>]
            let ``invalid frontmatter capture substrings are leading to None`` () =
                Assert.All(
                    [Frontmatter.FSharp.Binding.invalidEndFrontmatter; Frontmatter.FSharp.Binding.invalidStartFrontmatter],
                    (fun fm ->
                        Assert.True ((Frontmatter.tryExtractFromString FSharpFrontmatter fm).IsNone)
                    )
                )

        module IO =

            open System.IO

            [<Fact>]
            let ``valid frontmatter substring is extracted from valid mandatory field test file`` () =

                let actual = File.ReadAllText("fixtures/Frontmatter/Binding/valid@1.0.0.fsx") |> Frontmatter.tryExtractFromString FSharpFrontmatter

                Assert.True actual.IsSome
                Assert.Equal (Frontmatter.FSharp.Binding.validMandatoryFrontmatterExtracted, actual.Value)

            [<Fact>]
            let ``valid frontmatter substring is correctly from all fields test file`` () =

                let actual = File.ReadAllText("fixtures/Frontmatter/Binding/valid@2.0.0.fsx") |> Frontmatter.tryExtractFromString FSharpFrontmatter

                Assert.True actual.IsSome
                Assert.Equal (Frontmatter.FSharp.Binding.validFullFrontmatterExtracted, actual.Value)

            [<Fact>]
            let ``frontmatter substring is extracted although metadata is missing fields`` () =

                let actual = File.ReadAllText("fixtures/Frontmatter/Binding/invalid@0.0.fsx") |> Frontmatter.tryExtractFromString FSharpFrontmatter

                Assert.True actual.IsSome
                Assert.Equal (Frontmatter.FSharp.Binding.invalidMissingMandatoryFrontmatterExtracted, actual.Value)

module Python = 
    module Comment =

        module InMemory =

            [<Fact>]
            let ``valid single frontmatter can be extracted`` () =
                Assert.Equal(
                    expected = Frontmatter.Python.Comment.validMandatoryFrontmatterExtracted,
                    actual = (Frontmatter.extractFromString PythonFrontmatter Frontmatter.Python.Comment.validMandatoryFrontmatter)
                )
                

            [<Fact>]
            let ``valid frontmatter capture guides lead to results`` () =
                Assert.All(
                    [Frontmatter.Python.Comment.validMandatoryFrontmatter; Frontmatter.Python.Comment.validFullFrontmatter; Frontmatter.Python.Comment.invalidMissingMandatoryFrontmatter],
                    (fun fm ->
                        Assert.True ((Frontmatter.tryExtractFromString PythonFrontmatter fm).IsSome)
                    )
                )

            [<Fact>]
            let ``valid frontmatter capture guides lead to correctly extracted substrings`` () =
                Assert.All(
                    [
                        Frontmatter.Python.Comment.validMandatoryFrontmatter, Frontmatter.Python.Comment.validMandatoryFrontmatterExtracted 
                        Frontmatter.Python.Comment.validFullFrontmatter, Frontmatter.Python.Comment.validFullFrontmatterExtracted
                        Frontmatter.Python.Comment.invalidMissingMandatoryFrontmatter, Frontmatter.Python.Comment.invalidMissingMandatoryFrontmatterExtracted
                    ],
                    (fun (fm, expected) ->
                        let actual = Frontmatter.extractFromString PythonFrontmatter  fm
                        Assert.Equal(expected, actual)
                    )
                )

            [<Fact>]
            let ``invalid frontmatter capture substrings are leading to None`` () =
                Assert.All(
                    [Frontmatter.Python.Comment.invalidEndFrontmatter; Frontmatter.Python.Comment.invalidStartFrontmatter],
                    (fun fm ->
                        Assert.True ((Frontmatter.tryExtractFromString PythonFrontmatter fm).IsNone)
                    )
                )

        module IO =

            open System.IO

            [<Fact>]
            let ``valid frontmatter substring is extracted from valid mandatory field test file`` () =

                let actual = File.ReadAllText("fixtures/Frontmatter/Comment/valid@1.0.0.py") |> Frontmatter.tryExtractFromString PythonFrontmatter

                Assert.True actual.IsSome
                Assert.Equal (Frontmatter.Python.Comment.validMandatoryFrontmatterExtracted, actual.Value)

            [<Fact>]
            let ``valid frontmatter substring is correctly from all fields test file`` () =

                let actual = File.ReadAllText("fixtures/Frontmatter/Comment//valid@2.0.0.py") |> Frontmatter.tryExtractFromString PythonFrontmatter

                Assert.True actual.IsSome
                Assert.Equal (Frontmatter.Python.Comment.validFullFrontmatterExtracted, actual.Value)

            [<Fact>]
            let ``frontmatter substring is extracted although metadata is missing fields`` () =

                let actual = File.ReadAllText("fixtures/Frontmatter/Comment//invalid@0.0.py") |> Frontmatter.tryExtractFromString PythonFrontmatter

                Assert.True actual.IsSome
                Assert.Equal (Frontmatter.Python.Comment.invalidMissingMandatoryFrontmatterExtracted, actual.Value)

    module Binding =

        module InMemory =

            [<Fact>]
            let ``valid single frontmatter can be extracted`` () =
                Assert.Equal(
                    expected = Frontmatter.Python.Binding.validMandatoryFrontmatterExtracted,
                    actual = (Frontmatter.extractFromString PythonFrontmatter Frontmatter.Python.Binding.validMandatoryFrontmatter)
                )

            [<Fact>]
            let ``valid frontmatter capture guides lead to results`` () =
                Assert.All(
                    [Frontmatter.Python.Binding.validMandatoryFrontmatter; Frontmatter.Python.Binding.validFullFrontmatter; Frontmatter.Python.Binding.invalidMissingMandatoryFrontmatter],
                    (fun fm ->
                        Assert.True ((Frontmatter.tryExtractFromString PythonFrontmatter fm).IsSome)
                    )
                )

            [<Fact>]
            let ``valid frontmatter capture guides lead to correctly extracted substrings`` () =
                Assert.All(
                    [
                        Frontmatter.Python.Binding.validMandatoryFrontmatter, Frontmatter.Python.Binding.validMandatoryFrontmatterExtracted 
                        Frontmatter.Python.Binding.validFullFrontmatter, Frontmatter.Python.Binding.validFullFrontmatterExtracted
                        Frontmatter.Python.Binding.invalidMissingMandatoryFrontmatter, Frontmatter.Python.Binding.invalidMissingMandatoryFrontmatterExtracted
                    ],
                    (fun (fm, expected) ->
                        let actual = Frontmatter.extractFromString PythonFrontmatter fm
                        Assert.Equal(expected, actual)
                    )
                )

            [<Fact>]
            let ``invalid frontmatter capture substrings are leading to None`` () =
                Assert.All(
                    [Frontmatter.Python.Binding.invalidEndFrontmatter; Frontmatter.Python.Binding.invalidStartFrontmatter],
                    (fun fm ->
                        Assert.True ((Frontmatter.tryExtractFromString PythonFrontmatter fm).IsNone)
                    )
                )

        module IO =

            open System.IO

            [<Fact>]
            let ``valid frontmatter substring is extracted from valid mandatory field test file`` () =

                let actual = File.ReadAllText("fixtures/Frontmatter/Binding/valid@1.0.0.py") |> Frontmatter.tryExtractFromString PythonFrontmatter

                Assert.True actual.IsSome
                Assert.Equal (Frontmatter.Python.Binding.validMandatoryFrontmatterExtracted, actual.Value)

            [<Fact>]
            let ``valid frontmatter substring is correctly from all fields test file`` () =

                let actual = File.ReadAllText("fixtures/Frontmatter/Binding/valid@2.0.0.py") |> Frontmatter.tryExtractFromString PythonFrontmatter

                Assert.True actual.IsSome
                Assert.Equal (Frontmatter.Python.Binding.validFullFrontmatterExtracted, actual.Value)

            [<Fact>]
            let ``frontmatter substring is extracted although metadata is missing fields`` () =

                let actual = File.ReadAllText("fixtures/Frontmatter/Binding/invalid@0.0.py") |> Frontmatter.tryExtractFromString PythonFrontmatter

                Assert.True actual.IsSome
                Assert.Equal (Frontmatter.Python.Binding.invalidMissingMandatoryFrontmatterExtracted, actual.Value)
