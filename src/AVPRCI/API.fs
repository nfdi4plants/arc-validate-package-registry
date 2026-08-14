module API

open System
open AVPR.Staging

open Argu
open CLIArgs
open Domain

open AVPRClient

module PublishedPackageDiscovery =

    let get (client: AVPRClient.Client) =
        client.GetPackageIndexAsync()
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> Array.ofSeq

type PublishAPI =
    static member publishPendingPackages (verbose: bool) (repo_root: string) (args: ParseResults<PublishArgs>) = 
    
        let isDryRun = args.TryGetResult(PublishArgs.Dry_Run).IsSome
        let baseUrl =
            args.GetResult(PublishArgs.Base_Url)
            |> RegistryEndpoint.normalize

        if isDryRun then
            printfn "Dry run mode enabled. No changes will be pushed to the package database."
            printfn ""

        printfn $"Target registry: {baseUrl}"

        let apiKey = args.GetResult(PublishArgs.API_Key)

        let client = 
            let httpClient = new System.Net.Http.HttpClient()
            httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey)
            let client = new AVPRClient.Client(httpClient)
            client.BaseUrl <- baseUrl
            client

        let published_packages = PublishedPackageDiscovery.get client

        //! Paths are relative to the root of the project, since the script is executed from the repo root in CI
        let all_staged_packages = 
            StagingRepository.discover(repo_root)

        let already_published_pending_packages = 
            all_staged_packages
            |> Array.filter (fun i -> i.Metadata.Publish)
            |> Array.filter (fun i -> 
                Array.exists (fun p -> ClientMappings.identityEquals p i) published_packages
            )

        let new_packages =
            all_staged_packages
            |> Array.filter (fun i -> i.Metadata.Publish)
            |> Array.filter (fun i -> 
                not <| Array.exists (fun p -> ClientMappings.identityEquals p i) published_packages
            )
        
        if verbose then
            printfn ""
            printfn $"Comparing database and repo content hashes..."
            printfn ""

        already_published_pending_packages
        |> Array.iter (fun i -> 
            try
                ClientMappings.toPackageContentHash true i
                |> client.VerifyPackageContentAsync
                |> Async.AwaitTask
                |> Async.RunSynchronously
            with e ->
                if isDryRun then
                    printfn $"[E]: {e.Message}"
                    printfn $"[{i.Metadata.Name}@{StagedValidationPackage.getSemanticVersionString i}]: Package content hash does not match the published package"
                    printfn $"  Make sure that the package file has not been modified after publication! ({i.RepoPath})"
                else
                    failwith $"[{i.RepoPath}]: Package content hash does not match the published package"
        )

        // Publish the pending packages, and add the content hash to the database


        if isDryRun then
            printfn ""
            printfn $"!! the following packages and content hashes would be submitted to {baseUrl}: !!"
            printfn ""
            
            new_packages
            |> Array.iter (fun i -> printfn $"[{i.Metadata.Name}@{StagedValidationPackage.getSemanticVersionString i}]")
            
            if verbose then 
                printfn ""
                printfn "Details:"
                printfn ""
                new_packages
                |> Array.iter (fun i ->
                    let p = ClientMappings.toValidationPackage DateTimeOffset.Now i
                    AVPRClient.ValidationPackage.printJson p
                )
        else
            printfn ""
            printfn $"!! publishing pending packages and content hashes to {baseUrl}: !!"
            printfn ""
            new_packages
            |> Array.iter (fun i ->
                let p = ClientMappings.toValidationPackage DateTimeOffset.Now i
                try
                    printfn $"[{i.Metadata.Name}@{StagedValidationPackage.getSemanticVersionString i}]: Publishing package..."
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
