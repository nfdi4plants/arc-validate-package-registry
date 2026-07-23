let [<Literal>]PACKAGE_METADATA = """(*
---
Name: agdafair
Description: Validates if the ARC contains image data that can be imported into the AgriGaia platform for training of AI models.
Summary: |
  Validates if the ARC contains image data that is produced by a process (meaning it is annotated via data provenance)
MajorVersion: 0
MinorVersion: 0
PatchVersion: 1
Publish: true
Authors:
  - FullName: Kevin Schneider
    Affiliation: DataPLANT
Tags:
  - Name: AgriGaia
  - Name: AgDaFAIR
  - Name: AI
ReleaseNotes: |
  - first release for simple jpg, jpeg, png, tif, tiff image data validation
---
*)"""

#r "nuget: ARCtrl"
#r "nuget: ARCtrl.QueryModel, 3.0.0-alpha.4"
#r "nuget: ARCExpect.Core, 7.0.0-alpha"

open ARCtrl
open ARCtrl.QueryModel
open ARCExpect
open Expecto
open System.IO

module CLIArgs =

    type Args = {
        // -i and -o will always get passed if the package is called via arc-validate
        InputPath : string
        OutputPath : string
        // package-specific cli args
        AgriGaiaHookEndpoint: string
        AgriGaiaUserName: string
    }

    let defaultArgs = {
        InputPath = Directory.GetCurrentDirectory()
        OutputPath = Directory.GetCurrentDirectory()
        AgriGaiaHookEndpoint = "https://api.agdafair.agri-gaia.edvsz.hs-osnabrueck.de/agdafair/import"
        AgriGaiaUserName = "towamhof"
    }

    let rec parseArgs args state =
        match args with
        | "-h" :: value :: rest ->
            parseArgs rest { state with AgriGaiaHookEndpoint = value }

        | "-u" :: value :: rest ->
            parseArgs rest { state with AgriGaiaUserName = value }
    
        | "-i" :: value :: rest ->
            parseArgs rest { state with InputPath = value }

        | "-o" :: value :: rest ->
            parseArgs rest { state with OutputPath = value }

        | unknown :: _ ->
            failwith $"Unknown argument: {unknown}. Valid arguments are: -h <AgriGaiaHookEndpoint>, -u <AgriGaiaUserName>, -i <InputPath>, -o <OutputPath>"

        | [] ->
            state

let args =
    fsi.CommandLineArgs
    |> Array.skip 1
    |> Array.toList
    |> fun args -> CLIArgs.parseArgs args CLIArgs.defaultArgs

let arcDir = args.InputPath

let arc =
    try ARC.load arcDir with
    | _ -> ARC(identifier = "unable to load arc from this dir")

let is_image_data (n: QNode) =

    let is_image_file (filePath: string) =
        let ext = Path.GetExtension(filePath).ToLower()
        ext = ".jpg" || ext = ".jpeg" || ext = ".png" || ext = ".tif" || ext = ".tiff"

    n.isData && is_image_file(n.FilePath)
   

let image_nodes =
    // s.Data seems to fail if none are there?
    let study_nodes =
        arc.Studies
        |> Array.ofSeq
        |> Array.collect (fun s -> try s.Data |> Array.ofSeq with | _ -> [||])

    let assay_nodes =
        arc.Assays
        |> Array.ofSeq
        |> Array.collect (fun a -> try a.Data |> Array.ofSeq with | _ -> [||])

    Array.append study_nodes assay_nodes
    |> Array.filter is_image_data

let critical_validation_cases =
    testList "AgDaFAIR" [
        test "ARC contains image data" {Expect.isGreaterThan image_nodes.Length 0 "The ARC does not contain any image data with provenance annotations."}
        test "ARC has Description" {Expect.isSome arc.Description "The ARC does not have a Description."}
        test "ARC has Title" {Expect.isSome arc.Title "The ARC does not have a Title."}
    ]

// configure cqc hook via this workaround
// this package will therefore not have hook endpoint set via metadata
let package_metadata = Setup.Metadata(PACKAGE_METADATA, AVPRIndex.Frontmatter.FSharpFrontmatter)
package_metadata.CQCHookEndpoint <- args.AgriGaiaHookEndpoint

open System.Collections.Generic

let payload = 
    Dictionary<string,obj>([
        // must use default values here because validation will be performed on runtime after the initialization of this object
        KeyValuePair("Name", box (arc.Title |> Option.defaultValue "")) ;
        KeyValuePair("Description", box (arc.Description |> Option.defaultValue "")) ;
        KeyValuePair("DatasetType", box "AgriImageDataResource") ;
        KeyValuePair("UserName", box args.AgriGaiaUserName)
    ])

Setup.ValidationPackage(
    metadata = package_metadata,
    CriticalValidationCases = [ critical_validation_cases ]
)
|> Execute.ValidationPipeline(
    basePath = arcDir,
    BadgeLabelText = $"{image_nodes.Length} images ready for AgriGaia",
    Payload = payload
)