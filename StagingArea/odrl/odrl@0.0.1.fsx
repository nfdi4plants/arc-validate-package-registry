let [<Literal>]PACKAGE_METADATA = """(*
---
Name: odrl
Summary: Validates if the ARC contains the necessary metadata to be specify its terms of use.
Description: |
  Validates if the ARC contains the necessary metadata to specify its terms of use.
  - 
MajorVersion: 0
MinorVersion: 0
PatchVersion: 1
Publish: true
Authors:
  - FullName: Lukas Weil
    Email: weil@rptu.de
    Affiliation: RPTU Kaiserslautern
    AffiliationLink: http://rptu.de/startseite

Tags:
  - Name: ARC
  - Name: ODRL
  - Name: Policy
  - Name: License
ReleaseNotes: |
  - Fix package passing validation against Investigation file containing only section keys
---
*)"""


#r "nuget: ARCExpect, 2.0.0"

open ARCExpect
open System.Xml
open System.IO
open Expecto

open System.Threading.Tasks
open System.Net.Http
open System.Text
    
module ValidationResult = 
    
    let fromJUnitString (s : string) =
        
        let doc = new XmlDocument()
        let stream = new System.IO.StringReader(s)
        doc.Load(stream)
        let suite = doc.SelectNodes("/testsuites/testsuite[@name='odrl validation']").Item(0);
        let testCases = suite.SelectNodes("testcase") |> Seq.cast<XmlNode>
        let passedTests =
            testCases
            |> Seq.filter (fun tc -> tc.SelectNodes("failure").Count = 0)
            |> Seq.map (fun tc -> tc.Attributes.["name"].Value)
            |> Seq.toList
            |> List.sort
            |> List.length
        let failedTests = 
            testCases
            |> Seq.filter (fun tc -> tc.SelectNodes("failure").Count > 0)
            |> Seq.map (fun tc -> tc.Attributes.["name"].Value)
            |> Seq.toList
            |> List.sort
            |> List.length
        let erroredTests =
            testCases
            |> Seq.filter (fun tc -> tc.SelectNodes("error").Count > 0)
            |> Seq.map (fun tc -> tc.Attributes.["name"].Value)
            |> Seq.toList
            |> List.sort
            |> List.length
        
        ValidationResult.create((passedTests + failedTests + erroredTests),passedTests,failedTests,erroredTests)

    let empty = 
        ValidationResult.create(0,0,0,0)


let validationServiceURL = "https://odrl.gi.denbi.de/api/validate"
let rendererServiceURL = "https://odrl.gi.denbi.de/api/render"


let arcDir = Directory.GetCurrentDirectory()
let odrlFilePath = Path.Combine(arcDir, "odrl.json")


let odrlFileExists = File.Exists odrlFilePath 

let tryPostStringAsync (url: string) (jsonString: string) : Task<Result<string,string>> =
    task {
        use httpClient = new HttpClient()
        
        // Serialize the string content into JSON format
        //let jsonContent = sprintf "\"%s\"" jsonString // Wrap the string as JSON (a quoted string)
        
        // Create the content with UTF-8 encoding and application/json content type
        let content = new StringContent(jsonString, Encoding.UTF8, "application/json")
        
        try
            // Send the POST request
            let! response = httpClient.PostAsync(url, content)
            
            // Ensure the response status is successful
            response.EnsureSuccessStatusCode() |> ignore
            
            // Read the response as a string
            let! responseBody = response.Content.ReadAsStringAsync()
        
            return (Ok responseBody)
        with
        | ex -> 
            // Handle errors (e.g., network issues, HTTP errors)
            printfn "An error occurred: %s" ex.Message
            return (Error ex.Message)
    }


let tryPostByteArrayAsync (url: string) (jsonString: string) : Task<Result<byte [],string>> =
    task {
        use httpClient = new HttpClient()
        
        // Serialize the string content into JSON format
        //let jsonContent = sprintf "\"%s\"" jsonString // Wrap the string as JSON (a quoted string)
        
        // Create the content with UTF-8 encoding and application/json content type
        let content = new StringContent(jsonString, Encoding.UTF8, "application/json")
        
        try
            // Send the POST request
            let! response = httpClient.PostAsync(url, content)
            
            // Ensure the response status is successful
            response.EnsureSuccessStatusCode() |> ignore
            
            // Read the response as a string
            let! responseBody = response.Content.ReadAsByteArrayAsync()
        
            return (Ok responseBody)
        with
        | ex -> 
            // Handle errors (e.g., network issues, HTTP errors)
            printfn "An error occurred: %s" ex.Message
            return (Error ex.Message)
    }

let metadata = Setup.Metadata(PACKAGE_METADATA)
let foldername = $"{metadata.Name}@{AVPRIndex.Domain.ValidationPackageMetadata.getSemanticVersionString metadata}"
let resultFolder = Path.Combine(arcDir, ".arc-validate-results", foldername)
let offerFilePath = Path.Combine(resultFolder, "offer.pdf")

let handleValidationServiceResult (jUnit : string) = 
    
    let labelText = $"{metadata.Name}@{AVPRIndex.Domain.ValidationPackageMetadata.getSemanticVersionString metadata}"
    
    let summaryPath = Path.Combine(resultFolder, "validation_summary.json")
    let badgePath = Path.Combine(resultFolder, "badge.svg")
    let jUnitPath = Path.Combine(resultFolder, "validation_report.xml")

    let validationPackageSummary = ARCExpect.ValidationPackageSummary.create metadata
    let validationResult = ValidationResult.fromJUnitString jUnit
    let validationSummary = ARCExpect.ValidationSummary.create(validationResult,ValidationResult.empty,validationPackageSummary)

    Directory.CreateDirectory(resultFolder) |> ignore

    validationSummary |> Execute.SummaryCreation(summaryPath)
    File.WriteAllText(jUnitPath,jUnit)
    validationSummary
    |> Execute.BadgeCreation(
        badgePath, 
        labelText
    )
  
let handleError (testName : string ) (errorMessage : string) = 
    let case = 
        testCase testName (fun () -> 
            failwith errorMessage
        )
    Setup.ValidationPackage(
        metadata = Setup.Metadata(PACKAGE_METADATA),
        CriticalValidationCases = [case]
    )
    |> Execute.ValidationPipeline(
        basePath = arcDir
    )

if odrlFileExists then

    let odrlContent = File.ReadAllText odrlFilePath
    let odrlValidationTask = tryPostStringAsync validationServiceURL odrlContent

    match odrlValidationTask.Result with
    | Ok odrlValidationResult -> 
        odrlValidationResult
        |> handleValidationServiceResult

        let odrlPDFTask = tryPostByteArrayAsync rendererServiceURL odrlContent
        match odrlPDFTask.Result with
         | Ok odrlPDFResult ->         
            printfn "success"
            File.WriteAllBytes(offerFilePath,odrlPDFResult)
         | Error errorMessage -> 
            printfn "erorrr"
            ()
            //handleError "ODRL pdf file could not be created" errorMessage

    | Error errorMessage -> 
        handleError "ODRL json file is valid" errorMessage
else
    handleError  "ODRL json file exists" "ODRL json file does not exist"
        