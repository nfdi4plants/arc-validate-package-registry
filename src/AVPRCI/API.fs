module API

open System
open System.IO
open AVPRIndex
open System.IO
open System.Text.Json

open Argu
open CLIArgs
open Domain

open AVPRIndex
open AVPRClient

type GenIndexAPI = 
    static member generatePreviewIndex (verbose: bool) (repo_root: string) (args: ParseResults<GeneratePreviewIndexArgs>) = 
        
        let out_path = 
            args.TryGetResult(GeneratePreviewIndexArgs.Output_Folder) 
            |> Option.defaultValue "."
            |> fun o -> Path.Combine(o, "avpr-preview-index.json")

        JsonSerializer.Serialize(
            value = AVPRRepo.getStagedPackages(repo_root),
            options = JsonSerializerOptions(WriteIndented = true)
        )
        |> fun json -> File.WriteAllText(out_path, json)

        if verbose then 
            printfn "Generated preview index at: %s" out_path
            printfn ""

        0

type CheckAPI =
    static member prePublishChecks (verbose: bool) (repo_root: string) = 
        
        if verbose then 
            printfn "pulling current preview index..." 
            printfn ""

        let current_preview_index = 
            AVPRRepo.getPreviewIndex()
        
        if verbose then 
            printfn $"{current_preview_index.Length} packages in the preview index" 
            printfn ""
            printfn "collecting staged packages..." 
            printfn ""

        let all_packages_in_staging_area =
            AVPRRepo.getStagedPackages(repo_root)

        if verbose then 
            printfn $"{all_packages_in_staging_area.Length} packages in the staging area" 
            printfn ""
            printfn "performing pre-publish checks..." 
            printfn ""

        // all packages in staging area that are not listed in the current preview index
        let staging_diff =
            all_packages_in_staging_area
            // only keep packages that either differ in content from their entry, or are not there at all
            // this filter step keeps packages that incorrectly update published packages, which will be filtered in the next step.
            |> Array.filter (fun pending ->
                not <| Array.exists (fun indexed -> ValidationPackageIndex.contentEquals pending indexed) current_preview_index 
            )
            |> Array.filter (fun pending ->
                let is_indexed = Array.exists (fun indexed -> ValidationPackageIndex.identityEquals pending indexed) current_preview_index 
                let is_indexed_and_published = Array.exists (fun indexed -> ValidationPackageIndex.identityEquals pending indexed && indexed.Metadata.Publish) current_preview_index 
                if is_indexed_and_published then
                    // package is on the preview index and already set to publish
                    // we want to fail here, as those packages should stay immutable
                    failwithf $"package {pending.Metadata.Name} with version {ValidationPackageIndex.getSemanticVersionString pending} is already indexed and set to publish. This is not allowed. Publish a new version instead."
                else
                    if is_indexed then
                        if verbose then printfn $"package {pending.Metadata.Name} with version {ValidationPackageIndex.getSemanticVersionString pending} is already indexed and will be updated with this release."
                        true
                    else
                        if verbose then printfn $"package {pending.Metadata.Name} with version {ValidationPackageIndex.getSemanticVersionString pending} is new and will be added with this release."
                        true
            )

        if verbose then 
            printfn $"{staging_diff.Length} packages are pending for publication" 
            printfn ""
            printfn "writing results to env..." 
            printfn ""

        // https://stackoverflow.com/questions/70123328/how-to-set-environment-variables-in-github-actions-using-python
        let GITHUB_ENV = 
            let env = Environment.GetEnvironmentVariable("GITHUB_ENV")
            if String.IsNullOrEmpty(env) then
                printfn "GITHUB_ENV not found in environment, writing to <repo_root>/pre-publish-check.txt"
                let p = Path.Combine(repo_root, "check.txt")
                let f = new FileInfo(p)
                p
            else env


        if staging_diff.Length > 0 then 
            File.AppendAllLines(GITHUB_ENV, ["UPDATE_PREVIEW_INDEX=true"])
        else
            File.AppendAllLines(GITHUB_ENV, ["UPDATE_PREVIEW_INDEX=false"])

        printfn $"""GITHUB_ENV={File.ReadAllText(GITHUB_ENV)}"""
        0

type PublishAPI =
    static member publishPendingPackages (verbose: bool) (repo_root: string) (args: ParseResults<PublishArgs>) = 
    
        let isDryRun = args.TryGetResult(PublishArgs.Dry_Run).IsSome

        if isDryRun then
            printfn "Dry run mode enabled. No changes will be pushed to the package database."
            printfn ""

        let apiKey = args.GetResult(PublishArgs.API_Key)

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
            AVPRRepo.getStagedPackages(repo_root)

        let published_indexed_packages = 
            all_indexed_packages
            |> Array.filter (fun i -> i.Metadata.Publish)
            |> Array.filter (fun i -> 
                Array.exists (fun (p: AVPRClient.ValidationPackage) -> p.IdentityEquals(i)) published_packages
            )

        let pending_indexed_packages =
            all_indexed_packages
            |> Array.filter (fun i -> i.Metadata.Publish)
            |> Array.filter (fun i -> 
                not <| Array.exists (fun (p: AVPRClient.ValidationPackage) -> p.IdentityEquals(i)) published_packages
            )
        
        if verbose then
            printfn ""
            printfn $"Comparing database and repo content hashes..."
            printfn ""

        published_indexed_packages
        |> Array.iter (fun i -> 
            try
                i.toPackageContentHash(true)
                |> client.VerifyPackageContentAsync
                |> Async.AwaitTask
                |> Async.RunSynchronously
            with e ->
                if isDryRun then
                    printfn $"[E]: {e.Message}"
                    printfn $"[{i.Metadata.Name}@{ValidationPackageIndex.getSemanticVersionString i}]: Package content hash does not match the published package"
                    printfn $"  Make sure that the package file has not been modified after publication! ({i.RepoPath})"
                else
                    failwith $"[{i.RepoPath}]: Package content hash does not match the published package"
        )

        // Publish the pending packages, and add the content hash to the database


        if isDryRun then
            printfn ""
            printfn $"!! the following packages and content hashes will be submitted to the production DB: !!"
            printfn ""
            
            pending_indexed_packages
            |> Array.iter (fun i -> printfn $"[{i.Metadata.Name}@{ValidationPackageIndex.getSemanticVersionString i}]")
            
            if verbose then 
                printfn ""
                printfn "Details:"
                printfn ""
                pending_indexed_packages
                |> Array.iter (fun i ->
                    let p = i.toValidationPackage()
                    AVPRClient.ValidationPackage.printJson p
                )
        else
            printfn ""
            printfn $"!! publishing pending packages and content hashes to the production DB: !!"
            printfn ""
            pending_indexed_packages
            |> Array.iter (fun i ->
                let p = i.toValidationPackage()
                try
                    printfn $"[{i.Metadata.Name}@{ValidationPackageIndex.getSemanticVersionString i}]: Publishing package..."
                    p
                    |> client.CreatePackageAsync
                    |> Async.AwaitTask
                    |> Async.RunSynchronously
                    |> ignore
                with e ->
                    failwith $"CreatePackage: [{i.RepoPath}]: failed with {System.Environment.NewLine}{e.Message}{System.Environment.NewLine}Package info:{System.Environment.NewLine}{AVPRClient.ValidationPackage.toJson p}"
            )
            printfn "done."

        0
