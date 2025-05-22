let [<Literal>]PACKAGE_METADATA = """(*
---
Name: hhu-cmml
Summary: Validates if the ARC contains the necessary metadata for plant sample submission to HHU CMML.
Description: |
  Validates if the ARC contains the necessary metadata for plant sample submission to HHU CMML.
  This is validates against "Swate template study sheet for plant samples" 
  https://str.nfdi4plants.org/template/226689ec-4be3-4143-b775-f8856c8ed6a5
  The following metadata is required (may not be empty):
    - Input [Source Name]
    - Parameter [sample submission date]
    - Characteristic [organism] OBI:0100026
  The following metadata is recommended:
    - Characteristic [biological replicate] DPBO:0000042
    - Characteristic [organism part] EFO:0000635
    - Characteristic [plant age] DPBO:0000033
    - Characteristic [genotype] EFO:0000513
    - Parameter [normalisation factor]
    - Parameter [dry weight]
    - Parameter [fresh weight]
    - Characteristic [resuspension volume]
    - Characteristic [resuspension solution]
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

// Check whether a building block with ontology column header contains values
let containsFilledOutColumnCVT (cvt : CvTerm) (tokenColumns : IParam list list) =
    let column = 
        tokenColumns 
        |> Seq.tryFind (fun column ->
            Param.getValueAsTerm column.Head = cvt
        )
    match column with
    | Some (h :: []) -> Expecto.Tests.failtestNoStackf $"{cvt.Name} column only contains header"            
    | Some (h :: vals) -> 
        vals
        |> List.iteri (fun i token ->
            if (Param.getValueAsTerm token).Name = "" then
                Expecto.Tests.failtestNoStackf $"column {cvt.Name} contains empty value at index {i}"                  
        )
    | _ -> Expecto.Tests.failtestNoStackf $"table contains no {cvt.Name} header"

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



let plantGrowthSheets = 
    studyFiles
    |> Seq.collect (fun s ->
        s
        |> Seq.filter (fun kv ->
            kv.Value 
            |> Seq.concat
            |> Seq.exists (fun token -> 
                token.Name = "ProtocolType" 
                && 
                (Param.getValueAsTerm token).Name = "plant growth protocol"
            )
        )   
    )


// Validation Cases:
let cases = 
    testList "cases" [  // naming is difficult here

        ARCExpect.validationCase (TestID.Name "plant growth table") {
            if plantGrowthSheets |> Seq.isEmpty then
                Expecto.Tests.failtestNoStackf "No plant growth table found"            
        }

        if plantGrowthSheets |> Seq.isEmpty |> not then
            for table in plantGrowthSheets do
            // ARCExpect.validationCase (TestID.Name $"{table.Key}: Source Name") {
            //     table.Value
            //     |> containsFilledOutColumnName "Source Name"
            // }
            ARCExpect.validationCase (TestID.Name $"{table.Key}: organism") {
                table.Value
                |> containsFilledOutColumnCVT (CvTerm.create("OBI:0100026","organism","OBI"))
            }
            ARCExpect.validationCase (TestID.Name $"{table.Key}: sample submission date") {
                table.Value
                |> containsFilledOutColumnName "sample submission date"
            }
    ]


let nonCriticalCases = 
    testList "cases" [  // naming is difficult here

        if plantGrowthSheets |> Seq.isEmpty |> not then
            for table in plantGrowthSheets do
               
                ARCExpect.validationCase (TestID.Name $"{table.Key}: biological replicate") {
                    table.Value
                    |> containsFilledOutColumnCVT (CvTerm.create("DPBO:0000042","biological replicate","DPBO"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: organism part") {
                    table.Value
                    |> containsFilledOutColumnCVT (CvTerm.create("EFO:0000635","organism part","EFO"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: plant age") {
                    table.Value
                    |> containsFilledOutColumnCVT (CvTerm.create("DPBO:0000033","plant age","DPBO"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: genotype") {
                    table.Value
                    |> containsFilledOutColumnCVT (CvTerm.create("EFO:0000513","genotype","EFO"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: normalisation factor") {
                    table.Value
                    |> containsFilledOutColumnName "normalisation factor"
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: dry weight") {
                    table.Value
                    |> containsFilledOutColumnName "dry weight"
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: fresh weight") {
                    table.Value
                    |> containsFilledOutColumnName "fresh weight"
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: resuspension volume") {
                    table.Value
                    |> containsFilledOutColumnName "resuspension volume"
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: resuspension solution") {
                    table.Value
                    |> containsFilledOutColumnName "resuspension solution"
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
