using System.Text.Json.Serialization;

namespace PackageRegistryService.Models;

public sealed class CommandInputBinding
{
    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("prefix")]
    public string Prefix { get; set; } = "";
}
