let [<Literal>]PACKAGE_METADATA = """(*
---
Name: invenio
Summary: Validates if the ARC contains the necessary metadata to be publishable via Invenio.
Description: |
  Validates if the ARC contains the necessary metadata to be publishable via Invenio.
  The following metadata is required:
  - Investigation has title and description
  - All persons in Investigation Contacts must have a name, last name, affiliation, valid email, and valid ORCID
  License file is required.
MajorVersion: 3
MinorVersion: 2
PatchVersion: 0
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
  - Only one contact is required to have E-Mail, ORCID and affiliation, not all contacts. https://github.com/orgs/nfdi4plants/discussions/10
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

// Validation Cases:
let cases = 
    testList INVMSO.``Investigation Metadata``.INVESTIGATION.key.Name [
        // Investigation has title
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``.Name} exists") {
            imd
            |> Validate.ParamCollection.ContainsNonKeyParamWithTerm
                INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``
        }
        // Investigation title is not empty
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``.Name} is not empty") {
            imd
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``)
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }
        // Investigation has description
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``.Name} exists") {
            imd
            |> Validate.ParamCollection.ContainsNonKeyParamWithTerm
                INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``
        }
        // Investigation description is not empty
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``.Name} is not empty") {
            imd
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``)
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }
        // Investigation has contacts with name, last name, affiliation and email
        // Investigation Person First Name
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``.Name} exists") {
            filledFirstNames
            |> Validate.ParamCollection.ContainsNonKeyParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``
        }
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``.Name} is not empty") {
            filledFirstNames
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``)
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }
        // Investigation Person Last Name
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``.Name} exists") {
            filledLastNames
            |> Validate.ParamCollection.ContainsNonKeyParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``
        }
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``.Name} is not empty") {
            filledLastNames
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``)
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }

        // Investigation Person Email
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Investigation Person Email``.Name} is valid") {
            filledEmails
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``)
            |> Seq.filter (Param.getValueAsString >> (<>) "Metadata Section Key")
            |> Seq.filter (Param.getValueAsString >> (<>) "")
            |> Seq.iter (Validate.Param.ValueMatchesRegex StringValidationPattern.email)
        }
        // Investigation Person ORCID
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Comment[ORCID]``.Name} is valid") {
            filledOrcids
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Comment[ORCID]``)
            |> Seq.filter (Param.getValueAsString >> (<>) "Metadata Section Key")
            |> Seq.filter (Param.getValueAsString >> (<>) "")
            |> Seq.iter (Validate.Param.ValueMatchesRegex StringValidationPattern.orcid)
        }
        ARCExpect.validationCase (TestID.Name "There is at least one contact which contains E-Mail, ORCID and affiliation") {
            if filledFirstNames.Length = 1 then
                failwith "There are zero contacts"
            else 
                let mutable hasCompleteContact = false
                for i = 1 to filledFirstNames.Length - 1 do
                    let hasOrcid = filledOrcids.[i] |> Param.getValueAsString |> (<>) ""
                    let hasEmail = filledEmails.[i] |> Param.getValueAsString |> (<>) ""
                    let hasAffiliation = filledAffiliations.[i] |> Param.getValueAsString |> (<>) ""
                    if hasOrcid && hasEmail && hasAffiliation then 
                        hasCompleteContact <- true
                if not hasCompleteContact then
                    failwith "There is no contact with E-Mail, ORCID and affiliation"
        }
        // License file exists
        ARCExpect.validationCase (TestID.Name "License exists") {
            absoluteDirectoryPaths
            |> Validate.ParamCollection.SatisfiesPredicate (
                Seq.exists (
                    Param.getValueAsString 
                    >> fun (v : string) -> 
                        v.ToLower()
                        |> fun lowerV -> lowerV = "license" || lowerV = "license.md"
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