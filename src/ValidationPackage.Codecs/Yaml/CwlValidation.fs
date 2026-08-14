namespace ValidationPackage.Codecs.Yaml

open ValidationPackage.Model

[<RequireQualifiedAccess>]
module internal CwlValidation =

    let validateParameters (parameters: CommandInputParameter array) =
        CommandInputParameter.validate parameters
