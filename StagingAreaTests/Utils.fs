module Utils

open AVPRIndex
open AVPRIndex.Domain
open Xunit
open System 
open System.IO
open Fake.DotNet
open Fake.Core

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

type Assert with

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
        Assert.Equal(result.ExitCode, 0)

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
        Assert.Equal<string list>([], result.Errors)
        Assert.Equal(result.ExitCode, 0)

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