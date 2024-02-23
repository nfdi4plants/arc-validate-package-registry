#r "nuget: dotenv.net, 3.1.3"
#r "nuget: AVPRClient, 0.0.1"
#load "domain.fsx"

open Domain
open System
open System.IO
open System.Text.Json
open dotenv.net
open System.Security.Cryptography

let isDryRun = fsi.CommandLineArgs[1] = "--dry-run"

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
    static member createOfIndex (i: ValidationPackageIndex) = 
        let p = new AVPRClient.ValidationPackage()
        p.Name <- i.Metadata.Name
        p.Description <- i.Metadata.Description
        p.MajorVersion <- i.Metadata.MajorVersion
        p.MinorVersion <- i.Metadata.MinorVersion
        p.PatchVersion <- i.Metadata.PatchVersion
        p.PackageContent <- File.ReadAllBytes(i.RepoPath)
        p.ReleaseDate <- DateTimeOffset.Now
        p.Tags <- i.Metadata.Tags
        p.ReleaseNotes <- i.Metadata.ReleaseNotes
        p.Authors <- 
            i.Metadata.Authors
            |> Array.map (fun author -> 
                let a = AVPRClient.Author()
                a.FullName <- author.FullName
                a.Email <- author.Email
                a.Affiliation <- author.Affiliation
                a.AffiliationLink <- author.AffiliationLink
                a
            )
        p

    static member toJson (p: AVPRClient.ValidationPackage) = 
        JsonSerializer.Serialize(p, jsonSerializerOptions)

    static member printJson (p: AVPRClient.ValidationPackage) = 
        let json = AVPRClient.ValidationPackage.toJson p
        printfn ""
        printfn $"Package info:{System.Environment.NewLine}{json}"
        printfn ""


type AVPRClient.PackageContentHash with
    static member createOfIndex (i: ValidationPackageIndex) = 
        let h = new AVPRClient.PackageContentHash()
        h.PackageName <- i.Metadata.Name
        h.PackageMajorVersion <- i.Metadata.MajorVersion
        h.PackageMinorVersion <- i.Metadata.MinorVersion
        h.PackagePatchVersion <- i.Metadata.PatchVersion
        h.Hash <- 
            let md5 = MD5.Create()
            md5.ComputeHash(File.ReadAllBytes(i.RepoPath)) |> Convert.ToHexString
        h

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
    "src/PackageRegistryService/Data/arc-validate-package-index.json"
    |> File.ReadAllText
    |> JsonSerializer.Deserialize<ValidationPackageIndex[]>

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
        client.VerifyPackageContentAsync(
            AVPRClient.PackageContentHash.createOfIndex i
        )
        |> Async.AwaitTask
        |> Async.RunSynchronously
    with e ->
        if isDryRun then
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
        let p = AVPRClient.ValidationPackage.createOfIndex(i)
        let h = AVPRClient.PackageContentHash.createOfIndex(i)
        AVPRClient.ValidationPackage.printJson p
        AVPRClient.PackageContentHash.printJson h
)

else
    printfn ""
    printfn $"!! publishing pending packages and content hashes to the production DB: !!"
    printfn ""
    pending_indexed_packages
    |> Array.iter (fun i ->
        let p = AVPRClient.ValidationPackage.createOfIndex(i)
        let h = AVPRClient.PackageContentHash.createOfIndex(i)
        try
            printfn $"[{i.Metadata.Name}@{i.Metadata.MajorVersion}.{i.Metadata.MinorVersion}.{i.Metadata.PatchVersion}]: Publishing package..."
            client.CreatePackageAsync(p)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
            printfn $"Done."
            printfn ""
        with e ->
            failwith $"CreatePackage: [{i.RepoPath}]: failed with {System.Environment.NewLine}{e.Message}{System.Environment.NewLine}Package info:{System.Environment.NewLine}{AVPRClient.ValidationPackage.toJson p}"
        try
            printfn $"[{i.Metadata.Name}@{i.Metadata.MajorVersion}.{i.Metadata.MinorVersion}.{i.Metadata.PatchVersion}]: Publishing package content hash...{System.Environment.NewLine}"
            client.CreatePackageContentHashAsync(
                AVPRClient.PackageContentHash.createOfIndex(i)
            )
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
            printfn $"Done."
            printfn ""
        with e ->
            failwith $"CreatePackageContentHash: [{i.RepoPath}]: {System.Environment.NewLine}{e.Message}{System.Environment.NewLine}Hash info:{System.Environment.NewLine}{AVPRClient.PackageContentHash.toJson h}"
    )