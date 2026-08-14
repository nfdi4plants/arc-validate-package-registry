using PackageRegistryService.Models;

namespace PackageRegistryService.Services;

/// <summary>
/// Exposes the running service's build identity and rendered release notes.
/// </summary>
public interface IServiceReleaseInfoProvider
{
    /// <summary>Gets the current service, API, build, and release identity.</summary>
    ServiceVersionDocument Current { get; }

    /// <summary>Gets the full service release notes rendered as HTML.</summary>
    string ReleaseNotesHtml { get; }
}
