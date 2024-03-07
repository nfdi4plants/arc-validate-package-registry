namespace StagingDirectory

open System
open System.IO
open System.Text
open System.Text.RegularExpressions
open Xunit
open Utils

module DirectoryContent =

    [<Fact>]
    let ``Contains files`` () =
        ReferenceObjects.all_files_in_staging_area
        |> Assert.NotEmpty

    [<Fact>]
    let ``Contains fsx files`` () =
        ReferenceObjects.all_files_in_staging_area 
        |> Array.filter (fun p -> (p.EndsWith(".fsx")))
        |> Assert.NotEmpty

    [<Fact>]
    let ``Only contains fsx files`` () =
        ReferenceObjects.all_files_in_staging_area 
        |> Array.filter (fun p -> not (p.EndsWith(".fsx")))
        |> Assert.Empty

    [<Fact>]
    let ``All filenames are unique`` () =
        ReferenceObjects.all_files_in_staging_area 
        |> Array.groupBy (fun p -> Path.GetFileName p) 
        |> Array.filter (fun (k, v) -> v.Length > 1)
        |> Assert.Empty

    [<Fact>]
    let ``All file names are valid`` () =
        Assert.All(
            ReferenceObjects.all_files_in_staging_area,
            Assert.FileNameValid
        )

    [<Fact>]
    let ``Only 2 directories deep`` () =
        ReferenceObjects.all_files_in_staging_area
        |> Array.filter (fun p -> p.Split(Path.DirectorySeparatorChar).Length > 3)
        |> Assert.Empty