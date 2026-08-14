namespace ValidationPackage.Codecs.Yaml.Decoders

open System
open YAMLicious
open YAMLicious.YAMLiciousTypes
open ValidationPackage.Model
open ValidationPackage.Codecs.Yaml

[<RequireQualifiedAccess>]
module internal Cwl =

    let commandInputType element =
        let value =
            match element with
            | YAMLElement.Value _
            | YAMLElement.Object [ YAMLElement.Value _ ] ->
                Decode.string element
            | _ ->
                invalidArg
                    "element"
                    "CWL command input type must be one supported scalar type string"

        try
            CommandInputType.fromCwlString value
        with
        | :? ArgumentException ->
            invalidArg "element" $"unsupported CWL command input type: {value}"

    let commandInputBinding element =
        let entries = Strict.mapping "inputBinding" element
        Strict.validateAllowedFields "inputBinding" [| "prefix"; "position" |] entries

        CommandInputBinding.create(
            Position = (
                Strict.tryField "position" entries
                |> Option.map Decode.int
                |> Option.defaultValue 0
            ),
            Prefix = Strict.requiredScalar "inputBinding" "prefix" entries
        )

    let commandInputParameter element =
        let entries = Strict.mapping "command input" element

        Strict.validateAllowedFields
            "command input"
            [| "id"; "type"; "label"; "doc"; "inputBinding" |]
            entries

        CommandInputParameter.create(
            Strict.requiredScalar "command input" "id" entries,
            entries
            |> Strict.requiredField "command input" "type"
            |> commandInputType,
            entries
            |> Strict.requiredField "command input" "inputBinding"
            |> commandInputBinding,
            Label = (
                Strict.optionalScalar "command input" "label" entries
                |> Option.defaultValue ""
            ),
            Doc = (
                Strict.optionalScalar "command input" "doc" entries
                |> Option.defaultValue ""
            )
        )

    let commandInputParameters element =
        match element with
        | YAMLElement.Sequence _
        | YAMLElement.Object [ YAMLElement.Sequence _ ] ->
            element
            |> Decode.array commandInputParameter
            |> CwlValidation.validateParameters
        | _ ->
            invalidArg "element" "AVPR Inputs must use the CWL array form"
