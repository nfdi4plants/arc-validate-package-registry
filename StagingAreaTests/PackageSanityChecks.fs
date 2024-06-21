namespace PackageSanityChecks

open System
open System.IO
open Xunit
open Utils

open AVPRIndex

module Metadata =
    
    [<Fact>]
    let ``Metadata versions match file names`` () =
        Assert.All(
            ReferenceObjects.all_staged_packages_paths,
            (fun path ->
                let file_name_version = (Path.GetFileNameWithoutExtension path).Split('@')[1]
                let metadata_version = path |> ValidationPackageMetadata.extractFromScript |> ValidationPackageMetadata.getSemanticVersionString
                Assert.Equal(file_name_version, metadata_version)
            )
        )


module ValidationScripts =
    
    [<Fact>]
    let ``All packages run`` () =
        Assert.All(
            ReferenceObjects.all_staged_packages_paths,
            Assert.ScriptRuns [||]
        )