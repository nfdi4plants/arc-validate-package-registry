#r "nuget: YamlDotNet, 13.7.1"
#r "nuget: Fake.Core.Process, 6.0.0"
//#r "nuget: ARCValidationPackages, 2.0.0-preview.1" <-- use this (or ARCtrl) once the data model is stable

open System
open System.IO
open YamlDotNet.Serialization
open System.Text.Json
// open ARCValidationPackages <-- use this (or ARCtrl) once the data model is stable

// This is the F# version of /src/PackageRegistryService/Models/ValidationPackageIndex.cs
// and should be equal to the implementation in arc-validate (until there is a common codebase/domain)
[<AutoOpen>]
module Domain = 

    let jsonSerializerOptions = JsonSerializerOptions(WriteIndented = true)

    type Author() =
        member val FullName = "" with get,set
        member val Email = "" with get,set
        member val Affiliation = "" with get,set
        member val AffiliationLink = "" with get,set

        override this.GetHashCode() =
            hash (
                this.FullName, 
                this.Email, 
                this.Affiliation, 
                this.AffiliationLink
            )

        override this.Equals(other) =
            match other with
            | :? Author as a -> 
                (
                    this.FullName, 
                    this.Email, 
                    this.Affiliation, 
                    this.AffiliationLink
                ) = (
                    a.FullName, 
                    a.Email, 
                    a.Affiliation, 
                    a.AffiliationLink
                )
            | _ -> false

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
            FileName: string
            LastUpdated: System.DateTimeOffset
            ContentHash: string
            Metadata: ValidationPackageMetadata
        } with
            static member create (
                repoPath: string, 
                fileName: string, 
                lastUpdated: System.DateTimeOffset,
                contentHash: string,
                metadata: ValidationPackageMetadata
            ) = 
                { 
                    RepoPath = repoPath 
                    FileName = fileName
                    LastUpdated = lastUpdated 
                    ContentHash = contentHash
                    Metadata = metadata
                }
            static member create (
                repoPath: string, 
                lastUpdated: System.DateTimeOffset,
                metadata: ValidationPackageMetadata
            ) = 

                let md5 = System.Security.Cryptography.MD5.Create()

                ValidationPackageIndex.create(
                    repoPath = repoPath,
                    fileName = Path.GetFileNameWithoutExtension(repoPath),
                    lastUpdated = lastUpdated,
                    contentHash = (md5.ComputeHash(File.ReadAllBytes(repoPath)) |> Convert.ToHexString),
                    metadata = metadata
                )
                
            /// returns true when the two packages will have the same stable identifier (consisting of name and semver from their metadata fields)
            static member identityEquals (first: ValidationPackageIndex) (second: ValidationPackageIndex) =
                first.Metadata.Name = second.Metadata.Name
                && first.Metadata.MajorVersion = second.Metadata.MajorVersion
                && first.Metadata.MinorVersion = second.Metadata.MinorVersion
                && first.Metadata.PatchVersion = second.Metadata.PatchVersion

            /// returns true when the two packages have the same content hash
            static member contentEquals (first: ValidationPackageIndex) (second: ValidationPackageIndex) =
                first.ContentHash = second.ContentHash

            static member toJson (i: ValidationPackageIndex) = 
                JsonSerializer.Serialize(i, jsonSerializerOptions)

            static member printJson (i: ValidationPackageIndex) = 
                let json = ValidationPackageIndex.toJson i
                printfn ""
                printfn $"Indexed Package info:{System.Environment.NewLine}{json}"
                printfn ""

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

            let md5 = System.Security.Cryptography.MD5.Create()

            ValidationPackageIndex.create(
                repoPath = repoPath,
                fileName = Path.GetFileName(repoPath),
                lastUpdated = lastUpdated,
                contentHash = (md5.ComputeHash(File.ReadAllBytes(repoPath)) |> Convert.ToHexString),
                metadata = ValidationPackageMetadata.extractFromScript(repoPath)
            )

module Utils =
    
    let truncateDateTime (date: System.DateTimeOffset) =
        DateTimeOffset.ParseExact(
            date.ToString("yyyy-MM-dd HH:mm:ss zzzz"), 
            "yyyy-MM-dd HH:mm:ss zzzz", 
            System.Globalization.CultureInfo.InvariantCulture
        )

module AVPRRepo =

    ///! Paths are relative to the root of the project, since the script is executed from the repo root in CI
    let getStagedPackages() = 
        Directory.GetFiles("src/PackageRegistryService/StagingArea", "*.fsx", SearchOption.AllDirectories)
        |> Array.map (fun x -> x.Replace('\\',Path.DirectorySeparatorChar).Replace('/',Path.DirectorySeparatorChar))
        |> Array.map (fun p -> 
            ValidationPackageIndex.create(
                repoPath = p.Replace(Path.DirectorySeparatorChar, '/'), // use front slash always here, otherwise the backslash will be escaped with another backslah on windows when writing the json
                lastUpdated = Utils.truncateDateTime System.DateTimeOffset.Now // take local time with offset if file will be changed with this commit
            )
        )
    
    ///! Paths are relative to the root of the project, since the script is executed from the repo root in CI
    let getIndexedPackages() = 
        "src/PackageRegistryService/Data/arc-validate-package-index.json"
        |> File.ReadAllText
        |> JsonSerializer.Deserialize<ValidationPackageIndex[]>

