using System.Text.Json.Serialization;

namespace PackageRegistryService.Models;

/// <summary>
/// Describes the versioned identity of the running registry service.
/// </summary>
/// <param name="Service">The service name and version.</param>
/// <param name="Api">The supported API versions.</param>
/// <param name="Build">The build provenance.</param>
/// <param name="Release">The matching release information.</param>
public sealed record ServiceVersionDocument(
    [property: JsonPropertyName("service")] ServiceIdentity Service,
    [property: JsonPropertyName("api")] ApiIdentity Api,
    [property: JsonPropertyName("build")] BuildIdentity Build,
    [property: JsonPropertyName("release")] ReleaseIdentity Release);

/// <summary>Identifies the running service and its version.</summary>
/// <param name="Name">The stable service name.</param>
/// <param name="Version">The service version.</param>
public sealed record ServiceIdentity(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version")] string Version);

/// <summary>Identifies the API versions supported by the service.</summary>
/// <param name="Versions">The supported API version identifiers.</param>
public sealed record ApiIdentity(
    [property: JsonPropertyName("versions")] string[] Versions);

/// <summary>Describes the source and environment of the running build.</summary>
/// <param name="Revision">The source revision.</param>
/// <param name="Channel">The build or deployment channel.</param>
/// <param name="Created">The image creation time, when recorded.</param>
public sealed record BuildIdentity(
    [property: JsonPropertyName("revision")] string Revision,
    [property: JsonPropertyName("channel")] string Channel,
    [property: JsonPropertyName("created")] DateTimeOffset? Created);

/// <summary>Describes the release represented by the running build.</summary>
/// <param name="Name">The release name.</param>
/// <param name="Summary">A short release summary.</param>
/// <param name="NotesUrl">The service-relative release-notes URL.</param>
public sealed record ReleaseIdentity(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("summary")] string Summary,
    [property: JsonPropertyName("notesUrl")] string NotesUrl);
