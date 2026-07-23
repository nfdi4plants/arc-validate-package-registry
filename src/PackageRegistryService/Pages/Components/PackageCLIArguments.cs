using static AVPRIndex.Domain;

namespace PackageRegistryService.Pages.Components
{
    public class PackageCLIArguments
    {
        private static string Escape(string value) => System.Security.SecurityElement.Escape(value) ?? "";

        public static string Render(CLIArgument[]? cliArguments)
        {
            // omit the whole section when the package declares no arguments
            if (cliArguments == null || cliArguments.Length == 0)
            {
                return "";
            }

            var rows = string.Join(
                System.Environment.NewLine,
                cliArguments.Select(a =>
                {
                    var flags = string.Join(", ", (a.Flags ?? []).Select(f => $"<code>{Escape(f)}</code>"));
                    return $@"    <tr>
      <td>{flags}</td>
      <td>{Escape(a.Description)}</td>
      <td><code>{Escape(a.Example)}</code></td>
    </tr>";
                }));

            return $@"<section>
  <h2>Available Commands</h2>
  <table>
    <thead>
      <tr>
        <th scope=""col"">Flags</th>
        <th scope=""col"">Description</th>
        <th scope=""col"">Example</th>
      </tr>
    </thead>
    <tbody>
{rows}
    </tbody>
  </table>
</section>
<hr />
";
        }
    }
}
