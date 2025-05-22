let [<Literal>]PACKAGE_METADATA = """(*
---
Name: plant-growth
Summary: Validates if the ARC contains the necessary metadata to describe conditions for plant growth.
Description: |
    Validates if the ARC contains an annotation table with protocol type "Plant Growth Protocol" and if it exists, whether it contains the following fields:

        Critical fields:
        - organism (OBI:0100026)
        - growth day length (DPBO:0000041)
        - light intensity exposure (PECO:0007224)        
        - temperature day (DPBO:0000007)
        - temperature night (DPBO:0000008)       

        Non critical fields:
        - genotype (EFO:0000513)
        - study type (PECO:0007231)
        - Reference Time Point (NCIT:C82576)
        - growth plot design (DPBO:0000001)
        - plant growth medium exposure (PECO:0007147)
        - humidity day (DPBO:0000005)
        - humidity night (DPBO:0000006)
        - plant nutrient exposure (PECO:0007241)
        - abiotic plant exposure (PECO:0007191)
        - biotic plant exposure (PECO:0007357)
        - watering exposure (PECO:0007383)

MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: true
Authors:
  - FullName: Heinrich Lukas Weil
    Email: weil@nfdi4plants.org
    Affiliation: RPTU Kaiserslautern
    AffiliationLink: http://rptu.de/startseite
Tags:
  - Name: validation
  - Name: growth
  - Name: plant
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
                //Param.getValueAsTerm token = (CvTerm.create("DPBO:1000164","plant growth protocol","DPBO"))
                (Param.getValueAsTerm token).Name = "plant growth protocol"
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

// Validation Cases:
let cases = 
    testList "cases" [  // naming is difficult here

        ARCExpect.validationCase (TestID.Name "plant growth table") {
            if plantGrowthSheets |> Seq.isEmpty then
                Expecto.Tests.failtestNoStackf "No plant growth table found"            
        }

        if plantGrowthSheets |> Seq.isEmpty |> not then
            for table in plantGrowthSheets do
                ARCExpect.validationCase (TestID.Name $"{table.Key}: organism") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("OBI:0100026","organism","OBI"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: growth day length") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("DPBO:0000041","growth day length","DPBO"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: light intensity exposure") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("PECO:0007224","light intensity exposure","PECO"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: humidity day") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("DPBO:0000005","humidity day","DPBO"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: temperature day") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("DPBO:0000007","temperature day","DPBO"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: temperature night") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("DPBO:0000008","temperature night","DPBO"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: watering exposure") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("PECO:0007383","watering exposure","PECO"))
                }
    ]

let nonCriticalCases = 
    testList "cases" [  // naming is difficult here


        if plantGrowthSheets |> Seq.isEmpty |> not then
            for table in plantGrowthSheets do
                ARCExpect.validationCase (TestID.Name $"{table.Key}: genotype") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("EFO:0000513","genotype","EFO"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: study type") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("PECO:0007231","study type","PECO"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: Reference Time Point") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("NCIT:C82576","Reference Time Point","NCIT"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: growth plot design") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("DPBO:0000001","growth plot design","DPBO"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: plant growth medium exposure") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("PECO:0007147","plant growth medium exposure","PECO"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: humidity night") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("DPBO:0000006","humidity night","DPBO"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: plant nutrient exposure") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("PECO:0007241","plant nutrient exposure","PECO"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: abiotic plant exposure") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("PECO:0007191","abiotic plant exposure","PECO"))
                }
                ARCExpect.validationCase (TestID.Name $"{table.Key}: biotic plant exposure") {
                    table.Value
                    |> containsFilledOutColumn (CvTerm.create("PECO:0007357","biotic plant exposure","PECO"))
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