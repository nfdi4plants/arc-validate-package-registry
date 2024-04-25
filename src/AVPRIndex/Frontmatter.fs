namespace AVPRIndex

open Domain
open System
open System.Text
open System.IO
open System.Security.Cryptography
open YamlDotNet.Serialization

[<AutoOpen>]
module Frontmatter = 

    /// the frontmatter start string if the package uses yaml frontmatter as comment
    let [<Literal>] frontMatterCommentStart = "(*\n---"
    /// the frontmatter end string if the package uses yaml frontmatter as comment
    let [<Literal>] frontMatterCommentEnd = "---\n*)"

    /// the frontmatter start string if the package uses yaml frontmatter as a string binding to be re-used in the package code
    let [<Literal>] frontmatterBindingStart = "let [<Literal>]PACKAGE_METADATA = \"\"\"(*\n---"
    /// the frontmatter end string if the package uses yaml frontmatter as a string binding to be re-used in the package code
    let [<Literal>] frontmatterBindingEnd = "---\n*)\"\"\""


    let containsCommentFrontmatter (str: string) =
        str.StartsWith(frontMatterCommentStart, StringComparison.Ordinal) && str.Contains(frontMatterCommentEnd)

    let containsBindingFrontmatter (str: string) =
        str.StartsWith(frontmatterBindingStart, StringComparison.Ordinal) && str.Contains(frontmatterBindingEnd)

    let tryExtractFromString (str: string) =
        let norm = str.ReplaceLineEndings("\n")
        if containsCommentFrontmatter norm then
            norm.Substring(
                frontMatterCommentStart.Length, 
                (norm.IndexOf(frontMatterCommentEnd, StringComparison.Ordinal) - frontMatterCommentStart.Length))
            |> Some
        elif containsBindingFrontmatter norm then
            norm.Substring(
                frontmatterBindingStart.Length, 
                (norm.IndexOf(frontmatterBindingEnd, StringComparison.Ordinal) - frontmatterBindingStart.Length))
            |> Some
        else 
            None

    let extractFromString (str: string) =
        match tryExtractFromString str with
        | Some frontmatter -> frontmatter
        | None -> failwith $"input has no correctly formatted frontmatter."

    let yamlDeserializer() = 
        DeserializerBuilder()
            .WithNamingConvention(NamingConventions.PascalCaseNamingConvention.Instance)
            .Build()

    type ValidationPackageMetadata with
        
        static member extractFromString (str: string) =
            let frontmatter = tryExtractFromString str
            match frontmatter with
            | Some frontmatter ->
                let result = 
                    yamlDeserializer().Deserialize<ValidationPackageMetadata>(frontmatter)
                result
            | None ->
                failwith $"string has no correctly formatted frontmatter."

        static member tryExtractFromString (str: string) =
            try 
                ValidationPackageMetadata.extractFromString str |> Some
            with e ->
                printfn $"error parsing package metadata: {e.Message}"
                None

        static member extractFromScript (scriptPath: string) =
            scriptPath
            |> File.ReadAllText
            |> ValidationPackageMetadata.extractFromString 

        static member tryExtractFromScript (scriptPath: string) =
            try 
                ValidationPackageMetadata.extractFromScript scriptPath |> Some
            with e ->
                printfn $"error parsing package metadata: {e.Message}"
                None

    type ValidationPackageIndex with

        static member create (
            repoPath: string, 
            lastUpdated: System.DateTimeOffset
        ) = 
            ValidationPackageIndex.create(
                repoPath = repoPath,
                lastUpdated = lastUpdated,
                metadata = ValidationPackageMetadata.extractFromScript(repoPath)
            )