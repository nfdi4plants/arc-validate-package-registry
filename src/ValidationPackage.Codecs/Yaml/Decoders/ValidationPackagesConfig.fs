namespace ValidationPackage.Codecs.Yaml.Decoders

open YAMLicious.YAMLiciousTypes
open ValidationPackage.Model
open ValidationPackage.Codecs
open ValidationPackage.Codecs.Yaml

[<RequireQualifiedAccess>]
module internal ValidationPackagesConfig =

    let private rollForward path value =
        match value with
        | "disable" -> RollForwardPolicy.Disable
        | "latest_patch" -> RollForwardPolicy.LatestPatch
        | "latest_minor" -> RollForwardPolicy.LatestMinor
        | _ -> invalidArg "yaml" $"{path}: unsupported roll_forward value '{value}'"

    let private inputValue path element =
        let content = Strict.scalarContent path element

        match content.Style with
        | Some ScalarStyle.SingleQuoted
        | Some ScalarStyle.DoubleQuoted ->
            ValidationPackageInputValue.string content.Value
        | Some(ScalarStyle.Block _) ->
            invalidArg "yaml" $"{path}: input strings must use quoted scalar syntax"
        | None
        | Some ScalarStyle.Plain ->
            match content.Value with
            | "null" -> ValidationPackageInputValue.nullValue()
            | "true" -> ValidationPackageInputValue.boolean true
            | "false" -> ValidationPackageInputValue.boolean false
            | value when ValidationPackageInputValue.isJsonInteger value ->
                ValidationPackageInputValue.integer value
            | value when ValidationPackageInputValue.isJsonNumber value ->
                ValidationPackageInputValue.floatingPoint value
            | value ->
                invalidArg
                    "yaml"
                    $"{path}: '{value}' is not a JSON-compatible scalar; strings must be quoted"

    let private inputs path element =
        let entries = Strict.mapping path element

        entries
        |> Array.map (fun (id, value) ->
            ValidationPackageInput.create(id, inputValue ($"{path}['{id}']") value)
        )

    let private selection index element =
        let path = $"$.validation_packages[{index}]"
        let entries = Strict.mapping path element

        Strict.validateAllowedFields
            path
            [| "name"; "version"; "roll_forward"; "inputs" |]
            entries

        let name = Strict.requiredScalar path "name" entries

        let version =
            entries
            |> Strict.requiredScalar path "version"
            |> Strict.parseSemVer ($"{path}.version")

        let policy =
            Strict.optionalScalar path "roll_forward" entries
            |> Option.map (rollForward ($"{path}.roll_forward"))
            |> Option.defaultValue RollForwardPolicy.Disable

        let configuredInputs =
            Strict.tryField "inputs" entries
            |> Option.map (inputs ($"{path}.inputs"))
            |> Option.defaultValue Array.empty

        ValidationPackageSelection.create(
            name,
            version,
            RollForward = policy,
            Inputs = configuredInputs
        )

    let currentDecoder element =
        Strict.validateSyntax "$" element
        let entries = Strict.mapping "$" element
        Strict.validateAllowedFields "$" [| "$schema"; "arc_specification"; "validation_packages" |] entries

        match Strict.schemaValue entries with
        | Some schema when schema = SchemaUris.ValidationPackagesConfigV1 -> ()
        | Some schema ->
            invalidArg "yaml" $"UnsupportedSchema: unsupported validation-packages schema '{schema}'"
        | None ->
            invalidArg "yaml" "canonical validation-packages config requires $schema"

        let arcSpecification =
            Strict.optionalScalar "$" "arc_specification" entries
            |> Option.map (Strict.parseSemVer "$.arc_specification")

        let selections =
            entries
            |> Strict.requiredField "$" "validation_packages"
            |> Strict.sequence "$.validation_packages"
            |> Array.mapi selection

        match arcSpecification with
        | Some version -> ValidationPackagesConfig.create(selections, ArcSpecification = version)
        | None -> ValidationPackagesConfig.create(selections)

    let private legacySelection index element =
        let path = $"$.validation_packages[{index}]"
        let entries = Strict.mapping path element
        Strict.validateAllowedFields path [| "name"; "version" |] entries
        let name = Strict.requiredScalar path "name" entries

        match Strict.optionalScalar path "version" entries with
        | Some value ->
            LegacyValidationPackageSelection.create(
                name,
                Version = Strict.parseSemVer ($"{path}.version") value
            )
        | None -> LegacyValidationPackageSelection.create(name)

    let legacyDecoder element =
        Strict.validateSyntax "$" element
        let entries = Strict.mapping "$" element
        Strict.validateAllowedFields "$" [| "arc_specification"; "validation_packages" |] entries

        if Strict.tryField "$schema" entries |> Option.isSome then
            invalidArg "yaml" "legacy validation-packages config must not contain $schema"

        let arcSpecification =
            Strict.optionalScalar "$" "arc_specification" entries
            |> Option.map (Strict.parseSemVer "$.arc_specification")

        let selections =
            Strict.tryField "validation_packages" entries
            |> Option.map (Strict.sequence "$.validation_packages")
            |> Option.defaultValue Array.empty
            |> Array.mapi legacySelection

        match arcSpecification with
        | Some version -> LegacyValidationPackagesConfig.create(selections, ArcSpecification = version)
        | None -> LegacyValidationPackagesConfig.create(selections)

    let decoder element =
        let entries = Strict.mapping "$" element

        match Strict.schemaValue entries with
        | None ->
            element
            |> legacyDecoder
            |> DecodedValidationPackagesConfig.legacy
        | Some schema when schema = SchemaUris.ValidationPackagesConfigV1 ->
            element
            |> currentDecoder
            |> DecodedValidationPackagesConfig.canonical
        | Some schema ->
            invalidArg "yaml" $"UnsupportedSchema: unsupported validation-packages schema '{schema}'"
