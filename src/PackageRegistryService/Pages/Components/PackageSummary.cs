using YamlDotNet.Core.Tokens;

namespace PackageRegistryService.Pages.Components
{
    public record PackageSummary(string Name, string Summary, string [] Tags, string LatestVersion, DateOnly ReleaseDate)
    {
        public static string Render(PackageSummary summary)
        {
            return $@"<tr>
<th scope=""row""><a href=""/package/{summary.Name}"">{summary.Name}</a></th>
<td>{PackageDescription.RenderSmall(summary.Summary)}</td>
<td><a href=""/package/{summary.Name}/{summary.LatestVersion}"">{summary.LatestVersion}</a></td>
<td>{summary.ReleaseDate}</td>
<td>{string.Join("; ", summary.Tags.Select(PackageTag.RenderLink))}</td>
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
<th scope=""col"">Summary</th>
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
