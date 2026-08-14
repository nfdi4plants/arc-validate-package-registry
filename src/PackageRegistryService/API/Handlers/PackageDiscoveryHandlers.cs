using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using PackageRegistryService.API.Contracts;
using PackageRegistryService.Models;
using PortableSemVer = global::ValidationPackage.Model.SemVer;

namespace PackageRegistryService.API.Handlers;

/// <summary>
/// A content-free database projection of a published package identity.
/// </summary>
public sealed class PackageVersionProjection
{
    /// <summary>The package name.</summary>
    public required string Name { get; init; }

    /// <summary>The semantic-version major component.</summary>
    public required int Major { get; init; }

    /// <summary>The semantic-version minor component.</summary>
    public required int Minor { get; init; }

    /// <summary>The semantic-version patch component.</summary>
    public required int Patch { get; init; }

    /// <summary>The semantic-version prerelease suffix.</summary>
    public required string PreRelease { get; init; }

    /// <summary>The semantic-version build metadata suffix.</summary>
    public required string BuildMetadata { get; init; }
}

/// <summary>
/// A content-free database projection of package metadata for one published version.
/// </summary>
public sealed class PackageMetadataProjection
{
    /// <summary>The package name.</summary>
    public required string Name { get; init; }

    /// <summary>The semantic-version major component.</summary>
    public required int Major { get; init; }

    /// <summary>The semantic-version minor component.</summary>
    public required int Minor { get; init; }

    /// <summary>The semantic-version patch component.</summary>
    public required int Patch { get; init; }

    /// <summary>The semantic-version prerelease suffix.</summary>
    public required string PreRelease { get; init; }

    /// <summary>The semantic-version build metadata suffix.</summary>
    public required string BuildMetadata { get; init; }

    /// <summary>A short description suitable for package listings.</summary>
    public required string Summary { get; init; }

    /// <summary>The full package description.</summary>
    public required string Description { get; init; }

    /// <summary>The date on which this package version was released.</summary>
    public required DateOnly ReleaseDate { get; init; }

    /// <summary>Ontology annotations used to categorize the package.</summary>
    public required ICollection<OntologyAnnotation> Tags { get; init; }

    /// <summary>Release notes for this package version.</summary>
    public required string ReleaseNotes { get; init; }

    /// <summary>The optional continuous-quality-control hook endpoint.</summary>
    public required string CQCHookEndpoint { get; init; }

    /// <summary>The package authors.</summary>
    public required ICollection<Author> Authors { get; init; }

    /// <summary>The language used by the executable validation package.</summary>
    public required string ProgrammingLanguage { get; init; }

    /// <summary>The command-line inputs accepted by the package.</summary>
    public required ICollection<CommandInputParameter> Inputs { get; init; }
}

/// <summary>
/// Builds no-tracking, content-free projections used by discovery handlers.
/// </summary>
public static class PackageDiscoveryQueries
{
    /// <summary>
    /// Creates a query for package names and full semantic-version components.
    /// </summary>
    /// <param name="database">The registry database context.</param>
    /// <returns>A composable query that does not select executable package content.</returns>
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

    /// <summary>
    /// Creates a query for package metadata without executable content.
    /// </summary>
    /// <param name="database">The registry database context.</param>
    /// <returns>A composable no-tracking metadata query.</returns>
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

/// <summary>
/// Handles lightweight package discovery requests without download side effects.
/// </summary>
public static class PackageDiscoveryHandlers
{
    /// <summary>
    /// Gets the deterministic index of every published package identity.
    /// </summary>
    /// <param name="database">The registry database context.</param>
    /// <returns>An HTTP result containing package names and canonical versions.</returns>
    public static async Task<Ok<ValidationPackageIdentity[]>> GetPackageIndex(
        ValidationPackageDb database)
    {
        var packages = await PackageDiscoveryQueries.PackageVersions(database).ToArrayAsync();

        Array.Sort(packages, CompareIndexEntries);

        return TypedResults.Ok(packages.Select(ToIdentity).ToArray());
    }

    /// <summary>
    /// Gets every published version of a package in descending semantic-version order.
    /// </summary>
    /// <param name="name">The package name.</param>
    /// <param name="database">The registry database context.</param>
    /// <returns>The canonical versions, or a not-found result when the package is unknown.</returns>
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

    /// <summary>
    /// Gets metadata for one exact package version without loading executable content.
    /// </summary>
    /// <param name="name">The package name.</param>
    /// <param name="version">The full semantic version.</param>
    /// <param name="database">The registry database context.</param>
    /// <returns>The metadata or an error result for an invalid, missing, or inconsistent package.</returns>
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

    /// <summary>
    /// Orders index entries by package name and then descending semantic version.
    /// </summary>
    private static int CompareIndexEntries(
        PackageVersionProjection first,
        PackageVersionProjection second)
    {
        var nameComparison = StringComparer.Ordinal.Compare(first.Name, second.Name);
        return nameComparison != 0
            ? nameComparison
            : -PortableSemVer.compareIdentity(ToSemVer(first), ToSemVer(second));
    }

    /// <summary>
    /// Converts a projected package identity to its API contract.
    /// </summary>
    private static ValidationPackageIdentity ToIdentity(PackageVersionProjection package) =>
        new()
        {
            Name = package.Name,
            Version = ToCanonicalVersion(package)
        };

    /// <summary>
    /// Converts projected service metadata to its content-free API contract.
    /// </summary>
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

    /// <summary>
    /// Formats a projected version in canonical semantic-version form.
    /// </summary>
    private static string ToCanonicalVersion(PackageVersionProjection package) =>
        PortableSemVer.toString(ToSemVer(package));

    /// <summary>
    /// Converts a version projection to the portable semantic-version model.
    /// </summary>
    private static PortableSemVer ToSemVer(PackageVersionProjection package) =>
        ParseStoredVersion(
            package.Name,
            package.Major,
            package.Minor,
            package.Patch,
            package.PreRelease,
            package.BuildMetadata);

    /// <summary>
    /// Converts a metadata projection to the portable semantic-version model.
    /// </summary>
    private static PortableSemVer ToSemVer(PackageMetadataProjection package) =>
        ParseStoredVersion(
            package.Name,
            package.Major,
            package.Minor,
            package.Patch,
            package.PreRelease,
            package.BuildMetadata);

    /// <summary>
    /// Reconstructs and validates a semantic version stored as database columns.
    /// </summary>
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
