namespace StagingDirectory

open System
open System.IO
open Xunit
open Utils

open AVPRIndex

module FileContent =

    [<Fact>]
    let ``All fsharp files have frontmatter`` () =
        Assert.All(
            ReferenceObjects.all_staged_fsharp_packages_paths,
            File.ReadAllText >> (fun x -> x.ReplaceLineEndings("\n")) >> Assert.ContainsFSharpFrontmatter
        )

    [<Fact>]
    let ``All fsharp files have valid metadata`` () =
        Assert.All(
            ReferenceObjects.all_staged_fsharp_packages_paths,
            (fun p -> 
                p
                |> ValidationPackageMetadata.extractFromScript
                |> Assert.MetadataValid
            )
        )


    [<Fact>]
    let ``All python files have frontmatter`` () =
        Assert.All(
            ReferenceObjects.all_staged_python_packages_paths,
            File.ReadAllText >> (fun x -> x.ReplaceLineEndings("\n")) >> Assert.ContainsPythonFrontmatter
        )

    [<Fact>]
    let ``All python files have valid metadata`` () =
        Assert.All(
            ReferenceObjects.all_staged_python_packages_paths,
            (fun p -> 
                p
                |> ValidationPackageMetadata.extractFromScript
                |> Assert.MetadataValid
            )
        )