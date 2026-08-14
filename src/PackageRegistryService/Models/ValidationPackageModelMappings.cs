using AVPR.Staging;
using Portable = global::ValidationPackage.Model;

namespace PackageRegistryService.Models;

public static class ValidationPackageModelMappings
{
    public static Author ToServiceModel(this Portable.Author author) => new()
    {
        FullName = author.FullName,
        Email = author.Email,
        Affiliation = author.Affiliation,
        AffiliationLink = author.AffiliationLink
    };

    public static Portable.Author ToPortableModel(this Author author) => new()
    {
        FullName = author.FullName,
        Email = author.Email,
        Affiliation = author.Affiliation,
        AffiliationLink = author.AffiliationLink
    };

    public static OntologyAnnotation ToServiceModel(
        this Portable.OntologyAnnotation annotation) => new()
    {
        Name = annotation.Name,
        TermSourceREF = annotation.TermSourceREF,
        TermAccessionNumber = annotation.TermAccessionNumber
    };

    public static Portable.OntologyAnnotation ToPortableModel(
        this OntologyAnnotation annotation) => new()
    {
        Name = annotation.Name,
        TermSourceREF = annotation.TermSourceREF,
        TermAccessionNumber = annotation.TermAccessionNumber
    };

    public static CommandInputType ToServiceModel(
        this Portable.CommandInputType inputType) => new()
    {
        PrimitiveType = inputType.PrimitiveType switch
        {
            Portable.CwlPrimitive.Boolean => CwlPrimitive.Boolean,
            Portable.CwlPrimitive.Int => CwlPrimitive.Int,
            Portable.CwlPrimitive.Long => CwlPrimitive.Long,
            Portable.CwlPrimitive.Float => CwlPrimitive.Float,
            Portable.CwlPrimitive.Double => CwlPrimitive.Double,
            Portable.CwlPrimitive.String => CwlPrimitive.String,
            _ => throw new ArgumentOutOfRangeException(
                nameof(inputType),
                inputType.PrimitiveType,
                "Unsupported portable CWL primitive type")
        },
        IsNullable = inputType.IsNullable
    };

    public static Portable.CommandInputType ToPortableModel(
        this CommandInputType inputType) => new()
    {
        PrimitiveType = inputType.PrimitiveType switch
        {
            CwlPrimitive.Boolean => Portable.CwlPrimitive.Boolean,
            CwlPrimitive.Int => Portable.CwlPrimitive.Int,
            CwlPrimitive.Long => Portable.CwlPrimitive.Long,
            CwlPrimitive.Float => Portable.CwlPrimitive.Float,
            CwlPrimitive.Double => Portable.CwlPrimitive.Double,
            CwlPrimitive.String => Portable.CwlPrimitive.String,
            _ => throw new ArgumentOutOfRangeException(
                nameof(inputType),
                inputType.PrimitiveType,
                "Unsupported service CWL primitive type")
        },
        IsNullable = inputType.IsNullable
    };

    public static CommandInputBinding ToServiceModel(
        this Portable.CommandInputBinding binding) => new()
    {
        Position = binding.Position,
        Prefix = binding.Prefix
    };

    public static Portable.CommandInputBinding ToPortableModel(
        this CommandInputBinding binding) => new()
    {
        Position = binding.Position,
        Prefix = binding.Prefix
    };

    public static CommandInputParameter ToServiceModel(
        this Portable.CommandInputParameter input) => new()
    {
        Id = input.Id,
        Type = input.Type.ToServiceModel(),
        Label = input.Label,
        Doc = input.Doc,
        InputBinding = input.InputBinding.ToServiceModel()
    };

    public static Portable.CommandInputParameter ToPortableModel(
        this CommandInputParameter input) => new()
    {
        Id = input.Id,
        Type = input.Type.ToPortableModel(),
        Label = input.Label,
        Doc = input.Doc,
        InputBinding = input.InputBinding.ToPortableModel()
    };

    public static ValidationPackage ToServiceModel(
        this Portable.ValidationPackageMetadata metadata,
        byte[] packageContent,
        DateOnly releaseDate)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(packageContent);
        Portable.CommandInputParameter.validate(metadata.Inputs ?? []);

        return new ValidationPackage
        {
            Name = metadata.Name,
            Summary = metadata.Summary,
            Description = metadata.Description,
            MajorVersion = metadata.MajorVersion,
            MinorVersion = metadata.MinorVersion,
            PatchVersion = metadata.PatchVersion,
            PreReleaseVersionSuffix = metadata.PreReleaseVersionSuffix,
            BuildMetadataVersionSuffix = metadata.BuildMetadataVersionSuffix,
            PackageContent = packageContent,
            ReleaseDate = releaseDate,
            Tags = (metadata.Tags ?? []).Select(ToServiceModel).ToList(),
            ReleaseNotes = metadata.ReleaseNotes,
            Authors = (metadata.Authors ?? []).Select(ToServiceModel).ToList(),
            CQCHookEndpoint = metadata.CQCHookEndpoint,
            ProgrammingLanguage = metadata.ProgrammingLanguage,
            Inputs = (metadata.Inputs ?? []).Select(ToServiceModel).ToList()
        };
    }

    public static Portable.ValidationPackageMetadata ToPortableModel(
        this ValidationPackage package)
    {
        ArgumentNullException.ThrowIfNull(package);

        var metadata = new Portable.ValidationPackageMetadata
        {
            Name = package.Name,
            Summary = package.Summary,
            Description = package.Description,
            MajorVersion = package.MajorVersion,
            MinorVersion = package.MinorVersion,
            PatchVersion = package.PatchVersion,
            PreReleaseVersionSuffix = package.PreReleaseVersionSuffix,
            BuildMetadataVersionSuffix = package.BuildMetadataVersionSuffix,
            ProgrammingLanguage = package.ProgrammingLanguage,
            Authors = (package.Authors ?? []).Select(ToPortableModel).ToArray(),
            Tags = (package.Tags ?? []).Select(ToPortableModel).ToArray(),
            ReleaseNotes = package.ReleaseNotes,
            CQCHookEndpoint = package.CQCHookEndpoint,
            Inputs = (package.Inputs ?? []).Select(ToPortableModel).ToArray()
        };

        Portable.CommandInputParameter.validate(metadata.Inputs);
        return metadata;
    }

    public static ValidationPackage ToServiceModel(
        this StagedValidationPackage stagedPackage)
    {
        ArgumentNullException.ThrowIfNull(stagedPackage);

        return stagedPackage.Metadata.ToServiceModel(
            NormalizedContent.fromFile(stagedPackage.RepoPath),
            DateOnly.FromDateTime(stagedPackage.LastUpdated.DateTime));
    }
}
