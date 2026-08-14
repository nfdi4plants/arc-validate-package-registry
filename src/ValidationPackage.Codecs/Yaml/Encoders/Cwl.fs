namespace ValidationPackage.Codecs.Yaml.Encoders

open ValidationPackage.Model
open ValidationPackage.Codecs.Yaml

[<RequireQualifiedAccess>]
module internal Cwl =

    let commandInputType (inputType: CommandInputType) =
        inputType
        |> CommandInputType.toCwlString
        |> Encoding.string

    let commandInputBinding (binding: CommandInputBinding) =
        [
            if binding.Position <> 0 then
                "position", Encoding.int binding.Position

            if binding.Prefix <> "" then
                "prefix", Encoding.string binding.Prefix
        ]
        |> Encoding.object

    let commandInputParameter (parameter: CommandInputParameter) =
        [
            "id", Encoding.string parameter.Id
            "type", commandInputType parameter.Type

            if parameter.Label <> "" then
                "label", Encoding.string parameter.Label

            if parameter.Doc <> "" then
                "doc", Encoding.string parameter.Doc

            "inputBinding", commandInputBinding parameter.InputBinding
        ]
        |> Encoding.object

    let commandInputParameters parameters =
        parameters
        |> CwlValidation.validateParameters
        |> Array.map commandInputParameter
        |> Encoding.array
