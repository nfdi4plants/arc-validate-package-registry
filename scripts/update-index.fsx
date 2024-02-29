#r "nuget: AVPRIndex, 0.0.1"

open AVPRIndex
open Domain
open Frontmatter

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open YamlDotNet
open YamlDotNet.Serialization

type ProcessResult = { 
    ExitCode : int; 
    StdOut : string; 
    StdErr : string 
}

let executeProcess (processName: string) (processArgs: string) =
    let psi = new Diagnostics.ProcessStartInfo(processName, processArgs) 
    psi.UseShellExecute <- false
    psi.RedirectStandardOutput <- true
    psi.RedirectStandardError <- true
    psi.CreateNoWindow <- true        
    let proc = Diagnostics.Process.Start(psi) 
    let output = new Text.StringBuilder()
    let error = new Text.StringBuilder()
    proc.OutputDataReceived.Add(fun args -> output.Append(args.Data) |> ignore)
    proc.ErrorDataReceived.Add(fun args -> error.Append(args.Data) |> ignore)
    proc.BeginErrorReadLine()
    proc.BeginOutputReadLine()
    proc.WaitForExit()
    { ExitCode = proc.ExitCode; StdOut = output.ToString(); StdErr = error.ToString() }

let truncateDateTime (date: System.DateTimeOffset) =
    DateTimeOffset.ParseExact(
        date.ToString("yyyy-MM-dd HH:mm:ss zzzz"), 
        "yyyy-MM-dd HH:mm:ss zzzz", 
        System.Globalization.CultureInfo.InvariantCulture
    )

//! Paths are relative to root, since the script will be called in repo root.
let packages = 
    Directory.GetFiles("src/PackageRegistryService/StagingArea", "*.fsx", SearchOption.AllDirectories)
    |> Array.map (fun x -> x.Replace('\\',Path.DirectorySeparatorChar).Replace('/',Path.DirectorySeparatorChar))

let changed_files = File.ReadAllLines("file_changes.txt") |> set |> Set.map (fun x -> x.Replace('\\',Path.DirectorySeparatorChar).Replace('/',Path.DirectorySeparatorChar))
    
let index = 
    packages
    |> Array.map (fun package ->
        if changed_files.Contains(package) then
        
            printfn $"{package} was changed in this commit.{System.Environment.NewLine}"

            ValidationPackageIndex.create(
                repoPath = package.Replace(Path.DirectorySeparatorChar, '/'), // use front slash always here, otherwise the backslash will be escaped with another backslah on windows when writing the json
                lastUpdated = truncateDateTime System.DateTimeOffset.Now // take local time with offset if file will be changed with this commit
            )
    
        else
            printfn $"{package} was not changed in this commit."
            printfn $"getting history for {package}"

            let history = executeProcess "git" $"log -1 --pretty=format:'%%ci' {package}"
            let time = 
                System.DateTimeOffset.ParseExact(
                    history.StdOut.Replace("'",""), 
                    "yyyy-MM-dd HH:mm:ss zzz", 
                    System.Globalization.CultureInfo.InvariantCulture
                )
        
            printfn $"history is at {time}{System.Environment.NewLine}"

            ValidationPackageIndex.create(
                repoPath = package.Replace(Path.DirectorySeparatorChar, '/'), // use front slash always here, otherwise the backslash will be escaped with another backslah on windows when writing the json
                lastUpdated = time // take time indicated by git history
            )
    )

JsonSerializer.Serialize(index, options = JsonSerializerOptions(WriteIndented = true))
|> fun json -> File.WriteAllText("src/PackageRegistryService/Data/arc-validate-package-index.json", json)

