namespace PackageSanityChecks

open System
open System.IO
open Xunit
open Utils

open AVPRIndex

module Metadata =
    
    [<Fact>]
    let ``fsharp Metadata versions match file names`` () =
        Assert.All(
            ReferenceObjects.all_staged_fsharp_packages_paths,
            (fun path ->
                let file_name_version = (Path.GetFileNameWithoutExtension path).Split('@')[1]
                let metadata_version = path |> ValidationPackageMetadata.extractFromScript |> ValidationPackageMetadata.getSemanticVersionString
                Assert.Equal(file_name_version, metadata_version)
            )
        )
    
    [<Fact>]
    let ``python Metadata versions match file names`` () =
        Assert.All(
            ReferenceObjects.all_staged_python_packages_paths,
            (fun path ->
                let file_name_version = (Path.GetFileNameWithoutExtension path).Split('@')[1]
                let metadata_version = path |> ValidationPackageMetadata.extractFromScript |> ValidationPackageMetadata.getSemanticVersionString
                Assert.Equal(file_name_version, metadata_version)
            )
        )

module ValidationScripts =
    
    [<Fact>]
    let ``single fsharp package runs`` () = Assert.FSharpScriptRuns [||] "fixtures/Frontmatter/Binding/valid@2.0.0.fsx"

    [<Fact>]
    let ``All fsharp packages run`` () =
        Assert.All(
            ReferenceObjects.all_staged_fsharp_packages_paths,
            Assert.FSharpScriptRuns [||]
        )
    
    [<Fact>]
    let ``single python package runs`` () = Assert.PythonScriptRuns [||] "fixtures/Frontmatter/Binding/valid@2.0.0.py"

    [<Fact>]
    let ``All python packages run`` () =
        Assert.All(
            ReferenceObjects.all_staged_python_packages_paths,
            Assert.PythonScriptRuns [||]
        )