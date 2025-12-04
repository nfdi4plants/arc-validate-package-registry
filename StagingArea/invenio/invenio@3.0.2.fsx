let [<Literal>]PACKAGE_METADATA = """(*
---
Name: invenio
Summary: Validates if the ARC contains the necessary metadata to be publishable via Invenio.
Description: |
  Validates if the ARC contains the necessary metadata to be publishable via Invenio.
  The following metadata is required:
  - Investigation has title and description
  - All persons in Investigation Contacts must have a name, last name, affiliation, valid email, and valid ORCID
MajorVersion: 3
MinorVersion: 0
PatchVersion: 2
Publish: true
Authors:
  - FullName: Oliver Maus
    Affiliation: DataPLANT
  - FullName: Christopher Lux
    Email: lux@csbiology.de
    Affiliation: RPTU Kaiserslautern
    AffiliationLink: http://rptu.de/startseite
  - FullName: Lukas Weil
    Email: weil@rptu.de
    Affiliation: DataPLANT
Tags:
  - Name: ARC
  - Name: data publication
ReleaseNotes: |
  - Add test for valid ORCIDs
  - Specify name of checks for valid emails
---
*)"""

#r "nuget: ARCExpect, 5.0.1"

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
    let imd =
        absoluteDirectoryPaths
        |> Investigation.parseMetadataSheetsFromTokens() arcDir 
        |> List.concat 
    // fill each Contact to completeness
    let firstNames = imd |> List.filter (fun cvp -> Param.getCvName cvp = INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``.Name)
    let lastNames = imd |> List.filter (fun cvp -> Param.getCvName cvp = INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``.Name)
    let affiliations = imd |> List.filter (fun cvp -> Param.getCvName cvp = INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``.Name)
    let emails = imd |> List.filter (fun cvp -> Param.getCvName cvp = INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``.Name)
    let orcids = imd |> List.filter (fun cvp -> Param.getCvName cvp = INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Comment[ORCID]``.Name)
    let longestRow =
        [firstNames; lastNames; affiliations; emails; orcids]
        |> List.fold (fun acc cvps -> System.Math.Max(acc, Seq.length cvps)) 0
    let addEmptyValues l emptyValueCvp (cvpList : IParam list) =
        if l = cvpList.Length then
            cvpList
        else 
            cvpList @ (List.init (l - cvpList.Length) (fun _ -> emptyValueCvp))
    let filledFirstNames = addEmptyValues longestRow (CvParam(INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``, Value "")) firstNames
    let filledLastNames = addEmptyValues longestRow (CvParam(INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``, Value "")) lastNames
    let filledAffiliations = addEmptyValues longestRow (CvParam(INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``, Value "")) affiliations
    let filledEmails = addEmptyValues longestRow (CvParam(INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``, Value "")) emails
    let filledOrcids = addEmptyValues longestRow (CvParam(INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Comment[ORCID]``, Value "")) orcids
    let groupedImd = Seq.groupBy (fun cvp -> Param.getCvName cvp) imd
    groupedImd
    |> Seq.collect (
        fun (cvpName, cvps) ->
            match cvpName with
            | x when x = INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``.Name -> filledFirstNames |> Seq.ofList
            | x when x = INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``.Name -> filledLastNames |> Seq.ofList
            | x when x = INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``.Name -> filledAffiliations |> Seq.ofList
            | x when x = INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``.Name -> filledEmails |> Seq.ofList
            | x when x = INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Comment[ORCID]``.Name -> filledOrcids |> Seq.ofList
            | _ -> cvps
    )


// Validation Cases:
let cases = 
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
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Investigation Person Email``.Name} is valid") {
            investigationMetadata
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``)
            |> Seq.filter (Param.getValueAsString >> (<>) "Metadata Section Key")
            |> Seq.iter (Validate.Param.ValueMatchesRegex StringValidationPattern.email)
        }
        // Investigation Person ORCID
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Comment[ORCID]``.Name} exists") {
        investigationMetadata
        |> Validate.ParamCollection.ContainsNonKeyParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Comment[ORCID]``
        }
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Comment[ORCID]``.Name} is valid") {
            investigationMetadata
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Comment[ORCID]``)
            |> Seq.filter (Param.getValueAsString >> (<>) "Metadata Section Key")
            |> Seq.iter (Validate.Param.ValueMatchesRegex StringValidationPattern.orcid)
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