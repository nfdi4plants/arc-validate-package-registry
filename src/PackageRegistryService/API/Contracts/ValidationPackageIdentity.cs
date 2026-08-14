namespace PackageRegistryService.API.Contracts;

/// <summary>
/// The canonical identity of one published validation-package version.
/// </summary>
public sealed class ValidationPackageIdentity
{
    /// <summary>The validation-package name.</summary>
    public required string Name { get; init; }

    /// <summary>The canonical full semantic version.</summary>
    public required string Version { get; init; }
}
