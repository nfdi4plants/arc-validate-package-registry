module Utils

open AVPRIndex
open AVPRIndex.Domain
open Xunit
open System 
open System.IO
open System.Text
open Fake.DotNet
open Fake.Core
open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Diagnostics
open FSharp.Compiler.Text

module internal Tool =

    let run (tool: string) (args: string []) =
        CreateProcess.fromRawCommand tool args
        |> CreateProcess.redirectOutputIfNotRedirected
        |> fun p ->
        
            let results = System.Collections.Generic.List<ConsoleMessage>()

            let errorF msg = 
                Trace.traceError msg
                results.Add(ConsoleMessage.CreateError msg)

            let messageF msg =
                Trace.trace msg
                results.Add(ConsoleMessage.CreateOut msg)

            CreateProcess.withOutputEventsNotNull messageF errorF p
            |> CreateProcess.map (fun prev -> prev, (results |> List.ofSeq))
        |> CreateProcess.map (fun (r, results) -> ProcessResult.New r.ExitCode results)
        |> Proc.run

module internal Compiler =

    let fsharpChecker = FSharpChecker.Create()

    let formatFSharpDiagnostics (diagnostics: FSharpDiagnostic seq) =
        diagnostics
        |> Seq.map string
        |> String.concat Environment.NewLine

    /// Locate FSharp.Compiler.Interactive.Settings.dll from the running .NET SDK.
    /// This assembly provides the `fsi` object that the `dotnet fsi` host injects into
    /// scripts. It ships in the SDK's `FSharp/` folder (not in the FSharp.Compiler.Service
    /// nuget package), so `useFsiAuxLib` alone cannot find it when checking scripts here.
    let fsiSettingsDll =
        let candidateRoots =
            [
                Environment.GetEnvironmentVariable "DOTNET_ROOT"
                (try Path.GetDirectoryName(Diagnostics.Process.GetCurrentProcess().MainModule.FileName) with _ -> null)
                Path.GetDirectoryName(typeof<obj>.Assembly.Location)
            ]
            |> List.filter (String.IsNullOrWhiteSpace >> not)
            |> List.collect (fun r -> [ r; Path.Combine(r, ".."); Path.Combine(r, "..", ".."); Path.Combine(r, "..", "..", "..") ])
            |> List.map Path.GetFullPath
            |> List.distinct
        candidateRoots
        |> List.tryPick (fun root ->
            let sdkDir = Path.Combine(root, "sdk")
            if Directory.Exists sdkDir then
                Directory.GetFiles(sdkDir, "FSharp.Compiler.Interactive.Settings.dll", SearchOption.AllDirectories)
                |> Array.sortDescending // prefer the newest SDK
                |> Array.tryHead
            else None
        )

type Assert with

    static member FSharpScriptCompiles (scriptPath: string) =
        let safeScriptName =
            scriptPath
            |> Path.GetFileNameWithoutExtension
            |> Encoding.UTF8.GetBytes
            |> Convert.ToHexString

        let virtualScriptPath =
            Path.Combine(
                Path.GetDirectoryName(Path.GetFullPath(scriptPath)),
                $"ValidationPackage_{safeScriptName}.fsx"
            )

        let sourceText =
            // Emulate the `dotnet fsi` host, since these packages are always executed via fsi:
            // `open` the FSI settings module so the `fsi` object (e.g. fsi.CommandLineArgs)
            // resolves during type-checking, just as it does when the package runs under fsi.
            // The `#line 1` directive resets numbering so diagnostics still point at the
            // original source lines despite the prepended prelude.
            let originalSource = File.ReadAllText scriptPath
            let prelude =
                "open FSharp.Compiler.Interactive.Settings\n"
                + "#line 1\n"
            (prelude + originalSource)
            |> SourceText.ofString

        // Make the FSI settings assembly available to the checker so the prepended `open`
        // resolves. useFsiAuxLib cannot find it against the nuget-packaged compiler, so we
        // reference the SDK's copy explicitly when we can locate it.
        let otherFlags =
            match Compiler.fsiSettingsDll with
            | Some dll -> [| "-r:" + dll |]
            | None -> [||]

        let projectOptions, projectOptionDiagnostics =
            Compiler.fsharpChecker.GetProjectOptionsFromScript(
                virtualScriptPath,
                sourceText,
                otherFlags = otherFlags,
                assumeDotNetFramework = false,
                useSdkRefs = true,
                useFsiAuxLib = true
            )
            |> Async.RunSynchronously

        let parseResults, checkAnswer =
            Compiler.fsharpChecker.ParseAndCheckFileInProject(
                virtualScriptPath,
                0,
                sourceText,
                projectOptions
            )
            |> Async.RunSynchronously

        let checkDiagnostics =
            match checkAnswer with
            | FSharpCheckFileAnswer.Succeeded checkResults -> checkResults.Diagnostics
            | FSharpCheckFileAnswer.Aborted ->
                Assert.True(false, $"F# type checking was aborted: {scriptPath}")
                [||]

        let errors =
            seq {
                yield! projectOptionDiagnostics
                yield! parseResults.Diagnostics
                yield! checkDiagnostics
            }
            |> Seq.filter (fun diagnostic -> diagnostic.Severity = FSharpDiagnosticSeverity.Error)
            |> Seq.toArray

        Assert.True(
            errors.Length = 0,
            $"F# script did not compile: {scriptPath}{Environment.NewLine}{Compiler.formatFSharpDiagnostics errors}"
        )

    static member PythonScriptCompiles (scriptPath: string) =
        let compileOnly =
            "import pathlib, sys; path = pathlib.Path(sys.argv[1]); compile(path.read_text(encoding='utf-8'), str(path), 'exec')"

        let result =
            Tool.run "uv" [|"run"; "--no-project"; "--"; "python"; "-c"; compileOnly; scriptPath|]

        Assert.Equal(0, result.ExitCode)

    static member FSharpScriptRuns (args: string []) (scriptPath: string)=
        let args = Array.concat [|[|scriptPath|]; args|]
        //let outPath = Path.GetDirectoryName scriptPath
        let result = 
            DotNet.exec 
                (fun p -> 
                    {
                        p with
                            RedirectOutput = true
                            PrintRedirectedOutput = true
                    }
                )
                "fsi" 
                (args |> String.concat " ")
        //let packageName = Path.GetFileNameWithoutExtension(scriptPath)
        //let outputFolder = Path.Combine([|outPath; ".arc-validate-results"; packageName|])
        //Assert.True(Directory.Exists(outputFolder))
        //Assert.True(File.Exists(Path.Combine(outputFolder,"badge.svg")))
        //Assert.True(File.Exists(Path.Combine(outputFolder,"validation_report.xml")))
        //Assert.True(File.Exists(Path.Combine(outputFolder,"validation_summary.json")))
        Assert.Equal<string list>([], result.Errors)
        Assert.Equal(0, result.ExitCode)

    static member PythonScriptRuns (args: string []) (scriptPath: string)=
        let args = Array.concat [|[|"run"; scriptPath|]; args|]
        //let outPath = Path.GetDirectoryName scriptPath
        let result = Tool.run "uv" args
        //let packageName = Path.GetFileNameWithoutExtension(scriptPath)
        //let outputFolder = Path.Combine([|outPath; ".arc-validate-results"; packageName|])
        //Assert.True(Directory.Exists(outputFolder))
        //Assert.True(File.Exists(Path.Combine(outputFolder,"badge.svg")))
        //Assert.True(File.Exists(Path.Combine(outputFolder,"validation_report.xml")))
        //Assert.True(File.Exists(Path.Combine(outputFolder,"validation_summary.json")))
        //Assert.Equal<string list>([], result.Errors)
        Assert.Equal(0, result.ExitCode)

    static member ContainsFSharpFrontmatter (script: string) =
        let containsCommentFrontmatter = script.StartsWith(Frontmatter.FSharp.frontMatterCommentStart, StringComparison.Ordinal) && script.Contains(Frontmatter.FSharp.frontMatterCommentEnd)
        let containsBindingFrontmatter = script.StartsWith(Frontmatter.FSharp.frontmatterBindingStart, StringComparison.Ordinal) && script.Contains(Frontmatter.FSharp.frontmatterBindingEnd)
        Assert.True(containsCommentFrontmatter || containsBindingFrontmatter)

    static member ContainsPythonFrontmatter (script: string) =
        let containsCommentFrontmatter = script.StartsWith(Frontmatter.Python.frontMatterCommentStart, StringComparison.Ordinal) && script.Contains(Frontmatter.Python.frontMatterCommentEnd)
        let containsBindingFrontmatter = script.StartsWith(Frontmatter.Python.frontmatterBindingStart, StringComparison.Ordinal) && script.Contains(Frontmatter.Python.frontmatterBindingEnd)
        Assert.True(containsCommentFrontmatter || containsBindingFrontmatter)


    static member FileNameValid(path:string) =
        let fileName = Path.GetFileName(path)
        let folderName = Path.GetDirectoryName(path) |> Path.GetFileName
        let pattern = sprintf @"^%s@%s\.(fsx|py)$" folderName AVPRIndex.Globals.SEMVER_REGEX_PATTERN[1.. (AVPRIndex.Globals.SEMVER_REGEX_PATTERN.Length - 2)] // first and last characters of that regex are start/end signifiers
        Assert.Matches(pattern, fileName)
