let [<Literal>]PACKAGE_METADATA = """(*
---
Name: pride
Summary: Validates if the ARC contains the necessary metadata to be publishable via PRIDE.
Description: |
    Validates if the ARC contains the necessary metadata to be publishable via PRIDE.
    The following metadata is required:
        - Investigation has title and description
        - Investigation has Keywords comment in correct format
        - All persons in Investigation Contacts must have a first name, last name, affiliation and valid email
        - Study has protocol, tissue & species in correct format
        - Assay has protocol, technology type, instrument model, and fixed and/or variable modification in correct format
MajorVersion: 1
MinorVersion: 0
PatchVersion: 1
Publish: false
Authors:
  - FullName: Oliver Maus
    Email: maus@nfdi4plants.org
    Affiliation: RPTU Kaiserslautern
    AffiliationLink: http://rptu.de/startseite
  - FullName: Christopher Lux
    Email: lux@csbiology.de
    Affiliation: RPTU Kaiserslautern
    AffiliationLink: http://rptu.de/startseite
Tags:
  - Name: validation
  - Name: pride
  - Name: proteomics
ReleaseNotes: |
  - initial release
  - metadata validation added:
    - Investigation has title and description
    - Investigation has Keywords comment in correct format
    - All persons in Investigation Contacts must have a first name, last name, affiliation and valid email
    - Study has protocol, tissue & species in correct format 
    - Assay has protocol, technology type, instrument model, and fixed and/or variable modification in correct format
---
*)"""

#r "nuget: ARCExpect, 3.0.0"

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

let investigationMetadata = 
    absoluteDirectoryPaths
    |> Investigation.parseMetadataSheetsFromTokens() arcDir 
    |> List.concat 

let studyMetadata = 
    absoluteDirectoryPaths
    |> Study.parseMetadataSheetsFromTokens() arcDir

let assayMetadata =
    absoluteDirectoryPaths
    |> Assay.parseMetadataSheetsFromTokens() arcDir

let studyProcessGraphTokens = 
    try 
        absoluteDirectoryPaths
        |> Study.parseProcessGraphColumnsFromTokens arcDir
        |> Seq.collect Map.values
        |> List.concat
    with
        | _ -> List.empty

let assayProcessGraphTokens =
    try 
        absoluteDirectoryPaths
        |> Assay.parseProcessGraphColumnsFromTokens arcDir
        |> Seq.collect Map.values
        |> List.concat
    with
        | _ -> List.empty

let organismTokens=
    studyProcessGraphTokens
    |> List.tryFind (fun cvpList -> cvpList.Head |> Param.getValueAsTerm = (CvTerm.create("OBI:0100026","organism","OBI")))
    |> Option.defaultValue []

let tissueTokens=
    studyProcessGraphTokens
    |> List.tryFind (fun cvpList -> cvpList.Head |> Param.getValueAsTerm = (CvTerm.create("NCIT:C12801","Tissue","NCIT")))
    |> Option.defaultValue []

let instrumentTokens =
    assayProcessGraphTokens
    |> List.tryFind (fun cvpList -> cvpList.Head |> Param.getValueAsTerm = (CvTerm.create("MS:1000031","instrument model","MS")))
    |> Option.defaultValue []


let techTypeName = CvTerm.create("ASSMSO:00000011", "Assay Technology Type", "ASSMSO")
let techTypeTAN = CvTerm.create("ASSMSO:00000013", "Assay Technology Type Term Accession Number", "ASSMSO")
let techTypeTSR = CvTerm.create("ASSMSO:00000015", "Assay Technology Type Term Source REF", "ASSMSO")

// Helper functions (to deposit in ARCExpect later):
let characterLimit (lowerLimit : int option) (upperLimit : int option) =
    match lowerLimit, upperLimit with
    | None, None -> System.Text.RegularExpressions.Regex(@"^.{0,}$")
    | Some ll, None -> System.Text.RegularExpressions.Regex($"^.{{{ll},}}$")
    | None, Some ul -> System.Text.RegularExpressions.Regex($"^.{{0,{ul}}}$")
    | Some ll, Some ul -> System.Text.RegularExpressions.Regex($"^.{{{ll},{ul}}}$")

type ErrorMessage with

    static member ofIParamCollection error iParamCollection =

        let iParam = Seq.head iParamCollection

        let str = new StringBuilder()    
        str.AppendFormat("['{0}', ..] {1}\n",  Param.getCvName iParam, error) |> ignore 

        match Param.tryGetValueOfCvParamAttr "FilePath" iParam with
        | Some path ->
            str.AppendFormat(" > filePath '{0}'\n", path) |> ignore         
        | None -> ()

        match Param.tryGetValueOfCvParamAttr "Worksheet" iParam with
        | Some sheet ->
            str.AppendFormat(" > sheet '{0}'", sheet) |> ignore         
        | None -> ()

        match Param.tryGetValueOfCvParamAttr "Row" iParam with
        | Some row -> 
            str.AppendFormat(" > row '{0}'", row) |> ignore
        | None -> ()

        match Param.tryGetValueOfCvParamAttr "Column" iParam with
        | Some column -> 
            str.AppendFormat(" > column '{0}'", column) |> ignore
        | None -> ()        
                
        match Param.tryGetValueOfCvParamAttr "Line" iParam with
        | Some line ->
            str.AppendFormat(" > line '{0}'", line) |> ignore
        | None -> ()

        match Param.tryGetValueOfCvParamAttr "Position" iParam with
        | Some position -> 
            str.AppendFormat(" > position '{0}'", position) |> ignore
        | None -> ()
        str.ToString()

type Validate.ParamCollection with

    static member forAll (projection : #IParam -> bool) (paramCollection : #seq<#IParam>) =
        match Seq.forall projection paramCollection with
        | true  -> ()
        | false ->
            ErrorMessage.ofIParamCollection $"does not satisfy the requirements" paramCollection
            |> Expecto.Tests.failtestNoStackf "%s"


organismTokens |> Seq.forall (fun ip -> match ip.Value with CvValue _ -> true | _ -> false)

// Validation Cases:
let cases = 
    testList "cases" [  // naming is difficult here
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``.Name) {
            investigationMetadata
            |> Validate.ParamCollection.ContainsParamWithTerm
                INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``.Name) {
            investigationMetadata
            |> Validate.ParamCollection.ContainsParamWithTerm
                INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``
        }//recheck
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``.Name} exists") {
            investigationMetadata
            |> Validate.ParamCollection.ContainsParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``
        }
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``.Name} is not empty") {
            investigationMetadata
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``)
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }
        //recheck
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``.Name} exists") {
            investigationMetadata
            |> Validate.ParamCollection.ContainsParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``
        }
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``.Name} is not empty") {
            investigationMetadata
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``)
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }     
        //recheck
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``.Name} exists") {
            investigationMetadata
            |> Validate.ParamCollection.ContainsParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``
        }
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``.Name} is not empty") {
            investigationMetadata
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``)
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        } 
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``.Name} exists") {
        investigationMetadata
        |> Validate.ParamCollection.ContainsParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Investigation Person Email``.Name) {
            investigationMetadata
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``)
            |> Seq.filter (Param.getValueAsString >> (<>) "Metadata Section Key")
            |> Seq.iter (Validate.Param.ValueMatchesRegex StringValidationPattern.email)
        }
        // missing: how to get specific comment? (here: Keywords Comment)
        //ARCExpect.validationCase (TestID.Name "Comment: Keywords") {
        //    commis
        //    |> Seq.iter (Validate.Param.ValueMatchesRegex StringValidationPattern.email)    // needs special Regex
        //}
        ARCExpect.validationCase (TestID.Name STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.key.Name) {
            studyMetadata
            |> List.iter(fun study ->
                study
                |> Validate.ParamCollection.ContainsParamWithTerm STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.key
            )
        }
        ARCExpect.validationCase (TestID.Name "STUDY PROTOCOLS description") {
            studyMetadata
            |> List.iter(fun study ->
                study
                |> Seq.filter (fun iparam -> Param.getTerm iparam = STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.``Study Protocol Description``)
                |> Seq.filter (Param.getValueAsString >> (<>) "Metadata Section Key")
                |> Seq.iter (Validate.Param.ValueMatchesRegex (characterLimit (Some 50) (Some 500)))
            )
        }
        ARCExpect.validationCase (TestID.Name "organism header exist") {
            organismTokens
            |> Validate.ParamCollection.SatisfiesPredicate (
                    fun iparams -> 
                    iparams |> List.exists (fun iparam -> iparam.Value = ParamValue.CvValue (CvTerm.create("OBI:0100026","organism","OBI")))
            )
        }
        ARCExpect.validationCase (TestID.Name "organism column has terms") {
            organismTokens
            |> Validate.ParamCollection.forAll (fun ip -> match ip.Value with CvValue _ -> true | _ -> false)
        }
        ARCExpect.validationCase (TestID.Name "Tissue") {
            tissueTokens
            |> Validate.ParamCollection.SatisfiesPredicate (
                fun iparams ->
                    iparams
                    |> List.exists (fun iparam -> iparam.Value = ParamValue.CvValue (CvTerm.create("NCIT:C12801","Tissue","NCIT")))
            )
        }//recheck
        ARCExpect.validationCase (TestID.Name "Tissue terms") {
            tissueTokens
            |> Validate.ParamCollection.forAll (fun ip -> match ip.Value with CvValue _ -> true | _ -> false)
        }
        ARCExpect.validationCase (TestID.Name techTypeName.Name) {
            assayMetadata
            |> List.iter(fun assay -> 
                assay
                |> Validate.ParamCollection.ContainsParamWithTerm techTypeName
            )
        }
        ARCExpect.validationCase (TestID.Name techTypeTAN.Name) {
            assayMetadata
            |> List.iter(fun assay -> 
                assay
                |> Validate.ParamCollection.ContainsParamWithTerm techTypeTAN
            )
        }
        ARCExpect.validationCase (TestID.Name techTypeTSR.Name) {
            assayMetadata
            |> List.iter(fun assay -> 
                assay
                |>  Validate.ParamCollection.ContainsParamWithTerm techTypeTSR
            )
        }
        ARCExpect.validationCase (TestID.Name "Instrument Model") {
            instrumentTokens
            |> Validate.ParamCollection.SatisfiesPredicate (
                fun iparams ->
                    iparams
                    |> List.exists (fun iparam -> iparam.Value = ParamValue.CvValue (CvTerm.create("MS:1000031","instrument model","MS")))
            )
        }
        ARCExpect.validationCase (TestID.Name "Modification") {
            ARCExpect.either (fun _ ->
                assayProcessGraphTokens
                |> List.concat
                |> Validate.ParamCollection.SatisfiesPredicate (
                    fun iparams -> 
                    iparams |> List.exists (fun iparam -> iparam.Value = ParamValue.CvValue (CvTerm.create("MS:1003021","Fixed modification","MS")))
                )
            ) (fun _ ->
                assayProcessGraphTokens
                |> List.concat
                |> Validate.ParamCollection.SatisfiesPredicate (
                    fun iparams -> 
                    iparams |> List.exists (fun iparam -> iparam.Value = ParamValue.CvValue (CvTerm.create("MS:1003022","Variable modification","MS")))
                )
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