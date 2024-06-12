using PackageRegistryService.Models;
using System.Text;
using static AVPRIndex.Domain;

namespace PackageRegistryService.Pages.Components
{
    public class Package
    {
        public static string Render(
            string packageName,
            string packageVersion,
            string packageSummary,
            string packageDescription,
            string packageReleaseNotes,
            string packageContent,
            DateOnly packageReleaseDate,
            string[] packageTags,
            Author[] packageAuthors,
            string versionTable,
            int downloads
        )
        {
            return $@"<section>
  <hgroup>
    <h1>Validation Package <mark>{packageName}</mark></h1>
    <p>{PackageTag.RenderAllLinksInline(packageTags)}</p>
    <p><strong>v{packageVersion}</strong> released on {packageReleaseDate}</p>
    <p>by {PackageAuthor.RenderAllLinksInline(packageAuthors.Select(a => a.FullName).ToArray())}</p>
    <p>{downloads} Downloads</p>
  </hgroup>
  <p style=""display:block"">{packageSummary}</p>
</section>
<hr />
<section>
  <h4>Install with <a href=""https://github.com/nfdi4plants/arc-validate"">arc-validate</a></h4>
  <pre><code> arc-validate package install {packageName} --package-version {packageVersion}</code></pre>
  <h4>Include in a <a href=""https://doi.org/10.1111/tpj.16474"">PLANTDataHUB CQC pipeline</a></h4>
  <pre><code>validation_packages:
  - name: {packageName}
    version: {packageVersion}</code></pre>
</section>
<hr />
<section> 
  <h2>Description</h2>
  {PackageDescription.Render(packageDescription)}
</section>
<hr />
<section>
  <details>
    <summary role=""button"" class=""primary"">Release notes</summary>
      {PackageReleaseNotes.Render(packageReleaseNotes)}
  </details>
  <hr />
  <details>
    <summary role=""button"" class=""primary"">Browse code (v{packageVersion})</summary>
      <pre><code>{System.Security.SecurityElement.Escape(packageContent)}</code></pre>
  </details>
  <hr />
  <details>
    <summary role=""button"" class=""primary"">Available versions</summary>
    {versionTable}
  </details>
</section>
<hr />
";
        }
    }
}
