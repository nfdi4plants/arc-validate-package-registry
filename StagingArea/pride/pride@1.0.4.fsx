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
PatchVersion: 4
Publish: true
Authors:
  - FullName: Oliver Maus
    Affiliation: DataPLANT
  - FullName: Christopher Lux
    Email: lux@csbiology.de
    Affiliation: RPTU Kaiserslautern
    AffiliationLink: http://rptu.de/startseite
Tags:
  - Name: validation
  - Name: pride
  - Name: proteomics
ReleaseNotes: |
  - Bug fix: Fixed/variable modification values must not be empty
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
    |> List.concat

let assayMetadata =
    absoluteDirectoryPaths
    |> Assay.parseMetadataSheetsFromTokens() arcDir
    |> List.concat

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

let organismTokens =
    studyProcessGraphTokens
    |> List.tryFind (fun cvpList -> cvpList.Head |> Param.getValueAsTerm = (CvTerm.create("OBI:0100026","organism","OBI")))
    |> Option.defaultValue []

let tissueTokens =
    studyProcessGraphTokens
    |> List.tryFind (fun cvpList -> cvpList.Head |> Param.getValueAsTerm = (CvTerm.create("NCIT:C12801","Tissue","NCIT")))
    |> Option.defaultValue []

let instrumentTokens =
    assayProcessGraphTokens
    |> List.tryFind (fun cvpList -> cvpList.Head |> Param.getValueAsTerm = (CvTerm.create("MS:1000031","instrument model","MS")))
    |> Option.defaultValue []

let modTokens =
    assayProcessGraphTokens
    |> List.filter (
        fun cvpList -> 
            cvpList.Head |> Param.getValueAsTerm = (CvTerm.create("MS:1003021","Fixed modification","MS")) ||
            cvpList.Head |> Param.getValueAsTerm = (CvTerm.create("MS:1003022","Variable modification","MS"))
    )
    |> List.concat

let techTypeName = CvTerm.create("ASSMSO:00000011", "Assay Technology Type", "ASSMSO")
let techTypeTAN = CvTerm.create("ASSMSO:00000013", "Assay Technology Type Term Accession Number", "ASSMSO")
let techTypeTSR = CvTerm.create("ASSMSO:00000015", "Assay Technology Type Term Source REF", "ASSMSO")


// Helper functions (to deposit in ARCExpect later):

open System.Text


let characterLimit (lowerLimit : int option) (upperLimit : int option) =
    match lowerLimit, upperLimit with
    | None, None -> RegularExpressions.Regex(@"^.{0,}$")
    | Some ll, None -> RegularExpressions.Regex($"^.{{{ll},}}$")
    | None, Some ul -> RegularExpressions.Regex($"^.{{0,{ul}}}$")
    | Some ll, Some ul -> RegularExpressions.Regex($"^.{{{ll},{ul}}}$")


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

    static member AllTermsSatisfyPredicate (projection : #IParam -> bool) (paramCollection : #seq<#IParam>) =
        match Seq.forall projection paramCollection with
        | true  -> ()
        | false ->
            ErrorMessage.ofIParamCollection $"does not satisfy the requirements" paramCollection
            |> Expecto.Tests.failtestNoStackf "%s"


// Validation Cases:
let investigationCases = 
    testList INVMSO.``Investigation Metadata``.INVESTIGATION.key.Name [
        // Investigation has title
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``.Name) {
            investigationMetadata
            |> Validate.ParamCollection.ContainsNonKeyParamWithTerm
                INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``
        }

        // Investigation has description
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``.Name) {
            investigationMetadata
            |> Validate.ParamCollection.ContainsNonKeyParamWithTerm
                INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``
        }

        // Investigation has contacts with name, last name, affiliation and email
        // Investigation Person First Name
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``.Name} exists") {
            investigationMetadata
            |> Validate.ParamCollection.ContainsNonKeyParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``
        }
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``.Name} is not empty") {
            investigationMetadata
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``)
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }

        // Investigation Person Last Name
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``.Name} exists") {
            investigationMetadata
            |> Validate.ParamCollection.ContainsNonKeyParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``
        }
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``.Name} is not empty") {
            investigationMetadata
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``)
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }

        // Investigation Person Affiliation
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``.Name} exists") {
            investigationMetadata
            |> Validate.ParamCollection.ContainsNonKeyParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``
        }
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``.Name} is not empty") {
            investigationMetadata
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``)
            |> Seq.filter (Param.getValueAsString >> (<>) "Metadata Section Key")
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }

        // Investigation Person Email
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``.Name} exists") {
        investigationMetadata
        |> Validate.ParamCollection.ContainsNonKeyParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Investigation Person Email``.Name) {
            investigationMetadata
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``)
            |> Seq.filter (Param.getValueAsString >> (<>) "Metadata Section Key")
            |> Seq.iter (Validate.Param.ValueMatchesRegex StringValidationPattern.email)
        }

        // Investigation hast Keywords comment
        //ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.INVESTIGATION.key}.Keywords exists") {
        //    investigationMetadata
        //    //|> Validate.ParamCollection.ContainsNonKeyParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``
        //    |> Validate.ParamCollection.ContainsParamWithTerm (CvTerm.create "Keywords")
        //}
    ]

let studyCases =
    testList STDMSO.``Study Metadata``.STUDY.key.Name [
        // Study has protocol in correct format
        ARCExpect.validationCase (TestID.Name STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.key.Name) {
            studyMetadata
            |> Validate.ParamCollection.ContainsParamWithTerm STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.key
        }

        // Study protocol is in correct format
        ARCExpect.validationCase (TestID.Name "STUDY PROTOCOLS description") {
            studyMetadata|> Seq.filter (fun iparam -> Param.getTerm iparam = STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.``Study Protocol Description``)
            |> Seq.filter (Param.getValueAsString >> (<>) "Metadata Section Key")
            |> Seq.iter (Validate.Param.ValueMatchesRegex (characterLimit (Some 50) (Some 500)))
        }

        // Study has tissue header in process graph
        ARCExpect.validationCase (TestID.Name "Tissue") {
            tissueTokens
            |> Validate.ParamCollection.SatisfiesPredicate (
                fun iparams ->
                    iparams
                    |> List.exists (fun iparam -> iparam.Value = ParamValue.CvValue (CvTerm.create("NCIT:C12801","Tissue","NCIT")))
            )
        }

        // Study has tissue values as terms (CvValues)
        ARCExpect.validationCase (TestID.Name "Tissue terms") {
            tissueTokens
            |> Validate.ParamCollection.AllTermsSatisfyPredicate (fun ip -> match ip.Value with CvValue _ -> true | _ -> false)
        }

        // Study has species in correct format
        ARCExpect.validationCase (TestID.Name "Organism") {
            organismTokens
            |> Validate.ParamCollection.SatisfiesPredicate (
                fun iparams ->
                    iparams
                    |> List.exists (fun iparam -> iparam.Value = ParamValue.CvValue (CvTerm.create("OBI:0100026","organism","OBI")))
            )
        }

        // Study has species values as terms (CvValues)
        ARCExpect.validationCase (TestID.Name "Organism terms") {
            tissueTokens
            |> Validate.ParamCollection.AllTermsSatisfyPredicate (fun ip -> match ip.Value with CvValue _ -> true | _ -> false)
        }
    ]

let assayCases =
    testList ASSMSO.``Assay Metadata``.ASSAY.key.Name [
        // Assay has protocol
        //ARCExpect.validationCase (TestID.Name techTypeTAN.Name) {
        //    assayMetadata
        //    |> Validate.ParamCollection.ContainsParamWithTerm techTypeTAN
        //}

        // Assay has protocol in correct format
        //ARCExpect.validationCase (TestID.Name "ASSAY PROTOCOLS description") {
        //    assayMetadata
        //    |> Seq.filter (fun iparam -> Param.getTerm iparam = techTypeTAN)
        //    |> Seq.filter (Param.getValueAsString >> (<>) "Metadata Section Key")
        //    |> Seq.iter (Validate.Param.ValueMatchesRegex (characterLimit (Some 50) (Some 500)))
        //}

        // Assay has technology type in correct format
        ARCExpect.validationCase (TestID.Name techTypeTAN.Name) {
            assayMetadata
            |> Validate.ParamCollection.ContainsParamWithTerm techTypeTAN
        }
        ARCExpect.validationCase (TestID.Name techTypeName.Name) {
            assayMetadata
            |> Validate.ParamCollection.ContainsParamWithTerm techTypeName
        }
        ARCExpect.validationCase (TestID.Name techTypeTSR.Name) {
            assayMetadata
            |> Validate.ParamCollection.ContainsParamWithTerm techTypeTSR
        }

        // Assay has instrument model
        ARCExpect.validationCase (TestID.Name "instrument model") {
            instrumentTokens
            |> Validate.ParamCollection.SatisfiesPredicate (
                fun iparams ->
                    iparams
                    |> List.exists (fun iparam -> iparam.Value = ParamValue.CvValue (CvTerm.create("MS:1000031","instrument model","MS")))
            )
        }

        // Assay has instrument model in correct format
        ARCExpect.validationCase (TestID.Name "instrument model terms") {
            instrumentTokens
            |> Validate.ParamCollection.AllTermsSatisfyPredicate (fun ip -> match ip.Value with CvValue _ -> true | _ -> false)
        }

        // Assay has fixed or variable modification
        ARCExpect.validationCase (TestID.Name "modification") {
            modTokens
            |> Validate.ParamCollection.SatisfiesPredicate (
                fun iparams ->
                    iparams
                    |> List.exists (
                        fun iparam -> 
                            iparam.Value = ParamValue.CvValue (CvTerm.create("MS:1003021","Fixed modification","MS")) ||
                            iparam.Value = ParamValue.CvValue (CvTerm.create("MS:1003022","Variable modification","MS"))
                    )
            )
        }

        // Assay has fixed or variable modification and values are not empty
        ARCExpect.validationCase (TestID.Name "modification values") {
            modTokens
            |> Validate.ParamCollection.SatisfiesPredicate (
                fun iparams ->
                    iparams
                    |> List.forall (
                        fun iparam -> 
                            iparam.Value 
                            |> ParamValue.getValueAsString
                            <> System.String.Empty
                    )
            )
        }
    ]


// Execution:

Setup.ValidationPackage(
    metadata = Setup.Metadata(PACKAGE_METADATA),
    CriticalValidationCases = [investigationCases; studyCases; assayCases]
)
|> Execute.ValidationPipeline(
    basePath = arcDir
)