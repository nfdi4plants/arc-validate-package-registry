namespace ValidationPackage.Codecs

open System
open System.Globalization
open System.Text
open Thoth.Json.Core

[<RequireQualifiedAccess>]
module JsonRuntime =

    let rec private writeJson value =
        match value with
        | Json.String value ->
            let escaped =
                value
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\b", "\\b")
                    .Replace("\f", "\\f")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t")

            $"\"{escaped}\""
        | Json.Number value ->
#if FABLE_COMPILER
            value.ToString()
#else
            value.ToString("R", CultureInfo.InvariantCulture)
#endif
        | Json.Null -> "null"
        | Json.Boolean value -> if value then "true" else "false"
        | Json.Array values ->
            values
            |> List.map writeJson
            |> String.concat ","
            |> fun content -> "[" + content + "]"
        | Json.Object fields ->
            fields
            |> List.map (fun (key, fieldValue) ->
                writeJson (Json.String key) + ":" + writeJson fieldValue
            )
            |> String.concat ","
            |> fun content -> "{" + content + "}"

    let private encoderHelpers =
        { new IEncoderHelpers<Json> with
            member _.encodeString value = Json.String value
            member _.encodeChar value = Json.String(string value)
            member _.encodeDecimalNumber value = Json.Number value
            member _.encodeBool value = Json.Boolean value
            member _.encodeNull () = Json.Null
            member _.encodeObject values = Json.Object(Seq.toList values)
            member _.encodeArray values = Json.Array(Array.toList values)
            member _.encodeList values = Json.Array values
            member _.encodeSeq values = Json.Array(Seq.toList values)
            member _.encodeResizeArray values = Json.Array(Seq.toList values)
            member _.encodeSignedIntegralNumber value = Json.Number(float value)
            member _.encodeUnsignedIntegralNumber value = Json.Number(float value)
        }

    let private decoderHelpers =
        { new IDecoderHelpers<Json> with
            member _.isString value =
                match value with
                | Json.String _ -> true
                | _ -> false

            member _.isNumber value =
                match value with
                | Json.Number _ -> true
                | _ -> false

            member _.isBoolean value =
                match value with
                | Json.Boolean _ -> true
                | _ -> false

            member _.isNullValue value = value = Json.Null

            member _.isArray value =
                match value with
                | Json.Array _ -> true
                | _ -> false

            member _.isObject value =
                match value with
                | Json.Object _ -> true
                | _ -> false

            member _.hasProperty name value =
                match value with
                | Json.Object fields ->
                    fields |> List.exists (fun (key, _) -> key = name)
                | _ -> false

            member _.isIntegralValue value =
                match value with
                | Json.Number number -> Math.Floor(number) = number
                | _ -> false

            member _.asString value =
                match value with
                | Json.String value -> value
                | _ -> invalidArg "value" "Expected a JSON string"

            member _.asFloat value =
                match value with
                | Json.Number value -> value
                | _ -> invalidArg "value" "Expected a JSON number"

            member _.asFloat32 value =
                match value with
                | Json.Number value -> float32 value
                | _ -> invalidArg "value" "Expected a JSON number"

            member _.asInt value =
                match value with
                | Json.Number value -> int value
                | _ -> invalidArg "value" "Expected a JSON number"

            member _.asBoolean value =
                match value with
                | Json.Boolean value -> value
                | _ -> invalidArg "value" "Expected a JSON boolean"

            member _.asArray value =
                match value with
                | Json.Array values -> List.toArray values
                | _ -> invalidArg "value" "Expected a JSON array"

            member _.getProperty(name, value) =
                match value with
                | Json.Object fields ->
                    fields
                    |> List.find (fun (key, _) -> key = name)
                    |> snd
                | _ -> invalidArg "value" "Expected a JSON object"

            member _.getProperties value =
                match value with
                | Json.Object fields -> fields |> Seq.map fst
                | _ -> Seq.empty

            member _.anyToString value = writeJson value
        }

    module private Parser =

        let parse (source: string) =
            let index = ref 0

            let fail message =
                invalidArg "json" $"Invalid JSON at position {index.Value}: {message}"

            let peek () =
                if index.Value < source.Length then
                    Some source[index.Value]
                else
                    None

            let take () =
                match peek () with
                | Some value ->
                    index.Value <- index.Value + 1
                    value
                | None -> fail "unexpected end of input"

            let rec skipWhitespace () =
                match peek () with
                | Some(' ' | '\t' | '\r' | '\n') ->
                    index.Value <- index.Value + 1
                    skipWhitespace ()
                | _ -> ()

            let expect expected =
                let actual = take ()
                if actual <> expected then fail $"expected '{expected}'"

            let parseHexCharacter () =
                if index.Value + 4 > source.Length then
                    fail "incomplete Unicode escape"

                let digits = source.Substring(index.Value, 4)
                index.Value <- index.Value + 4

                match Int32.TryParse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture) with
                | true, value -> char value
                | false, _ -> fail "invalid Unicode escape"

            let parseString () =
                expect '\"'
                let result = StringBuilder()
                let mutable complete = false

                while not complete do
                    match take () with
                    | '\"' -> complete <- true
                    | '\\' ->
                        let escaped =
                            match take () with
                            | '\"' -> '\"'
                            | '\\' -> '\\'
                            | '/' -> '/'
                            | 'b' -> '\b'
                            | 'f' -> '\f'
                            | 'n' -> '\n'
                            | 'r' -> '\r'
                            | 't' -> '\t'
                            | 'u' -> parseHexCharacter ()
                            | _ -> fail "unsupported escape sequence"

                        result.Append(escaped) |> ignore
                    | value when int value < 0x20 ->
                        fail "unescaped control character"
                    | value -> result.Append(value) |> ignore

                result.ToString()

            let parseLiteral literal value =
                for expected in literal do
                    expect expected
                value

            let parseNumber () =
                let start = index.Value

                let isNumberCharacter value =
                    (value >= '0' && value <= '9')
                    || value = '-'
                    || value = '+'
                    || value = '.'
                    || value = 'e'
                    || value = 'E'

                while peek () |> Option.exists isNumberCharacter do
                    index.Value <- index.Value + 1

                let token = source.Substring(start, index.Value - start)

                match Double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture) with
                | true, value -> Json.Number value
                | false, _ -> fail $"invalid number '{token}'"

            let rec parseValue () =
                skipWhitespace ()

                match peek () with
                | Some '\"' -> Json.String(parseString ())
                | Some '{' -> parseObject ()
                | Some '[' -> parseArray ()
                | Some 't' -> parseLiteral "true" (Json.Boolean true)
                | Some 'f' -> parseLiteral "false" (Json.Boolean false)
                | Some 'n' -> parseLiteral "null" Json.Null
                | Some('-' | '0' | '1' | '2' | '3' | '4' | '5' | '6' | '7' | '8' | '9') ->
                    parseNumber ()
                | Some value -> fail $"unexpected character '{value}'"
                | None -> fail "expected a value"

            and parseObject () =
                expect '{'
                skipWhitespace ()
                let fields = ResizeArray<string * Json>()

                if peek () = Some '}' then
                    index.Value <- index.Value + 1
                else
                    let mutable complete = false

                    while not complete do
                        skipWhitespace ()
                        if peek () <> Some '\"' then fail "object keys must be strings"
                        let key = parseString ()
                        skipWhitespace ()
                        expect ':'
                        fields.Add(key, parseValue ())
                        skipWhitespace ()

                        match take () with
                        | ',' -> ()
                        | '}' -> complete <- true
                        | _ -> fail "expected ',' or '}'"

                Json.Object(Seq.toList fields)

            and parseArray () =
                expect '['
                skipWhitespace ()
                let values = ResizeArray<Json>()

                if peek () = Some ']' then
                    index.Value <- index.Value + 1
                else
                    let mutable complete = false

                    while not complete do
                        values.Add(parseValue ())
                        skipWhitespace ()

                        match take () with
                        | ',' -> ()
                        | ']' -> complete <- true
                        | _ -> fail "expected ',' or ']'"

                Json.Array(Seq.toList values)

            try
                if isNull source then nullArg "source"
                let value = parseValue ()
                skipWhitespace ()
                if index.Value <> source.Length then
                    fail "unexpected trailing content"
                Ok value
            with error ->
                Error error.Message

    let encode (encoder: Encoder<'T>) (value: 'T) =
        value
        |> encoder
        |> fun encodable -> encodable.Encode(encoderHelpers)
        |> writeJson

    let decode (decoder: Decoder<'T>) json =
        match Parser.parse json with
        | Error message -> Error message
        | Ok value ->
            match decoder.Decode(decoderHelpers, value) with
            | Ok result -> Ok result
            | Error error -> Error(Decode.errorToString decoderHelpers error)

    let private validateKnownFields path allowed fields =
        fields
        |> List.countBy fst
        |> List.tryFind (fun (_, count) -> count > 1)
        |> Option.iter (fun (name, _) ->
            invalidArg "json" $"{path}: duplicate field '{name}'"
        )

        fields
        |> List.iter (fun (name, _) ->
            if allowed |> List.exists (fun expected -> expected = name) |> not then
                invalidArg "json" $"{path}: unknown field '{name}'"
        )

    let private requiredField path name fields =
        match fields |> List.tryPick (fun (key, value) -> if key = name then Some value else None) with
        | Some value -> value
        | None -> invalidArg "json" $"{path}: missing required field '{name}'"

    let private validateCommandInputBinding path value =
        match value with
        | Json.Object fields ->
            validateKnownFields path [ "prefix"; "position" ] fields
            requiredField path "prefix" fields |> ignore
        | _ -> invalidArg "json" $"{path}: inputBinding must be an object"

    let private validateCommandInput path value =
        match value with
        | Json.Object fields ->
            validateKnownFields path [ "id"; "type"; "label"; "doc"; "inputBinding" ] fields
            requiredField path "id" fields |> ignore
            requiredField path "type" fields |> ignore

            fields
            |> requiredField path "inputBinding"
            |> validateCommandInputBinding ($"{path}.inputBinding")
        | _ -> invalidArg "json" $"{path}: command input must be an object"

    let private validateValidationPackageJson value =
        match value with
        | Json.Object fields ->
            fields
            |> List.countBy fst
            |> List.tryFind (fun (_, count) -> count > 1)
            |> Option.iter (fun (name, _) ->
                invalidArg "json" $"$: duplicate field '{name}'"
            )

            fields
            |> List.tryPick (fun (name, value) -> if name = "Inputs" then Some value else None)
            |> Option.iter (fun inputs ->
                match inputs with
                | Json.Array values ->
                    values
                    |> List.iteri (fun index value ->
                        validateCommandInput ($"$.Inputs[{index}]") value
                    )
                | _ -> invalidArg "json" "$.Inputs must be an array"
            )
        | _ -> ()

    let decodeValidationPackage (decoder: Decoder<'T>) json =
        match Parser.parse json with
        | Error message -> Error message
        | Ok value ->
            try
                validateValidationPackageJson value

                match decoder.Decode(decoderHelpers, value) with
                | Ok result -> Ok result
                | Error error -> Error(Decode.errorToString decoderHelpers error)
            with error ->
                Error error.Message
