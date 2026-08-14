using PackageRegistryService.Models;

namespace PackageRegistryService.API.Contracts;

/// <summary>
/// Validation-package metadata without executable package content.
/// </summary>
public sealed class ValidationPackageMetadata
{
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string Summary { get; init; }
    public required string Description { get; init; }
    public required DateOnly ReleaseDate { get; init; }
    public ICollection<OntologyAnnotation> Tags { get; init; } = [];
    public string ReleaseNotes { get; init; } = "";
    public string CQCHookEndpoint { get; init; } = "";
    public ICollection<Author> Authors { get; init; } = [];
    public string ProgrammingLanguage { get; init; } = "";
    public ICollection<CommandInputParameter> Inputs { get; init; } = [];
}
