using PackageRegistryService.Models;

namespace PackageRegistryService.API.Contracts;

/// <summary>
/// Validation-package metadata without executable package content.
/// </summary>
public sealed class ValidationPackageMetadata
{
    /// <summary>The validation-package name.</summary>
    public required string Name { get; init; }

    /// <summary>The canonical full semantic version.</summary>
    public required string Version { get; init; }

    /// <summary>A short description suitable for package listings.</summary>
    public required string Summary { get; init; }

    /// <summary>The full package description.</summary>
    public required string Description { get; init; }

    /// <summary>The date on which this package version was released.</summary>
    public required DateOnly ReleaseDate { get; init; }

    /// <summary>Ontology annotations used to categorize the package.</summary>
    public ICollection<OntologyAnnotation> Tags { get; init; } = [];

    /// <summary>Release notes for this package version.</summary>
    public string ReleaseNotes { get; init; } = "";

    /// <summary>The optional endpoint used for continuous-quality-control integration.</summary>
    public string CQCHookEndpoint { get; init; } = "";

    /// <summary>The package authors.</summary>
    public ICollection<Author> Authors { get; init; } = [];

    /// <summary>The language used by the executable validation package.</summary>
    public string ProgrammingLanguage { get; init; } = "";

    /// <summary>The command-line inputs accepted by the package.</summary>
    public ICollection<CommandInputParameter> Inputs { get; init; } = [];
}
