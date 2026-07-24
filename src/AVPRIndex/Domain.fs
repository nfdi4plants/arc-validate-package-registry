namespace AVPRIndex

open System
open System.IO
open System.Text
open System.Text.Json
open System.Security.Cryptography

[<AutoOpen>]
module Domain =

    let jsonSerializerOptions = JsonSerializerOptions(WriteIndented = true)

    type SemVer() =
        member val Major = -1 with get,set
        member val Minor = -1 with get,set
        member val Patch = -1 with get,set
        member val PreRelease = "" with get,set
        member val BuildMetadata = "" with get,set

        override this.GetHashCode() =
            hash (
                this.Major, 
                this.Minor, 
                this.Patch, 
                this.PreRelease, 
                this.BuildMetadata
            )

        override this.Equals(other) =
            match other with
                | :? SemVer as s -> 
                    (
                        this.Major, 
                        this.Minor, 
                        this.Patch, 
                        this.PreRelease, 
                        this.BuildMetadata
                    ) = (
                        s.Major, 
                        s.Minor, 
                        s.Patch, 
                        s.PreRelease, 
                        s.BuildMetadata
                    )
                | _ -> false

        static member create (
            major: int,
            minor: int,
            patch: int,
            ?PreRelease: string,
            ?BuildMetadata: string
        ) =
            let tmp = SemVer(
                Major = major,
                Minor = minor,
                Patch = patch
            )
            PreRelease |> Option.iter (fun x -> tmp.PreRelease <- x)
            BuildMetadata |> Option.iter (fun x -> tmp.BuildMetadata <- x)
            tmp

        static member tryParse (version: string) =
            match version |> Globals.SEMVER_REGEX.Match |> fun m -> m.Success, m.Groups with
            | true, groups ->
                let major = groups.["major"].Value |> int
                let minor = groups.["minor"].Value |> int
                let patch = groups.["patch"].Value |> int
                let preRelease = groups.["prerelease"].Value
                let buildMetadata = groups.["buildmetadata"].Value
                Some(SemVer.create(major, minor, patch, preRelease, buildMetadata))
            | _ -> None

        static member toString (semVer: SemVer) =
            match (semVer.PreRelease, semVer.BuildMetadata) with
            | (pr, bm) when pr <> "" && bm <> "" -> $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}-{pr}+{bm}"
            | (pr, bm) when pr <> "" && bm = "" -> $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}-{pr}"
            | (pr, bm) when pr = "" && bm <> "" -> $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}+{bm}"
            | _ -> $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}"


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
            ?Email: string,
            ?Affiliation: string,
            ?AffiliationLink: string
        ) =
            let tmp = Author(
                FullName = fullName
            )
            Email |> Option.iter (fun x -> tmp.Email <- x)
            Affiliation |> Option.iter (fun x -> tmp.Affiliation <- x)
            AffiliationLink |> Option.iter (fun x -> tmp.AffiliationLink <- x)

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
            ?TermSourceRef: string,
            ?TermAccessionNumber: string
        ) =
            let tmp = new OntologyAnnotation(Name = name)
            TermSourceRef |> Option.iter (fun x -> tmp.TermSourceREF <- x)
            TermAccessionNumber |> Option.iter (fun x -> tmp.TermAccessionNumber <- x)
            tmp

    type CwlPrimitive =
        | Boolean = 0
        | Int = 1
        | Long = 2
        | Float = 3
        | Double = 4
        | String = 5

    type CommandInputType() =

        let mutable primitiveType = CwlPrimitive.String

        member _.PrimitiveType
            with get () = primitiveType
            and set value =
                if not (Enum.IsDefined(typeof<CwlPrimitive>, value)) then
                    invalidArg "PrimitiveType" $"Invalid primitive type: {value}"

                primitiveType <- value

        member val IsNullable = false with get,set

        override this.GetHashCode() =
            hash (
                this.PrimitiveType,
                this.IsNullable
            )

        override this.Equals(other) =
            match other with
            | :? CommandInputType as c ->
                (
                    this.PrimitiveType,
                    this.IsNullable
                ) = (
                    c.PrimitiveType,
                    c.IsNullable
                )
            | _ -> false

        static member create (
            primitiveType: CwlPrimitive,
            ?IsNullable: bool
        ) =
            let tmp = CommandInputType(PrimitiveType = primitiveType)
            IsNullable |> Option.iter (fun x -> tmp.IsNullable <- x)
            tmp


    type CommandInputBinding() =
        member val Position: int = 0 with get,set
        member val Prefix: string = "" with get,set
        member val Separate: bool = true with get,set

        override this.GetHashCode() =
            hash (
                this.Position,
                this.Prefix,
                this.Separate
            )

        override this.Equals(other) =
            match other with
            | :? CommandInputBinding as c ->
                (
                    this.Position,
                    this.Prefix,
                    this.Separate
                ) = (
                    c.Position,
                    c.Prefix,
                    c.Separate
                )
            | _ -> false

        static member create (
            ?Position: int,
            ?Prefix: string,
            ?Separate: bool
        ) =
            let tmp = CommandInputBinding()
            Position |> Option.iter (fun x -> tmp.Position <- x)
            Prefix |> Option.iter (fun x -> tmp.Prefix <- x)
            Separate |> Option.iter (fun x -> tmp.Separate <- x)
            tmp

    type CommandInputParameter() =
        member val Id: string = "" with get,set
        member val Type: CommandInputType = (CommandInputType.create(CwlPrimitive.String, false)) with get,set
        member val Label: string = "" with get,set
        member val Doc: string = "" with get,set
        member val InputBinding = CommandInputBinding() with get,set

        override this.GetHashCode() =
            hash (
                this.Id,
                this.Type,
                this.Label,
                this.Doc,
                this.InputBinding
            )

        override this.Equals(other) =
            match other with
            | :? CommandInputParameter as i ->
                (
                    this.Id,
                    this.Type,
                    this.Label,
                    this.Doc,
                    this.InputBinding
                ) = (
                    i.Id,
                    i.Type,
                    i.Label,
                    i.Doc,
                    i.InputBinding
                )
            | _ -> false

        static member create (
            id: string,
            inputType: CommandInputType,
            inputBinding: CommandInputBinding,
            ?Label: string,
            ?Doc: string
        ) =
            let tmp = CommandInputParameter(Id = id, Type = inputType, InputBinding = inputBinding)
            Label |> Option.iter (fun x -> tmp.Label <- x)
            Doc |> Option.iter (fun x -> tmp.Doc <- x)

            tmp

    type CLIArgument() =

        member val Flags: string [] = Array.empty<string> with get,set
        member val Description = "" with get,set
        member val Example = "" with get,set

        override this.GetHashCode() =
            hash (
                this.Flags,
                this.Description,
                this.Example
            )

        override this.Equals(other) =
            match other with
            | :? CLIArgument as cli ->
                (
                    this.Flags,
                    this.Description,
                    this.Example
                ) = (
                    cli.Flags,
                    cli.Description,
                    cli.Example
                )
            | _ -> false

        static member create (
            flags: string [],
            ?Description: string,
            ?Example: string
        ) =
            let tmp = CLIArgument(Flags = flags)
            Description |> Option.iter (fun x -> tmp.Description <- x)
            Example |> Option.iter (fun x -> tmp.Example <- x)
            tmp

    type ValidationPackageMetadata() =
        // mandatory fields
        member val Name = "" with get,set
        member val Summary = "" with get,set
        member val Description = "" with get,set
        member val MajorVersion = -1 with get,set
        member val MinorVersion = -1 with get,set
        member val PatchVersion = -1 with get,set
        member val PreReleaseVersionSuffix = "" with get,set
        member val BuildMetadataVersionSuffix = "" with get,set
        member val ProgrammingLanguage = "" with get,set
        // optional fields
        member val Publish = false with get,set
        member val Authors: Author [] = Array.empty<Author> with get,set
        member val Tags: OntologyAnnotation [] = Array.empty<OntologyAnnotation> with get,set
        member val ReleaseNotes = "" with get,set
        member val CQCHookEndpoint = "" with get,set
        member val Inputs: CommandInputParameter [] = Array.empty<CommandInputParameter> with get,set

        override this.GetHashCode() =
            hash (
                this.Name, 
                this.Summary, 
                this.Description, 
                this.MajorVersion, 
                this.MinorVersion, 
                this.PatchVersion, 
                this.PreReleaseVersionSuffix,
                this.BuildMetadataVersionSuffix,
                this.ProgrammingLanguage,
                this.Publish,
                this.Authors,
                this.Tags,
                this.ReleaseNotes,
                this.CQCHookEndpoint,
                this.Inputs
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
                    this.PreReleaseVersionSuffix,
                    this.BuildMetadataVersionSuffix,
                    this.ProgrammingLanguage,
                    this.Publish,
                    this.Authors,
                    this.Tags,
                    this.ReleaseNotes,
                    this.CQCHookEndpoint,
                    this.Inputs
                ) = (
                    vpm.Name, 
                    vpm.Summary, 
                    vpm.Description, 
                    vpm.MajorVersion, 
                    vpm.MinorVersion, 
                    vpm.PatchVersion, 
                    vpm.PreReleaseVersionSuffix,
                    vpm.BuildMetadataVersionSuffix,
                    vpm.ProgrammingLanguage,
                    vpm.Publish,
                    vpm.Authors,
                    vpm.Tags,
                    vpm.ReleaseNotes,
                    vpm.CQCHookEndpoint,
                    vpm.Inputs
                )
            | _ -> false
        
        static member create (
            name: string,
            summary: string,
            description: string, 
            majorVersion: int, 
            minorVersion: int, 
            patchVersion: int,
            programmingLanguage: string,
            ?PreReleaseVersionSuffix: string,
            ?BuildMetadataVersionSuffix: string,
            ?Publish: bool,
            ?Authors: Author [],
            ?Tags: OntologyAnnotation [],
            ?ReleaseNotes,
            ?CQCHookEndpoint,
            ?Inputs: CommandInputParameter []
        ) =
            let tmp = ValidationPackageMetadata(
                Name = name,
                Summary = summary,
                Description = description,
                MajorVersion = majorVersion,
                MinorVersion = minorVersion,
                PatchVersion = patchVersion,
                ProgrammingLanguage = programmingLanguage
            )

            PreReleaseVersionSuffix |> Option.iter (fun x -> tmp.PreReleaseVersionSuffix <- x)
            BuildMetadataVersionSuffix |> Option.iter (fun x -> tmp.BuildMetadataVersionSuffix <- x)
            Publish |> Option.iter (fun x -> tmp.Publish <- x)
            Authors |> Option.iter (fun x -> tmp.Authors <- x)
            Tags |> Option.iter (fun x -> tmp.Tags <- x)
            ReleaseNotes |> Option.iter (fun x -> tmp.ReleaseNotes <- x)
            CQCHookEndpoint |> Option.iter (fun x -> tmp.CQCHookEndpoint <- x)
            Inputs |> Option.iter (fun x -> tmp.Inputs <- x)

            tmp
        
        static member tryGetSemanticVersion(m: ValidationPackageMetadata) = 
            SemVer.create(
                m.MajorVersion,
                m.MinorVersion,
                m.PatchVersion,
                m.PreReleaseVersionSuffix,
                m.BuildMetadataVersionSuffix
            )
            |> SemVer.toString
            |> SemVer.tryParse // there is no buit-in validation on the constructor/create function, so we'll take a detour via parsing roundtrip using the regex

        static member getSemanticVersion(m: ValidationPackageMetadata) = 
            m
            |> ValidationPackageMetadata.tryGetSemanticVersion
            |> Option.get

        static member tryGetSemanticVersionString(m: ValidationPackageMetadata) = 
            m
            |> ValidationPackageMetadata.tryGetSemanticVersion
            |> Option.map SemVer.toString

        static member getSemanticVersionString(m: ValidationPackageMetadata) = 
            m
            |> ValidationPackageMetadata.tryGetSemanticVersion
            |> Option.map SemVer.toString
            |> Option.get


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
                ValidationPackageIndex.create(
                    repoPath = repoPath,
                    fileName = Path.GetFileName(repoPath),
                    lastUpdated = lastUpdated,
                    contentHash = Hash.hashFile repoPath,
                    metadata = metadata
                )
                
            /// returns true when the two packages will have the same stable identifier (consisting of name and semver from their metadata fields)
            static member identityEquals (first: ValidationPackageIndex) (second: ValidationPackageIndex) =
                first.Metadata.Name = second.Metadata.Name
                && first.Metadata.MajorVersion = second.Metadata.MajorVersion
                && first.Metadata.MinorVersion = second.Metadata.MinorVersion
                && first.Metadata.PatchVersion = second.Metadata.PatchVersion
                && first.Metadata.PreReleaseVersionSuffix = second.Metadata.PreReleaseVersionSuffix
                && first.Metadata.BuildMetadataVersionSuffix = second.Metadata.BuildMetadataVersionSuffix

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
                
            static member tryGetSemanticVersion(i: ValidationPackageIndex) = 
                i.Metadata |> ValidationPackageMetadata.tryGetSemanticVersion

            static member getSemanticVersion(i: ValidationPackageIndex) = 
                i.Metadata |> ValidationPackageMetadata.getSemanticVersion

            static member tryGetSemanticVersionString(i: ValidationPackageIndex) = 
                i.Metadata |> ValidationPackageMetadata.tryGetSemanticVersionString

            static member getSemanticVersionString(i: ValidationPackageIndex) = 
                i.Metadata |> ValidationPackageMetadata.getSemanticVersionString

            member this.PrettyPrint() =
                $" {this.Metadata.Name} @ version {this.Metadata.MajorVersion}.{this.Metadata.MinorVersion}.{this.Metadata.PatchVersion}{System.Environment.NewLine}{_.Metadata.Description}{System.Environment.NewLine}Last Updated: {this.LastUpdated}{System.Environment.NewLine}"
