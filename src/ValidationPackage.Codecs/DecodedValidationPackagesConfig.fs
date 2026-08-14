namespace ValidationPackage.Codecs

open Fable.Core
open ValidationPackage.Model

[<AttachMembers>]
type DecodedValidationPackagesConfig private (
    isLegacy: bool,
    canonical: ValidationPackagesConfig,
    legacy: LegacyValidationPackagesConfig
) =

    let _isLegacy = isLegacy
    let _canonical = canonical
    let _legacy = legacy

    member _.IsLegacy = _isLegacy
    member _.Canonical = _canonical
    member _.Legacy = _legacy

    static member canonical(config: ValidationPackagesConfig) =
        DecodedValidationPackagesConfig(
            false,
            config,
            Unchecked.defaultof<LegacyValidationPackagesConfig>
        )

    static member legacy(config: LegacyValidationPackagesConfig) =
        DecodedValidationPackagesConfig(
            true,
            Unchecked.defaultof<ValidationPackagesConfig>,
            config
        )
