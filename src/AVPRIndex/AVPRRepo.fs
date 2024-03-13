namespace AVPRIndex

open System
open System.IO
open System.Text.Json
open Domain
open Globals
open Frontmatter
open FsHttp

type AVPRRepo =

    ///! Paths are relative to the root of the project, since the script is executed from the repo root in CI
    /// Path is adjustable by passing `RepoRoot`
    static member getStagedPackages(?RepoRoot: string) = 

        let path = 
            defaultArg 
                (RepoRoot |> Option.map (fun p -> Path.Combine(p, STAGING_AREA_RELATIVE_PATH))) 
                STAGING_AREA_RELATIVE_PATH

        Directory.GetFiles(path, "*.fsx", SearchOption.AllDirectories)
        |> Array.map (fun x -> x.Replace('\\',Path.DirectorySeparatorChar).Replace('/',Path.DirectorySeparatorChar))
        |> Array.map (fun p -> 
            ValidationPackageIndex.create(
                repoPath = p.Replace(Path.DirectorySeparatorChar, '/'), // use front slash always here, otherwise the backslash will be escaped with another backslah on windows when writing the json
                lastUpdated = Utils.truncateDateTime DateTimeOffset.Now // take local time with offset if file will be changed with this commit
            )
        )

    static member getPreviewIndex() = 
        try
            http {
                GET "https://github.com/nfdi4plants/arc-validate-package-registry/releases/download/preview-index/avpr-preview-index.json"
            }
            |> Request.send
            |> Response.deserializeJson<ValidationPackageIndex[]>
        with e as exn ->
            printfn $"Failed to fetch current preview index: {exn.Message}"
            [||]


