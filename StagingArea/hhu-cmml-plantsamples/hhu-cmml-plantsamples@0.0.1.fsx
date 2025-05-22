let [<Literal>]PACKAGE_METADATA = """(*
---
Name: hhu-cmml
Summary: Validates if the ARC contains the necessary metadata for plant sample submission to HHU CMML.
Description: |
  Validates if the ARC contains the necessary metadata for plant sample submission to HHU CMML.
  This is validates against "Swate template study sheet for plant samples" 
  https://str.nfdi4plants.org/template/226689ec-4be3-4143-b775-f8856c8ed6a5
  The following metadata is required (may not be empty):
  - Parameter [sample submission date]
  - Characteristic [organism]
  - Characteristic [biological replicate]
  The following metadata is recommended:
  - Characteristic [organism part]
MajorVersion: 0
MinorVersion: 0
PatchVersion: 1
Publish: false
Authors:
  - FullName: Dominik Brilhaus
    Affiliation: CEPLAS
    AffiliationLink: https://ceplas.eu
Tags:
  - Name: ARC
  - Name: CMML
ReleaseNotes: |
  - First version of CMML plant sample sheet validation package
---
*)"""

#r "nuget: ARCExpect, 4.0.0"

open ControlledVocabulary
open Expecto
open ARCExpect
open ARCTokenization
open ARCTokenization.StructuralOntology
open System.IO
open System.Text
open FSharpAux

// Input:
// let arcDir = Directory.GetCurrentDirectory()

// TEST
let arcDir = @"/Users/dominikbrilhaus/Downloads/cmml-sample-validate"

// Values:
let absoluteDirectoryPaths = FileSystem.parseARCFileSystem arcDir

let studyFiles = 
    try 
        absoluteDirectoryPaths
        |> Study.parseProcessGraphColumnsFromTokens arcDir
    with
        | _ -> seq{Map.empty}

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


let anyStudySheets = 
    studyFiles |> Seq.concat


// Validation Cases:
let cases = 
    testList "cases" [  // naming is difficult here

        for table in anyStudySheets do
                ARCExpect.validationCase (TestID.Name $"{table.Key}: organism") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("OBI:0100026","organism","OBI"))
                }
    ]

let nonCriticalCases = 
    testList "cases" [  // naming is difficult here

        if anyStudySheets |> Seq.isEmpty |> not then
            for table in anyStudySheets do
               
                ARCExpect.validationCase (TestID.Name $"{table.Key}: organism part") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("EFO:0000635","organism part","EFO"))
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