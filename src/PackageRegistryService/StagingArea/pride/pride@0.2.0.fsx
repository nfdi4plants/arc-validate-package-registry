(*
---
Name: pride
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
PatchVersion: 0
Publish: true
Authors:
  - FullName: Oliver Maus
    Email: maus@nfdi4plants.org
    Affiliation: RPTU Kaiserslautern
    AffiliationLink: http://rptu.de/startseite
Tags:
  - validation
  - pride
  - proteomics
ReleaseNotes: "
  - initial release
  - metadata validation added:
    - Investigation has title and description
    - Investigation has Keywords comment in correct format
    - All persons in Investigation Contacts must have a first name, last name, affiliation and valid email
    - Study has protocol, tissue & species in correct format
    - Assay has protocol, technology type, instrument model, and fixed and/or variable modification in correct format
    "
---
*)

#r "nuget: ARCExpect"
#r "nuget: Anybadge.NET"
#r "nuget: ARCValidationPackages"
#r "nuget: FSharpAux"

open ARCExpect
open ARCTokenization
open ARCTokenization.StructuralOntology
open ControlledVocabulary
open Expecto
open ARCValidationPackages
open ARCValidationPackages.API
open System.IO
open FSharpAux


// Input:

let arcDir = @"C:\Repos\git.nfdi4plants.org\ArcPrototype"
let outDir = arcDir
let outDirBadge = Path.Combine(arcDir, "Invenio_badge.svg")
let outDirResXml = Path.Combine(arcDir, "Invenio_results.xml")


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

Investigation.parseMetadataSheetsFromTokens() absoluteFilePaths |> List.concat |> Seq.iter (Param.getCvName >> printfn "%s")
Investigation.parseMetadataSheetsFromTokens() absoluteFilePaths |> List.concat |> Seq.iter (Param.getTerm >> printfn "%A")

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

let commis =
    invFileTokensNoMdSecKeys
    |> Seq.filter (Param.getTerm >> (=) Terms.StructuralTerms.userComment)

let stdFileProcTokens =
    absoluteFilePaths
    |> Seq.choose (
        fun cvp ->
            let cvpV = CvParam.getValueAsString cvp
            if String.contains "isa.study.xlsx" cvpV then
                ARCTokenization.Study.parseProcessGraphColumnsFromFile cvpV
                |> Some
            else None
    )

let stdFileMdsTokens =
    Study.parseMetadataSheetsFromTokens () absoluteFilePaths
    |> List.concat
    |> ARCGraph.fillTokenList Terms.StudyMetadata.ontology
    |> Seq.concat
    |> Seq.concat
    |> Seq.map snd

let stdFileMdsTokensNoMdSecKeys =
    stdFileMdsTokens
    |> Seq.filter (Param.getValue >> (<>) Terms.StructuralTerms.metadataSectionKey.Name) 

let stdProtocols =
    stdFileMdsTokensNoMdSecKeys
    |> Seq.filter (Param.getTerm >> (=) STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.key)

let allStdGraphTokens = 
    stdFileProcTokens
    |> Seq.collect Map.values
    |> List.concat
    |> List.concat

let organismTokens =
    stdFileProcTokens
    |> Seq.collect Map.values
    |> List.concat
    |> List.tryFind (fun cvpList -> cvpList.Head |> Param.getValueAsTerm = (CvTerm.create("OBI:0100026","organism","OBI")))
    |> Option.defaultValue []

let tissueTokens =
    stdFileProcTokens
    |> Seq.collect Map.values
    |> List.concat
    |> List.tryFind (fun cvpList -> cvpList.Head |> Param.getValueAsTerm = (CvTerm.create("NCIT:12801","Tissue","NCIT")))
    |> Option.defaultValue []

let assFileProcTokens =
    absoluteFilePaths
    |> Seq.choose (
        fun cvp ->
            let cvpV = CvParam.getValueAsString cvp
            if String.contains "isa.assay.xlsx" cvpV then
                ARCTokenization.Assay.parseProcessGraphColumnsFromFile cvpV
                |> Some
            else None
    )

let assFileMdsTokens =
    Assay.parseMetadataSheetsFromTokens () absoluteFilePaths
    |> List.concat
    |> ARCGraph.fillTokenList Terms.AssayMetadata.ontology
    |> Seq.concat
    |> Seq.concat
    |> Seq.map snd

let techTypeName = CvTerm.create("ASSMSO:00000011", "Assay Technology Type", "ASSMSO")
let techTypeTAN = CvTerm.create("ASSMSO:00000013", "Assay Technology Type Term Accession Number", "ASSMSO")
let techTypeTSR = CvTerm.create("ASSMSO:00000015", "Assay Technology Type Term Source REF", "ASSMSO")

let assFileMdsTokensNoMdSecKeys =
    assFileMdsTokens
    |> Seq.filter (Param.getValue >> (<>) Terms.StructuralTerms.metadataSectionKey.Name) 

let assTechTypeTAN =
    stdFileMdsTokensNoMdSecKeys
    |> Seq.filter (Param.getTerm >> (=) techTypeTAN)

let assTechTypeTSR =
    stdFileMdsTokensNoMdSecKeys
    |> Seq.filter (Param.getTerm >> (=) techTypeTSR)

let assTechTypeName =
    stdFileMdsTokensNoMdSecKeys
    |> Seq.filter (Param.getTerm >> (=) techTypeName)

let allAssGraphTokens = 
    assFileProcTokens
    |> Seq.collect Map.values
    |> List.concat
    |> List.concat


// Helper functions (to deposit in ARCExpect later):

let characterLimit (lowerLimit : int option) (upperLimit : int option) =
    match lowerLimit, upperLimit with
    | None, None -> System.Text.RegularExpressions.Regex(@"^.{0,}$")
    | Some ll, None -> System.Text.RegularExpressions.Regex($"^.{{{ll},}}$")
    | None, Some ul -> System.Text.RegularExpressions.Regex($"^.{{0,{ul}}}$")
    | Some ll, Some ul -> System.Text.RegularExpressions.Regex($"^.{{{ll},{ul}}}$")


open System.Text

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


// Validation Cases:

let cases = 
    testList "cases" [  // naming is difficult here
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``.Name) {
            invFileTokensNoMdSecKeys
            |> Validate.ParamCollection.ContainsParamWithTerm
                INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``.Name) {
            invFileTokensNoMdSecKeys
            |> Validate.ParamCollection.ContainsParamWithTerm
                INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``.Name) {
            contactsFns
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``.Name) {
            contactsLns
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``.Name) {
            contactsAffs
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``.Name) {
            contactsEmails
            |> Validate.ParamCollection.ContainsParamWithTerm INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Investigation Person Email``.Name) {
            contactsEmails
            |> Seq.iter (Validate.Param.ValueMatchesRegex StringValidationPattern.email)
        }
        // missing: how to get specific comment? (here: Keywords Comment)
        //ARCExpect.validationCase (TestID.Name "Comment: Keywords") {
        //    commis
        //    |> Seq.iter (Validate.Param.ValueMatchesRegex StringValidationPattern.email)    // needs special Regex
        //}
        ARCExpect.validationCase (TestID.Name STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.key.Name) {
            stdProtocols
            |> Validate.ParamCollection.ContainsParamWithTerm STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.key
        }
        ARCExpect.validationCase (TestID.Name STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.key.Name) {
            stdProtocols
            |> Seq.iter (Validate.Param.ValueMatchesRegex (characterLimit (Some 50) (Some 500)))
        }
        ARCExpect.validationCase (TestID.Name "organism") {
            allStdGraphTokens
            |> Validate.ParamCollection.ContainsParamWithTerm (CvTerm.create("OBI:0100026","organism","OBI"))
        }
        ARCExpect.validationCase (TestID.Name "organism terms") {
            organismTokens
            |> Validate.ParamCollection.forAll (fun ip -> match ip.Value with CvValue _ -> true | _ -> false)
        }
        ARCExpect.validationCase (TestID.Name "Tissue") {
            allStdGraphTokens
            |> Validate.ParamCollection.ContainsParamWithTerm (CvTerm.create("NCIT:12801","Tissue","NCIT"))
        }
        ARCExpect.validationCase (TestID.Name "Tissue terms") {
            tissueTokens
            |> Validate.ParamCollection.forAll (fun ip -> match ip.Value with CvValue _ -> true | _ -> false)
        }
        ARCExpect.validationCase (TestID.Name techTypeName.Name) {
            assTechTypeName
            |> Validate.ParamCollection.ContainsParamWithTerm techTypeName
        }
        ARCExpect.validationCase (TestID.Name techTypeTAN.Name) {
            assTechTypeTAN
            |> Validate.ParamCollection.ContainsParamWithTerm techTypeTAN
        }
        ARCExpect.validationCase (TestID.Name techTypeTSR.Name) {
            assTechTypeTSR
            |> Validate.ParamCollection.ContainsParamWithTerm techTypeTSR
        }
        ARCExpect.validationCase (TestID.Name "Instrument Model") {
            allStdGraphTokens
            |> Validate.ParamCollection.ContainsParamWithTerm (CvTerm.create("MS:1000031","instrument model","MS"))
        }
        ARCExpect.validationCase (TestID.Name "Modification") {
            ARCExpect.either (fun _ ->
                allStdGraphTokens
                |> Validate.ParamCollection.ContainsParamWithTerm (CvTerm.create("MS:1003021","Fixed modification","MS"))
            ) (fun _ ->
                allStdGraphTokens
                |> Validate.ParamCollection.ContainsParamWithTerm (CvTerm.create("MS:1003022","Variable modification","MS"))
            )
        }
    ]


// Execution:

Execute.ValidationPipeline(outDirResXml, outDirBadge, "PRIDE") cases