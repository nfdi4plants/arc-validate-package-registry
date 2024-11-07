let [<Literal>]PACKAGE_METADATA = """(*
---
Name: hhu-cmml
Summary: Validates if the ARC contains the necessary metadata for sample submission to HHU CMML.
Description: |
  Validates if the ARC contains the necessary metadata for sample submission to HHU CMML.
  The following metadata is required:
  - Investigation has title and description
  - All persons in Investigation Contacts must have a name, last name, affiliation and valid email
MajorVersion: 0
MinorVersion: 0
PatchVersion: 1
Publish: false
Authors:
  - FullName: Dominik Brilhaus
    Affiliation: CEPLAS
    AffiliationLink: 
Tags:
  - Name: ARC
  - Name: CMML
ReleaseNotes: |
  - First version of CMML validation package
---
*)"""

#r "nuget: ARCExpect, 4.0.1"

open ControlledVocabulary
open Expecto
open ARCExpect
open ARCTokenization
open ARCTokenization.StructuralOntology
open System.IO

// Input:
// let arcDir = Directory.GetCurrentDirectory()
let arcDir = "/Users/dominikbrilhaus/datahub-dataplant/ARC_24-0006"

// Values:
let absoluteDirectoryPaths = FileSystem.parseARCFileSystem arcDir

let investigationMetadata = 
    absoluteDirectoryPaths
    |> Investigation.parseMetadataSheetsFromTokens() arcDir 
    |> List.concat
    
let studyMetadata = 
    absoluteDirectoryPaths
    |> Study.parseMetadataSheetsFromTokens() arcDir 
    |> List.concat


let studyProcessGraphTokens = 
    try 
        absoluteDirectoryPaths
        |> Study.parseProcessGraphColumnsFromTokens arcDir
        |> Seq.collect Map.values
        |> List.concat
    with
        | _ -> List.empty

let organismTokens=
    studyProcessGraphTokens
    |> List.tryFind (fun cvpList -> cvpList.Head |> Param.getValueAsTerm = (CvTerm.create("OBI:0100026","organism","OBI")))
    |> Option.defaultValue []

// Validation Cases:
// let cases = 
//     testList INVMSO.``Investigation Metadata``.INVESTIGATION.key.Name [
//         // Investigation has title
//         ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``.Name) {
//             investigationMetadata
//             |> Validate.ParamCollection.ContainsNonKeyParamWithTerm
//                 INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``
//         }   

//     ]
    
let cases = 
    testList STDMSO.``Study Metadata``.STUDY.key.Name [
        // Study has identifier
        ARCExpect.validationCase (TestID.Name STDMSO.``Study Metadata``.STUDY.``Study Identifier``.Name) {
            studyMetadata
            |> Validate.ParamCollection.ContainsNonKeyParamWithTerm
                STDMSO.``Study Metadata``.STUDY.``Study Identifier``
        }   
        
        // Study has title
        ARCExpect.validationCase (TestID.Name STDMSO.``Study Metadata``.STUDY.``Study Title``.Name) {
            studyMetadata
            |> Validate.ParamCollection.ContainsNonKeyParamWithTerm
                STDMSO.``Study Metadata``.STUDY.``Study Title``
        }   

        // Study has title
        ARCExpect.validationCase (TestID.Name STDMSO.``Study Metadata``.STUDY.``Study Title``.Name) {
            studyMetadata
            |> Validate.ParamCollection.ContainsNonKeyParamWithTerm
                STDMSO.``Study Metadata``.STUDY.``Study Title``
        }

        ARCExpect.validationCase (TestID.Name "organism header exist") {
            organismTokens
            |> Validate.ParamCollection.SatisfiesPredicate (
                    fun iparams -> 
                    iparams |> List.exists (fun iparam -> iparam.Value = ParamValue.CvValue (CvTerm.create("OBI:0100026","organism","OBI")))
            )
        }



    ]
     

// Execution:

Setup.ValidationPackage(
    metadata = Setup.Metadata(PACKAGE_METADATA),
    CriticalValidationCases = [cases]
)
|> Execute.ValidationPipeline(
    basePath = arcDir
)