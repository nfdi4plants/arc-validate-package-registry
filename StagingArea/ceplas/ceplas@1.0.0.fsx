let [<Literal>]PACKAGE_METADATA = """(*
---
Name: ceplas-experimental
Summary: Validates whether the ARC contains the minimal metadata to meet the CEPLAS quality criteria for a typical experimental ARC.
Description: |
    ## Critical
        - ARC contains README
        - ARC contains any LICENSE file
        - Investigation contains title
        - Investigation contains description
        - Investigation contains contact
        - Investigation contacts contain first name, last name, email, affiliation, ORCID
        - ARC contains at least one study or one assay
        - Every study and assay must contain at least one annotation table
        - ARC contains annotated "raw" data (e.g. raw dataset file or URL)
    ## Non-Critical
        - Every study and assay contains top-level metadata
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
  - Name: experimental
  - Name: quality-arc
ReleaseNotes: |
  - initial release
---
*)"""

#r "nuget: ARCExpect.Core, 7.0.0-alpha"
#r "nuget: ARCtrl, 3.0.0"
#r "nuget: ARCtrl.QueryModel, 3.0.0-alpha.2"
#r "nuget: Expecto"
#r "nuget: Fable.SimpleHttp"

// open ControlledVocabulary
open ARCtrl
open ARCtrl.QueryModel
open Expecto
open ARCExpect
// open ARCTokenization
// open ARCTokenization.StructuralOntology
open System.IO
open Fable.SimpleHttp
open System.Text
// open FSharpAux


let pathIsUrl (p: string) =
    p.StartsWith("http:") || p.StartsWith("https:")

type URLStatus =
    | Malformed
    | Resolves
    | Fails

let urlResolves (url: string) =

    async {
        try
            let! (statusCode, responseText) = Http.get url

            match statusCode with
            | 200 -> return Resolves
            | _ -> return Fails

        with
            | _ -> return Malformed
    }
    |> Async.RunSynchronously



// // Identify loodr tables / determine table connections
// // based on at least one intersecting Input/Output reference
// let isPreviousProcessOf (processA: ArcTable) (processB: ArcTable) : bool =    
//     match processB.TryGetInputColumn() with
//         | Some _ -> 
//             match processA.TryGetOutputColumn() with 
//             | Some _ -> 
//                 Set.intersect (set processA.OutputNames) (set processB.InputNames)
//                     |> Seq.length
//                     |> fun x -> x > 0
//             | None -> 
//                 printfn "%s" $"INFO: No Output column found in {processA.Name}"
//                 false
//         | None -> 
//             printfn "%s" $"INFO: No Input column found in {processB.Name}"
//             false





// Input:

let arcDir = Directory.GetCurrentDirectory()

////////////////////////

let home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile)
let arcDir = home + "/datahub-dataplant/Facultative-CAM-in-Talinum"
////////////////////////

let arc = ARC.load arcDir   


arc.DataContextMapping()



// Values:

let criticalCases =     
    testList "criticalCases" [
    
    ////////////////////////////////////
    ////// ARC root
    ////////////////////////////////////

    // TestCase Critical: ARC contains README

    testCase "ARC contains README" <| fun _ ->
        
        let readmeNames = ["README.md"; "readme.md"; "README.txt"; "readme.txt"; "Readme.md"; "readme"; "README"]

        let containsReadme = 

            readmeNames
            |> List.exists (fun n -> 
                let p = Path.Combine(arcDir, n)
                File.Exists p        
            )

        if not containsReadme then
            failwithf "ARC does not contain README in any of the given paths: %O" readmeNames

    // TestCase Critical: ARC contains any LICENSE file

    testCase "ARC contains any LICENSE file" <| fun _ ->
        
        let licenseNames = ["LICENSE.md"; "license.md"; "LICENSE.txt"; "license.txt"; "License.md"; "license"; "LICENSE";
                            "LICENCE.md"; "licence.md"; "LICENCE.txt"; "licence.txt"; "Licence.md"; "licence"; "LICENCE"]

        let containsLicense = 

            licenseNames
            |> List.exists (fun n -> 
                let p = Path.Combine(arcDir, n)
                File.Exists p        
            )

        if not containsLicense then
            failwithf "ARC does not contain LICENSE file in any of the given paths: %O" licenseNames

    ////////////////////////////////////
    ////// ARC Investigation
    ////////////////////////////////////        

    // TestCase Critical: Investigation contains title

    testCase $"Investigation {arc.Identifier} contains title" <| fun _ ->
        // Investigation title exists
        if arc.Title.IsNone then
            failwith $"Investigation {arc.Identifier} contains no title"
        // Investigation title is longer than 3 characters
        if arc.Title.Value.Length < 4 then
            failwith $"Investigation {arc.Identifier} contains no meaningful title (i.e. longer than 3 characters):\"{arc.Title.Value}\""       

    // TestCase Critical: Investigation contains description

    testCase $"Investigation {arc.Identifier} contains description" <| fun _ ->
        // Investigation description exists
        if arc.Description.IsNone then
            failwith $"Investigation {arc.Identifier} contains no description"
        // Investigation description is longer than 30 characters
        if arc.Description.Value.Length < 31 then
            failwith $"Investigation {arc.Identifier} contains no meaningful description (i.e. longer than 30 characters):\"{arc.Description.Value}\""

    // TestCase Critical: Investigation contains contact

    testCase $"Investigation {arc.Identifier} contains contact" <| fun _ ->
        if arc.Contacts.Count = 0 then
            failwith $"Investigation {arc.Identifier} contains no contact"
    
    // TestCase Critical: Investigation contacts contain first name, last name, email, affiliation, ORCID
        
    for c in arc.Contacts do

        testCase $"Contact contains first name" <| fun _ ->
            if c.FirstName.IsNone then
                failwith $"Contact contains no first name"

        testCase $"Contact contains last name" <| fun _ ->
            if c.LastName.IsNone then
                failwith $"Contact contains no last name"

        testCase $"Contact contains email" <| fun _ ->
            if c.EMail.IsNone then
                failwith $"Contact contains no email"
        
        testCase $"Contact contains affiliation" <| fun _ ->
            if c.Affiliation.IsNone then
                failwith $"Contact contains no affiliation"
        
        testCase $"Contact contains ORCID" <| fun _ ->
            if c.ORCID.IsNone then
                failwith $"Contact contains no ORCID"


    ////////////////////////////////////
    ////// ARC Study + Assay
    ////////////////////////////////////    
    
    // TestCase Critical: ARC contains at least one study or one assay

    testCase "ARC contains at least one study or one assay" <| fun _ ->

        if arc.StudyCount + arc.AssayCount = 0 then
            failwith "ARC does not contain any study or assay"

    // TestCase Critical: Every study and assay must contain at least one annotation table

    //// Studies
    for s in arc.Studies do
        
        // Study contains annotation table
        testCase $"Study {s.Identifier} contains annotation table" <| fun _ ->
            if s.TableCount = 0 then
                failwith $"Study {s.Identifier} contains no annotation table"
        
        // Study contains annotation table with more than 2 columns and 0 rows
        for t in s.Tables do
            testCase $"Table {t.Name} of study {s.Identifier} contains basic information" <| fun _ ->
                
                if t.ColumnCount < 2 then
                    failwith $"Table {t.Name} contains less than 2 columns"
                if t.RowCount = 0 then
                    failwith $"Table {t.Name} contains no rows"
    
    //// Assays
    for a in arc.Assays do
        // Assay contains annotation table
        testCase $"Assay {a.Identifier} contains annotation table" <| fun _ ->
            if a.TableCount = 0 then
                failwith $"Assay {a.Identifier} contains no annotation table"

        // Assay contains annotation table with more than 2 columns and 0 rows
        for t in a.Tables do
            testCase $"Table {t.Name} of assay {a.Identifier} contains basic information" <| fun _ ->
                
                if t.ColumnCount < 2 then
                    failwith $"Table {t.Name} contains less than 2 columns"
                if t.RowCount = 0 then
                    failwith $"Table {t.Name} contains no rows"
    
    // TestCase Critical: ARC contains annotated "raw" data (e.g. raw dataset file or URL)
        
        // data entity should resolve
            // 1. annotation resolves local file
            // 2. if not local (./dataset), resolves URL
        // TODO: data entity should be annotated with at least one of Characteristic, Parameter, Factor

    testCase "ARC contains annotated data entities" <| fun _ ->
        if arc.ArcTables.Data.Count = 0 then
            failwith "ARC contains no annotated data entities"
    
    // Reminder: this is for an "experimental ARC", hence only checking for data entities in assays

    for a in arc.Assays do
        for d in a.Data |> Seq.distinctBy (fun d -> d.Name) do

            let filePath = if d.FilePath = "" then d.Name else d.FilePath 

            testCase $"Data path {filePath} of assay {a.Identifier} resolves to local file or folder or a URL" <| fun _ ->

                // Check whether path (i.e. Output [Data]) resolves to URL

                if pathIsUrl filePath then
                    match urlResolves filePath with
                    | Resolves -> ()
                    | Fails -> failwith $"Url {filePath} in assay {a.Identifier} could not be resolved"
                    | Malformed -> failwith $"Url {filePath} in assay {a.Identifier} is malformed"

                else

                // Check whether path (i.e. Output [Data]) resolves to local file / folder

                    let p = d.DataContext.Value.GetAbsolutePathForAssay(a.Identifier)
                    let fullPath = Path.Combine(arcDir, p)

                    if (File.Exists fullPath || Directory.Exists fullPath) |> not then
                            failwith $"Data path {filePath} does not resolve to existing local file or folder and was not identified as URL"

    ]
    

let nonCriticalCases =
    testList "nonCriticalCases" [

    // process graph: I/O connections (sample-sample-material-data)
    // ARC should not have non-connected annotation tables


    testCase "ARC contains annotated data entities" <| fun _ ->
        if arc.ArcTables.Data.Count = 0 then
            failwith "ARC contains no annotated data entities"  
    
    
    // every data entity should be derived from a Source or Sample


    ////////////////////////////////////
    ////// ARC Study + Assay
    ////////////////////////////////////
    
    // TestCase Non-Critical: Every study and assay contains top-level metadata

    //// Studies
    
    for s in arc.Studies do
        // Study contains useful title
        testCase $"Study {s.Identifier} contains title" <| fun _ ->
            // Study title exists
            if s.Title.IsNone then
                failwith $"Study {s.Identifier} contains no title"
            // Study title is longer than 3 characters
            if s.Title.Value.Length < 4 then
                failwith $"Study {s.Identifier} contains no meaningful title (i.e. longer than 3 characters):\"{s.Title.Value}\""
        
        // Study contains useful description
        testCase $"Study {s.Identifier} contains description" <| fun _ ->
            // Study description exists
            if s.Description.IsNone then
                failwith $"Study {s.Identifier} contains no description"
            // Study description is longer than 30 characters
            if s.Description.Value.Length < 31 then
                failwith $"Study {s.Identifier} contains no meaningful description (i.e. longer than 30 characters):\"{s.Description.Value}\""

        // Study contains contacts
        testCase $"Study {s.Identifier} contains contacts" <| fun _ ->
            if s.Contacts.Count < 1 then
                failwith $"Study {s.Identifier} contains no contacts"

    //// Assays

    for a in arc.Assays do

        // Assay contains useful title
        testCase $"Assay {a.Identifier} contains title" <| fun _ ->
            // Assay title exists
            if a.Title.IsNone then
                failwith $"Assay {a.Identifier} contains no title"
            // Assay title is longer than 4 characters
            if a.Title.Value.Length < 4 then
                failwith $"Assay {a.Identifier} contains no meaningful title (i.e. longer than 3 characters):\"{a.Title.Value}\""
        
        // Assay contains useful description
        testCase $"Assay {a.Identifier} contains description" <| fun _ ->
            // Assay description exists
            if a.Description.IsNone then
                failwith $"Assay {a.Identifier} contains no description"
            // Assay description is longer than 30 characters
            if a.Description.Value.Length < 31 then
                failwith $"Assay {a.Identifier} contains no meaningful description (i.e. longer than 30 characters):\"{a.Description.Value}\""

        // Assay contains performers
        testCase $"Study {a.Identifier} contains contacts" <| fun _ ->
            if a.Performers.Count < 1 then
                failwith $"Study {a.Identifier} contains no performers"

        // Assay contains measurement type
        testCase $"Assay {a.Identifier} contains top-level metadata measurement type" <| fun _ ->
            if a.MeasurementType.IsNone then
                failwith $"Assay {a.Identifier} contains no top-level metadata measurement type"
        
        // Assay contains technology type
        testCase $"Assay {a.Identifier} contains top-level metadata technology type" <| fun _ ->
            if a.TechnologyType.IsNone then
                failwith $"Assay {a.Identifier} contains no top-level metadata technology type"
        
        // Assay contains technology platform
        testCase $"Assay {a.Identifier} contains top-level metadata technology platform" <| fun _ ->
            if a.TechnologyPlatform.IsNone then
                failwith $"Assay {a.Identifier} contains no top-level metadata technology platform"


    // every annotation table should contain Input, Output, ProtocolUri


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


//// run tests locally

runTestsWithCLIArgs [] [||] criticalCases
runTestsWithCLIArgs [] [||] nonCriticalCases


for i in arc.ArcTables do
    printf "%s" i.Name

    let currentIOs = set [i.InputNames, i.OutputNames]

    currentIOs
    |> Seq.iter (fun x -> printf "%s" x.ToString)

    
Set.difference




nonCriticalCases

//// TODO
/// - simple read out testCases for description



// let a = arc.Assays[0]

// let d = a.Data[0]

// let p = d.DataContext.Value.GetAbsolutePathForAssay(a.Identifier)

// p