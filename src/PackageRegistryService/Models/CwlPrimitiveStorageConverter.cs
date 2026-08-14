using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PackageRegistryService.Models;

/// <summary>
/// Converts supported CWL primitive values to and from their stable database representation.
/// </summary>
public sealed class CwlPrimitiveStorageConverter : ValueConverter<CwlPrimitive, string>
{
    /// <summary>
    /// Creates the Entity Framework value converter for CWL primitives.
    /// </summary>
    public CwlPrimitiveStorageConverter()
        : base(
            primitive => ToStorageValue(primitive),
            storedValue => FromStorageValue(storedValue))
    {
    }

    /// <summary>
    /// Converts a primitive to the lowercase value stored in JSON-backed metadata.
    /// </summary>
    /// <param name="primitive">The primitive value.</param>
    /// <returns>The stable storage value.</returns>
    public static string ToStorageValue(CwlPrimitive primitive) =>
        primitive switch
        {
            CwlPrimitive.Boolean => "boolean",
            CwlPrimitive.Int => "int",
            CwlPrimitive.Long => "long",
            CwlPrimitive.Float => "float",
            CwlPrimitive.Double => "double",
            CwlPrimitive.String => "string",
            _ => throw new ArgumentOutOfRangeException(nameof(primitive), primitive, "Unsupported CWL primitive type")
        };

    /// <summary>
    /// Parses a lowercase primitive value read from stored metadata.
    /// </summary>
    /// <param name="storedValue">The stored primitive name.</param>
    /// <returns>The corresponding primitive value.</returns>
    public static CwlPrimitive FromStorageValue(string storedValue) =>
        storedValue switch
        {
            "boolean" => CwlPrimitive.Boolean,
            "int" => CwlPrimitive.Int,
            "long" => CwlPrimitive.Long,
            "float" => CwlPrimitive.Float,
            "double" => CwlPrimitive.Double,
            "string" => CwlPrimitive.String,
            _ => throw new ArgumentException($"Unsupported stored CWL primitive type: {storedValue}", nameof(storedValue))
        };
}
