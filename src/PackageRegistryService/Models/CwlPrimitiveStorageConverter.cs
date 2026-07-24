using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using static AVPRIndex.Domain;

namespace PackageRegistryService.Models;

public sealed class CwlPrimitiveStorageConverter : ValueConverter<CwlPrimitive, string>
{
    public CwlPrimitiveStorageConverter()
        : base(
            primitive => ToStorageValue(primitive),
            storedValue => FromStorageValue(storedValue))
    {
    }

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
