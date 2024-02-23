(*
---
Name: invenio
Description: |
    Validates if the ARC contains the necessary metadata to be publishable via Invenio.
    The following metadata is required:
        - Investigation has title and description
        - All persons in Investigation Contacts must have a name, last name, affiliation and valid email
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: true
Authors:
    - FullName: Oliver Maus
    - Affiliation: DataPLANT
Tags:
    - ARC
    - data publication
ReleaseNotes: "Initial release"
---
*)

#r "nuget: ARCExpect"
#r "nuget: Anybadge.NET"
#r "nuget: ARCValidationPackages"

open ARCExpect
open ARCTokenization
open ARCTokenization.StructuralOntology
open ControlledVocabulary
open Expecto
open ARCValidationPackages
open ARCValidationPackages.API
open System.IO


// Input:

let arcDir = Directory.GetCurrentDirectory()
let outDirBadge = Path.Combine(arcDir, "invenio_badge.svg")
let outDirResXml = Path.Combine(arcDir, "invenio_results.xml")


// Values:

let absoluteDirectoryPaths = FileSystem.parseAbsoluteDirectoryPaths arcDir
let absoluteFilePaths = FileSystem.parseAbsoluteFilePaths arcDir

let invFileTokens = 
    Investigation.parseMetadataSheetsFromTokens() absoluteFilePaths 
    |> List.concat
    |> ARCGraph.fillTokenList Terms.InvestigationMetadata.ontology
    |> Seq.concat
    |> Seq.concat
    |> Seq.map snd

let invFileTokensNoMdSecKeys =
    invFileTokens
    |> Seq.filter (Param.getValue >> (<>) Terms.StructuralTerms.metadataSectionKey.Name) 

let contactsFns =
    invFileTokensNoMdSecKeys
    |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``)

let contactsLns =
    invFileTokensNoMdSecKeys
    |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``)

let contactsAffs =
    invFileTokensNoMdSecKeys
    |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``)

let contactsEmails =
    invFileTokensNoMdSecKeys
    |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``)


// Validation Cases:

let cases = 
    testList INVMSO.``Investigation Metadata``.INVESTIGATION.key.Name [
        // Investigation has title
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``.Name) {
            invFileTokensNoMdSecKeys
            |> Validate.ParamCollection.ContainsParamWithTerm
                INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``
        }
        // Investigation has description
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``.Name) {
            invFileTokensNoMdSecKeys
            |> Validate.ParamCollection.ContainsParamWithTerm
                INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``
        }
        // All Investigation contacts have a name
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Investigation Person First Name``.Name) {
            contactsFns
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }
        // All Investigation contacts have a last name
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Investigation Person Last Name``.Name) {
            contactsLns
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }
        // All Investigation contacts have an affiliation
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``.Name) {
            contactsAffs
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }
        // All Investigation contacts have a valid email
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Investigation Person Email``.Name) {
            contactsEmails
            |> Seq.iter (Validate.Param.ValueMatchesRegex StringValidationPattern.email)
        }
    ]


// Execution:

Execute.ValidationPipeline(outDirResXml, outDirBadge, "Invenio") cases