let [<Literal>]PACKAGE_METADATA = """(*
---
Name: ceplas-03-biodata
Summary: Validates the ARC's "biological" data (e.g. measured, raw or processed, datasets)
Description: |
    ## Critical quality criteria
    - ARC contains 'raw' data (e.g. raw dataset file or URL)
    - ARC assay dataset file exists
    // - ARC run data file exists
    - Every data entity derives from a Source or Sample
    - Every data entity is annotated with at least one of Characteristic, Parameter, Factor
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

type UrlResolution =
    | Resolves of statusCode: int
    | HttpError of statusCode: int
    | Malformed of error: string
    | Unreachable of error: string

let urlResolves (url: string) =
    async {
        match System.Uri.TryCreate(url, System.UriKind.Absolute) with
        | false, _ ->
            return Malformed "Invalid URL"

        | true, uri when uri.Scheme <> "http" && uri.Scheme <> "https" ->
            return Malformed $"Unsupported URL scheme: {uri.Scheme}"

        | true, _ ->
            try
                let! statusCode, _responseText = Http.get url

                if statusCode >= 200 && statusCode < 300 then
                    return Resolves statusCode
                else
                    return HttpError statusCode

            with ex ->
                return Unreachable ex.Message
    }


// Input:

let arcDir = Directory.GetCurrentDirectory()

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
            
            Expect.isGreaterThan arc.ArcTables.Data.Count 0
                "ARC contains no data entities"

        // data entity should resolve
            // 1. annotation resolves local file
            // 2. if not local (./dataset), resolves URL

        // TestCase Critical: ARC assay dataset file exists

        for a in arc.Assays do
            for d in a.Data |> Seq.distinctBy (fun d -> d.Name) do

                let filePath = if d.FilePath = "" then d.Name else d.FilePath

                testCaseAsync $"Data path {filePath} of assay {a.Identifier} resolves to local file or folder or a URL" <| async {
                    if pathIsUrl filePath then
                        let! result = urlResolves filePath

                        match result with
                        | Resolves _ ->
                            ()

                        | HttpError statusCode ->
                            Expect.isLessThan statusCode 300
                                $"Url {filePath} in assay {a.Identifier} returned an unsuccessful HTTP status"

                        | Malformed error ->
                            Expect.isTrue false
                                $"Url {filePath} in assay {a.Identifier} is malformed: {error}"

                        | Unreachable error ->
                            Expect.isTrue false
                                $"Url {filePath} in assay {a.Identifier} could not be reached: {error}"

                    else
                        let p = d.DataContext.Value.GetAbsolutePathForAssay(a.Identifier)
                        let fullPath = Path.Combine(arcDir, p)

                        Expect.isTrue (File.Exists fullPath || Directory.Exists fullPath)
                            $"Data path {filePath} does not resolve to existing local file or folder and was not identified as URL"
                }

        // TestCase Critical: ARC run data file exists
        // TODO: currently not fully possible, since `GetAbsolutePathForRun` does not exist https://github.com/nfdi4plants/ARCtrl/issues/629


        for d in arc.ArcTables.Data do

        // TestCase Critical: Every data entity derives from a Source or Sample

            testCase $"Data entity {d.Name} derives from a Source or Sample"  <| fun _ ->

                let firstSamplesContainBlank =  d.FirstSamples |> List.exists (fun q -> q.Name = "")
                
                Expect.isFalse ((d.FirstSamples.IsEmpty || firstSamplesContainBlank) && d.Sources.Count = 0)
                    $"Data entity {d.Name} does not derive from a Source or Sample"
        
        // TestCase Critical: Every data entity is annotated with at least one of Characteristic, Parameter, Factor
            
            testCase $"Data entity {d.Name} contains at least one of Characteristic, Parameter, Factor"  <| fun _ ->
                Expect.isNonEmpty d.PreviousValues
                    $"Data entity {d.Name} is not associated with any annotation value"

    ]


// let nonCriticalCases =
//     testList "nonCriticalCases" [



//     ]

// Execution:
Setup.ValidationPackage(
    metadata = Setup.Metadata(
        PACKAGE_METADATA,
        AVPRIndex.Frontmatter.FrontmatterLanguage.FSharpFrontmatter
        ),
    CriticalValidationCases = [criticalCases],
    NonCriticalValidationCases = []
)
|> Execute.ValidationPipeline(
    basePath = arcDir
)