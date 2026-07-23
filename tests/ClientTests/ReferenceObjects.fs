module ReferenceObjects

open Utils
open AVPRIndex
open AVPRClient

let date = System.DateTime.ParseExact("2021-01-01", "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)

let testDate = System.DateTimeOffset.Parse("01/01/2024")

module Author = 
    
    let mandatoryFieldsClient = AVPRClient.Author(FullName = "test")

    let allFieldsClient =
        AVPRClient.Author(
            FullName = "test",
            Email = "test@test.test",
            Affiliation = "testaffiliation",
            AffiliationLink = "test.com"
        )

    let mandatoryFieldsIndex = AVPRIndex.Domain.Author(FullName = "test")

    let allFieldsIndex =
        AVPRIndex.Domain.Author(
            FullName = "test",
            Email = "test@test.test",
            Affiliation = "testaffiliation",
            AffiliationLink = "test.com"
        )

module OntologyAnnotation = 
    
    let mandatoryFieldsClient = AVPRClient.OntologyAnnotation(Name = "test")

    let allFieldsClient = AVPRClient.OntologyAnnotation(
        Name = "test",
        TermSourceREF = "REF",
        TermAccessionNumber = "TAN"
    )

    let mandatoryFieldsIndex = AVPRIndex.Domain.OntologyAnnotation(Name = "test")

    let allFieldsIndex = AVPRIndex.Domain.OntologyAnnotation(
        Name = "test",
        TermSourceREF = "REF",
        TermAccessionNumber = "TAN"
    )

module CLIArgument =

    let mandatoryFieldsClient = AVPRClient.CLIArgument(Flags = ResizeArray [ "-i" ])

    let allFieldsClient =
        AVPRClient.CLIArgument(
            Flags = ResizeArray [ "-i"; "--input" ],
            Description = "Input ARC path",
            Example = "./my-arc"
        )

    let mandatoryFieldsIndex = AVPRIndex.Domain.CLIArgument(Flags = [| "-i" |])

    let allFieldsIndex =
        AVPRIndex.Domain.CLIArgument(
            Flags = [| "-i"; "--input" |],
            Description = "Input ARC path",
            Example = "./my-arc"
        )

module ValidationPackageMetadata =

    let mandatoryFields = ValidationPackageMetadata(
        Name = "name",
        Summary = "summary" ,
        Description = "description" ,
        MajorVersion = 1,
        MinorVersion = 0,
        PatchVersion = 0,
        ProgrammingLanguage = "FSharp"
    )

    let allFields_cqcHookAddition = ValidationPackageMetadata(
        Name = "name",
        Summary = "summary" ,
        Description = "description" ,
        MajorVersion = 1,
        MinorVersion = 0,
        PatchVersion = 0,
        ProgrammingLanguage = "FSharp",
        Publish = true,
        Authors = [|Author.allFieldsIndex|],
        Tags = [|OntologyAnnotation.allFieldsIndex|],
        ReleaseNotes = "releasenotes",
        CQCHookEndpoint = "hookendpoint",
        CLIArguments = [| |]
    )

    let allFields_semVerAddition = ValidationPackageMetadata(
        Name = "name",
        Summary = "summary" ,
        Description = "description" ,
        MajorVersion = 1,
        MinorVersion = 0,
        PatchVersion = 0,
        ProgrammingLanguage = "FSharp",
        PreReleaseVersionSuffix = "use",
        BuildMetadataVersionSuffix = "suffixes",
        Publish = true,
        Authors = [|Author.allFieldsIndex|],
        Tags = [|OntologyAnnotation.allFieldsIndex|],
        ReleaseNotes = "releasenotes",
        CQCHookEndpoint = "hookendpoint",
        CLIArguments = [| |]
    )

    let allFields_cliargsAddition = ValidationPackageMetadata(
        Name = "name",
        Summary = "summary" ,
        Description = "description" ,
        MajorVersion = 1,
        MinorVersion = 0,
        PatchVersion = 0,
        ProgrammingLanguage = "FSharp",
        PreReleaseVersionSuffix = "use",
        BuildMetadataVersionSuffix = "suffixes",
        Publish = true,
        Authors = [|Author.allFieldsIndex|],
        Tags = [|OntologyAnnotation.allFieldsIndex|],
        ReleaseNotes = "releasenotes",
        CQCHookEndpoint = "hookendpoint",
        CLIArguments = [| CLIArgument.allFieldsIndex |]
    )

module Hash =

    let expected_hash_cqcHookAddition = "D76692DF8B591B0C63789C638190C6BF"

    let allFields_cqcHookAddition = AVPRClient.PackageContentHash(
        PackageName = "name",
        PackageMajorVersion = 1,
        PackageMinorVersion = 0,
        PackagePatchVersion = 0,
        PackagePreReleaseVersionSuffix = "",
        PackageBuildMetadataVersionSuffix = "",
        Hash = expected_hash_cqcHookAddition
    )

    let expected_hash_semVerAddition = "AA408D5E4ABDCD52979DC7AB1D289E49"

    let allFields_semVerAddition = AVPRClient.PackageContentHash(
        PackageName = "name",
        PackageMajorVersion = 1,
        PackageMinorVersion = 0,
        PackagePatchVersion = 0,
        PackagePreReleaseVersionSuffix = "use",
        PackageBuildMetadataVersionSuffix = "suffixes",
        Hash = expected_hash_semVerAddition
    )

    let expected_hash_cliargsAddition = "5142A8191A58896DF0ACF91B6BAE0B7F"

    let allFields_cliargsAddition = AVPRClient.PackageContentHash(
        PackageName = "name",
        PackageMajorVersion = 1,
        PackageMinorVersion = 0,
        PackagePatchVersion = 0,
        PackagePreReleaseVersionSuffix = "use",
        PackageBuildMetadataVersionSuffix = "suffixes",
        Hash = expected_hash_cliargsAddition
    )

module BinaryContent =

    open System.IO

    let expected_content_cqcHookAddition = "(*
---
Name: name
Summary: summary
Description: description
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: true
Authors:
  - FullName: test
    Email: test@test.test
    Affiliation: testaffiliation
    AffiliationLink: test.com
Tags:
  - Name: test
    TermSourceREF: REF
    TermAccessionNumber: TAN
ReleaseNotes: releasenotes
CQCHookEndpoint: hookendpoint
---
*)

printfn \"yes\""                                    .ReplaceLineEndings("\n")

    let expected_binary_content_cqcHookAddition = expected_content_cqcHookAddition |> System.Text.Encoding.UTF8.GetBytes

    let expected_content_semVerAddition = "(*
---
Name: name
Summary: summary
Description: description
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
PreReleaseVersionSuffix: use
BuildMetadataVersionSuffix: suffixes
Publish: true
Authors:
  - FullName: test
    Email: test@test.test
    Affiliation: testaffiliation
    AffiliationLink: test.com
Tags:
  - Name: test
    TermSourceREF: REF
    TermAccessionNumber: TAN
ReleaseNotes: releasenotes
CQCHookEndpoint: hookendpoint
---
*)

printfn \"yes\""                                    .ReplaceLineEndings("\n")

    let expected_binary_content_semVerAddition = expected_content_semVerAddition |> System.Text.Encoding.UTF8.GetBytes

    let expected_content_cliargsAddition = "(*
---
Name: name
Summary: summary
Description: description
CLIArguments:
  - Flags:
      - -i
      - --input
    Description: Input ARC path
    Example: ./my-arc
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
PreReleaseVersionSuffix: use
BuildMetadataVersionSuffix: suffixes
Publish: true
Authors:
  - FullName: test
    Email: test@test.test
    Affiliation: testaffiliation
    AffiliationLink: test.com
Tags:
  - Name: test
    TermSourceREF: REF
    TermAccessionNumber: TAN
ReleaseNotes: releasenotes
CQCHookEndpoint: hookendpoint
---
*)

printfn \"yes\""                                    .ReplaceLineEndings("\n")

    let expected_binary_content_cliargsAddition = expected_content_cliargsAddition |> System.Text.Encoding.UTF8.GetBytes

module ValidationPackage =

    open System.IO

    let allFields_cqcHookAddition = AVPRClient.ValidationPackage(
        Name = "name",
        Summary = "summary" ,
        Description = "description" ,
        MajorVersion = 1,
        MinorVersion = 0,
        PatchVersion = 0,
        PreReleaseVersionSuffix = "",
        BuildMetadataVersionSuffix = "",
        ProgrammingLanguage= "FSharp",
        PackageContent = BinaryContent.expected_binary_content_cqcHookAddition,
        ReleaseDate = date,
        Authors = [|Author.allFieldsClient|],
        Tags = [|OntologyAnnotation.allFieldsClient|],
        ReleaseNotes = "releasenotes",
        CQCHookEndpoint = "hookendpoint",
        CLIArguments = ResizeArray [ ]
    )

    let allFields_semVerAddition = AVPRClient.ValidationPackage(
        Name = "name",
        Summary = "summary" ,
        Description = "description" ,
        MajorVersion = 1,
        MinorVersion = 0,
        PatchVersion = 0,
        PreReleaseVersionSuffix = "use",
        BuildMetadataVersionSuffix = "suffixes",
        ProgrammingLanguage= "FSharp",
        PackageContent = BinaryContent.expected_binary_content_semVerAddition,
        ReleaseDate = date,
        Authors = [|Author.allFieldsClient|],
        Tags = [|OntologyAnnotation.allFieldsClient|],
        ReleaseNotes = "releasenotes",
        CQCHookEndpoint = "hookendpoint",
        CLIArguments = ResizeArray [ ]
    )

    let allFields_cliargsAddition = AVPRClient.ValidationPackage(
        Name = "name",
        Summary = "summary" ,
        Description = "description" ,
        MajorVersion = 1,
        MinorVersion = 0,
        PatchVersion = 0,
        PreReleaseVersionSuffix = "use",
        BuildMetadataVersionSuffix = "suffixes",
        ProgrammingLanguage= "FSharp",
        PackageContent = BinaryContent.expected_binary_content_cliargsAddition,
        ReleaseDate = date,
        Authors = [|Author.allFieldsClient|],
        Tags = [|OntologyAnnotation.allFieldsClient|],
        ReleaseNotes = "releasenotes",
        CQCHookEndpoint = "hookendpoint",
        CLIArguments = ResizeArray [ CLIArgument.allFieldsClient ]
    )

module ValidationPackageIndex =

    open System.IO

    let allFields_cqcHookAddition = AVPRIndex.Domain.ValidationPackageIndex.create(
        repoPath = "",
        fileName = "",
        lastUpdated = System.DateTime.Now,
        contentHash = "",
        metadata = AVPRIndex.Domain.ValidationPackageMetadata.create(
            name = "name",
            summary = "summary" ,
            description = "description" ,
            majorVersion = 1,
            minorVersion = 0,
            patchVersion = 0,
            programmingLanguage= "FSharp",
            Authors = [|Author.allFieldsIndex|],
            Tags = [|OntologyAnnotation.allFieldsIndex|],
            ReleaseNotes = "releasenotes",
            CQCHookEndpoint = "hookendpoint",
            CLIArguments = [| |]
        )
    )

    let allFields_semVerAddition = AVPRIndex.Domain.ValidationPackageIndex.create(
        repoPath = "",
        fileName = "",
        lastUpdated = System.DateTime.Now,
        contentHash = "",
        metadata = AVPRIndex.Domain.ValidationPackageMetadata.create(
            name = "name",
            summary = "summary" ,
            description = "description" ,
            majorVersion = 1,
            minorVersion = 0,
            patchVersion = 0,
            programmingLanguage= "FSharp",
            PreReleaseVersionSuffix = "use",
            BuildMetadataVersionSuffix = "suffixes",
            Authors = [|Author.allFieldsIndex|],
            Tags = [|OntologyAnnotation.allFieldsIndex|],
            ReleaseNotes = "releasenotes",
            CQCHookEndpoint = "hookendpoint",
            CLIArguments = [| |]
        )
    )

    let allFields_cliargsAddition = AVPRIndex.Domain.ValidationPackageIndex.create(
        repoPath = "",
        fileName = "",
        lastUpdated = System.DateTime.Now,
        contentHash = "",
        metadata = AVPRIndex.Domain.ValidationPackageMetadata.create(
            name = "name",
            summary = "summary" ,
            description = "description" ,
            majorVersion = 1,
            minorVersion = 0,
            patchVersion = 0,
            programmingLanguage= "FSharp",
            PreReleaseVersionSuffix = "use",
            BuildMetadataVersionSuffix = "suffixes",
            Authors = [|Author.allFieldsIndex|],
            Tags = [|OntologyAnnotation.allFieldsIndex|],
            ReleaseNotes = "releasenotes",
            CQCHookEndpoint = "hookendpoint",
            CLIArguments = [|CLIArgument.allFieldsIndex|]
        )
    )