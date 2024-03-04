﻿namespace AVPRIndex

open System
open System.IO
open System.Text.Json
open System.Security.Cryptography

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

        static member create (
            fullName: string,
            ?email: string,
            ?affiliation: string,
            ?affiliationLink: string
        ) =
            let tmp = new Author()

            tmp.FullName <- fullName
            if email.IsSome then
                tmp.Email <- email.Value
            if affiliation.IsSome then
                tmp.Affiliation <- affiliation.Value
            if affiliationLink.IsSome then
                tmp.AffiliationLink <- affiliationLink Value

            tmp

    type OntologyAnnotation() =

        member val Name = "" with get,set
        member val TermSourceREF = "" with get,set
        member val TermAccessionNumber = "" with get,set

        override this.GetHashCode() =
            hash (
                this.Name, 
                this.TermSourceREF, 
                this.TermAccessionNumber
            )

        override this.Equals(other) =
            match other with
            | :? OntologyAnnotation as oa -> 
                (
                    this.Name, 
                    this.TermSourceREF, 
                    this.TermAccessionNumber
                ) = (
                    oa.Name, 
                    oa.TermSourceREF, 
                    oa.TermAccessionNumber
                )
            | _ -> false

        static member create (
            name: string,
            ?termSourceRef: string,
            ?termAccessionNumber: string
        ) =
            let tmp = new OntologyAnnotation()

            tmp.Name <- name
            if termSourceRef.IsSome then
                tmp.TermSourceREF <- termSourceRef.Value
            if termAccessionNumber.IsSome then
                tmp.TermAccessionNumber <- termAccessionNumber.Value

            tmp

    type ValidationPackageMetadata() =
        // mandatory fields
        member val Name = "" with get,set
        member val Summary = "" with get,set
        member val Description = "" with get,set
        member val MajorVersion = 0 with get,set
        member val MinorVersion = 0 with get,set
        member val PatchVersion = 0 with get,set
        // optional fields
        member val Publish = false with get,set
        member val Authors: Author [] = Array.empty<Author> with get,set
        member val Tags: OntologyAnnotation [] = Array.empty<OntologyAnnotation> with get,set
        member val ReleaseNotes = "" with get,set

        override this.GetHashCode() =
            hash (
                this.Name, 
                this.Summary, 
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
                    this.Summary, 
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
                    vpm.Summary, 
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
        
        static member create (
            name: string,
            summary: string,
            description: string, 
            majorVersion: int, 
            minorVersion: int, 
            patchVersion: int,
            ?Publish: bool,
            ?Authors: Author [],
            ?Tags: OntologyAnnotation [],
            ?ReleaseNotes
        ) = 
            let tmp = ValidationPackageMetadata()
            tmp.Name <- name
            tmp.Summary <- summary
            tmp.Description <- description
            tmp.MajorVersion <- majorVersion
            tmp.MinorVersion <- minorVersion
            tmp.PatchVersion <- patchVersion
            Publish |> Option.iter (fun x -> tmp.Publish <- x)
            Authors |> Option.iter (fun x -> tmp.Authors <- x)
            Tags |> Option.iter (fun x -> tmp.Tags <- x)
            ReleaseNotes |> Option.iter (fun x -> tmp.ReleaseNotes <- x)
        
            tmp
        
        static member getSemanticVersionString(m: ValidationPackageMetadata) = $"{m.MajorVersion}.{m.MinorVersion}.{m.PatchVersion}"

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

                let md5 = MD5.Create()

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
                
            static member getSemanticVersionString(i: ValidationPackageIndex) = $"{i.Metadata.MajorVersion}.{i.Metadata.MinorVersion}.{i.Metadata.PatchVersion}";

            member this.PrettyPrint() =
                $" {this.Metadata.Name} @ version {this.Metadata.MajorVersion}.{this.Metadata.MinorVersion}.{this.Metadata.PatchVersion}{System.Environment.NewLine}{_.Metadata.Description}{System.Environment.NewLine}Last Updated: {this.LastUpdated}{System.Environment.NewLine}"
