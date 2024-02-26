using PackageRegistryService.Models;
using System.Text;

namespace PackageRegistryService.Pages.Components
{
    public class Package
    {
        public static string Render(
            string packageName,
            string packageVersion,
            string packageDescription,
            string packageReleaseNotes,
            string packageContent,
            DateOnly packageReleaseDate,
            string[] packageTags,
            Author[] packageAuthors,
            string versionTable
        )
        {
            return $@"<section>
  <hgroup>
    <h1>Validation Package <mark>{packageName}</mark></h1>
    <p>{PackageTag.RenderAllInline(packageTags)}</p>
    <p><strong>v{packageVersion}</strong> released on {packageReleaseDate}</p>
  </hgroup>
</section>
<hr />
<section>
  <h4>Install with <a href=""a"">arc-validate</a></h3>
  <pre><code> arc-validate package install {packageName} --package-version {packageVersion}</code></pre>
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
      <pre><code>{packageContent}</code></pre>
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
