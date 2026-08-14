namespace PackageRegistryService.Models;

/// <summary>
/// Identifies the supported scalar primitive types from the CWL type system.
/// </summary>
public enum CwlPrimitive
{
    /// <summary>A Boolean value.</summary>
    Boolean,

    /// <summary>A 32-bit integer value.</summary>
    Int,

    /// <summary>A 64-bit integer value.</summary>
    Long,

    /// <summary>A single-precision floating-point value.</summary>
    Float,

    /// <summary>A double-precision floating-point value.</summary>
    Double,

    /// <summary>A text value.</summary>
    String
}
