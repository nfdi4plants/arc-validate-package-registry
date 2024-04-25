#r "nuget: dotenv.net, 3.1.3"
#r "nuget: AVPRIndex, 0.1.0"
#r "nuget: AVPRClient, 0.0.4"
#r "nuget: FsHttp, 14.5.0"

open AVPRIndex
open AVPRIndex.Domain
open type AVPRClient.Extensions
open FsHttp

let current_preview_index = 
    AVPRRepo.getPreviewIndex()

let all_packages_in_staging_area =
    AVPRRepo.getStagedPackages()

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
                printfn $"package {pending.Metadata.Name} with version {ValidationPackageIndex.getSemanticVersionString pending} is already indexed and will be updated with this release."
                true
            else
                printfn $"package {pending.Metadata.Name} with version {ValidationPackageIndex.getSemanticVersionString pending} is new and will be added with this release."
                true
    )

open System
open System.IO

// https://stackoverflow.com/questions/70123328/how-to-set-environment-variables-in-github-actions-using-python
let GITHUB_ENV = Environment.GetEnvironmentVariable("GITHUB_ENV")

if staging_diff.Length > 0 then 
    File.AppendAllLines(GITHUB_ENV, ["UPDATE_PREVIEW_INDEX=true"])
else
    File.AppendAllLines(GITHUB_ENV, ["UPDATE_PREVIEW_INDEX=false"])

printfn $"""GITHUB_ENV={File.ReadAllText(GITHUB_ENV)}"""