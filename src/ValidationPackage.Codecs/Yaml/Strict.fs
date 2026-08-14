namespace ValidationPackage.Codecs.Yaml

open System
open YAMLicious
open YAMLicious.YAMLiciousTypes
open ValidationPackage.Model

[<RequireQualifiedAccess>]
module internal Strict =

    let private fail path message =
        invalidArg "yaml" $"{path}: {message}"

    let private validateContent path (content: YAMLContent) =
        if content.Anchor.IsSome then
            fail path "anchors are not supported"

        if content.Tag.IsSome then
            fail path "explicit tags are not supported"

    let rec validateSyntax path element =
        match element with
        | YAMLElement.Mapping(key, value) ->
            validateContent path key

            if key.Value = "<<" then
                fail path "merge keys are not supported"

            validateSyntax ($"{path}.{key.Value}") value
        | YAMLElement.Value content ->
            validateContent path content
        | YAMLElement.Sequence values
        | YAMLElement.Object values ->
            values
            |> List.iteri (fun index value -> validateSyntax ($"{path}[{index}]") value)
        | YAMLElement.Alias _ ->
            fail path "aliases are not supported"
        | YAMLElement.DocumentStart
        | YAMLElement.DocumentEnd ->
            fail path "multiple YAML documents are not supported"
        | YAMLElement.Nil ->
            fail path "implicit null values are not supported"
        | YAMLElement.Comment _ -> ()

    let read yaml =
        if isNull yaml then
            nullArg "yaml"

        YAMLicious.Reader.read yaml

    let readStrict yaml =
        let element = read yaml
        validateSyntax "$" element
        element

    let mapping path element =
        let values =
            match element with
            | YAMLElement.Object values -> values
            | _ -> fail path "must be a mapping"

        let entries =
            values
            |> List.map (fun value ->
                match value with
                | YAMLElement.Mapping(key, fieldValue) ->
                    validateContent path key
                    key.Value, fieldValue
                | _ -> fail path "must contain only mapping fields"
            )
            |> List.toArray

        entries
        |> Array.countBy fst
        |> Array.tryFind (fun (_, count) -> count > 1)
        |> Option.iter (fun (key, _) -> fail path $"duplicate field '{key}'")

        entries

    let sequence path element =
        match element with
        | YAMLElement.Sequence values
        | YAMLElement.Object [ YAMLElement.Sequence values ] ->
            values |> List.toArray
        | _ -> fail path "must be a sequence"

    let scalarContent path element =
        match element with
        | YAMLElement.Value content
        | YAMLElement.Object [ YAMLElement.Value content ] ->
            validateContent path content
            content
        | YAMLElement.Nil
        | YAMLElement.Object [ YAMLElement.Nil ] ->
            fail path "implicit null values are not supported"
        | _ -> fail path "must be a scalar"

    let scalar path element =
        (scalarContent path element).Value

    let validateAllowedFields path allowed entries =
        entries
        |> Array.iter (fun (key, _) ->
            if allowed |> Array.exists (fun expected -> expected = key) |> not then
                fail path $"unknown field '{key}'"
        )

    let tryField name entries =
        entries
        |> Array.tryPick (fun (key, value) -> if key = name then Some value else None)

    let requiredField path name entries =
        match tryField name entries with
        | Some value -> value
        | None -> fail path $"missing required field '{name}'"

    let optionalScalar path name entries =
        tryField name entries
        |> Option.map (scalar ($"{path}.{name}"))

    let requiredScalar path name entries =
        entries
        |> requiredField path name
        |> scalar ($"{path}.{name}")

    let parseSemVer path value =
        match SemVer.tryParse value with
        | Some version -> version
        | None -> fail path $"must be a full semantic version, but was '{value}'"

    let schemaValue entries =
        tryField "$schema" entries
        |> Option.map (fun element ->
            let content = scalarContent "$.$schema" element
            let value = content.Value

            let isQuoted =
                match content.Style with
                | Some ScalarStyle.SingleQuoted
                | Some ScalarStyle.DoubleQuoted -> true
                | _ -> false

            if
                not isQuoted
                && (
                    value = "null"
                    || value = "true"
                    || value = "false"
                    || ValidationPackageInputValue.isJsonNumber value
                )
            then
                fail "$.$schema" "UnsupportedSchema: schema reference must be a string"

            value
        )
