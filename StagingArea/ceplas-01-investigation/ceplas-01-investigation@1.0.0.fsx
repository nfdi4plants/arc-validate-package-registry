let [<Literal>]PACKAGE_METADATA = """(*
---
Name: ceplas-01-investigation
Summary: Validates whether the ARC contains the minimal metadata to meet the CEPLAS quality criteria only on investigation level.
Description: |
    ## Critical quality criteria
    - ARC contains README
    - ARC contains any LICENSE file
    - Investigation contains title
    - Investigation contains description
    - Investigation contains contact
    - All investigation contacts contain first name and last name
    - At least two investigation contacts contain an affiliation and valid email

    ## Non-critical quality criteria
    - ARC contains README in recommended file format: README.md
    - ARC contains LICENSE file in recommended file format: LICENSE
    - Every investigation contact should have a valid email
    - Every investigation contact should have an affiliation
    - Every investigation contact should have an ORCID
    - At least one investigation contact should have role 'researcher'
    - At least one investigation contact should have role 'principal investigator'
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: true
Authors:
  - FullName: Dominik Brilhaus
    Email: brilhaus@hhu.de
    Affiliation: CEPLAS
    AffiliationLink: https://ceplas.eu
  - FullName: Heinrich Lukas Weil
    Email: weil@nfdi4plants.org
    Affiliation: RPTU Kaiserslautern
    AffiliationLink: http://rptu.de/startseite
Tags:
  - Name: ceplas
  - Name: investigation
  - Name: quality-arc
ReleaseNotes: |
    Release leveled validation packages.
---
*)"""

#r "nuget: ARCExpect.Core, 7.0.0-alpha"
#r "nuget: ARCtrl.QueryModel, 3.0.0-alpha.4"
#r "nuget: Fable.SimpleHttp"

open ARCtrl
open ARCtrl.QueryModel
open Expecto
open ARCExpect
open System.IO

let emailIsValid (email: string) =
    let pattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"
    System.Text.RegularExpressions.Regex.IsMatch(email, pattern)

// Input:

let arcDir = Directory.GetCurrentDirectory()


////////////////////////

let arc =
    try ARC.load arcDir with
    | _ -> ARC(identifier = "placeholder")

arc.MakeDataFilesAbsolute()
arc.DataContextMapping()



let readmeNames =
    set [
        "README"
        "README.md"
        "README.txt"
        "README.rst"
        "README.adoc"
        "README.asciidoc"
        "README.markdown"
        "README.mdown"
        "README.mkd"
        "README.org"
    ]

let readmeNamesLow = readmeNames |> Seq.map (fun n -> n.ToLowerInvariant()) |> set

let containsReadme =
    Directory.EnumerateFiles arcDir 
    |> Seq.map Path.GetFileName
    |> Seq.map (fun n -> n.ToLowerInvariant())
    |> Seq.exists readmeNamesLow.Contains

let readmeNamesOptions =                
    Set.union readmeNames readmeNamesLow
    |> String.concat ", "


let licenseNames =
    set [
        "LICENSE"
        "LICENSE.md"
        "LICENSE.txt"
        "LICENCE"
        "LICENCE.md"
        "LICENCE.txt"
        ]

let licenseNamesLow = licenseNames |> Seq.map (fun n -> n.ToLowerInvariant()) |> set

let containsLicense =
    Directory.EnumerateFiles(arcDir)
    |> Seq.map Path.GetFileName
    |> Seq.map (fun n -> n.ToLowerInvariant())
    |> Seq.exists licenseNamesLow.Contains

let licenseNamesOptions =                
    Set.union licenseNames licenseNamesLow
    |> String.concat ", " 


// Validations

let criticalCases =     
    testList "criticalCases" [

    ////////////////////////////////////
    ////// ARC root
    ////////////////////////////////////

    // TestCase Critical: ARC contains README

    testCase "ARC contains README" <| fun _ ->
        
        Expect.isTrue containsReadme
            $"""ARC does not contain a README. README.md is recommended. Expected one of: {readmeNamesOptions}"""

    // TestCase Critical: ARC contains any LICENSE file

    testCase "ARC contains LICENSE file" <| fun _ ->

        Expect.isTrue containsLicense
            $"""ARC does not contain a LICENSE file. Expected one of: {licenseNamesOptions}"""

    ////////////////////////////////////
    ////// ARC Investigation
    ////////////////////////////////////        

    // TestCase Critical: Investigation contains title

    testCase $"Investigation {arc.Identifier} contains title" <| fun _ ->

        // Investigation title exists        
        Expect.isSome arc.Title
            $"Investigation {arc.Identifier} contains no title"

        // Investigation title is longer than 3 characters
        Expect.isGreaterThan arc.Title.Value.Length 3 
            $"Investigation {arc.Identifier} contains no meaningful title (i.e. longer than 3 characters):\"{arc.Title.Value}\""       

    // TestCase Critical: Investigation contains description

    testCase $"Investigation {arc.Identifier} contains description" <| fun _ ->
        // Investigation description exists
        Expect.isSome arc.Description
            $"Investigation {arc.Identifier} contains no description"
        // Investigation description is longer than 30 characters
        Expect.isGreaterThan arc.Description.Value.Length 30
            $"Investigation {arc.Identifier} contains no meaningful description (i.e. longer than 30 characters):\"{arc.Description.Value}\""

    // TestCase Critical: Investigation contains contact

    testCase $"Investigation {arc.Identifier} contains contact" <| fun _ ->        
        
        Expect.notEqual arc.Contacts.Count 0
            $"Investigation {arc.Identifier} contains no contact"
    
    // TestCase Critical: All investigation contacts contain first name and last name

    for c in arc.Contacts |> Seq.distinctBy (fun c -> (c.FirstName, c.LastName)) do

        let fname = Option.defaultValue "" c.FirstName
        let lname = Option.defaultValue "" c.LastName

        let fullName = $"{fname} {lname}"

        testCase $"Contact {fullName} contains first name" <| fun _ ->
            Expect.isSome c.FirstName
                $"Contact {fullName} contains no first name"

        testCase $"Contact {fullName} contains last name" <| fun _ ->
            Expect.isSome c.LastName
                $"Contact {fullName} contains no last name"

    // TestCase Critical: At least two investigation contacts contain an affiliation and valid email
    
    testCase "At least two investigation contacts contain an affiliation and valid email" <| fun _ ->

        let validContacts =
            arc.Contacts
            |> Seq.filter (fun c ->
                c.EMail.IsSome
                && emailIsValid c.EMail.Value
                && c.Affiliation.IsSome
            )
            |> Seq.length

        Expect.isGreaterThanOrEqual validContacts 2
            $"Expected at least two contacts with a valid email and affiliation, but found {validContacts}."

    ]
    

let nonCriticalCases =
    testList "nonCriticalCases" [
    
    /////////////////////////////////////////////////////////////////
    ////// ARC Root
    /////////////////////////////////////////////////////////////////

    // TestCase Non-critical: ARC contains README in recommended file format: README.md

    if containsReadme then
        testCase "ARC contains README in recommended file format: README.md" <| fun _ ->

            let containsReadmeMd =
                Directory.EnumerateFiles(arcDir)
                |> Seq.map Path.GetFileName
                |> Seq.contains "README.md"

            Expect.isTrue containsReadmeMd
                "ARC contains README file in recommended file format: README.md"

    // TestCase Non-critical: ARC contains LICENSE file in recommended file format: LICENSE

    if containsLicense then

        testCase "ARC contains LICENSE file in recommended file format: LICENSE" <| fun _ ->
            let containsLICENSE =
                Directory.EnumerateFiles(arcDir)
                |> Seq.map Path.GetFileName
                |> Seq.contains "LICENSE"

            Expect.isTrue containsLICENSE
                $"ARC contains LICENSE file in recommended file format: LICENSE"

    /////////////////////////////////////////////////////////////////
    ////// ARC Investigation metadata
    /////////////////////////////////////////////////////////////////

    for c in arc.Contacts |> Seq.distinctBy (fun c -> (c.FirstName, c.LastName)) do
        let fname = Option.defaultValue "" c.FirstName
        let lname = Option.defaultValue "" c.LastName

        let fullName = $"{fname} {lname}"
    
    // TestCase Non-critical: Every investigation contact should have a valid email

        testCase $"Contact {fullName} contains email" <| fun _ ->
            match c.EMail with
            | None ->
                failtest $"Contact {fullName} contains no email"
            | Some email ->
                Expect.isTrue (emailIsValid email) $"'{email}' is not a valid email"
        
    // TestCase Non-critical: Every investigation contact should have an affiliation
        
        testCase $"Contact {fullName} contains affiliation" <| fun _ ->
            Expect.isSome c.Affiliation
                $"Contact {fullName} contains no affiliation"
    
    // TestCase Non-critical: Every investigation contact should have an ORCID
    
        testCase $"Contact {fullName} contains ORCID" <| fun _ ->
            Expect.isSome c.ORCID
                $"Contact {fullName} contains no ORCID"    
    
    // TestCase Non-critical: At least one investigation contact should have role 'researcher'

    let containsResearcher = 
        arc.Contacts
        |> Seq.exists (fun c -> c.Roles |> Seq.exists (fun oa -> 
                oa.NameText = "researcher")
                )
    
    testCase $"At least one investigation contact should have role 'researcher'" <| fun _ ->
        Expect.isTrue containsResearcher
            $"No investigation contact has role 'researcher'"

    // TestCase Non-critical: At least one investigation contact should have role 'principal investigator'
    
    let containsPI = 
        arc.Contacts
        |> Seq.exists (fun c -> c.Roles |> Seq.exists (fun oa -> 
                oa.NameText = "principal investigator")
                )
    
    testCase $"At least one investigation contact should have role 'principal investigator'" <| fun _ ->
        Expect.isTrue containsPI
            $"No investigation contact has role 'principal investigator'"
    
    ]

// Execution:
Setup.ValidationPackage(
    metadata = Setup.Metadata(
        PACKAGE_METADATA,
        AVPRIndex.Frontmatter.FrontmatterLanguage.FSharpFrontmatter
        ),
    CriticalValidationCases = [criticalCases],
    NonCriticalValidationCases = [nonCriticalCases]
)
|> Execute.ValidationPipeline(
    basePath = arcDir
)