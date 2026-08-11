let [<Literal>]PACKAGE_METADATA = """(*
---
Name: ceplas-03-biodata
Summary: Validates the ARC's "biological" data (e.g. measured, raw or processed, datasets)
Description: |
    ## Critical quality criteria
    - ARC contains 'raw' data (e.g. raw dataset file or URL)
    - ARC assay dataset file exists
    // - ARC run data file exists
    - Every data entity should be derived from a Source or Sample
    - Every data entity should be annotated with at least one of Characteristic, Parameter, Factor
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: true
Authors:
  - FullName: Dominik Brilhaus
    Email: brilhaus@hhu.de
    Affiliation: CEPLAS
    AffiliationLink: https://ceplas.eu
  - FullName: Heinrich Lukas Weil
    Email: weil@nfdi4plants.org
    Affiliation: RPTU Kaiserslautern
    AffiliationLink: http://rptu.de/startseite
Tags:
  - Name: ceplas
  - Name: quality-arc
  - Name: study
  - Name: assay
  - Name: "raw data"
ReleaseNotes: |
    Release leveled validation packages.
---
*)"""

#r "nuget: ARCExpect.Core, 7.0.0-alpha"
#r "nuget: ARCtrl.QueryModel, 3.0.0-alpha.4"
#r "nuget: Fable.SimpleHttp"

open ARCtrl
open ARCtrl.QueryModel
open Expecto
open ARCExpect
open System.IO
open Fable.SimpleHttp


let pathIsUrl (p: string) =
    p.StartsWith("http:") || p.StartsWith("https:")

type URLStatus =
    | Malformed
    | Resolves
    | Fails

let urlResolves (url: string) =

    async {
        try
            let! (statusCode, responseText) = Http.get url

            match statusCode with
            | 200 -> return Resolves
            | _ -> return Fails

        with
            | _ -> return Malformed
    }
    |> Async.RunSynchronously


// Input:

// let arcDir = Directory.GetCurrentDirectory()

//// TODO: remove ////////////////////
// Local Test
let arcDir = fsi.CommandLineArgs.[1]

////////////////////////

let arc =
    try ARC.load arcDir with
    | _ -> ARC(identifier = "placeholder")

arc.MakeDataFilesAbsolute()
arc.DataContextMapping()

// Validations

let criticalCases =     
    testList "criticalCases" [

        // TestCase Critical: ARC contains 'raw' data (e.g. raw dataset file or URL)
        // This includes any I/ONode of type Data (i.e. in study, assay or run)

        testCase "ARC contains data entities" <| fun _ ->
            if arc.ArcTables.Data.Count = 0 then
                failwith "ARC contains no data entities"

        // data entity should resolve
            // 1. annotation resolves local file
            // 2. if not local (./dataset), resolves URL

        // TestCase Critical: ARC assay dataset file exists

        for a in arc.Assays do
            for d in a.Data |> Seq.distinctBy (fun d -> d.Name) do

                let filePath = if d.FilePath = "" then d.Name else d.FilePath 

                testCase $"Data path {filePath} of assay {a.Identifier} resolves to local file or folder or a URL" <| fun _ ->

                    // Check whether path (i.e. Output [Data]) resolves to URL

                    if pathIsUrl filePath then
                        match urlResolves filePath with
                        | Resolves -> ()
                        | Fails -> failwith $"Url {filePath} in assay {a.Identifier} could not be resolved"
                        | Malformed -> failwith $"Url {filePath} in assay {a.Identifier} is malformed"

                    else

                    // Check whether path (i.e. Output [Data]) resolves to local file / folder

                        let p = d.DataContext.Value.GetAbsolutePathForAssay(a.Identifier)
                        let fullPath = Path.Combine(arcDir, p)

                        if (File.Exists fullPath || Directory.Exists fullPath) |> not then
                                failwith $"Data path {filePath} does not resolve to existing local file or folder and was not identified as URL"
        

        // TestCase Critical: ARC run data file exists
        // TODO: currently not fully possible, since `GetAbsolutePathForRun` does not exist https://github.com/nfdi4plants/ARCtrl/issues/629

        // for r in arc.Runs do
        //     for d in r.Data |> Seq.distinctBy (fun d -> d.Name) do

        //         let filePath = if d.FilePath = "" then d.Name else d.FilePath 

        //         testCase $"Data path {filePath} of run {r.Identifier} resolves to local file or folder or a URL" <| fun _ ->

        //             // Check whether path (i.e. Output [Data]) resolves to URL

        //             if pathIsUrl filePath then
        //                 match urlResolves filePath with
        //                 | Resolves -> ()
        //                 | Fails -> failwith $"Url {filePath} in run {r.Identifier} could not be resolved"
        //                 | Malformed -> failwith $"Url {filePath} in run {r.Identifier} is malformed"

        //             else

        //             // Check whether path (i.e. Output [Data]) resolves to local file / folder

        //                 let p = d.DataContext.Value.GetAbsolutePathForRun(r.Identifier)
        //                 let fullPath = Path.Combine(arcDir, p)

        //                 if (File.Exists fullPath || Directory.Exists fullPath) |> not then
        //                         failwith $"Data path {filePath} does not resolve to existing local file or folder and was not identified as URL"

        for d in arc.ArcTables.Data do

        // TestCase Critical: Every data entity should be derived from a Source or Sample

            testCase $"Data entity {d.Name} derives from a Source or Sample"  <| fun _ ->

                let firstSamplesContainBlank =  d.FirstSamples |> List.exists (fun q -> q.Name = "")
                
                if (d.FirstSamples.IsEmpty || firstSamplesContainBlank) && d.Sources.Count = 0 then
                    failwith $"Data entity {d.Name} does not derive from a Source or Sample"
        
        // TestCase Critical: Every data entity should be annotated with at least one of Characteristic, Parameter, Factor
            
            testCase $"Data entity {d.Name} contains at least one of Characteristic, Parameter, Factor"  <| fun _ ->
                if d.PreviousValues.IsEmpty then
                    failwith $"Data entity {d.Name} is not associated with any annotation value"

    ]


let nonCriticalCases =
    testList "nonCriticalCases" [



    ]

// Execution:
Setup.ValidationPackage(
    metadata = Setup.Metadata(
        PACKAGE_METADATA,
        AVPRIndex.Frontmatter.FrontmatterLanguage.FSharpFrontmatter
        ),
    CriticalValidationCases = [criticalCases],
    NonCriticalValidationCases = [nonCriticalCases]
)
|> Execute.ValidationPipeline(
    basePath = arcDir
)