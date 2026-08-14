using System.Text.Json.Serialization;

namespace PackageRegistryService.Models;

/// <summary>
/// Describes how a validation-package input binds to command-line arguments.
/// </summary>
public sealed class CommandInputBinding
{
    /// <summary>The positional ordering of the argument when an order is specified.</summary>
    [JsonPropertyName("position")]
    public int Position { get; set; }

    /// <summary>The command-line prefix used to pass the input value.</summary>
    [JsonPropertyName("prefix")]
    public string Prefix { get; set; } = "";
}
