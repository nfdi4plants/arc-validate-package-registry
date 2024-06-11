#r "nuget: dotenv.net, 3.1.3"
#r "nuget: AVPRIndex, 0.1.1"
#r "nuget: AVPRClient, 0.0.5"

open AVPRIndex
open type AVPRClient.Extensions
open Domain
open System
open System.IO
open System.Text.Json
open dotenv.net
open System.Security.Cryptography

printfn "script args:"

for arg in fsi.CommandLineArgs do
    printfn $"  {arg}"

printfn ""

let isDryRun = fsi.CommandLineArgs.Length > 1 && fsi.CommandLineArgs[1] = "--dry-run"

if isDryRun then
    printfn "Dry run mode enabled. No changes will be pushed to the package database."
    printfn ""

let envVars = 
    DotEnv.Fluent()
        .Read()

let apiKey = 
    if envVars.ContainsKey("APIKEY") then
        envVars["APIKEY"]
    else 
        let key = Environment.GetEnvironmentVariable("APIKEY")
        if String.IsNullOrWhiteSpace(key) then failwith "APIKEY not found"
        key

let jsonSerializerOptions = JsonSerializerOptions(WriteIndented = true)

type AVPRClient.ValidationPackage with
    
    static member toJson (p: AVPRClient.ValidationPackage) = 
        JsonSerializer.Serialize(p, jsonSerializerOptions)

    static member printJson (p: AVPRClient.ValidationPackage) = 
        let json = AVPRClient.ValidationPackage.toJson p
        printfn ""
        printfn $"Package info:{System.Environment.NewLine}{json}"
        printfn ""

type AVPRClient.PackageContentHash with
   
    static member toJson (h: AVPRClient.PackageContentHash) = 
        JsonSerializer.Serialize(h, jsonSerializerOptions)

    static member printJson (h: AVPRClient.PackageContentHash) = 
        let json = AVPRClient.PackageContentHash.toJson h
        printfn ""
        printfn $"Hash info:{System.Environment.NewLine}{json}"
        printfn ""



let client = 
    let httpClient = new System.Net.Http.HttpClient()
    httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey)
    new AVPRClient.Client(httpClient)

let published_packages = 
    client.GetAllPackagesAsync()
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> Array.ofSeq

//! Paths are relative to the root of the project, since the script is executed from the repo root in CI
let all_indexed_packages = 
    AVPRRepo.getPreviewIndex()

let published_indexed_packages = 
    all_indexed_packages
    |> Array.filter (fun i -> i.Metadata.Publish)
    |> Array.filter (fun i -> 
        Array.exists (fun (p: AVPRClient.ValidationPackage) -> 
            p.Name = i.Metadata.Name 
            && p.MajorVersion = i.Metadata.MajorVersion
            && p.MinorVersion = i.Metadata.MinorVersion
            && p.PatchVersion = i.Metadata.PatchVersion
        ) published_packages
    )

let pending_indexed_packages =
    all_indexed_packages
    |> Array.filter (fun i -> i.Metadata.Publish)
    |> Array.filter (fun i -> 
        not <| Array.exists (fun (p: AVPRClient.ValidationPackage) -> 
            p.Name = i.Metadata.Name 
            && p.MajorVersion = i.Metadata.MajorVersion
            && p.MinorVersion = i.Metadata.MinorVersion
            && p.PatchVersion = i.Metadata.PatchVersion
        ) published_packages
    )


// Validate wether the script content in the repo is the same as the published packages in the database.
// if this is not the case, published scripts were changed after being set to publish, which is not allowed.

open System
open System.Security.Cryptography

printfn ""
printfn $"Comparing database and repo content hashes..."
printfn ""

let md5 = MD5.Create()

published_indexed_packages
|> Array.iter (fun i -> 
    let repo_hash = md5.ComputeHash(File.ReadAllBytes(i.RepoPath)) |> Convert.ToHexString
    try
        i.toPackageContentHash()
        |> client.VerifyPackageContentAsync
        |> Async.AwaitTask
        |> Async.RunSynchronously
    with e ->
        if isDryRun then
            printfn $"[E]: {e.Message}"
            printfn $"[{i.Metadata.Name}@{i.Metadata.MajorVersion}.{i.Metadata.MinorVersion}.{i.Metadata.PatchVersion}]: Package content hash {repo_hash} does not match the published package"
            printfn $"  Make sure that the package file has not been modified after publication! ({i.RepoPath})"
        else
            failwith $"[{i.RepoPath}]: Package content hash {repo_hash} does not match the published package"
)

// Publish the pending packages, and add the content hash to the database

if isDryRun then
    printfn ""
    printfn $"!! the following packages and content hashes will be submitted to the production DB: !!"
    printfn ""
    pending_indexed_packages
    |> Array.iter (fun i ->
        let p = i.toValidationPackage()
        let h = i.toPackageContentHash()
        AVPRClient.ValidationPackage.printJson p
        AVPRClient.PackageContentHash.printJson h
)

else
    printfn ""
    printfn $"!! publishing pending packages and content hashes to the production DB: !!"
    printfn ""
    pending_indexed_packages
    |> Array.iter (fun i ->
        let p = i.toValidationPackage()
        let h = i.toPackageContentHash()
        try
            printfn $"[{i.Metadata.Name}@{i.Metadata.MajorVersion}.{i.Metadata.MinorVersion}.{i.Metadata.PatchVersion}]: Publishing package..."
            p
            |> client.CreatePackageAsync
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
            printfn $"Done."
            printfn ""
        with e ->
            failwith $"CreatePackage: [{i.RepoPath}]: failed with {System.Environment.NewLine}{e.Message}{System.Environment.NewLine}Package info:{System.Environment.NewLine}{AVPRClient.ValidationPackage.toJson p}"
        try
            printfn $"[{i.Metadata.Name}@{i.Metadata.MajorVersion}.{i.Metadata.MinorVersion}.{i.Metadata.PatchVersion}]: Publishing package content hash..."
            h
            |> client.CreatePackageContentHashAsync
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
            printfn $"Done."
            printfn ""
        with e ->
            failwith $"CreatePackageContentHash: [{i.RepoPath}]: {System.Environment.NewLine}{e.Message}{System.Environment.NewLine}Hash info:{System.Environment.NewLine}{AVPRClient.PackageContentHash.toJson h}"
    )