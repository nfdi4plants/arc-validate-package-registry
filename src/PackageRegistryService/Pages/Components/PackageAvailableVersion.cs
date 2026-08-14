using PackageRegistryService.Models;

using RegistryValidationPackage = PackageRegistryService.Models.ValidationPackage;

namespace PackageRegistryService.Pages.Components
{
    /// <summary>
    /// Renders links and tabular history for published package versions.
    /// </summary>
    public class PackageAvailableVersion
    {
        /// <summary>Renders a link to one exact package version.</summary>
        public static string Render(string packageName, string version) => $@"<a href=""/package/{packageName}/{version}"">{version}</a>";

        /// <summary>Renders a release-ordered table of available package versions.</summary>
        public static string RenderVersionTable(RegistryValidationPackage[] packages)
        {

            var content = $@"<table>
  <thead>
    <tr>
      <th scope=""col"">Version</th>
      <th scope=""col"">Released on</th>
   </tr>
  </thead>
  <tbody>
    {string.Join(
        System.Environment.NewLine, 
        packages
            .OrderByDescending(p => p.MajorVersion)
            .ThenByDescending(p => p.MinorVersion)
            .ThenByDescending(p => p.PatchVersion)
            .Select(p => $@"    <tr>
      <td>{Render(p.Name, p.GetSemanticVersionString())}</td>
      <td>{p.ReleaseDate}</td>
    </tr>"))}
  </tbody>
</table>";
            return content;

        }
    }
}
