namespace ValidationPackage.Codecs.Yaml

open YAMLicious
open YAMLicious.YAMLiciousTypes

[<RequireQualifiedAccess>]
module internal Encoding =

    let string value =
        YAMLContent.create(value, style = ScalarStyle.DoubleQuoted)
        |> YAMLElement.Value

    let plain value =
        YAMLContent.create(value, style = ScalarStyle.Plain)
        |> YAMLElement.Value

    let int value = Encode.int value
    let bool value = Encode.bool value
    let object values = Encode.object values

    let objectWithQuotedKeys values =
        values
        |> List.map (fun (key, value) ->
            YAMLElement.Mapping(
                YAMLContent.create(key, style = ScalarStyle.DoubleQuoted),
                value
            )
        )
        |> YAMLElement.Object

    let array values =
        values
        |> Array.toList
        |> YAMLElement.Sequence

    let write value = Encode.write 2 value
