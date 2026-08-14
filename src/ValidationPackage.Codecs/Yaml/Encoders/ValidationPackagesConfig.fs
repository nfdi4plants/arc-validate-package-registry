namespace ValidationPackage.Codecs.Yaml.Encoders

open ValidationPackage.Model
open ValidationPackage.Codecs
open ValidationPackage.Codecs.Yaml

[<RequireQualifiedAccess>]
module internal ValidationPackagesConfig =

    let private inputValue (value: ValidationPackageInputValue) =
        match value.Kind with
        | ValidationPackageInputValueKind.Null -> Encoding.plain "null"
        | ValidationPackageInputValueKind.Boolean
        | ValidationPackageInputValueKind.Integer
        | ValidationPackageInputValueKind.FloatingPoint -> Encoding.plain value.Value
        | ValidationPackageInputValueKind.String -> Encoding.string value.Value
        | kind -> invalidArg "value" $"unsupported validation-package input value kind: {kind}"

    let private inputs (values: ValidationPackageInput array) =
        values
        |> Array.map (fun input -> input.Id, inputValue input.Value)
        |> Array.toList
        |> Encoding.objectWithQuotedKeys

    let private rollForward policy =
        match policy with
        | RollForwardPolicy.Disable -> "disable"
        | RollForwardPolicy.LatestPatch -> "latest_patch"
        | RollForwardPolicy.LatestMinor -> "latest_minor"
        | value -> invalidArg "policy" $"unsupported roll-forward policy: {value}"

    let private selection (value: ValidationPackageSelection) =
        Encoding.object [
            "name", Encoding.string value.Name
            "version", value.Version |> SemVer.toString |> Encoding.string

            if value.RollForward <> RollForwardPolicy.Disable then
                "roll_forward", value.RollForward |> rollForward |> Encoding.string

            if value.Inputs.Length > 0 then
                "inputs", inputs value.Inputs
        ]

    let encode (config: ValidationPackagesConfig) =
        ValidationPackagesConfig.validate config |> ignore

        Encoding.object [
            "$schema", Encoding.string SchemaUris.ValidationPackagesConfigV1

            if config.HasArcSpecification then
                "arc_specification", config.ArcSpecification |> SemVer.toString |> Encoding.string

            "validation_packages",
            config.ValidationPackages
            |> Array.map selection
            |> Encoding.array
        ]
