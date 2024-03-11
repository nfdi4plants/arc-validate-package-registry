namespace AVPRIndex


open Domain
open System
open System.IO
open System.Security.Cryptography
open YamlDotNet.Serialization

module Frontmatter = 
   
    let frontMatterStart = $"(*{System.Environment.NewLine}---"
    let frontMatterEnd = $"---{System.Environment.NewLine}*)"

    let yamlDeserializer = 
        DeserializerBuilder()
            .WithNamingConvention(NamingConventions.PascalCaseNamingConvention.Instance)
            .Build()

    type ValidationPackageMetadata with
        
        static member extractFromScript (scriptPath: string) =
            let script = File.ReadAllText(scriptPath).ReplaceLineEndings()
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
                    printfn $"Error msg: {e.Message}."
                    ValidationPackageMetadata()
            else 
                printfn $"script at {scriptPath} has no correctly formatted frontmatter."
                ValidationPackageMetadata()

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