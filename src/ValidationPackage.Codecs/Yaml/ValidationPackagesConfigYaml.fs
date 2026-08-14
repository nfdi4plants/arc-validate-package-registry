namespace ValidationPackage.Codecs

[<RequireQualifiedAccess>]
module ValidationPackagesConfigYaml =

    let internal encoder = Yaml.Encoders.ValidationPackagesConfig.encode
    let decoder = Yaml.Decoders.ValidationPackagesConfig.decoder

    let encode config =
        config
        |> encoder
        |> Yaml.Encoding.write

    let decode yaml =
        try
            yaml
            |> Yaml.Strict.read
            |> decoder
            |> Ok
        with error ->
            Error error.Message

    let decodeOrFail yaml =
        match decode yaml with
        | Ok config -> config
        | Error message -> invalidArg "yaml" message

    let decodeCurrent yaml =
        try
            yaml
            |> Yaml.Strict.read
            |> Yaml.Decoders.ValidationPackagesConfig.currentDecoder
            |> Ok
        with error ->
            Error error.Message

    let decodeCurrentOrFail yaml =
        match decodeCurrent yaml with
        | Ok config -> config
        | Error message -> invalidArg "yaml" message

    let decodeLegacy yaml =
        try
            yaml
            |> Yaml.Strict.read
            |> Yaml.Decoders.ValidationPackagesConfig.legacyDecoder
            |> Ok
        with error ->
            Error error.Message

    let decodeLegacyOrFail yaml =
        match decodeLegacy yaml with
        | Ok config -> config
        | Error message -> invalidArg "yaml" message
