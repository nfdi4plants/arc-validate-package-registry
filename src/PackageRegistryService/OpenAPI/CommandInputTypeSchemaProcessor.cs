using NJsonSchema;
using NJsonSchema.Generation;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using PackageRegistryService.Models;

namespace PackageRegistryService.OpenAPI;

/// <summary>
/// Publishes command-input types as their compact CWL string schema instead of storage objects.
/// </summary>
public sealed class CommandInputTypeSchemaProcessor : ISchemaProcessor, IDocumentProcessor
{
    /// <summary>The complete set of command-input type strings accepted by the API.</summary>
    public static readonly string[] SupportedValues =
    [
        "boolean",
        "boolean?",
        "int",
        "int?",
        "long",
        "long?",
        "float",
        "float?",
        "double",
        "double?",
        "string",
        "string?"
    ];

    /// <summary>
    /// Replaces the reflected service-model schema with a constrained string schema.
    /// </summary>
    /// <param name="context">The schema generation context.</param>
    public void Process(SchemaProcessorContext context)
    {
        if (context.ContextualType.Type != typeof(CommandInputType))
        {
            return;
        }

        context.Schema.Type = JsonObjectType.String;
        context.Schema.IsNullableRaw = false;
        context.Schema.Format = null;
        context.Schema.Properties.Clear();
        context.Schema.AllOf.Clear();
        context.Schema.AnyOf.Clear();
        context.Schema.OneOf.Clear();
        context.Schema.Enumeration.Clear();
        context.Schema.EnumerationNames.Clear();

        foreach (var value in SupportedValues)
        {
            context.Schema.Enumeration.Add(value);
        }

        context.Schema.Description =
            "A supported scalar CWL command input type, optionally nullable via a trailing '?'.";
    }

    /// <summary>
    /// Finalizes command-input requirements and removes storage-only definitions.
    /// </summary>
    /// <param name="context">The generated OpenAPI document context.</param>
    public void Process(DocumentProcessorContext context)
    {
        var inputParameter = context.Document.Definitions[nameof(CommandInputParameter)];
        foreach (var propertyName in new[] { "id", "type", "inputBinding" })
        {
            inputParameter.RequiredProperties.Add(propertyName);
            inputParameter.Properties[propertyName].IsNullableRaw = false;
        }

        // The schema processor runs after reflection has visited the normalized
        // CLR object. Remove the now-unreferenced storage enum so it cannot leak
        // into generated public clients.
        context.Document.Definitions.Remove(nameof(CwlPrimitive));
    }
}
