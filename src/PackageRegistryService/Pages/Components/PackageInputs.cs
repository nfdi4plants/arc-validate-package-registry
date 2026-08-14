using PackageRegistryService.Models;
using System.Text.Encodings.Web;

namespace PackageRegistryService.Pages.Components
{
    /// <summary>
    /// Renders declared package inputs as an encoded command-line reference table.
    /// </summary>
    public class PackageInputs
    {
        /// <summary>HTML-encodes a possibly absent value.</summary>
        private static string Escape(string? value) => HtmlEncoder.Default.Encode(value ?? "");

        /// <summary>Formats an input type for display, including optionality.</summary>
        private static string RenderType(CommandInputType inputType)
        {
            var cwlType = CommandInputType.ToCwlString(inputType);
            return inputType.IsNullable
                ? $"{cwlType[..^1]} (optional)"
                : cwlType;
        }

        /// <summary>Renders the optional label and documentation for one input.</summary>
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

        /// <summary>Renders an input's command-line prefix and positional details.</summary>
        private static string RenderBinding(CommandInputBinding? binding)
        {
            binding ??= new CommandInputBinding();

            var prefix = $"<code>{Escape(binding.Prefix)}</code>";

            var details = new List<string>();
            if (binding.Position != 0)
            {
                details.Add($"position: {binding.Position}");
            }

            return details.Count == 0
                ? prefix
                : $"{prefix}<br /><small>{string.Join("; ", details)}</small>";
        }

        /// <summary>
        /// Renders all declared inputs, or no markup when the package has none.
        /// </summary>
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
      <td><code>{Escape(RenderType(input.Type))}</code></td>
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
