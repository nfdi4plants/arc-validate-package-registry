namespace ValidationPackage.Codecs

[<RequireQualifiedAccess>]
module ValidationPackageJson =

    let encoder = Json.Encoders.ValidationPackage.encode
    let decoder = Json.Decoders.ValidationPackage.decoder

    let encode metadata =
        JsonRuntime.encode encoder metadata

    let decode json : Result<ValidationPackage.Model.ValidationPackageMetadata, string> =
        match JsonRuntime.decodeValidationPackage decoder json with
        | Ok metadata ->
            try
                ValidationPackage.Model.CommandInputParameter.validate metadata.Inputs
                |> ignore

                Ok metadata
            with error ->
                Error error.Message
        | Error error -> Error error

    let decodeOrFail json =
        match decode json with
        | Ok metadata -> metadata
        | Error message -> invalidArg "json" message
