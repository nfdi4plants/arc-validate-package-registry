namespace PackageRegistryService.Pages.Components
{
    public record PackageSummary(string Name, string Description,  string [] Tags, string LatestVersion)
    {
        public static string Render(PackageSummary summary)
        {
            return $@"<tr>
<th scope=""row""><a href=""/package/{summary.Name}"">{summary.Name}</a></th>
<td>{summary.Description}</td>
<td>{summary.LatestVersion}</td>
<td>{string.Join("", summary.Tags.Select(t => $"<b>{t} </b>"))}</td>
</tr>";
        }

        public static string RenderList(IEnumerable<PackageSummary> summaries)
        {
            var content = @$"<h1>All available validation packages</h1><br>#
<table class=""striped"">
<thead>
<tr>
<th scope=""col"">Name</th>
<th scope=""col"">Description</th>
<th scope=""col"">Latest version</th>
<th scope=""col"">Tags</th>
</tr>
</thead>
{string.Join(System.Environment.NewLine, summaries.Select(PackageSummary.Render))}
</table>";
            return content;
        }
    }
}
