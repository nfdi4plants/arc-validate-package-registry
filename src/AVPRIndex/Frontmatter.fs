namespace AVPRIndex

open Domain
open System
open System.Text
open System.IO
open System.Security.Cryptography
open YamlDotNet.Serialization

[<AutoOpen>]
module Frontmatter = 

    type FrontmatterLanguage =
        | FSharpFrontmatter
        | PythonFrontmatter

        static member fromString (str: string) =
            match str.ToLowerInvariant() with
            | "fsharp" | "fs" | "f#" -> FrontmatterLanguage.FSharpFrontmatter
            | "python" | "py" -> FrontmatterLanguage.PythonFrontmatter
            | _ -> failwith $"unsupported frontmatter language: {str}"

        static member toString (lang: FrontmatterLanguage) =
            match lang with
            | FSharpFrontmatter -> "FSharp"
            | PythonFrontmatter -> "Python"

    module FSharp =
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
            | None -> failwith $"""
input 

{str}

has no correctly formatted FSharp frontmatter."""

    module Python =
        /// the frontmatter start string if the package uses yaml frontmatter as comment
        let [<Literal>] frontMatterCommentStart = "\"\"\"\n---"
        /// the frontmatter end string if the package uses yaml frontmatter as comment
        let [<Literal>] frontMatterCommentEnd = "---\n\"\"\""

        /// the frontmatter start string if the package uses yaml frontmatter as a string binding to be re-used in the package code
        let [<Literal>] frontmatterBindingStart = "PACKAGE_METADATA = \"\"\"\n---"
        /// the frontmatter end string if the package uses yaml frontmatter as a string binding to be re-used in the package code
        let [<Literal>] frontmatterBindingEnd = "---\n\"\"\""

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
            | None -> failwith $"""
input 

{str}

has no correctly formatted Python frontmatter."""

    let tryExtractFromString (lang:FrontmatterLanguage) (str: string) =
        match lang with
        | FSharpFrontmatter -> FSharp.tryExtractFromString str
        | PythonFrontmatter -> Python.tryExtractFromString str

    let extractFromString (lang:FrontmatterLanguage) (str: string) =
        match lang with
        | FSharpFrontmatter -> FSharp.extractFromString str
        | PythonFrontmatter -> Python.extractFromString str

    let yamlDeserializer() = 
        DeserializerBuilder()
            .WithNamingConvention(NamingConventions.PascalCaseNamingConvention.Instance)
            .Build()

    type ValidationPackageMetadata with
        
        static member extractFromString (lang: FrontmatterLanguage) (str: string) =
            let frontmatter = tryExtractFromString lang str
            match frontmatter with
            | Some frontmatter ->
                let result = 
                    yamlDeserializer().Deserialize<ValidationPackageMetadata>(frontmatter)
                result.ProgrammingLanguage <- FrontmatterLanguage.toString lang
                result
            | None ->
                failwith $"""
string 

{str}

has no correctly formatted {lang}."""

        static member tryExtractFromString (lang: FrontmatterLanguage) (str: string) =
            try 
                let vpm = ValidationPackageMetadata.extractFromString lang str 
                Some vpm
            with e ->
                printfn $"error parsing package metadata: {e.Message}"
                None

        static member extractFromScript (scriptPath: string) =

            let lang = 
                match Path.GetExtension(scriptPath).ToLowerInvariant() with
                | ".fsx" -> FrontmatterLanguage.FSharpFrontmatter
                | ".py" -> FrontmatterLanguage.PythonFrontmatter
                | ext -> failwith $"unsupported script extension: {ext}"

            scriptPath
            |> File.ReadAllText
            |> ValidationPackageMetadata.extractFromString lang

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