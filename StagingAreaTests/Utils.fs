module Utils

open AVPRIndex
open AVPRIndex.Domain
open Xunit
open System 
open System.IO
open FSharp.Compiler.CodeAnalysis

type Assert with

    static member ScriptCompiles (script: string) =
        let t = Path.GetTempFileName()
        let tempPath = Path.ChangeExtension(t, ".dll")
        let checker = FSharpChecker.Create()
        let errors, exitCode =
            checker.Compile([| "fsc.exe"; "-o"; tempPath; "-a"; script |]) 
            |> Async.RunSynchronously
        Assert.Empty(errors)
        Assert.Equal(0, exitCode)

    static member ContainsFrontmatter (script: string) =
        let containsCommentFrontmatter = script.StartsWith(Frontmatter.frontMatterCommentStart, StringComparison.Ordinal) && script.Contains(Frontmatter.frontMatterCommentEnd)
        let containsBindingFrontmatter = script.StartsWith(Frontmatter.frontmatterBindingStart, StringComparison.Ordinal) && script.Contains(Frontmatter.frontmatterBindingEnd)
        Assert.True(containsCommentFrontmatter || containsBindingFrontmatter)


    static member FileNameValid(path:string) =
        let fileName = Path.GetFileName(path)
        let folderName = Path.GetDirectoryName(path) |> Path.GetFileName
        let pattern = sprintf @"^%s@\d+\.\d+\.\d+\.fsx$" folderName
        Assert.Matches(pattern, fileName)