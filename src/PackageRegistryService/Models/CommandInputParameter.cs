using System.Text.Json.Serialization;

namespace PackageRegistryService.Models;

/// <summary>
/// Describes one declared command-line input accepted by a validation package.
/// </summary>
public sealed class CommandInputParameter
{
    /// <summary>The stable input identifier.</summary>
    [JsonPropertyName("id"), JsonRequired]
    public string Id { get; set; } = "";

    /// <summary>The supported scalar type and nullability of the input.</summary>
    [JsonPropertyName("type"), JsonRequired]
    public CommandInputType Type { get; set; } = new();

    /// <summary>An optional human-readable label.</summary>
    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

    /// <summary>Optional author-provided documentation.</summary>
    [JsonPropertyName("doc")]
    public string Doc { get; set; } = "";

    /// <summary>The command-line binding for the input.</summary>
    [JsonPropertyName("inputBinding"), JsonRequired]
    public CommandInputBinding InputBinding { get; set; } = new();
}
