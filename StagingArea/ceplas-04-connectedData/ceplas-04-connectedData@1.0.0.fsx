let [<Literal>]PACKAGE_METADATA = """(*
---
Name: ceplas-04-connectedData
Summary: Validates whether data and metadata in the ARC are properly connected
Description: |
    ## Critical quality criteria
    - Every annotation table contains an Input
    - Every annotation table contains an Output
    - Every annotation table contains a Protocol reference
    - ARC annotation tables are connected
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
ReleaseNotes: |
    - Release leveled validation packages.
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
open System.Text

//////////////////////////// 
// TODO to be added to ARCtrl https://github.com/nfdi4plants/ARCtrl/pull/633
type ArcTable with
    member this.TryGetProtocolUriColumn() =
        this.TryGetColumnByHeader(CompositeHeader.ProtocolUri)
////////////////////////////

// Input:

let arcDir = Directory.GetCurrentDirectory()

////////////////////////

let arc =
    try ARC.load arcDir with
    | _ -> ARC(identifier = "placeholder")

arc.MakeDataFilesAbsolute()
arc.DataContextMapping()

// Collection of all tables in the ARC together with a set of their I/O nodes
// This is used to check whether each table contains at least one overlapping I/O with any other table
// Allows duplicate table names (i.e. between multiple studies / assays)

let tableNodes = 

    let trimDataFileName (n: string) = n.TrimStart(Array.ofSeq "./")

    let tableNodeGetter (collectionID : string ) (tables : ArcTables) = 
        tables
        |> Seq.map (fun t ->
            $"{t.Name} in {collectionID}", 
            set [
                yield! t.InputNames |> List.map trimDataFileName
                yield! t.OutputNames |> List.map trimDataFileName
                ]
        )

    let assayTables = 
        arc.Assays
        |> Seq.collect (fun a -> tableNodeGetter $"Assay {a.Identifier}" a)
    let studyTables = 
        arc.Studies
        |> Seq.collect (fun s -> tableNodeGetter $"Study {s.Identifier}" s)
    let runTables = 
        arc.Runs
        |> Seq.collect (fun r -> tableNodeGetter $"Run {r.Identifier}" r)
    
    Seq.concat [
        assayTables
        studyTables
        runTables
        ]

// Values:

let criticalCases =     
    testList "criticalCases" [

    /////////////////////////////////////////////////////////////////

    for t in arc.ArcTables do

        // TestCase Critical: Every annotation table contains an Input
        
        testCase $"Table {t.Name} contains `Input`" <| fun _ ->
                
            Expect.isGreaterThanOrEqual t.InputNames.Length 1
                $"Table {t.Name} contains no Input"

        // TestCase Critical: Every annotation table contains an Output
        
        testCase $"Table {t.Name} contains `Output`" <| fun _ ->
                
                Expect.isGreaterThanOrEqual t.OutputNames.Length 1
                    $"Table {t.Name} contains no Output"

        // TestCase Critical: Every annotation table contains a Protocol reference

        testCase $"Table {t.Name} contains `Protocol`" <| fun _ ->
            Expect.isTrue
                (t.TryGetProtocolUriColumn().IsSome || t.TryGetProtocolNameColumn().IsSome)
                $"Table {t.Name} contains no 'Protocol Uri' nor 'Protocol REF' column"
    
    /////////////////////////////////////////////////////////////////

    // TestCase Critical: ARC annotation tables are connected
    for name, nodes in tableNodes do    
        
        testCase $"ARC annotation table ({name}) is connected"  <| fun _ ->

            let tableConnection = tableNodes |> Seq.exists (fun (n, nds) ->
                    if n <> name then
                        Set.intersect nodes nds
                        |> Seq.length
                        |> (<>) 0
                    else
                        false
                )
            
            Expect.isTrue tableConnection
                $"Annotation table {name} is not connected to any other annotation table"
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


