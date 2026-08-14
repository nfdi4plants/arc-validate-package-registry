using System.Net;
using PackageRegistryService.Models;

namespace PackageRegistryService.Pages.Components;

/// <summary>
/// Renders the running build identity and service release notes.
/// </summary>
public static class Releases
{
    private const string RepositoryUrl =
        "https://github.com/nfdi4plants/arc-validate-package-registry";

    /// <summary>
    /// Renders current release provenance followed by the complete release notes.
    /// </summary>
    /// <param name="current">The running service's release identity.</param>
    /// <param name="releaseNotesHtml">The rendered service release notes.</param>
    /// <returns>The releases page content.</returns>
    public static string Render(
        ServiceVersionDocument current,
        string releaseNotesHtml
    )
    {
        var version = WebUtility.HtmlEncode(current.Service.Version);
        var channel = WebUtility.HtmlEncode(current.Build.Channel);
        var revision = WebUtility.HtmlEncode(current.Build.Revision);
        var releaseName = WebUtility.HtmlEncode(current.Release.Name);
        var releaseSummary = WebUtility.HtmlEncode(current.Release.Summary);
        var created = current.Build.Created?.ToString("u") ?? "not recorded";
        var revisionHtml =
            current.Build.Revision == "unknown"
                ? revision
                : $@"<a href=""{RepositoryUrl}/commit/{Uri.EscapeDataString(current.Build.Revision)}""><code>{revision}</code></a>";

        return $@"<section>
<h1>AVPR service releases</h1>
<article>
  <header><strong>Currently running: {version} - {releaseName}</strong></header>
  <p>{releaseSummary}</p>
  <dl>
    <dt>Release channel</dt>
    <dd><code>{channel}</code></dd>
    <dt>Source revision</dt>
    <dd>{revisionHtml}</dd>
    <dt>Image created</dt>
    <dd>{WebUtility.HtmlEncode(created)}</dd>
  </dl>
</article>
</section>
<section>
{releaseNotesHtml}
</section>";
    }
}
