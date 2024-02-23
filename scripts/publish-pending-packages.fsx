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

let envVars = 
    DotEnv.Fluent()
        .WithExceptions()
        .Read()

let apiKey = 
    if System.Environment.GetEnvironmentVariable("APIKEY") = null then
        try
            envVars["APIKEY"]
        with e -> 
            failwith "APIKEY not found"
    else
        failwith "APIKEY not found"

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

let md5 = MD5.Create()

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
        failwith $"[{i.RepoPath}]: Package content hash {repo_hash} does not match the published package"
)

// Publish the pending packages, and add the content hash to the database

if isDryRun then
    printfn "the following packages and content hashes will be submitted to the production DB:"
    printfn ""
    pending_indexed_packages
    |> Array.iter (fun i ->
        let p = AVPRClient.ValidationPackage.createOfIndex(i)
        let h = AVPRClient.PackageContentHash.createOfIndex(i)
        printfn $"""
Package info:
    Name: {p.Name}
    Description: {p.Description}
    MajorVersion: {p.MajorVersion}
    MinorVersion: {p.MinorVersion}
    PatchVersion: {p.PatchVersion}
    PackageContent(Length): {p.PackageContent.Length}
    Tags: {p.Tags}
    Authors: {p.Authors}
    ReleaseNotes: {p.ReleaseNotes}
    ReleaseDate: {p.ReleaseDate}

Hash info:
    PackageName: {h.PackageName}
    PackageMajorVersion: {h.PackageMajorVersion}
    PackageMinorVersion: {h.PackageMinorVersion}
    PackagePatchVersion: {h.PackagePatchVersion}
    Hash: {h.Hash}
"""
)

else
    pending_indexed_packages
    |> Array.iter (fun i ->
        let p = AVPRClient.ValidationPackage.createOfIndex(i)
        let h = AVPRClient.PackageContentHash.createOfIndex(i)
        try
            client.CreatePackageAsync(p)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
        with e ->
            failwith $"""CreatePackage: [{i.RepoPath}]: failed with {e.Message}. 
        
    Package info:
        Name: {p.Name}
        Description: {p.Description}
        MajorVersion: {p.MajorVersion}
        MinorVersion: {p.MinorVersion}
        PatchVersion: {p.PatchVersion}
        PackageContent(Length): {p.PackageContent.Length}
        Tags: {p.Tags}
        Authors: {p.Authors}
        ReleaseNotes: {p.ReleaseNotes}
        ReleaseDate: {p.ReleaseDate}
    """
    
        try

            printfn "%O" h.Hash
            printfn "%O" h.PackageName
            printfn "%O" h.PackageMajorVersion
            printfn "%O" h.PackageMinorVersion
            printfn "%O" h.PackagePatchVersion

            client.CreatePackageContentHashAsync(
                AVPRClient.PackageContentHash.createOfIndex(i)
            )
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
        with e ->
            failwith $"""CreatePackageContentHash: [{i.RepoPath}]: failed with {e.Message}
    Hash info:
        PackageName: {h.PackageName}
        PackageMajorVersion: {h.PackageMajorVersion}
        PackageMinorVersion: {h.PackageMinorVersion}
        PackagePatchVersion: {h.PackagePatchVersion}
        Hash: {h.Hash}
    """
    )