namespace PackageSanityChecks

open System
open System.IO
open Xunit
open Utils


module Metadata =
    
    [<Fact>]
    let ``Metadata versions match file names`` () =
        Assert.All(
            ReferenceObjects.all_staged_packages_metadata,
            (fun (path, metadata) ->
                let file_name_version = (Path.GetFileNameWithoutExtension path).Split('@')[1]
                let metadata_version = sprintf "%i.%i.%i" metadata.MajorVersion metadata.MinorVersion metadata.PatchVersion
                Assert.Equal(file_name_version, metadata_version)
            )
        )


module ValidationScripts =
    
    [<Fact>]
    let ``All packages compile`` () =
        Assert.All(
            ReferenceObjects.all_staged_packages_paths,
            Assert.ScriptCompiles
        )