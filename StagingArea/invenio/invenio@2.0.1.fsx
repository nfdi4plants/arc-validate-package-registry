let [<Literal>]PACKAGE_METADATA = """(*
---
Name: invenio
Summary: Validates if the ARC contains the necessary metadata to be publishable via Invenio.
Description: |
  Validates if the ARC contains the necessary metadata to be publishable via Invenio.
  The following metadata is required:
  - Investigation has title and description
  - All persons in Investigation Contacts must have a name, last name, affiliation and valid email
MajorVersion: 2
MinorVersion: 0
PatchVersion: 1
Publish: true
Authors:
  - FullName: Oliver Maus
    Affiliation: DataPLANT
  - FullName: Christopher Lux
    Email: lux@csbiology.de
    Affiliation: RPTU Kaiserslautern
    AffiliationLink: http://rptu.de/startseite
Tags:
  - Name: ARC
  - Name: data publication
ReleaseNotes: |
  - Rework the tokenisation and acess to the metadata in Accordance to ARKTokenization 6.0.0/ARCExpect 2.0.0
---
*)"""

#r "nuget: ARCExpect, 3.0.0"

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


// Validation Cases:
let cases = 
    testList INVMSO.``Investigation Metadata``.INVESTIGATION.key.Name [
        // Investigation has title
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``.Name) {
            investigationMetadata
            |> Validate.ParamCollection.ContainsParamWithTerm
                INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``
        }
        // Investigation has description
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``.Name) {
            investigationMetadata
            |> Validate.ParamCollection.ContainsParamWithTerm
                INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``
        }
        // Investigation has contacts with name, last name, affiliation and email
        // Investigation Person First Name
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``.Name} exists") {
            investigationMetadata
            |> Validate.ParamCollection.ContainsParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``
        }
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``.Name} is not empty") {
            investigationMetadata
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``)
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }
        // Investigation Person Last Name
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``.Name} exists") {
            investigationMetadata
            |> Validate.ParamCollection.ContainsParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``
        }
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``.Name} is not empty") {
            investigationMetadata
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``)
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }
        // Investigation Person Affiliation
        ARCExpect.validationCase (TestID.Name $"{INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``.Name} exists") {
            investigationMetadata
            |> Validate.ParamCollection.ContainsParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``
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
        |> Validate.ParamCollection.ContainsParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Investigation Person Email``.Name) {
            investigationMetadata
            |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``)
            |> Seq.filter (Param.getValueAsString >> (<>) "Metadata Section Key")
            |> Seq.iter (Validate.Param.ValueMatchesRegex StringValidationPattern.email)
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