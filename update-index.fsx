#r "nuget: YamlDotNet, 13.7.1"
#r "nuget: Fake.Core.Process, 6.0.0"
#r "nuget: ARCValidationPackages, 2.0.0-preview.1"

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open YamlDotNet
open YamlDotNet.Serialization
open ARCValidationPackages

[<AutoOpen>]
module Frontmatter = 
   
    let frontMatterStart = $"(*{System.Environment.NewLine}---"
    let frontMatterEnd = $"---{System.Environment.NewLine}*)"

    let yamlDeserializer = 
        DeserializerBuilder()
            .WithNamingConvention(NamingConventions.PascalCaseNamingConvention.Instance)
            .Build()

    type ValidationPackageMetadata with
        
        static member extractFromScript (scriptPath: string) =
            let script = File.ReadAllText(scriptPath)
            if script.StartsWith(frontMatterStart, StringComparison.Ordinal) && script.Contains(frontMatterEnd) then
                let frontmatter = 
                    script.Substring(
                        frontMatterStart.Length, 
                        (script.IndexOf(frontMatterEnd, StringComparison.Ordinal) - frontMatterEnd.Length))
                try 
                    let result = 
                        yamlDeserializer.Deserialize<ValidationPackageMetadata>(frontmatter)
                    result
                with e as exn -> 
                    printfn $"error parsing package metadata at {scriptPath}. Make sure that all required metadata tags are included."
                    ValidationPackageMetadata()
            else 
                printfn $"script at {scriptPath} has no correctly formatted frontmatter."
                ValidationPackageMetadata()

    type ValidationPackageIndex with

        static member create (
            repoPath: string, 
            lastUpdated: System.DateTimeOffset
        ) = 
            ValidationPackageIndex.create(
                repoPath = repoPath,
                fileName = Path.GetFileNameWithoutExtension(repoPath).Split("@").[0],
                lastUpdated = lastUpdated,
                metadata = ValidationPackageMetadata.extractFromScript(repoPath)
            )

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

