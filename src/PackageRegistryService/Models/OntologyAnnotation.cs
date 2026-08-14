namespace PackageRegistryService.Models;

/// <summary>
/// Describes an ontology-backed package tag stored with registry metadata.
/// </summary>
public sealed class OntologyAnnotation
{
    /// <summary>The human-readable ontology term name.</summary>
    public string Name { get; set; } = "";

    /// <summary>The ontology or term-source identifier.</summary>
    public string TermSourceREF { get; set; } = "";

    /// <summary>The accession identifying the term within its source.</summary>
    public string TermAccessionNumber { get; set; } = "";
}
