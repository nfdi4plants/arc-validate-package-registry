using System.Text.Json;
using System.Text.Json.Serialization;

namespace PackageRegistryService.Models;

/// <summary>
/// Represents one supported scalar CWL input type and its nullability.
/// </summary>
[JsonConverter(typeof(CommandInputTypeJsonConverter))]
public sealed class CommandInputType
{
    private CwlPrimitive primitiveType = CwlPrimitive.String;

    /// <summary>The primitive CWL type.</summary>
    public CwlPrimitive PrimitiveType
    {
        get => primitiveType;
        set
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Unsupported CWL primitive type");
            }

            primitiveType = value;
        }
    }

    /// <summary>Whether the input may be omitted or supplied as null.</summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Parses the compact CWL scalar-type notation used by public JSON contracts.
    /// </summary>
    /// <param name="value">A supported CWL type, optionally followed by a nullable marker.</param>
    /// <returns>The corresponding service input type.</returns>
    /// <exception cref="ArgumentException">The value does not name a supported scalar type.</exception>
    public static CommandInputType FromCwlString(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var isNullable = value.EndsWith("?", StringComparison.Ordinal);
        var primitiveName = isNullable ? value[..^1] : value;
        var primitive = primitiveName switch
        {
            "boolean" => CwlPrimitive.Boolean,
            "int" => CwlPrimitive.Int,
            "long" => CwlPrimitive.Long,
            "float" => CwlPrimitive.Float,
            "double" => CwlPrimitive.Double,
            "string" => CwlPrimitive.String,
            _ => throw new ArgumentException(
                $"Unsupported CWL command input type: {value}",
                nameof(value))
        };

        return new CommandInputType
        {
            PrimitiveType = primitive,
            IsNullable = isNullable
        };
    }

    /// <summary>
    /// Formats a service input type using compact CWL scalar-type notation.
    /// </summary>
    /// <param name="inputType">The input type to format.</param>
    /// <returns>The CWL scalar type with an optional nullable marker.</returns>
    public static string ToCwlString(CommandInputType inputType)
    {
        ArgumentNullException.ThrowIfNull(inputType);

        var primitive = inputType.PrimitiveType switch
        {
            CwlPrimitive.Boolean => "boolean",
            CwlPrimitive.Int => "int",
            CwlPrimitive.Long => "long",
            CwlPrimitive.Float => "float",
            CwlPrimitive.Double => "double",
            CwlPrimitive.String => "string",
            _ => throw new ArgumentOutOfRangeException(
                nameof(inputType),
                inputType.PrimitiveType,
                "Unsupported CWL primitive type")
        };

        return inputType.IsNullable ? $"{primitive}?" : primitive;
    }
}

/// <summary>
/// Serializes <see cref="CommandInputType"/> as the compact CWL scalar-type string.
/// </summary>
public sealed class CommandInputTypeJsonConverter : JsonConverter<CommandInputType>
{
    /// <inheritdoc />
    public override CommandInputType Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException(
                "CWL command input type must be one supported scalar type string");
        }

        var value = reader.GetString()!;

        try
        {
            return CommandInputType.FromCwlString(value);
        }
        catch (ArgumentException)
        {
            throw new JsonException($"Unsupported CWL command input type: {value}");
        }
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        CommandInputType value,
        JsonSerializerOptions options)
    {
        try
        {
            writer.WriteStringValue(CommandInputType.ToCwlString(value));
        }
        catch (ArgumentException error)
        {
            throw new JsonException(error.Message, error);
        }
    }
}
