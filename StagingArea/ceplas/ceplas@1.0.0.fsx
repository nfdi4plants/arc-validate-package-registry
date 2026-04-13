let [<Literal>]PACKAGE_METADATA = """(*
---
Name: ceplas-experimental
Summary: Validates if the ARC contains the necessary metadata to meet the CEPLAS quality criteria.
Description: ""
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: true
Authors:
  - FullName: Dominik Brilhaus
    Email: brilhaus@hhu.de
    Affiliation: HHU Düsseldorf
    AffiliationLink: http://ceplas.eu
  - FullName: Heinrich Lukas Weil
    Email: weil@nfdi4plants.org
    Affiliation: RPTU Kaiserslautern
    AffiliationLink: http://rptu.de/startseite
Tags:
  - Name: ceplas
  - Name: quality-arc
ReleaseNotes: |
  - initial release
---
*)"""

// #r "nuget: ARCExpect, 2.0.0"
#r "nuget: ARCtrl.QueryModel, 3.0.0-alpha.2"
#r "nuget: Expecto"

// open ControlledVocabulary
open ARCtrl
open ARCtrl.QueryModel
open Expecto
// open ARCExpect
// open ARCTokenization
// open ARCTokenization.StructuralOntology
open System.IO
open System.Text
// open FSharpAux

// Input:
let arcDir = Directory.GetCurrentDirectory()

let arc = ARC.load arcDir

// Values:

let criticalCases =     
    testList "" [
    
    ////// ARC root

    // ARC contains README

    testCase "ARC contains README" <| fun _ ->
        
        let readmeNames = ["README.md"; "readme.md"; "README.txt"; "readme.txt"; "Readme.md"; "readme"; "README"]

        let containsReadme = 

            readmeNames
            |> List.exists (fun n -> 
                let p = Path.Combine(arcDir, n)
                File.Exists p        
            )

        if not containsReadme then
            failwithf "ARC does not contain README in any of the given paths: %O" readmeNames

    // ARC contains any LICENSE

    testCase "ARC contains LICENSE" <| fun _ ->
        
        let licenseNames = ["LICENSE.md"; "license.md"; "LICENSE.txt"; "license.txt"; "License.md"; "license"; "LICENSE";
                            "LICENCE.md"; "licence.md"; "LICENCE.txt"; "licence.txt"; "Licence.md"; "licence"; "LICENCE"]

        let containsLicense = 

            licenseNames
            |> List.exists (fun n -> 
                let p = Path.Combine(arcDir, n)
                File.Exists p        
            )

        if not containsLicense then
            failwithf "ARC does not contain LICENSE in any of the given paths: %O" licenseNames


    ////// ARC Investigation

    // ARC investigation contains identifier
    // ARC investigation contains title
    // ARC investigation contains description
    // ARC investigation contains contact

    // ARC contains at least one study or one assay

    testCase "ARC contains at least one study or one assay" <| fun _ ->

        if arc.StudyCount + arc.AssayCount = 0 then
            failwith "ARC does not contain any study or assay"

    //// every study and every assay must contain at least one annotation table

    for s in arc.Studies do
        // ARC study contains annotation table
        testCase $"Study {s.Identifier} contains annotation table" <| fun _ ->
            // study should contain annotation table
            if s.TableCount = 0 then
                failwith $"Study {s.Identifier} contains no annotation table"
        
        for t in s.Tables do
            testCase $"Table {t.Name} of study {s.Identifier} contains basic information" <| fun _ ->
                
                if t.ColumnCount < 2 then
                    failwith $"Table {t.Name} contains less than 2 columns"
                if t.RowCount = 0 then
                    failwith $"Table {t.Name} contains no rows"
        
    for a in arc.Studies do
        // ARC assay contains annotation table
        testCase $"Assay {a.Identifier} contains annotation table" <| fun _ ->
            // assay should contain annotation table
            if a.TableCount = 0 then
                failwith $"Assay {a.Identifier} contains no annotation table"

        for t in a.Tables do
            testCase $"Table {t.Name} of assay {a.Identifier} contains basic information" <| fun _ ->
                
                if t.ColumnCount < 2 then
                    failwith $"Table {t.Name} contains less than 2 columns"
                if t.RowCount = 0 then
                    failwith $"Table {t.Name} contains no rows"


    //// every study and assay must contain top-level metadata

    for s in arc.Studies do
        // ARC study contains title
        testCase $"Study {s.Identifier} contains title" <| fun _ ->
            // study title should exist
            if s.Title.IsNone then
                failwith $"Study {s.Identifier} contains no title"
            // study title should be longer than 4 characters
            if s.Title.Value.Length < 4 then
                failwith $"Study {s.Identifier} contains no meaningful title (i.e. longer than 3 characters):\"{s.Title.Value}\""

        // ARC study contains title


        // ARC assay contains identifier
        // ARC assay contains title   


    for a in arc.Assays do
        // ARC assay measurement type
        testCase $"Assay {a.Identifier} contains top-level metadata measurement type" <| fun _ ->
            if a.MeasurementType.IsNone then
                failwith $"Assay {a.Identifier} contains no top-level metadata measurement type"


        // ARC assay technology type
        // ARC assay technology  platform
    
    //// assay must annotate data entities
        // data entity should resolve
            // 1. annotation resolves local file
            // 2. if not local (./dataset), resolves URL
        // data entity should be annotated with at least one of Characteristic, Parameter, Factor

    ]
       

let nonCriticalCases =
    testList "" [

    // process graph: I/O connections (sample-sample-material-data)
    // ARC should not have non-connected annotation tables
    
    // every data entity should be derived from a Source or Sample 




    // ARC assay contains description
    // ARC assay contains contact

    // should contain ProtocolUri


    ]


// Execution:
Setup.ValidationPackage(
    metadata = Setup.Metadata(PACKAGE_METADATA),
    CriticalValidationCases = [cases],
    NonCriticalValidationCases = [nonCriticalCases]
)
|> Execute.ValidationPipeline(
    basePath = arcDir
)