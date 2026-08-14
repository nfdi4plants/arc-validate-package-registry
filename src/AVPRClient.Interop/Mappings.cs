using System;
using System.Collections.Generic;
using System.Linq;
using Avpr = global::AVPRClient;
using Model = global::ValidationPackage.Model;

namespace AVPRClient.Interop;

public static class Mappings
{
    public static Model.ValidationPackageIdentity ToModel(
        this Avpr.ValidationPackageIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        return Model.ValidationPackageIdentity.create(
            ValueOrEmpty(identity.Name),
            ParseCanonicalVersion(identity.Version, nameof(identity)));
    }

    public static Avpr.ValidationPackageIdentity ToClient(
        this Model.ValidationPackageIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        return new Avpr.ValidationPackageIdentity
        {
            Name = identity.Name,
            Version = Model.SemVer.toString(identity.Version)
        };
    }

    public static Model.ValidationPackageIdentity[] ToModel(
        this ICollection<Avpr.ValidationPackageIdentity>? identities) =>
        (identities ?? Array.Empty<Avpr.ValidationPackageIdentity>())
            .Select(ToModel)
            .ToArray();

    public static ICollection<Avpr.ValidationPackageIdentity> ToClient(
        this Model.ValidationPackageIdentity[]? identities) =>
        (identities ?? Array.Empty<Model.ValidationPackageIdentity>())
            .Select(ToClient)
            .ToArray();

    public static bool IdentityEquals(
        this Avpr.ValidationPackage package,
        Avpr.ValidationPackage other)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(other);

        return package.Name == other.Name
            && package.MajorVersion == other.MajorVersion
            && package.MinorVersion == other.MinorVersion
            && package.PatchVersion == other.PatchVersion
            && package.PreReleaseVersionSuffix == other.PreReleaseVersionSuffix
            && package.BuildMetadataVersionSuffix == other.BuildMetadataVersionSuffix
            && package.ProgrammingLanguage == other.ProgrammingLanguage;
    }

    public static bool IdentityEquals(
        this Avpr.ValidationPackage package,
        Model.ValidationPackageMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(metadata);

        return package.Name == metadata.Name
            && package.MajorVersion == metadata.MajorVersion
            && package.MinorVersion == metadata.MinorVersion
            && package.PatchVersion == metadata.PatchVersion
            && ValueOrEmpty(package.PreReleaseVersionSuffix) == metadata.PreReleaseVersionSuffix
            && ValueOrEmpty(package.BuildMetadataVersionSuffix) == metadata.BuildMetadataVersionSuffix
            && package.ProgrammingLanguage == metadata.ProgrammingLanguage;
    }

    public static bool IdentityEquals(
        this Model.ValidationPackageMetadata metadata,
        Avpr.ValidationPackage package) =>
        package.IdentityEquals(metadata);

    public static Model.ValidationPackageMetadata ToModel(
        this Avpr.ValidationPackage package)
    {
        ArgumentNullException.ThrowIfNull(package);

        return new Model.ValidationPackageMetadata
        {
            Name = ValueOrEmpty(package.Name),
            Summary = ValueOrEmpty(package.Summary),
            Description = ValueOrEmpty(package.Description),
            MajorVersion = package.MajorVersion,
            MinorVersion = package.MinorVersion,
            PatchVersion = package.PatchVersion,
            PreReleaseVersionSuffix = ValueOrEmpty(package.PreReleaseVersionSuffix),
            BuildMetadataVersionSuffix = ValueOrEmpty(package.BuildMetadataVersionSuffix),
            ProgrammingLanguage = ValueOrEmpty(package.ProgrammingLanguage),
            Authors = package.Authors.ToModel(),
            Tags = package.Tags.ToModel(),
            ReleaseNotes = ValueOrEmpty(package.ReleaseNotes),
            CQCHookEndpoint = ValueOrEmpty(package.CQCHookEndpoint),
            Inputs = package.Inputs.ToModel()
        };
    }

    public static Model.ValidationPackageMetadata ToModel(
        this Avpr.ValidationPackageMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        var version = ParseCanonicalVersion(metadata.Version, nameof(metadata));
        var mapped = new Model.ValidationPackageMetadata
        {
            Name = ValueOrEmpty(metadata.Name),
            Summary = ValueOrEmpty(metadata.Summary),
            Description = ValueOrEmpty(metadata.Description),
            MajorVersion = version.Major,
            MinorVersion = version.Minor,
            PatchVersion = version.Patch,
            PreReleaseVersionSuffix = version.PreRelease,
            BuildMetadataVersionSuffix = version.BuildMetadata,
            ProgrammingLanguage = ValueOrEmpty(metadata.ProgrammingLanguage),
            Authors = metadata.Authors.ToModel(),
            Tags = metadata.Tags.ToModel(),
            ReleaseNotes = ValueOrEmpty(metadata.ReleaseNotes),
            CQCHookEndpoint = ValueOrEmpty(metadata.CQCHookEndpoint),
            Inputs = metadata.Inputs.ToModel()
        };

        Model.CommandInputParameter.validate(mapped.Inputs);
        return mapped;
    }

    public static Avpr.ValidationPackageMetadata ToClientMetadata(
        this Model.ValidationPackageMetadata metadata,
        DateTimeOffset releaseDate)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        var version = Model.ValidationPackageMetadata.getSemanticVersion(metadata);
        Model.CommandInputParameter.validate(metadata.Inputs ?? []);

        return new Avpr.ValidationPackageMetadata
        {
            Name = ValueOrEmpty(metadata.Name),
            Version = Model.SemVer.toString(version),
            Summary = ValueOrEmpty(metadata.Summary),
            Description = ValueOrEmpty(metadata.Description),
            ReleaseDate = releaseDate,
            ProgrammingLanguage = ValueOrEmpty(metadata.ProgrammingLanguage),
            Authors = metadata.Authors.ToClient(),
            Tags = metadata.Tags.ToClient(),
            ReleaseNotes = ValueOrEmpty(metadata.ReleaseNotes),
            CQCHookEndpoint = ValueOrEmpty(metadata.CQCHookEndpoint),
            Inputs = metadata.Inputs.ToClient()
        };
    }

    public static Avpr.ValidationPackage ToClient(
        this Model.ValidationPackageMetadata metadata,
        byte[] packageContent,
        DateTimeOffset releaseDate)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(packageContent);

        return new Avpr.ValidationPackage
        {
            Name = ValueOrEmpty(metadata.Name),
            Summary = ValueOrEmpty(metadata.Summary),
            Description = ValueOrEmpty(metadata.Description),
            MajorVersion = metadata.MajorVersion,
            MinorVersion = metadata.MinorVersion,
            PatchVersion = metadata.PatchVersion,
            PreReleaseVersionSuffix = ValueOrEmpty(metadata.PreReleaseVersionSuffix),
            BuildMetadataVersionSuffix = ValueOrEmpty(metadata.BuildMetadataVersionSuffix),
            ProgrammingLanguage = ValueOrEmpty(metadata.ProgrammingLanguage),
            PackageContent = packageContent,
            ReleaseDate = releaseDate,
            Authors = metadata.Authors.ToClient(),
            Tags = metadata.Tags.ToClient(),
            ReleaseNotes = ValueOrEmpty(metadata.ReleaseNotes),
            CQCHookEndpoint = ValueOrEmpty(metadata.CQCHookEndpoint),
            Inputs = metadata.Inputs.ToClient()
        };
    }

    public static Model.Author ToModel(this Avpr.Author author)
    {
        ArgumentNullException.ThrowIfNull(author);

        return new Model.Author
        {
            FullName = ValueOrEmpty(author.FullName),
            Email = ValueOrEmpty(author.Email),
            Affiliation = ValueOrEmpty(author.Affiliation),
            AffiliationLink = ValueOrEmpty(author.AffiliationLink)
        };
    }

    public static Avpr.Author ToClient(this Model.Author author)
    {
        ArgumentNullException.ThrowIfNull(author);

        return new Avpr.Author
        {
            FullName = ValueOrEmpty(author.FullName),
            Email = ValueOrEmpty(author.Email),
            Affiliation = ValueOrEmpty(author.Affiliation),
            AffiliationLink = ValueOrEmpty(author.AffiliationLink)
        };
    }

    public static Model.Author[] ToModel(this ICollection<Avpr.Author>? authors) =>
        (authors ?? Array.Empty<Avpr.Author>())
            .Select(ToModel)
            .ToArray();

    public static ICollection<Avpr.Author> ToClient(this Model.Author[]? authors) =>
        (authors ?? Array.Empty<Model.Author>())
            .Select(ToClient)
            .ToArray();

    public static Model.OntologyAnnotation ToModel(this Avpr.OntologyAnnotation tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        return new Model.OntologyAnnotation
        {
            Name = ValueOrEmpty(tag.Name),
            TermSourceREF = ValueOrEmpty(tag.TermSourceREF),
            TermAccessionNumber = ValueOrEmpty(tag.TermAccessionNumber)
        };
    }

    public static Avpr.OntologyAnnotation ToClient(this Model.OntologyAnnotation tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        return new Avpr.OntologyAnnotation
        {
            Name = ValueOrEmpty(tag.Name),
            TermSourceREF = ValueOrEmpty(tag.TermSourceREF),
            TermAccessionNumber = ValueOrEmpty(tag.TermAccessionNumber)
        };
    }

    public static Model.OntologyAnnotation[] ToModel(
        this ICollection<Avpr.OntologyAnnotation>? tags) =>
        (tags ?? Array.Empty<Avpr.OntologyAnnotation>())
            .Select(ToModel)
            .ToArray();

    public static ICollection<Avpr.OntologyAnnotation> ToClient(
        this Model.OntologyAnnotation[]? tags) =>
        (tags ?? Array.Empty<Model.OntologyAnnotation>())
            .Select(ToClient)
            .ToArray();

    public static Model.CommandInputType ToModel(this Avpr.CommandInputType inputType) =>
        inputType switch
        {
            Avpr.CommandInputType.Boolean => NewInputType(Model.CwlPrimitive.Boolean, false),
            Avpr.CommandInputType.Boolean_ => NewInputType(Model.CwlPrimitive.Boolean, true),
            Avpr.CommandInputType.Int => NewInputType(Model.CwlPrimitive.Int, false),
            Avpr.CommandInputType.Int_ => NewInputType(Model.CwlPrimitive.Int, true),
            Avpr.CommandInputType.Long => NewInputType(Model.CwlPrimitive.Long, false),
            Avpr.CommandInputType.Long_ => NewInputType(Model.CwlPrimitive.Long, true),
            Avpr.CommandInputType.Float => NewInputType(Model.CwlPrimitive.Float, false),
            Avpr.CommandInputType.Float_ => NewInputType(Model.CwlPrimitive.Float, true),
            Avpr.CommandInputType.Double => NewInputType(Model.CwlPrimitive.Double, false),
            Avpr.CommandInputType.Double_ => NewInputType(Model.CwlPrimitive.Double, true),
            Avpr.CommandInputType.String => NewInputType(Model.CwlPrimitive.String, false),
            Avpr.CommandInputType.String_ => NewInputType(Model.CwlPrimitive.String, true),
            _ => throw new ArgumentOutOfRangeException(
                nameof(inputType), inputType, "Unsupported generated CWL command input type")
        };

    public static Avpr.CommandInputType ToClient(this Model.CommandInputType inputType)
    {
        ArgumentNullException.ThrowIfNull(inputType);

        return (inputType.PrimitiveType, inputType.IsNullable) switch
        {
            (Model.CwlPrimitive.Boolean, false) => Avpr.CommandInputType.Boolean,
            (Model.CwlPrimitive.Boolean, true) => Avpr.CommandInputType.Boolean_,
            (Model.CwlPrimitive.Int, false) => Avpr.CommandInputType.Int,
            (Model.CwlPrimitive.Int, true) => Avpr.CommandInputType.Int_,
            (Model.CwlPrimitive.Long, false) => Avpr.CommandInputType.Long,
            (Model.CwlPrimitive.Long, true) => Avpr.CommandInputType.Long_,
            (Model.CwlPrimitive.Float, false) => Avpr.CommandInputType.Float,
            (Model.CwlPrimitive.Float, true) => Avpr.CommandInputType.Float_,
            (Model.CwlPrimitive.Double, false) => Avpr.CommandInputType.Double,
            (Model.CwlPrimitive.Double, true) => Avpr.CommandInputType.Double_,
            (Model.CwlPrimitive.String, false) => Avpr.CommandInputType.String,
            (Model.CwlPrimitive.String, true) => Avpr.CommandInputType.String_,
            _ => throw new ArgumentOutOfRangeException(
                nameof(inputType), inputType.PrimitiveType, "Unsupported model CWL command input type")
        };
    }

    public static Model.CommandInputBinding ToModel(this Avpr.CommandInputBinding? binding) =>
        new()
        {
            Position = binding?.Position ?? 0,
            Prefix = ValueOrEmpty(binding?.Prefix)
        };

    public static Avpr.CommandInputBinding ToClient(this Model.CommandInputBinding? binding) =>
        new()
        {
            Position = binding?.Position ?? 0,
            Prefix = ValueOrEmpty(binding?.Prefix)
        };

    public static Model.CommandInputParameter ToModel(this Avpr.CommandInputParameter input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var mapped = new Model.CommandInputParameter
        {
            Id = ValueOrEmpty(input.Id),
            Type = input.Type.ToModel(),
            Label = ValueOrEmpty(input.Label),
            Doc = ValueOrEmpty(input.Doc),
            InputBinding = input.InputBinding.ToModel()
        };

        Model.CommandInputParameter.validate([mapped]);
        return mapped;
    }

    public static Avpr.CommandInputParameter ToClient(this Model.CommandInputParameter input)
    {
        ArgumentNullException.ThrowIfNull(input);
        Model.CommandInputParameter.validate([input]);

        return new Avpr.CommandInputParameter
        {
            Id = ValueOrEmpty(input.Id),
            Type = input.Type.ToClient(),
            Label = ValueOrEmpty(input.Label),
            Doc = ValueOrEmpty(input.Doc),
            InputBinding = input.InputBinding.ToClient()
        };
    }

    public static Model.CommandInputParameter[] ToModel(
        this ICollection<Avpr.CommandInputParameter>? inputs)
    {
        var mapped = (inputs ?? Array.Empty<Avpr.CommandInputParameter>())
            .Select(ToModel)
            .ToArray();

        Model.CommandInputParameter.validate(mapped);
        return mapped;
    }

    public static ICollection<Avpr.CommandInputParameter> ToClient(
        this Model.CommandInputParameter[]? inputs)
    {
        var declarations = inputs ?? Array.Empty<Model.CommandInputParameter>();
        Model.CommandInputParameter.validate(declarations);
        return declarations
            .Select(ToClient)
            .ToArray();
    }

    private static Model.CommandInputType NewInputType(
        Model.CwlPrimitive primitive,
        bool isNullable) =>
        new()
        {
            PrimitiveType = primitive,
            IsNullable = isNullable
        };

    private static string ValueOrEmpty(string? value) => value ?? string.Empty;

    private static Model.SemVer ParseCanonicalVersion(string? value, string parameterName)
    {
        var canonical = ValueOrEmpty(value);
        var parsed = Model.SemVer.tryParse(canonical);

        if (parsed is null || Model.SemVer.toString(parsed.Value) != canonical)
        {
            throw new ArgumentException(
                $"'{canonical}' is not a canonical semantic version.",
                parameterName);
        }

        return parsed.Value;
    }
}
