using YamlDotNet.Core.Tokens;

namespace PackageRegistryService.Pages.Components
{
    public record PackageSummary(string Name, string Summary, string Language, string [] Tags, string LatestVersion, DateOnly ReleaseDate, int TotalDownloads)
    {
        public static string Render(PackageSummary summary)
        {
            return $@"<tr>
<th scope=""row""><a href=""/package/{summary.Name}"">{summary.Name}</a></th>
<td>{PackageLanguage.RenderTagOnly(summary.Language)}</td>
<td>{PackageDescription.RenderSmall(summary.Summary)}</td>
<td><a href=""/package/{summary.Name}/{summary.LatestVersion}"">{summary.LatestVersion}</a></td>
<td>{summary.ReleaseDate}</td>
<td>{string.Join("; ", summary.Tags.Select(PackageTag.RenderLink))}</td>
<td>{summary.TotalDownloads}</td>
</tr>";
        }

        public static string RenderTable(string headerText, string DescriptionText, IEnumerable<PackageSummary> summaries)
        {
            return @$"<h1>{headerText}</h1><br>
<p>{DescriptionText}</p>
<div class=""overflow-auto"">
<table class=""striped"">
<thead>
<tr>
<th scope=""col"">Name</th>
<th scope=""col"">Language</th>
<th scope=""col"">Summary</th>
<th scope=""col"">Latest stable version</th>
<th scope=""col"">Release date</th>
<th scope=""col"">Tags</th>
<th scope=""col"">Total Downloads</th>
</tr>
</thead>
{string.Join(System.Environment.NewLine, summaries.Select(PackageSummary.Render))}
</table>
</div>";
        }

        public static string RenderList(IEnumerable<PackageSummary> summaries)
        {
            var testPackages =
                summaries
                .Where(p => p.Name.Contains("test"));
            
            var prodPackages =
                summaries
                .Where(p => !p.Name.Contains("test"));

            var testContent = PackageSummary.RenderTable("Test Packages", "These packages are used primarily to internally test validation pipelines.", testPackages);
            var prodContent = PackageSummary.RenderTable("Available Validation Packages", "These packages are intended to be used in ARC validation pipelines.", prodPackages);

            return prodContent + "<br><br>" + testContent;
        }
    }
}
