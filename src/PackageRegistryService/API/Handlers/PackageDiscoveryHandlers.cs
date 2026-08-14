using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PackageRegistryService.API.Contracts;
using PackageRegistryService.Models;
using PortableSemVer = global::ValidationPackage.Model.SemVer;

namespace PackageRegistryService.API.Handlers;

public sealed class PackageVersionProjection
{
    public required string Name { get; init; }
    public required int Major { get; init; }
    public required int Minor { get; init; }
    public required int Patch { get; init; }
    public required string PreRelease { get; init; }
    public required string BuildMetadata { get; init; }
}

public sealed class PackageMetadataProjection
{
    public required string Name { get; init; }
    public required int Major { get; init; }
    public required int Minor { get; init; }
    public required int Patch { get; init; }
    public required string PreRelease { get; init; }
    public required string BuildMetadata { get; init; }
    public required string Summary { get; init; }
    public required string Description { get; init; }
    public required DateOnly ReleaseDate { get; init; }
    public required ICollection<OntologyAnnotation> Tags { get; init; }
    public required string ReleaseNotes { get; init; }
    public required string CQCHookEndpoint { get; init; }
    public required ICollection<Author> Authors { get; init; }
    public required string ProgrammingLanguage { get; init; }
    public required ICollection<CommandInputParameter> Inputs { get; init; }
}

public static class PackageDiscoveryQueries
{
    public static IQueryable<PackageVersionProjection> PackageVersions(
        ValidationPackageDb database) =>
        database.ValidationPackages
            .AsNoTracking()
            .Select(package => new PackageVersionProjection
            {
                Name = package.Name,
                Major = package.MajorVersion,
                Minor = package.MinorVersion,
                Patch = package.PatchVersion,
                PreRelease = package.PreReleaseVersionSuffix,
                BuildMetadata = package.BuildMetadataVersionSuffix
            });

    public static IQueryable<PackageMetadataProjection> PackageMetadata(
        ValidationPackageDb database) =>
        database.ValidationPackages
            .AsNoTracking()
            .Select(package => new PackageMetadataProjection
            {
                Name = package.Name,
                Major = package.MajorVersion,
                Minor = package.MinorVersion,
                Patch = package.PatchVersion,
                PreRelease = package.PreReleaseVersionSuffix,
                BuildMetadata = package.BuildMetadataVersionSuffix,
                Summary = package.Summary,
                Description = package.Description,
                ReleaseDate = package.ReleaseDate,
                Tags = package.Tags,
                ReleaseNotes = package.ReleaseNotes,
                CQCHookEndpoint = package.CQCHookEndpoint,
                Authors = package.Authors,
                ProgrammingLanguage = package.ProgrammingLanguage,
                Inputs = package.Inputs
            });
}

public static class PackageDiscoveryHandlers
{
    public static async Task<Ok<ValidationPackageIdentity[]>> GetPackageIndex(
        ValidationPackageDb database)
    {
        var packages = await PackageDiscoveryQueries.PackageVersions(database).ToArrayAsync();

        Array.Sort(packages, CompareIndexEntries);

        return TypedResults.Ok(packages.Select(ToIdentity).ToArray());
    }

    public static async Task<Results<Ok<string[]>, NotFound<string>>> GetPackageVersions(
        string name,
        ValidationPackageDb database)
    {
        var versions = await PackageDiscoveryQueries.PackageVersions(database)
            .Where(package => package.Name == name)
            .ToArrayAsync();

        if (versions.Length == 0)
        {
            return TypedResults.NotFound($"No package '{name}' available.");
        }

        Array.Sort(versions, (first, second) =>
            -PortableSemVer.compareIdentity(ToSemVer(first), ToSemVer(second)));

        return TypedResults.Ok(versions.Select(ToCanonicalVersion).ToArray());
    }

    public static async Task<Results<
        Ok<ValidationPackageMetadata>,
        BadRequest<string>,
        NotFound<string>,
        Conflict<string>>> GetPackageMetadata(
        string name,
        string version,
        ValidationPackageDb database)
    {
        var parsedVersion = PortableSemVer.tryParse(version);
        if (parsedVersion is null)
        {
            return TypedResults.BadRequest($"{version} is not a valid semantic version.");
        }

        var semanticVersion = parsedVersion.Value;
        var package = await PackageDiscoveryQueries.PackageMetadata(database)
            .Where(package =>
                package.Name == name
                && package.Major == semanticVersion.Major
                && package.Minor == semanticVersion.Minor
                && package.Patch == semanticVersion.Patch
                && package.PreRelease == semanticVersion.PreRelease
                && package.BuildMetadata == semanticVersion.BuildMetadata)
            .SingleOrDefaultAsync();

        if (package is null)
        {
            return TypedResults.NotFound($"No package '{name}' @ {version} available.");
        }

        try
        {
            global::ValidationPackage.Model.CommandInputParameter.validate(
                package.Inputs.Select(ValidationPackageModelMappings.ToPortableModel).ToArray());
        }
        catch (ArgumentException error)
        {
            return TypedResults.Conflict(
                $"Stored metadata for package '{name}' @ {version} is invalid: {error.Message}");
        }

        return TypedResults.Ok(ToMetadata(package));
    }

    private static int CompareIndexEntries(
        PackageVersionProjection first,
        PackageVersionProjection second)
    {
        var nameComparison = StringComparer.Ordinal.Compare(first.Name, second.Name);
        return nameComparison != 0
            ? nameComparison
            : -PortableSemVer.compareIdentity(ToSemVer(first), ToSemVer(second));
    }

    private static ValidationPackageIdentity ToIdentity(PackageVersionProjection package) =>
        new()
        {
            Name = package.Name,
            Version = ToCanonicalVersion(package)
        };

    private static ValidationPackageMetadata ToMetadata(PackageMetadataProjection package) =>
        new()
        {
            Name = package.Name,
            Version = PortableSemVer.toString(ToSemVer(package)),
            Summary = package.Summary,
            Description = package.Description,
            ReleaseDate = package.ReleaseDate,
            Tags = package.Tags,
            ReleaseNotes = package.ReleaseNotes,
            CQCHookEndpoint = package.CQCHookEndpoint,
            Authors = package.Authors,
            ProgrammingLanguage = package.ProgrammingLanguage,
            Inputs = package.Inputs
        };

    private static string ToCanonicalVersion(PackageVersionProjection package) =>
        PortableSemVer.toString(ToSemVer(package));

    private static PortableSemVer ToSemVer(PackageVersionProjection package) =>
        ParseStoredVersion(
            package.Name,
            package.Major,
            package.Minor,
            package.Patch,
            package.PreRelease,
            package.BuildMetadata);

    private static PortableSemVer ToSemVer(PackageMetadataProjection package) =>
        ParseStoredVersion(
            package.Name,
            package.Major,
            package.Minor,
            package.Patch,
            package.PreRelease,
            package.BuildMetadata);

    private static PortableSemVer ParseStoredVersion(
        string packageName,
        int major,
        int minor,
        int patch,
        string preRelease,
        string buildMetadata)
    {
        var stored = new PortableSemVer
        {
            Major = major,
            Minor = minor,
            Patch = patch,
            PreRelease = preRelease,
            BuildMetadata = buildMetadata
        };
        var canonical = PortableSemVer.toString(stored);
        var parsed = PortableSemVer.tryParse(canonical);

        if (parsed is null || PortableSemVer.toString(parsed.Value) != canonical)
        {
            throw new InvalidOperationException(
                $"Stored package '{packageName}' has an invalid semantic version: {canonical}");
        }

        return parsed.Value;
    }
}
