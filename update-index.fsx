open System
open System.IO
open System.Text.Json

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


let changed_files = File.ReadAllLines("file_changes.txt") |> set |> Set.map (fun x -> x.Replace('\\',Path.DirectorySeparatorChar).Replace('/',Path.DirectorySeparatorChar))

type ValidationPackageIndex =
    {
        Name: string
        LastUpdated: System.DateTime
    }

Directory.GetFiles("validation_packages", "*.fsx")
|> Array.map (fun package ->
    if changed_files.Contains(package) then
        Console.ForegroundColor <- ConsoleColor.Green
        printfn $"{package} was changed in this commit.{System.Environment.NewLine}"
        Console.ForegroundColor <- ConsoleColor.White
        { Name = package; LastUpdated = System.DateTime.Now}
    else
        printfn $"{package} was not changed in this commit."
        printfn $"getting history for {package}"
        let history = executeProcess "git" $"log -1 --pretty=format:'%%ci' {package}"
        printfn $"history is at {history.StdOut}{System.Environment.NewLine}"
        { Name = package; LastUpdated = System.DateTime.ParseExact(history.StdOut.Split(" ").[0].Replace("'",""), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)}
)
|> fun packages -> JsonSerializer.Serialize(packages, options = JsonSerializerOptions(WriteIndented = true))
|> fun json -> File.WriteAllText("validation_packages.json", json)

