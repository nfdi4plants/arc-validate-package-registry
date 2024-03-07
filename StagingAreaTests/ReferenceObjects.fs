module ReferenceObjects

open System.IO
open Xunit
open AVPRIndex
open AVPRIndex.Domain
open AVPRIndex.Frontmatter

// all packages are copied into the tesp assembly output, see project file
let staging_area_root = "StagingArea"

let all_files_in_staging_area = 
    Directory.GetFiles(staging_area_root, "*", SearchOption.AllDirectories)
    |> Array.map (fun x -> x.Replace('\\',Path.DirectorySeparatorChar).Replace('/',Path.DirectorySeparatorChar))

let all_staged_packages_paths =
    Directory.GetFiles(staging_area_root, "*.fsx", SearchOption.AllDirectories)
    |> Array.map (fun x -> x.Replace('\\',Path.DirectorySeparatorChar).Replace('/',Path.DirectorySeparatorChar))

let all_staged_packages_contents =
    all_staged_packages_paths
    |> Array.map (fun p ->
        p, 
        p |> (File.ReadAllText >> fun x -> x.ReplaceLineEndings())
    )

let all_staged_packages_metadata =
    all_staged_packages_paths
    |> Array.map (fun p -> 
        p,
        ValidationPackageMetadata.extractFromScript p
    )