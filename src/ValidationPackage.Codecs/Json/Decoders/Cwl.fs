namespace ValidationPackage.Codecs.Json.Decoders

open Thoth.Json.Core
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module internal Cwl =

    let commandInputType: Decoder<CommandInputType> =
        Decode.string
        |> Decode.andThen (fun value ->
            try
                value
                |> CommandInputType.fromCwlString
                |> Decode.succeed
            with error ->
                Decode.fail error.Message
        )

    let commandInputBinding: Decoder<CommandInputBinding> =
        Decode.object (fun get ->
            CommandInputBinding.create(
                Position = (
                    get.Optional.Field "position" Decode.int
                    |> Option.defaultValue 0
                ),
                Prefix = get.Required.Field "prefix" Decode.string
            )
        )

    let commandInputParameter: Decoder<CommandInputParameter> =
        Decode.object (fun get ->
            CommandInputParameter.create(
                get.Required.Field "id" Decode.string,
                get.Required.Field "type" commandInputType,
                get.Required.Field "inputBinding" commandInputBinding,
                Label = (
                    get.Optional.Field "label" Decode.string
                    |> Option.defaultValue ""
                ),
                Doc = (
                    get.Optional.Field "doc" Decode.string
                    |> Option.defaultValue ""
                )
            )
        )
