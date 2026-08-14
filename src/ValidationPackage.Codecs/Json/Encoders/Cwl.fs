namespace ValidationPackage.Codecs.Json.Encoders

open Thoth.Json.Core
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module internal Cwl =

    let commandInputType (inputType: CommandInputType) =
        inputType
        |> CommandInputType.toCwlString
        |> Encode.string

    let commandInputBinding (binding: CommandInputBinding) =
        Encode.object [
            "position", Encode.int binding.Position
            "prefix", Encode.string binding.Prefix
        ]

    let commandInputParameter (parameter: CommandInputParameter) =
        Encode.object [
            "id", Encode.string parameter.Id
            "type", commandInputType parameter.Type
            "label", Encode.string parameter.Label
            "doc", Encode.string parameter.Doc
            "inputBinding", commandInputBinding parameter.InputBinding
        ]
