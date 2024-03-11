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
        Assert.True(
            script.StartsWith(Frontmatter.frontMatterStart, StringComparison.Ordinal) && 
            script.Contains(Frontmatter.frontMatterEnd)
        )

    static member MetadataValid(m: ValidationPackageMetadata) =
        //test wether all required fields are present
        Assert.NotNull(m)
        Assert.NotNull(m.Name)
        Assert.NotEqual<string>(m.Name, "")
        Assert.NotNull(m.Summary)
        Assert.NotEqual<string>(m.Summary, "")
        Assert.NotNull(m.Description)
        Assert.NotEqual<string>(m.Description, "")
        Assert.NotNull(m.MajorVersion)
        Assert.True(m.MajorVersion >= 0)
        Assert.NotNull(m.MinorVersion)
        Assert.True(m.MinorVersion >= 0)
        Assert.NotNull(m.PatchVersion)
        Assert.True(m.PatchVersion >= 0)

    static member FileNameValid(path:string) =
        let fileName = Path.GetFileName(path)
        let folderName = Path.GetDirectoryName(path) |> Path.GetFileName
        let pattern = sprintf @"^%s@\d+\.\d+\.\d+\.fsx$" folderName
        Assert.Matches(pattern, fileName)