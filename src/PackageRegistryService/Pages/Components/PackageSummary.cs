using YamlDotNet.Core.Tokens;

namespace PackageRegistryService.Pages.Components
{
    public record PackageSummary(string Name, string Description, string [] Tags, string LatestVersion, DateOnly ReleaseDate)
    {
        public static string RenderDescription(string description)
        {
            return String.Join(
                System.Environment.NewLine,
                description
                    .Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None)
                    .Select(l => $@"<small style=""display:block"">{l}</small>")
            );
        }
        public static string Render(PackageSummary summary)
        {
            return $@"<tr>
<th scope=""row""><a href=""/package/{summary.Name}"">{summary.Name}</a></th>
<td>{RenderDescription(summary.Description)}</td>
<td><a href=""/package/{summary.Name}/{summary.LatestVersion}"">{summary.LatestVersion}</a></td>
<td>{summary.ReleaseDate}</td>
<td>{string.Join("; ", summary.Tags.Select(t => $@"<a href=""/packages?tag={t}"">{t}</a>"))}</td>
</tr>";
        }

        public static string RenderList(IEnumerable<PackageSummary> summaries)
        {
            var content = @$"<h1>All available validation packages</h1><br>
<div class=""overflow-auto"">
<table class=""striped"">
<thead>
<tr>
<th scope=""col"">Name</th>
<th scope=""col"">Description</th>
<th scope=""col"">Latest version</th>
<th scope=""col"">Release date</th>
<th scope=""col"">Tags</th>
</tr>
</thead>
{string.Join(System.Environment.NewLine, summaries.Select(PackageSummary.Render))}
</table>
</div>";
            return content;
        }
    }
}
