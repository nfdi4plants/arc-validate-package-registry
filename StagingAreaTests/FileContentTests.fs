namespace StagingDirectory

open System
open System.IO
open Xunit
open Utils

open AVPRIndex

module FileContent =

    [<Fact>]
    let ``All files have frontmatter`` () =
        Assert.All(
            ReferenceObjects.all_staged_packages_paths,
            File.ReadAllText >> (fun x -> x.ReplaceLineEndings("\n")) >> Assert.ContainsFrontmatter
        )

    [<Fact>]
    let ``All files have valid metadata`` () =
        Assert.All(
            ReferenceObjects.all_staged_packages_paths,
            (fun p -> 
                p
                |> ValidationPackageMetadata.extractFromScript
                |> Assert.MetadataValid
            )
        )