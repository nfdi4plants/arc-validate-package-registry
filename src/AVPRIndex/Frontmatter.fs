namespace AVPRIndex

open Domain
open System
open System.IO
open System.Security.Cryptography
open YamlDotNet.Serialization

module Frontmatter = 
   
    let [<Literal>] frontMatterStart = "(*\n---"
    let [<Literal>] frontMatterEnd = "---\n*)"

    let containsFrontmatter (str: string) =
        str.StartsWith(frontMatterStart, StringComparison.Ordinal) && str.Contains(frontMatterEnd)

    let tryExtractFromString (str: string) =
        let norm = str.ReplaceLineEndings("\n")
        if containsFrontmatter norm then
            norm.Substring(
                frontMatterStart.Length, 
                (norm.IndexOf(frontMatterEnd, StringComparison.Ordinal) - frontMatterEnd.Length))
            |> Some
        else 
            None

    let extractFromString (str: string) =
        match tryExtractFromString str with
        | Some frontmatter -> frontmatter
        | None -> failwith $"input has no correctly formatted frontmatter."

    let yamlDeserializer = 
        DeserializerBuilder()
            .WithNamingConvention(NamingConventions.PascalCaseNamingConvention.Instance)
            .Build()

    type ValidationPackageMetadata with
        
        static member extractFromString (str: string) =
            let frontmatter = tryExtractFromString str
            match frontmatter with
            | Some frontmatter ->
                let result = 
                    yamlDeserializer.Deserialize<ValidationPackageMetadata>(frontmatter)
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

            let md5 = MD5.Create()

            ValidationPackageIndex.create(
                repoPath = repoPath,
                fileName = Path.GetFileName(repoPath),
                lastUpdated = lastUpdated,
                contentHash = (md5.ComputeHash(File.ReadAllBytes(repoPath)) |> Convert.ToHexString),
                metadata = ValidationPackageMetadata.extractFromScript(repoPath)
            )