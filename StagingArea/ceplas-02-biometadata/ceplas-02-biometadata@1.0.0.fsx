let [<Literal>]PACKAGE_METADATA = """(*
---
Name: ceplas-02-biometadata
Summary: Validates the ARC's "biological" metadata.
Description: |
        ## Critical quality criteria
        - ARC contains at least one study or assay or workflow or run
        - ARC contains any annotation column (Characteristic, Parameter, Factor)
        - Every study contains at least one annotation table
        - Every study annotation table contains basic information
        - Every assay contains at least one annotation table
        - Every assay annotation table contains basic information
        - Every run contains at least one annotation table
        - Every run annotation table contains basic information

        ## Non-critical quality criteria
        - Every study contains a title
        - Every study contains a description
        - Every study contains contacts
        - Every assay contains a title
        - Every assay contains a description
        - Every assay contains performers
        - Every assay contains a measurement type
        - Every assay contains a technology type
        - Every assay contains a technology platform
        - Every workflow contains a title
        - Every workflow contains a description
        - Every workflow contains contacts
        - Every run contains a title
        - Every run contains a description
        - Every run contains performers
        - Every run contains a measurement type
        - Every run contains a technology type
        - Every run contains a technology platform
        - Every annotation table contains some annotation column

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
  - Name: study
  - Name: assay
  - Name: quality-arc
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



let hasAnnotationColumns (t: ARC)=
    t.ArcTables
    |> Seq.exists (fun t ->
        t.Columns
        |> Seq.exists (fun c ->
            c.Header.isCharacteristic || 
            c.Header.isParameter|| 
            c.Header.isFactor
        ))

let characteristicCount (t : ArcTable)=
    t.Columns
    |> Seq.filter (fun c -> c.Header.isCharacteristic)
    |> Seq.length

let parameterCount (t : ArcTable)=
    t.Columns
    |> Seq.filter (fun c -> c.Header.isParameter)
    |> Seq.length
    
let factorCount (t : ArcTable)=
    t.Columns
    |> Seq.filter (fun c -> c.Header.isFactor)
    |> Seq.length



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

    ////////////////////////////////////
    ////// ARC Study + Assay
    ////////////////////////////////////    
    
    // TestCase Critical: ARC contains at least one study or assay or workflow or run

    testCase "ARC contains at least one study or assay or workflow or run" <| fun _ ->

        if arc.StudyCount + arc.AssayCount + arc.WorkflowCount + arc.RunCount = 0 then
            failwith "ARC does not contain any study or assay or workflow or run"

    // TestCase Critical: ARC contains any annotation column (Characteristic, Parameter, Factor)

    testCase "ARC contains any annotation column (Characteristic, Parameter, Factor)" <| fun _ ->

        if not (hasAnnotationColumns arc) then
            failwith "ARC contains no annotation column (Characteristic, Parameter, Factor)"

    for s in arc.Studies do
        
        // TestCase Critical: Every study contains at least one annotation table
        testCase $"Study {s.Identifier} contains annotation table" <| fun _ ->
            if s.TableCount = 0 then
                failwith $"Study {s.Identifier} contains no annotation table"
        
        // TestCase Critical: Every study annotation table contains basic information
        // (more than 2 columns and 0 rows)
        
        for t in s.Tables do
            testCase $"Table {t.Name} of study {s.Identifier} contains basic information" <| fun _ ->
                
                if t.ColumnCount < 2 then
                    failwith $"Table {t.Name} contains less than 2 columns"
                if t.RowCount = 0 then
                    failwith $"Table {t.Name} contains no rows"

    for a in arc.Assays do
        
        // TestCase Critical: Every assay contains at least one annotation table
        testCase $"Assay {a.Identifier} contains annotation table" <| fun _ ->
            if a.TableCount = 0 then
                failwith $"Assay {a.Identifier} contains no annotation table"
        
        // TestCase Critical: Every assay annotation table contains basic information
        // (more than 2 columns and 0 rows)
        
        for t in a.Tables do
            testCase $"Table {t.Name} of assay {a.Identifier} contains basic information" <| fun _ ->
                
                if t.ColumnCount < 2 then
                    failwith $"Table {t.Name} contains less than 2 columns"
                if t.RowCount = 0 then
                    failwith $"Table {t.Name} contains no rows"
                    
    for r in arc.Runs do
        
        // TestCase Critical: Every run contains at least one annotation table
        testCase $"Run {r.Identifier} contains annotation table" <| fun _ ->
            if r.TableCount = 0 then
                failwith $"Run {r.Identifier} contains no annotation table"
        
        // TestCase Critical: Every run annotation table contains basic information
        // (more than 2 columns and 0 rows)
        
        for t in r.Tables do
            testCase $"Table {t.Name} of run {r.Identifier} contains basic information" <| fun _ ->
                
                if t.ColumnCount < 2 then
                    failwith $"Table {t.Name} contains less than 2 columns"
                if t.RowCount = 0 then
                    failwith $"Table {t.Name} contains no rows"


    ]
    

let nonCriticalCases =
    testList "nonCriticalCases" [

    /////////////////////////////////////////////////////////////////
    ////// ARC Study top level metadata
    /////////////////////////////////////////////////////////////////
        
    for s in arc.Studies do
        
        // TestCase Non-critical: Every study contains a title
        testCase $"Study {s.Identifier} contains title" <| fun _ ->
            // Study title exists
            if s.Title.IsNone then
                failwith $"Study {s.Identifier} contains no title"
            // Study title is longer than 3 characters
            if s.Title.Value.Length < 4 then
                failwith $"Study {s.Identifier} contains no meaningful title (i.e. longer than 3 characters):\"{s.Title.Value}\""
        
        // TestCase Non-critical: Every study contains a description
        testCase $"Study {s.Identifier} contains description" <| fun _ ->
            // Study description exists
            if s.Description.IsNone then
                failwith $"Study {s.Identifier} contains no description"
            // Study description is longer than 30 characters
            if s.Description.Value.Length < 31 then
                failwith $"Study {s.Identifier} contains no meaningful description (i.e. longer than 30 characters):\"{s.Description.Value}\""

        // TestCase Non-critical: Every study contains contacts
        testCase $"Study {s.Identifier} contains contacts" <| fun _ ->
            if s.Contacts.Count < 1 then
                failwith $"Study {s.Identifier} contains no contacts"
    
    /////////////////////////////////////////////////////////////////
    ////// ARC Assay top level metadata
    /////////////////////////////////////////////////////////////////

    for a in arc.Assays do

        // TestCase Non-critical: Every assay contains a title
        testCase $"Assay {a.Identifier} contains title" <| fun _ ->
            // Assay title exists
            if a.Title.IsNone then
                failwith $"Assay {a.Identifier} contains no title"
            // Assay title is longer than 4 characters
            if a.Title.Value.Length < 4 then
                failwith $"Assay {a.Identifier} contains no meaningful title (i.e. longer than 3 characters):\"{a.Title.Value}\""
        
        // TestCase Non-critical: Every assay contains a description
        testCase $"Assay {a.Identifier} contains description" <| fun _ ->
            // Assay description exists
            if a.Description.IsNone then
                failwith $"Assay {a.Identifier} contains no description"
            // Assay description is longer than 30 characters
            if a.Description.Value.Length < 31 then
                failwith $"Assay {a.Identifier} contains no meaningful description (i.e. longer than 30 characters):\"{a.Description.Value}\""

        // TestCase Non-critical: Every assay contains performers
        testCase $"Study {a.Identifier} contains contacts" <| fun _ ->
            if a.Performers.Count < 1 then
                failwith $"Study {a.Identifier} contains no performers"

        // TestCase Non-critical: Every assay contains a measurement type
        testCase $"Assay {a.Identifier} contains top-level metadata measurement type" <| fun _ ->
            if a.MeasurementType.IsNone then
                failwith $"Assay {a.Identifier} contains no top-level metadata measurement type"
        
        // TestCase Non-critical: Every assay contains a technology type
        testCase $"Assay {a.Identifier} contains top-level metadata technology type" <| fun _ ->
            if a.TechnologyType.IsNone then
                failwith $"Assay {a.Identifier} contains no top-level metadata technology type"
        
        // TestCase Non-critical: Every assay contains a technology platform
        testCase $"Assay {a.Identifier} contains top-level metadata technology platform" <| fun _ ->
            if a.TechnologyPlatform.IsNone then
                failwith $"Assay {a.Identifier} contains no top-level metadata technology platform"

    /////////////////////////////////////////////////////////////////
    ////// ARC Workflow top level metadata
    /////////////////////////////////////////////////////////////////

    for w in arc.Workflows do

        // TestCase Non-critical: Every workflow contains a title
        testCase $"Workflow {w.Identifier} contains title" <| fun _ ->
            // Workflow title exists
            if w.Title.IsNone then
                failwith $"Workflow {w.Identifier} contains no title"
            // Workflow title is longer than 4 characters
            if w.Title.Value.Length < 4 then
                failwith $"Workflow {w.Identifier} contains no meaningful title (i.e. longer than 3 characters):\"{w.Title.Value}\""
        
        // TestCase Non-critical: Every workflow contains a description
        testCase $"Workflow {w.Identifier} contains description" <| fun _ ->
            // Workflow description exists
            if w.Description.IsNone then
                failwith $"Workflow {w.Identifier} contains no description"
            // Workflow description is longer than 30 characters
            if w.Description.Value.Length < 31 then
                failwith $"Workflow {w.Identifier} contains no meaningful description (i.e. longer than 30 characters):\"{w.Description.Value}\""
        
        // TestCase Non-critical: Every workflow contains contacts
        testCase $"Workflow {w.Identifier} contains contacts" <| fun _ ->
            if w.Contacts.Count < 1 then
                failwith $"Workflow {w.Identifier} contains no contacts"

    /////////////////////////////////////////////////////////////////
    ////// ARC Run top level metadata
    /////////////////////////////////////////////////////////////////

    for r in arc.Runs do

        // TestCase Non-critical: Every run contains a title
        testCase $"Run {r.Identifier} contains title" <| fun _ ->
            // Run title exists
            if r.Title.IsNone then
                failwith $"Run {r.Identifier} contains no title"
            // Run title is longer than 4 characters
            if r.Title.Value.Length < 4 then
                failwith $"Run {r.Identifier} contains no meaningful title (i.e. longer than 3 characters):\"{r.Title.Value}\""
        
        // TestCase Non-critical: Every run contains a description
        testCase $"Run {r.Identifier} contains description" <| fun _ ->
            // Run description exists
            if r.Description.IsNone then
                failwith $"Run {r.Identifier} contains no description"
            // Run description is longer than 30 characters
            if r.Description.Value.Length < 31 then
                failwith $"Run {r.Identifier} contains no meaningful description (i.e. longer than 30 characters):\"{r.Description.Value}\""

        // TestCase Non-critical: Every run contains performers
        testCase $"Study {r.Identifier} contains contacts" <| fun _ ->
            if r.Performers.Count < 1 then
                failwith $"Study {r.Identifier} contains no performers"

        // TestCase Non-critical: Every run contains a measurement type
        testCase $"Run {r.Identifier} contains top-level metadata measurement type" <| fun _ ->
            if r.MeasurementType.IsNone then
                failwith $"Run {r.Identifier} contains no top-level metadata measurement type"
        
        // TestCase Non-critical: Every run contains a technology type
        testCase $"Run {r.Identifier} contains top-level metadata technology type" <| fun _ ->
            if r.TechnologyType.IsNone then
                failwith $"Run {r.Identifier} contains no top-level metadata technology type"
        
        // TestCase Non-critical: Every run contains a technology platform
        testCase $"Run {r.Identifier} contains top-level metadata technology platform" <| fun _ ->
            if r.TechnologyPlatform.IsNone then
                failwith $"Run {r.Identifier} contains no top-level metadata technology platform"


        // TestCase Non-critical: Every annotation table contains some annotation column
        
        if hasAnnotationColumns arc then

            for t in arc.ArcTables do
                testCase $"Table {t.Name} contains annotation column" <| fun _ ->
                
                let annoColCount = characteristicCount t + parameterCount t + factorCount t
                if annoColCount = 0 then
                    failwith $"Table {t.Name} contains no annotation column"  

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