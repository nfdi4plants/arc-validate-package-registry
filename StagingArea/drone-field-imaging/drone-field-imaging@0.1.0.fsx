let [<Literal>]PACKAGE_METADATA = """(*
---
Name: drone-field-imaging
Summary: Validates if the ARC contains the necessary metadata to describe drone fly-over image capture.
Description: |
        Critical fields:
            - "Date and Time"
            - "Longitude"
            - "Latitude"
            - "Absolute Altitude"
            - "Relative Altitude"
        
        Non-critical fields:
            - "Altitude Reference"
            - "Drone Manufacturer"
            - "Drone Model"
            - "Zoom Factor"
MajorVersion: 0
MinorVersion: 1
PatchVersion: 0
Publish: true
Authors:
  - FullName: Dominik Brilhaus
    Affiliation: CEPLAS
    AffiliationLink: https://ceplas.eu
Tags:
  - Name: validation
  - Name: drone flyover
  - Name: image capture
ReleaseNotes: |
  - initial release
---
*)"""

#r "nuget: ARCExpect, 2.0.0"

open ControlledVocabulary
open Expecto
open ARCExpect
open ARCTokenization
open ARCTokenization.StructuralOntology
open System.IO
open System.Text
open FSharpAux

// Input:
let arcDir = Directory.GetCurrentDirectory()

// Values:
let absoluteDirectoryPaths = FileSystem.parseARCFileSystem arcDir

let studyFiles = 
    try 
        absoluteDirectoryPaths
        |> Study.parseProcessGraphColumnsFromTokens arcDir
    with
        | _ -> seq{Map.empty}


let droneSheets = 
    studyFiles
    |> Seq.collect (fun s ->
        s
        |> Seq.filter (fun kv ->
            kv.Value 
            |> Seq.concat
            |> Seq.exists (fun token -> 
                token.Name = "ProtocolType" 
                && 
                (Param.getValueAsTerm token).Name = "drone image protocol"
            )
        )   
    )

let containsFilledOutColumn (term : CvTerm) (tokenColumns : IParam list list) =
    let column = 
        tokenColumns 
        |> Seq.tryFind (fun column ->
            Param.getValueAsTerm column.Head = term
        )
    match column with
    | Some (h :: []) -> Expecto.Tests.failtestNoStackf $"{term.Name} column only contains header"            
    | Some (h :: vals) -> 
        vals
        |> List.iteri (fun i token ->
            if (Param.getValueAsTerm token).Name = "" then
                Expecto.Tests.failtestNoStackf $"column {term.Name} contains empty value at index {i}"                  
        )
    | _ -> Expecto.Tests.failtestNoStackf $"table contains no {term.Name} header"

// Check whether a building block without ontology column header (i.e. only name) contains values
let containsFilledOutColumnName (name : string) (tokenColumns : IParam list list) =
    let column = 
        tokenColumns 
        |> Seq.tryFind (fun column ->
            (Param.getValueAsTerm column.Head).Name = name
        )
    match column with
    | Some (h :: []) -> Expecto.Tests.failtestNoStackf $"{name} column only contains header"            
    | Some (h :: vals) -> 
        vals
        |> List.iteri (fun i token ->
            if (Param.getValueAsTerm token).Name = "" then
                Expecto.Tests.failtestNoStackf $"column {name} contains empty value at index {i}"                  
        )
    | _ -> Expecto.Tests.failtestNoStackf $"table contains no {name} header"             

// Validation Cases:
let cases = 
    testList "cases" [  // naming is difficult here

        ARCExpect.validationCase (TestID.Name "drone image table") {
            if droneSheets |> Seq.isEmpty then
                Expecto.Tests.failtestNoStackf "No drone image table found"            
        }

        if droneSheets |> Seq.isEmpty |> not then
            for table in droneSheets do
                ARCExpect.validationCase (TestID.Name $"{table.Key}: Date and Time") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("NCIT:C37939","Date and Time","NCIT"))
                }

                ARCExpect.validationCase (TestID.Name $"{table.Key}: Longitude") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("NCIT:C68643","Longitude","NCIT"))
                }

                ARCExpect.validationCase (TestID.Name $"{table.Key}: Latitude") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("NCIT:C68642","Latitude","NCIT"))
                }
                
                ARCExpect.validationCase (TestID.Name $"{table.Key}: Absolute Altitude") {
                    table.Value
                    |> containsFilledOutColumnName "Absolute Altitude"
                }

                ARCExpect.validationCase (TestID.Name $"{table.Key}: Relative Altitude") {
                    table.Value
                    |> containsFilledOutColumnName "Relative Altitude"
                }

    ]

let nonCriticalCases = 
    testList "cases" [  // naming is difficult here

        if droneSheets |> Seq.isEmpty |> not then
            for table in droneSheets do
                ARCExpect.validationCase (TestID.Name $"{table.Key}: Altitude Reference") {
                    table.Value
                    |> containsFilledOutColumnName "Altitude Reference"
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: Drone Manufacturer") {
                    table.Value
                    |> containsFilledOutColumnName "Drone Manufacturer"
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: Drone Model") {
                    table.Value
                    |> containsFilledOutColumnName "Drone Model"
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: Zoom Factor") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("NCIT:C95005","Zoom Factor","NCIT"))
                }
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