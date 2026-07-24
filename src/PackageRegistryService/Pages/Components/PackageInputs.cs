using static AVPRIndex.Domain;
using System.Text.Encodings.Web;

namespace PackageRegistryService.Pages.Components
{
    public class PackageInputs
    {
        private static string Escape(string? value) => HtmlEncoder.Default.Encode(value ?? "");

        private static string RenderDocumentation(CommandInputParameter input)
        {
            var label = Escape(input.Label);
            var documentation = Escape(input.Doc);

            return (label, documentation) switch
            {
                ("", "") => "",
                (_, "") => $"<strong>{label}</strong>",
                ("", _) => documentation,
                _ => $"<strong>{label}</strong><br />{documentation}"
            };
        }

        private static string RenderBinding(CommandInputBinding? binding)
        {
            binding ??= new CommandInputBinding();

            var prefix = string.IsNullOrEmpty(binding.Prefix)
                ? "<em>positional</em>"
                : $"<code>{Escape(binding.Prefix)}</code>";

            var details = new List<string>();
            if (binding.Position != 0)
            {
                details.Add($"position: {binding.Position}");
            }

            if (!binding.Separate)
            {
                details.Add("separate: false");
            }

            return details.Count == 0
                ? prefix
                : $"{prefix}<br /><small>{string.Join("; ", details)}</small>";
        }

        public static string Render(CommandInputParameter[]? inputs)
        {
            if (inputs == null || inputs.Length == 0)
            {
                return "";
            }

            var rows = string.Join(
                System.Environment.NewLine,
                inputs.Select(input =>
                {
                    return $@"    <tr>
      <td><code>{Escape(input.Id)}</code></td>
      <td><code>{Escape(CommandInputType.toCwlString(input.Type))}</code></td>
      <td>{RenderBinding(input.InputBinding)}</td>
      <td>{RenderDocumentation(input)}</td>
    </tr>";
                }));

            return $@"<section>
  <h2>Available Commands</h2>
  <table>
    <thead>
      <tr>
        <th scope=""col"">Input</th>
        <th scope=""col"">Type</th>
        <th scope=""col"">Binding</th>
        <th scope=""col"">Documentation</th>
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
