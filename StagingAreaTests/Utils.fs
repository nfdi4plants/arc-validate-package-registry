module Utils

open AVPRIndex
open AVPRIndex.Domain
open Xunit
open System 
open System.IO
open Fake.DotNet

type Assert with

    static member ScriptRuns (args: string []) (scriptPath: string)=
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
        Assert.Equal(result.ExitCode, 0)
        Assert.Empty(result.Errors)

    static member ContainsFrontmatter (script: string) =
        let containsCommentFrontmatter = script.StartsWith(Frontmatter.frontMatterCommentStart, StringComparison.Ordinal) && script.Contains(Frontmatter.frontMatterCommentEnd)
        let containsBindingFrontmatter = script.StartsWith(Frontmatter.frontmatterBindingStart, StringComparison.Ordinal) && script.Contains(Frontmatter.frontmatterBindingEnd)
        Assert.True(containsCommentFrontmatter || containsBindingFrontmatter)


    static member FileNameValid(path:string) =
        let fileName = Path.GetFileName(path)
        let folderName = Path.GetDirectoryName(path) |> Path.GetFileName
        let pattern = sprintf @"^%s@\d+\.\d+\.\d+\.fsx$" folderName
        Assert.Matches(pattern, fileName)