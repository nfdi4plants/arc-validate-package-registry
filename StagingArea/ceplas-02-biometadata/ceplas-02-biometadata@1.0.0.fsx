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
        - Every study annotation table column contains values
        - Every study annotation table row contains values
        - Every assay contains at least one annotation table
        - Every assay annotation table contains basic information            
        - Every assay annotation table column contains values
        - Every assay annotation table row contains values
        - Every run contains at least one annotation table
        - Every run annotation table contains basic information            
        - Every run annotation table column contains values
        - Every run annotation table row contains values

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



let hasAnnotationColumns (a: ARC)=
    a.ArcTables
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

let isEmptyAnnoTable (t : ArcTable) =
    t.ISAValues
    |> Seq.forall (fun kv -> not kv.Value.HasValue)

let emptyAnnoCols (t : ArcTable) =
    t.ISAValues
    |> Seq.groupBy (fun kv -> kv.Value.NameText)
    |> Seq.choose (fun (header, values) ->
        if values |> Seq.forall (fun kv -> not kv.Value.HasValue) then
            Some header
        else
            None
    )


let isEmptyCell (cell: CompositeCell) =
    cell.isFreeText && cell.AsFreeText = ""
    ||
    cell.isTerm && cell.AsTerm.NameText = ""
    ||
    cell.isUnitized && fst cell.AsUnitized = ""
    ||
    cell.isData && cell.AsData.NameText = ""

let emptyAnnoRows (t: ArcTable) =
    [0 .. t.RowCount - 1]
    |> Seq.choose (fun rowIndex ->
        let row = t.GetRow rowIndex

        if row |> Seq.forall isEmptyCell then
            Some rowIndex
        else
            None
    )



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

    ////////////////////////////////////
    ////// ARC Study + Assay
    ////////////////////////////////////    
    
    // TestCase Critical: ARC contains at least one study or assay or workflow or run

    testCase "ARC contains at least one study or assay or workflow or run" <| fun _ ->

        Expect.isGreaterThan (arc.StudyCount + arc.AssayCount + arc.WorkflowCount + arc.RunCount) 0
            "ARC does not contain any study or assay or workflow or run"

    // TestCase Critical: ARC contains any annotation column (Characteristic, Parameter, Factor)

    testCase "ARC contains any annotation column (Characteristic, Parameter, Factor)" <| fun _ ->
        Expect.isTrue (hasAnnotationColumns arc)
            "ARC contains no annotation column (Characteristic, Parameter, Factor)"

    for s in arc.Studies do
        
        // TestCase Critical: Every study contains at least one annotation table
        testCase $"Study '{s.Identifier}' contains annotation table" <| fun _ ->
            
            Expect.isGreaterThan s.TableCount 0 
                $"Study '{s.Identifier}' contains no annotation table"
        
        for t in s.Tables do

            let tableAssociationText = $"Table '{t.Name}' of study '{s.Identifier}'"

            // TestCase Critical: Every study annotation table contains basic information            
                
            testCase $"{tableAssociationText} contains basic information" <| fun _ ->
                Expect.isFalse (isEmptyAnnoTable t)
                    $"{tableAssociationText} is empty"
                Expect.isGreaterThanOrEqual t.ColumnCount 2
                    $"{tableAssociationText} contains less than 2 columns"
                Expect.isGreaterThan t.RowCount 0
                    $"{tableAssociationText} contains no rows"

            if not (isEmptyAnnoTable t) then
                
                // TestCase Critical: Every study annotation table column contains values
                let emptyCols = emptyAnnoCols t
                let headers = String.concat ", " emptyCols

                testCase $"{tableAssociationText}: All columns contain values" <| fun _ ->
                    Expect.isTrue (emptyCols |> Seq.isEmpty)
                        $"{tableAssociationText} contains empty column(s): {headers}"            
             
                // TestCase Critical: Every study annotation table row contains values
                let emptyRows = emptyAnnoRows t
                let rowIds = emptyRows |> Seq.map string |> String.concat ", "

                testCase $"{tableAssociationText}: All rows contain values" <| fun _ ->
                    Expect.isTrue (emptyRows |> Seq.isEmpty)
                        $"{tableAssociationText} contains empty row(s): {rowIds}"

    for a in arc.Assays do
        
        // TestCase Critical: Every assay contains at least one annotation table
        testCase $"Assay '{a.Identifier}' contains annotation table" <| fun _ ->
            
            Expect.isGreaterThan a.TableCount 0
                $"Assay '{a.Identifier}' contains no annotation table"

        for t in a.Tables do

            let tableAssociationText = $"Table '{t.Name}' of assay '{a.Identifier}'"

            // TestCase Critical: Every assay annotation table contains basic information            
                
            testCase $"{tableAssociationText} contains basic information" <| fun _ ->
                Expect.isFalse (isEmptyAnnoTable t)
                    $"{tableAssociationText} is empty"
                Expect.isGreaterThanOrEqual t.ColumnCount 2
                    $"{tableAssociationText} contains less than 2 columns"
                Expect.isGreaterThan t.RowCount 0
                    $"{tableAssociationText} contains no rows"

            if not (isEmptyAnnoTable t) then
                
                // TestCase Critical: Every assay annotation table column contains values
                let emptyCols = emptyAnnoCols t
                let headers = String.concat ", " emptyCols

                testCase $"{tableAssociationText}: All columns contain values" <| fun _ ->
                    Expect.isTrue (emptyCols |> Seq.isEmpty)
                        $"{tableAssociationText} contains empty column(s): {headers}"            
             
                // TestCase Critical: Every assay annotation table row contains values
                let emptyRows = emptyAnnoRows t
                let rowIds = emptyRows |> Seq.map string |> String.concat ", "

                testCase $"{tableAssociationText}: All rows contain values" <| fun _ ->
                    Expect.isTrue (emptyRows |> Seq.isEmpty)
                        $"{tableAssociationText} contains empty row(s): {rowIds}"
                    
    for r in arc.Runs do
        
        // TestCase Critical: Every run contains at least one annotation table
        testCase $"Run {r.Identifier} contains annotation table" <| fun _ ->
            Expect.isGreaterThan r.TableCount 0
                $"Run {r.Identifier} contains no annotation table"
        
        for t in r.Tables do
                
            let tableAssociationText = $"Table '{t.Name}' of run '{r.Identifier}'"

            // TestCase Critical: Every run annotation table contains basic information            
                
            testCase $"{tableAssociationText} contains basic information" <| fun _ ->
                Expect.isFalse (isEmptyAnnoTable t)
                    $"{tableAssociationText} is empty"
                Expect.isGreaterThanOrEqual t.ColumnCount 2
                    $"{tableAssociationText} contains less than 2 columns"
                Expect.isGreaterThan t.RowCount 0
                    $"{tableAssociationText} contains no rows"

            if not (isEmptyAnnoTable t) then
                
                // TestCase Critical: Every run annotation table column contains values
                let emptyCols = emptyAnnoCols t
                let headers = String.concat ", " emptyCols

                testCase $"{tableAssociationText}: All columns contain values" <| fun _ ->
                    Expect.isTrue (emptyCols |> Seq.isEmpty)
                        $"{tableAssociationText} contains empty column(s): {headers}"            
             
                // TestCase Critical: Every run annotation table row contains values
                let emptyRows = emptyAnnoRows t
                let rowIds = emptyRows |> Seq.map string |> String.concat ", "

                testCase $"{tableAssociationText}: All rows contain values" <| fun _ ->
                    Expect.isTrue (emptyRows |> Seq.isEmpty)
                        $"{tableAssociationText} contains empty row(s): {rowIds}"

    ]
    

let nonCriticalCases =
    testList "nonCriticalCases" [

    /////////////////////////////////////////////////////////////////
    ////// ARC Study top level metadata
    /////////////////////////////////////////////////////////////////
        
    for s in arc.Studies do
        
        // TestCase Non-critical: Every study contains a title
        testCase $"Study '{s.Identifier}' contains title" <| fun _ ->
            // Study title exists
            Expect.isSome s.Title
                $"Study '{s.Identifier}' contains no title"
            // Study title is longer than 3 characters
            Expect.isGreaterThan s.Title.Value.Length 4
                $"Study '{s.Identifier}' contains no meaningful title (i.e. longer than 3 characters):'{s.Title.Value}'"
        
        // TestCase Non-critical: Every study contains a description
        testCase $"Study '{s.Identifier}' contains description" <| fun _ ->
            // Study description exists
            Expect.isSome s.Description
                $"Study '{s.Identifier}' contains no description"
            // Study description is longer than 30 characters
            Expect.isGreaterThan s.Description.Value.Length 30
                $"Study '{s.Identifier}' contains no meaningful description (i.e. longer than 30 characters):'{s.Description.Value}'"

        // TestCase Non-critical: Every study contains contacts
        testCase $"Study '{s.Identifier}' contains contacts" <| fun _ ->
            Expect.isGreaterThan s.Contacts.Count 0
                $"Study '{s.Identifier}' contains no contacts"
    
    /////////////////////////////////////////////////////////////////
    ////// ARC Assay top level metadata
    /////////////////////////////////////////////////////////////////

    for a in arc.Assays do

        // TestCase Non-critical: Every assay contains a title
        testCase $"Assay '{a.Identifier}' contains title" <| fun _ ->
            // Assay title exists
            Expect.isSome a.Title
                $"Assay '{a.Identifier}' contains no title"
            // Assay title is longer than 4 characters
            Expect.isGreaterThan a.Title.Value.Length 4
                $"Assay '{a.Identifier}' contains no meaningful title (i.e. longer than 3 characters):'{a.Title.Value}'"
        
        // TestCase Non-critical: Every assay contains a description
        testCase $"Assay '{a.Identifier}' contains description" <| fun _ ->
            // Assay description exists
            Expect.isSome a.Description
                $"Assay '{a.Identifier}' contains no description"
            // Assay description is longer than 30 characters
            Expect.isGreaterThan a.Description.Value.Length  30
                $"Assay '{a.Identifier}' contains no meaningful description (i.e. longer than 30 characters):'{a.Description.Value}'"

        // TestCase Non-critical: Every assay contains performers
        testCase $"Study '{a.Identifier}' contains contacts" <| fun _ ->
            Expect.isGreaterThan a.Performers.Count 0
                $"Study '{a.Identifier}' contains no performers"

        // TestCase Non-critical: Every assay contains a measurement type
        testCase $"Assay '{a.Identifier}' contains top-level metadata measurement type" <| fun _ ->
            Expect.isSome a.MeasurementType
                $"Assay '{a.Identifier}' contains no top-level metadata measurement type"
        
        // TestCase Non-critical: Every assay contains a technology type
        testCase $"Assay '{a.Identifier}' contains top-level metadata technology type" <| fun _ ->
            Expect.isSome a.TechnologyType
                $"Assay '{a.Identifier}' contains no top-level metadata technology type"
        
        // TestCase Non-critical: Every assay contains a technology platform
        testCase $"Assay '{a.Identifier}' contains top-level metadata technology platform" <| fun _ ->
            Expect.isSome a.TechnologyPlatform
                $"Assay '{a.Identifier}' contains no top-level metadata technology platform"

    /////////////////////////////////////////////////////////////////
    ////// ARC Workflow top level metadata
    /////////////////////////////////////////////////////////////////

    for w in arc.Workflows do

        // TestCase Non-critical: Every workflow contains a title
        testCase $"Workflow '{w.Identifier}' contains title" <| fun _ ->
            // Workflow title exists
            Expect.isSome w.Title
                $"Workflow '{w.Identifier}' contains no title"
            // Workflow title is longer than 4 characters
            Expect.isGreaterThan w.Title.Value.Length 3
                $"Workflow '{w.Identifier}' contains no meaningful title (i.e. longer than 3 characters):'{w.Title.Value}'"
        
        // TestCase Non-critical: Every workflow contains a description
        testCase $"Workflow '{w.Identifier}' contains description" <| fun _ ->
            // Workflow description exists
            Expect.isSome w.Description
                $"Workflow '{w.Identifier}' contains no description"
            // Workflow description is longer than 30 characters
            Expect.isGreaterThan w.Description.Value.Length 30
                $"Workflow '{w.Identifier}' contains no meaningful description (i.e. longer than 30 characters):'{w.Description.Value}'"
        
        // TestCase Non-critical: Every workflow contains contacts
        testCase $"Workflow '{w.Identifier}' contains contacts" <| fun _ ->
            Expect.isGreaterThan w.Contacts.Count 0
                $"Workflow '{w.Identifier}' contains no contacts"

    /////////////////////////////////////////////////////////////////
    ////// ARC Run top level metadata
    /////////////////////////////////////////////////////////////////

    for r in arc.Runs do

        // TestCase Non-critical: Every run contains a title
        testCase $"Run '{r.Identifier}' contains title" <| fun _ ->
            // Run title exists
            Expect.isSome r.Title
                $"Run '{r.Identifier}' contains no title"
            // Run title is longer than 4 characters
            Expect.isGreaterThan r.Title.Value.Length 3
                $"Run '{r.Identifier}' contains no meaningful title (i.e. longer than 3 characters):'{r.Title.Value}'"
        
        // TestCase Non-critical: Every run contains a description
        testCase $"Run '{r.Identifier}' contains description" <| fun _ ->
            // Run description exists
            Expect.isSome r.Description
                $"Run '{r.Identifier}' contains no description"
            // Run description is longer than 30 characters
            Expect.isGreaterThan r.Description.Value.Length 30
                $"Run '{r.Identifier}' contains no meaningful description (i.e. longer than 30 characters):'{r.Description.Value}'"

        // TestCase Non-critical: Every run contains performers
        testCase $"Run '{r.Identifier}' contains contacts" <| fun _ ->
            Expect.isGreaterThan r.Performers.Count 0
                $"Run '{r.Identifier}' contains no performers"

        // TestCase Non-critical: Every run contains a measurement type
        testCase $"Run '{r.Identifier}' contains top-level metadata measurement type" <| fun _ ->
            Expect.isSome r.MeasurementType
                $"Run '{r.Identifier}' contains no top-level metadata measurement type"
        
        // TestCase Non-critical: Every run contains a technology type
        testCase $"Run '{r.Identifier}' contains top-level metadata technology type" <| fun _ ->
            Expect.isSome r.TechnologyType
                $"Run '{r.Identifier}' contains no top-level metadata technology type"
        
        // TestCase Non-critical: Every run contains a technology platform
        testCase $"Run '{r.Identifier}' contains top-level metadata technology platform" <| fun _ ->
            Expect.isSome r.TechnologyPlatform
                $"Run '{r.Identifier}' contains no top-level metadata technology platform"


        // TestCase Non-critical: Every annotation table contains some annotation column
        
        if hasAnnotationColumns arc then

            for t in arc.ArcTables do
                testCase $"Table {t.Name} contains annotation column" <| fun _ ->
                
                    let annoColCount = characteristicCount t + parameterCount t + factorCount t
                
                    Expect.isGreaterThan annoColCount 0
                        $"Table {t.Name} contains no annotation column"

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