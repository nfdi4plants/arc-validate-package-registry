namespace PackageRegistryService.Models;

/// <summary>
/// Describes a validation-package author as stored by the registry service.
/// </summary>
public sealed class Author
{
    /// <summary>The author's display name.</summary>
    public string FullName { get; set; } = "";

    /// <summary>The author's contact email address.</summary>
    public string Email { get; set; } = "";

    /// <summary>The author's organizational affiliation.</summary>
    public string Affiliation { get; set; } = "";

    /// <summary>A link identifying the author's affiliation.</summary>
    public string AffiliationLink { get; set; } = "";
}
