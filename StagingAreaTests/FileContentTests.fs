namespace StagingDirectory

open System
open System.IO
open Xunit
open Utils

module FileContent =

    [<Fact>]
    let ``All files have frontmatter`` () =
        Assert.All(
            ReferenceObjects.all_staged_packages_contents,
            snd >> Assert.ContainsFrontmatter
        )

    [<Fact>]
    let ``All files have valid metadata`` () =
        Assert.All(
            ReferenceObjects.all_staged_packages_metadata,
            snd >> Assert.MetadataValid
        )