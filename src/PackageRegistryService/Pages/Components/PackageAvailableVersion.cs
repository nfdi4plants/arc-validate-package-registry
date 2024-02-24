using PackageRegistryService.Models;

namespace PackageRegistryService.Pages.Components
{
    public class PackageAvailableVersion
    {
        public static string Render(string packageName, string version) => $@"<a href=""/package/{packageName}/{version}"">{version}</a>";

        public static string RenderVersionTable(ValidationPackage[] packages)
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
";
            return content;

        }
    }
}
