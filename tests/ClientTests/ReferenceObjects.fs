module ReferenceObjects

let releaseDate =
    System.DateTimeOffset(2026, 7, 30, 12, 0, 0, System.TimeSpan.Zero)

let packageContent = System.Text.Encoding.UTF8.GetBytes("printfn \"validated\"\n")

module Author =

    let mandatoryClient = AVPRClient.Author(FullName = "Ada Lovelace")

    let allFieldsClient =
        AVPRClient.Author(
            FullName = "Ada Lovelace",
            Email = "ada@example.org",
            Affiliation = "Analytical Engine Institute",
            AffiliationLink = "https://example.org/institute"
        )

    let mandatoryModel =
        ValidationPackage.Model.Author(FullName = "Ada Lovelace")

    let allFieldsModel =
        ValidationPackage.Model.Author(
            FullName = "Ada Lovelace",
            Email = "ada@example.org",
            Affiliation = "Analytical Engine Institute",
            AffiliationLink = "https://example.org/institute"
        )

module OntologyAnnotation =

    let mandatoryClient = AVPRClient.OntologyAnnotation(Name = "validation")

    let allFieldsClient =
        AVPRClient.OntologyAnnotation(
            Name = "validation",
            TermSourceREF = "NCIT",
            TermAccessionNumber = "NCIT:C16205"
        )

    let mandatoryModel =
        ValidationPackage.Model.OntologyAnnotation(Name = "validation")

    let allFieldsModel =
        ValidationPackage.Model.OntologyAnnotation(
            Name = "validation",
            TermSourceREF = "NCIT",
            TermAccessionNumber = "NCIT:C16205"
        )

module CommandInput =

    let allFieldsClient =
        AVPRClient.CommandInputParameter(
            Id = "output",
            Type = AVPRClient.CommandInputType.String_,
            Label = "Output file",
            Doc = "Write output to this file",
            InputBinding =
                AVPRClient.CommandInputBinding(
                    Position = 2,
                    Prefix = "--output"
                )
        )

    let allFieldsModel =
        ValidationPackage.Model.CommandInputParameter(
            Id = "output",
            Type =
                ValidationPackage.Model.CommandInputType(
                    PrimitiveType = ValidationPackage.Model.CwlPrimitive.String,
                    IsNullable = true
                ),
            Label = "Output file",
            Doc = "Write output to this file",
            InputBinding =
                ValidationPackage.Model.CommandInputBinding(
                    Position = 2,
                    Prefix = "--output"
                )
        )

module Metadata =

    let allFields =
        ValidationPackage.Model.ValidationPackageMetadata(
            Name = "portable-package",
            Summary = "Portable package",
            Description = "Exercises the full model/client contract.",
            MajorVersion = 1,
            MinorVersion = 2,
            PatchVersion = 3,
            PreReleaseVersionSuffix = "rc.1",
            BuildMetadataVersionSuffix = "build.7",
            ProgrammingLanguage = "FSharp",
            Authors = [| Author.allFieldsModel |],
            Tags = [| OntologyAnnotation.allFieldsModel |],
            ReleaseNotes = "First portable interop release.",
            CQCHookEndpoint = "https://example.org/hooks/cqc",
            Inputs = [| CommandInput.allFieldsModel |]
        )

module Identity =

    let model =
        ValidationPackage.Model.ValidationPackageIdentity.create(
            "portable-package",
            ValidationPackage.Model.SemVer.tryParse("1.2.3-rc.1+build.7")
            |> Option.get
        )

    let client =
        AVPRClient.ValidationPackageIdentity(
            Name = "portable-package",
            Version = "1.2.3-rc.1+build.7"
        )

module MetadataEndpoint =

    let allFields =
        AVPRClient.ValidationPackageMetadata(
            Name = "portable-package",
            Version = "1.2.3-rc.1+build.7",
            Summary = "Portable package",
            Description = "Exercises the full model/client contract.",
            ReleaseDate = releaseDate,
            ProgrammingLanguage = "FSharp",
            Authors = [| Author.allFieldsClient |],
            Tags = [| OntologyAnnotation.allFieldsClient |],
            ReleaseNotes = "First portable interop release.",
            CQCHookEndpoint = "https://example.org/hooks/cqc",
            Inputs = ResizeArray [ CommandInput.allFieldsClient ]
        )

module ValidationPackage =

    let allFields =
        AVPRClient.ValidationPackage(
            Name = "portable-package",
            Summary = "Portable package",
            Description = "Exercises the full model/client contract.",
            MajorVersion = 1,
            MinorVersion = 2,
            PatchVersion = 3,
            PreReleaseVersionSuffix = "rc.1",
            BuildMetadataVersionSuffix = "build.7",
            ProgrammingLanguage = "FSharp",
            PackageContent = packageContent,
            ReleaseDate = releaseDate,
            Authors = [| Author.allFieldsClient |],
            Tags = [| OntologyAnnotation.allFieldsClient |],
            ReleaseNotes = "First portable interop release.",
            CQCHookEndpoint = "https://example.org/hooks/cqc",
            Inputs = ResizeArray [ CommandInput.allFieldsClient ]
        )

    let differentVersion =
        AVPRClient.ValidationPackage(
            Name = "portable-package",
            MajorVersion = 2,
            MinorVersion = 0,
            PatchVersion = 0,
            PreReleaseVersionSuffix = "",
            BuildMetadataVersionSuffix = "",
            ProgrammingLanguage = "FSharp"
        )
