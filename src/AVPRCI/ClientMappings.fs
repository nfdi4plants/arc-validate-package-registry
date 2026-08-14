module ClientMappings

open System
open AVPR.Staging
open ValidationPackage.Model

let private toClientInputType (inputType: CommandInputType) =
    match inputType.PrimitiveType, inputType.IsNullable with
    | CwlPrimitive.Boolean, false -> AVPRClient.CommandInputType.Boolean
    | CwlPrimitive.Boolean, true -> AVPRClient.CommandInputType.Boolean_
    | CwlPrimitive.Int, false -> AVPRClient.CommandInputType.Int
    | CwlPrimitive.Int, true -> AVPRClient.CommandInputType.Int_
    | CwlPrimitive.Long, false -> AVPRClient.CommandInputType.Long
    | CwlPrimitive.Long, true -> AVPRClient.CommandInputType.Long_
    | CwlPrimitive.Float, false -> AVPRClient.CommandInputType.Float
    | CwlPrimitive.Float, true -> AVPRClient.CommandInputType.Float_
    | CwlPrimitive.Double, false -> AVPRClient.CommandInputType.Double
    | CwlPrimitive.Double, true -> AVPRClient.CommandInputType.Double_
    | CwlPrimitive.String, false -> AVPRClient.CommandInputType.String
    | CwlPrimitive.String, true -> AVPRClient.CommandInputType.String_
    | value ->
        invalidArg
            "inputType"
            $"Unsupported CWL command input type: {value}"

let private toClientInputBinding (binding: CommandInputBinding) =
    AVPRClient.CommandInputBinding(
        Position = binding.Position,
        Prefix = binding.Prefix
    )

let private toClientInput (input: CommandInputParameter) =
    AVPRClient.CommandInputParameter(
        Id = input.Id,
        Type = toClientInputType input.Type,
        Label = input.Label,
        Doc = input.Doc,
        InputBinding = toClientInputBinding input.InputBinding
    )

let private toClientAuthor (author: Author) =
    AVPRClient.Author(
        FullName = author.FullName,
        Email = author.Email,
        Affiliation = author.Affiliation,
        AffiliationLink = author.AffiliationLink
    )

let private toClientTag (tag: OntologyAnnotation) =
    AVPRClient.OntologyAnnotation(
        Name = tag.Name,
        TermSourceREF = tag.TermSourceREF,
        TermAccessionNumber = tag.TermAccessionNumber
    )

let identityEquals
    (publishedPackage: AVPRClient.ValidationPackage)
    (stagedPackage: StagedValidationPackage)
    =
    let metadata = stagedPackage.Metadata

    publishedPackage.Name = metadata.Name
    && publishedPackage.MajorVersion = metadata.MajorVersion
    && publishedPackage.MinorVersion = metadata.MinorVersion
    && publishedPackage.PatchVersion = metadata.PatchVersion
    && publishedPackage.PreReleaseVersionSuffix = metadata.PreReleaseVersionSuffix
    && publishedPackage.BuildMetadataVersionSuffix = metadata.BuildMetadataVersionSuffix
    && publishedPackage.ProgrammingLanguage = metadata.ProgrammingLanguage

let toValidationPackage
    (releaseDate: DateTimeOffset)
    (stagedPackage: StagedValidationPackage)
    =
    let metadata = stagedPackage.Metadata
    CommandInputParameter.validate metadata.Inputs |> ignore

    AVPRClient.ValidationPackage(
        Name = metadata.Name,
        Summary = metadata.Summary,
        Description = metadata.Description,
        MajorVersion = metadata.MajorVersion,
        MinorVersion = metadata.MinorVersion,
        PatchVersion = metadata.PatchVersion,
        PreReleaseVersionSuffix = metadata.PreReleaseVersionSuffix,
        BuildMetadataVersionSuffix = metadata.BuildMetadataVersionSuffix,
        ProgrammingLanguage = metadata.ProgrammingLanguage,
        PackageContent = NormalizedContent.fromFile stagedPackage.RepoPath,
        ReleaseDate = releaseDate,
        Tags = (metadata.Tags |> Array.map toClientTag),
        ReleaseNotes = metadata.ReleaseNotes,
        Authors = (metadata.Authors |> Array.map toClientAuthor),
        CQCHookEndpoint = metadata.CQCHookEndpoint,
        Inputs = (metadata.Inputs |> Array.map toClientInput)
    )

let toPackageContentHash
    hashFileDirectly
    (stagedPackage: StagedValidationPackage)
    =
    let metadata = stagedPackage.Metadata

    AVPRClient.PackageContentHash(
        PackageName = metadata.Name,
        Hash = (
            if hashFileDirectly then
                ContentHash.ofFile stagedPackage.RepoPath
            else
                stagedPackage.ContentHash
        ),
        PackageMajorVersion = metadata.MajorVersion,
        PackageMinorVersion = metadata.MinorVersion,
        PackagePatchVersion = metadata.PatchVersion,
        PackagePreReleaseVersionSuffix = metadata.PreReleaseVersionSuffix,
        PackageBuildMetadataVersionSuffix = metadata.BuildMetadataVersionSuffix
    )
