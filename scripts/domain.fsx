#r "nuget: YamlDotNet, 13.7.1"
#r "nuget: Fake.Core.Process, 6.0.0"
//#r "nuget: ARCValidationPackages, 2.0.0-preview.1" <-- use this (or ARCtrl) once the data model is stable

open System
open System.IO
open YamlDotNet.Serialization
// open ARCValidationPackages <-- use this (or ARCtrl) once the data model is stable

// This is the F# version of /src/PackageRegistryService/Models/ValidationPackageIndex.cs
// and should be equal to the implementation in arc-validate (until there is a common codebase/domain)
[<AutoOpen>]
module Domain = 

    type Author() =
        member val FullName = "" with get,set
        member val Email = "" with get,set
        member val Affiliation = "" with get,set
        member val AffiliationLink = "" with get,set

    type ValidationPackageMetadata() =
        // mandatory fields
        member val Name = "" with get,set
        member val Description = "" with get,set
        member val MajorVersion = 0 with get,set
        member val MinorVersion = 0 with get,set
        member val PatchVersion = 0 with get,set
        // optional fields
        member val Publish = false with get,set
        member val Authors: Author [] = Array.empty<Author> with get,set
        member val Tags: string [] = Array.empty<string> with get,set
        member val ReleaseNotes = "" with get,set

        override this.GetHashCode() =
            hash (
                this.Name, 
                this.Description, 
                this.MajorVersion, 
                this.MinorVersion, 
                this.PatchVersion, 
                this.Publish,
                this.Authors,
                this.Tags,
                this.ReleaseNotes
            )

        override this.Equals(other) =
            match other with
            | :? ValidationPackageMetadata as vpm -> 
                (
                    this.Name, 
                    this.Description, 
                    this.MajorVersion, 
                    this.MinorVersion, 
                    this.PatchVersion, 
                    this.Publish,
                    this.Authors,
                    this.Tags,
                    this.ReleaseNotes
                ) = (
                    vpm.Name, 
                    vpm.Description, 
                    vpm.MajorVersion, 
                    vpm.MinorVersion, 
                    vpm.PatchVersion, 
                    vpm.Publish,
                    vpm.Authors,
                    vpm.Tags,
                    vpm.ReleaseNotes
                )
            | _ -> false

    type ValidationPackageIndex =
        {
            RepoPath: string
            FileName:string
            LastUpdated: System.DateTimeOffset
            Metadata: ValidationPackageMetadata
        } with
            static member create (
                repoPath: string, 
                fileName: string, 
                lastUpdated: System.DateTimeOffset,
                metadata: ValidationPackageMetadata

            ) = 
                { 
                    RepoPath = repoPath 
                    FileName = fileName
                    LastUpdated = lastUpdated 
                    Metadata = metadata
                }
            static member create (
                repoPath: string, 
                lastUpdated: System.DateTimeOffset,
                metadata: ValidationPackageMetadata
            ) = 
                ValidationPackageIndex.create(
                    repoPath = repoPath,
                    fileName = Path.GetFileNameWithoutExtension(repoPath),
                    lastUpdated = lastUpdated,
                    metadata = metadata
                )

[<AutoOpen>]
module Frontmatter = 
   
    let frontMatterStart = $"(*{System.Environment.NewLine}---"
    let frontMatterEnd = $"---{System.Environment.NewLine}*)"

    let yamlDeserializer = 
        DeserializerBuilder()
            .WithNamingConvention(NamingConventions.PascalCaseNamingConvention.Instance)
            .Build()

    type ValidationPackageMetadata with
        
        static member extractFromScript (scriptPath: string) =
            let script = File.ReadAllText(scriptPath)
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
                    ValidationPackageMetadata()
            else 
                printfn $"script at {scriptPath} has no correctly formatted frontmatter."
                ValidationPackageMetadata()

    type ValidationPackageIndex with

        static member create (
            repoPath: string, 
            lastUpdated: System.DateTimeOffset
        ) = 
            ValidationPackageIndex.create(
                repoPath = repoPath,
                fileName = Path.GetFileName(repoPath),
                lastUpdated = lastUpdated,
                metadata = ValidationPackageMetadata.extractFromScript(repoPath)
            )
