namespace AVPRIndex

open Domain
open System
open System.Globalization
open System.Text
open System.IO
open System.Security.Cryptography
open YamlDotNet.Core
open YamlDotNet.Core.Events
open YamlDotNet.Serialization

[<AutoOpen>]
module Frontmatter = 

    module private CommandInputYaml =

        let emitScalar (emitter: IEmitter) (value: string) =
            emitter.Emit(Scalar(value))

        let parseType (value: string) =
            let isNullable = value.EndsWith("?", StringComparison.Ordinal)
            let primitiveName =
                if isNullable then value.Substring(0, value.Length - 1)
                else value

            let primitiveType =
                match primitiveName with
                | "boolean" -> CwlPrimitive.Boolean
                | "int" -> CwlPrimitive.Int
                | "long" -> CwlPrimitive.Long
                | "float" -> CwlPrimitive.Float
                | "double" -> CwlPrimitive.Double
                | "string" -> CwlPrimitive.String
                | _ -> raise (YamlException($"unsupported CWL command input type: {value}"))

            CommandInputType.create(primitiveType, isNullable)

        let formatType (inputType: CommandInputType) =
            let primitiveName =
                match inputType.PrimitiveType with
                | CwlPrimitive.Boolean -> "boolean"
                | CwlPrimitive.Int -> "int"
                | CwlPrimitive.Long -> "long"
                | CwlPrimitive.Float -> "float"
                | CwlPrimitive.Double -> "double"
                | CwlPrimitive.String -> "string"
                | value -> raise (YamlException($"unsupported CWL primitive type: {value}"))

            if inputType.IsNullable then $"{primitiveName}?"
            else primitiveName

        let readScalar (parser: IParser) =
            parser.Consume<Scalar>().Value

        let readPosition (parser: IParser) =
            let value = readScalar parser
            match Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture) with
            | true, position -> position
            | false, _ -> raise (YamlException($"CWL input binding position must be an integer, but was: {value}"))

        let readSeparate (parser: IParser) =
            let value = readScalar parser
            match Boolean.TryParse(value) with
            | true, separate -> separate
            | false, _ -> raise (YamlException($"CWL input binding separate must be a boolean, but was: {value}"))

        let readBinding (parser: IParser) =
            let mutable mappingStart = Unchecked.defaultof<MappingStart>
            if not (parser.Accept<MappingStart>(&mappingStart)) then
                raise (YamlException("CWL command input inputBinding must be a mapping"))

            parser.Consume<MappingStart>() |> ignore
            let binding = CommandInputBinding()
            let mutable mappingEnd = Unchecked.defaultof<MappingEnd>

            while not (parser.Accept<MappingEnd>(&mappingEnd)) do
                let key = readScalar parser
                match key with
                | "position" -> binding.Position <- readPosition parser
                | "prefix" -> binding.Prefix <- readScalar parser
                | "separate" -> binding.Separate <- readSeparate parser
                | _ -> parser.SkipThisAndNestedEvents()

            parser.Consume<MappingEnd>() |> ignore
            binding

        let writeBinding (emitter: IEmitter) (binding: CommandInputBinding) =
            emitter.Emit(MappingStart())

            if binding.Position <> 0 then
                emitScalar emitter "position"
                emitScalar emitter (binding.Position.ToString(CultureInfo.InvariantCulture))

            if binding.Prefix <> "" then
                emitScalar emitter "prefix"
                emitScalar emitter binding.Prefix

            if not binding.Separate then
                emitScalar emitter "separate"
                emitScalar emitter "false"

            emitter.Emit(MappingEnd())

        let readParameter (parser: IParser) =
            let mutable mappingStart = Unchecked.defaultof<MappingStart>
            if not (parser.Accept<MappingStart>(&mappingStart)) then
                raise (YamlException("each CWL command input parameter must be a mapping"))

            parser.Consume<MappingStart>() |> ignore

            let mutable id = None
            let mutable inputType = None
            let mutable label = None
            let mutable doc = None
            let mutable inputBinding = None
            let mutable mappingEnd = Unchecked.defaultof<MappingEnd>

            while not (parser.Accept<MappingEnd>(&mappingEnd)) do
                let key = readScalar parser
                match key with
                | "id" -> id <- Some (readScalar parser)
                | "type" ->
                    let mutable typeScalar = Unchecked.defaultof<Scalar>
                    if not (parser.Accept<Scalar>(&typeScalar)) then
                        raise (YamlException("CWL command input type must be one supported scalar type string"))
                    inputType <- Some (parser |> readScalar |> parseType)
                | "label" -> label <- Some (readScalar parser)
                | "doc" -> doc <- Some (readScalar parser)
                | "inputBinding" -> inputBinding <- Some (readBinding parser)
                | _ -> parser.SkipThisAndNestedEvents()

            parser.Consume<MappingEnd>() |> ignore

            match id, inputType, inputBinding with
            | Some id, Some inputType, Some inputBinding ->
                let parameter = CommandInputParameter.create(id, inputType, inputBinding)
                label |> Option.iter (fun value -> parameter.Label <- value)
                doc |> Option.iter (fun value -> parameter.Doc <- value)
                parameter
            | _ ->
                let missingFields = [
                    if id.IsNone then "id"
                    if inputType.IsNone then "type"
                    if inputBinding.IsNone then "inputBinding"
                ]
                let missingFieldNames = String.Join(", ", missingFields)
                raise (YamlException($"CWL command input parameter is missing required field(s): {missingFieldNames}"))

        let writeParameter (emitter: IEmitter) (parameter: CommandInputParameter) =
            emitter.Emit(MappingStart())

            emitScalar emitter "id"
            emitScalar emitter parameter.Id
            emitScalar emitter "type"
            parameter.Type |> formatType |> emitScalar emitter

            if parameter.Label <> "" then
                emitScalar emitter "label"
                emitScalar emitter parameter.Label

            if parameter.Doc <> "" then
                emitScalar emitter "doc"
                emitScalar emitter parameter.Doc

            emitScalar emitter "inputBinding"
            writeBinding emitter parameter.InputBinding
            emitter.Emit(MappingEnd())

        let validateParameters (parameters: CommandInputParameter array) =
            parameters
            |> Array.iteri (fun index parameter ->
                if String.IsNullOrWhiteSpace(parameter.Id) then
                    raise (YamlException($"CWL command input parameter at index {index} requires a non-empty id"))
            )

            parameters
            |> Array.countBy (fun parameter -> parameter.Id)
            |> Array.tryFind (fun (_, count) -> count > 1)
            |> Option.iter (fun (id, _) ->
                raise (YamlException($"CWL command input parameter id must be unique, but was duplicated: {id}"))
            )

            parameters

        let readParameters (parser: IParser) =
            let mutable sequenceStart = Unchecked.defaultof<SequenceStart>
            if not (parser.Accept<SequenceStart>(&sequenceStart)) then
                raise (YamlException("AVPR Inputs must use the CWL array form"))

            parser.Consume<SequenceStart>() |> ignore
            let parameters = ResizeArray<CommandInputParameter>()
            let mutable sequenceEnd = Unchecked.defaultof<SequenceEnd>

            while not (parser.Accept<SequenceEnd>(&sequenceEnd)) do
                parameters.Add(readParameter parser)

            parser.Consume<SequenceEnd>() |> ignore
            parameters.ToArray() |> validateParameters

        let writeParameters (emitter: IEmitter) (parameters: CommandInputParameter array) =
            emitter.Emit(SequenceStart(AnchorName.Empty, TagName.Empty, true, SequenceStyle.Block))
            parameters |> Array.iter (writeParameter emitter)
            emitter.Emit(SequenceEnd())

    type CommandInputTypeYamlConverter() =

        interface IYamlTypeConverter with

            member _.Accepts(typeToConvert: Type) =
                typeToConvert = typeof<CommandInputType>

            member _.ReadYaml(parser: IParser, _typeToConvert: Type) =
                parser
                |> CommandInputYaml.readScalar
                |> CommandInputYaml.parseType
                :> obj

            member _.WriteYaml(emitter: IEmitter, value: obj, _typeToConvert: Type) =
                value
                :?> CommandInputType
                |> CommandInputYaml.formatType
                |> CommandInputYaml.emitScalar emitter

    type CommandInputBindingYamlConverter() =

        interface IYamlTypeConverter with

            member _.Accepts(typeToConvert: Type) =
                typeToConvert = typeof<CommandInputBinding>

            member _.ReadYaml(parser: IParser, _typeToConvert: Type) =
                CommandInputYaml.readBinding parser :> obj

            member _.WriteYaml(emitter: IEmitter, value: obj, _typeToConvert: Type) =
                value
                :?> CommandInputBinding
                |> CommandInputYaml.writeBinding emitter

    type CommandInputParameterYamlConverter() =

        interface IYamlTypeConverter with

            member _.Accepts(typeToConvert: Type) =
                typeToConvert = typeof<CommandInputParameter>

            member _.ReadYaml(parser: IParser, _typeToConvert: Type) =
                CommandInputYaml.readParameter parser :> obj

            member _.WriteYaml(emitter: IEmitter, value: obj, _typeToConvert: Type) =
                value
                :?> CommandInputParameter
                |> CommandInputYaml.writeParameter emitter

    type CommandInputParametersYamlConverter() =

        interface IYamlTypeConverter with

            member _.Accepts(typeToConvert: Type) =
                typeToConvert = typeof<CommandInputParameter array>

            member _.ReadYaml(parser: IParser, _typeToConvert: Type) =
                CommandInputYaml.readParameters parser :> obj

            member _.WriteYaml(emitter: IEmitter, value: obj, _typeToConvert: Type) =
                value
                :?> CommandInputParameter array
                |> CommandInputYaml.writeParameters emitter

    let private configureCommandInputConverters (builder: DeserializerBuilder) =
        builder
            .WithTypeConverter(CommandInputTypeYamlConverter())
            .WithTypeConverter(CommandInputBindingYamlConverter())
            .WithTypeConverter(CommandInputParameterYamlConverter())
            .WithTypeConverter(CommandInputParametersYamlConverter())

    let private configureCommandInputSerializerConverters (builder: SerializerBuilder) =
        builder
            .WithTypeConverter(CommandInputTypeYamlConverter())
            .WithTypeConverter(CommandInputBindingYamlConverter())
            .WithTypeConverter(CommandInputParameterYamlConverter())
            .WithTypeConverter(CommandInputParametersYamlConverter())

    type FrontmatterLanguage =
        | FSharpFrontmatter
        | PythonFrontmatter

        static member fromString (str: string) =
            match str.ToLowerInvariant() with
            | "fsharp" | "fs" | "f#" -> FrontmatterLanguage.FSharpFrontmatter
            | "python" | "py" -> FrontmatterLanguage.PythonFrontmatter
            | _ -> failwith $"unsupported frontmatter language: {str}"

        static member toString (lang: FrontmatterLanguage) =
            match lang with
            | FSharpFrontmatter -> "FSharp"
            | PythonFrontmatter -> "Python"

    module FSharp =
        /// the frontmatter start string if the package uses yaml frontmatter as comment
        let [<Literal>] frontMatterCommentStart = "(*\n---"
        /// the frontmatter end string if the package uses yaml frontmatter as comment
        let [<Literal>] frontMatterCommentEnd = "---\n*)"

        /// the frontmatter start string if the package uses yaml frontmatter as a string binding to be re-used in the package code
        let [<Literal>] frontmatterBindingStart = "let [<Literal>]PACKAGE_METADATA = \"\"\"(*\n---"
        /// the frontmatter end string if the package uses yaml frontmatter as a string binding to be re-used in the package code
        let [<Literal>] frontmatterBindingEnd = "---\n*)\"\"\""


        let containsCommentFrontmatter (str: string) =
            str.StartsWith(frontMatterCommentStart, StringComparison.Ordinal) && str.Contains(frontMatterCommentEnd)

        let containsBindingFrontmatter (str: string) =
            str.StartsWith(frontmatterBindingStart, StringComparison.Ordinal) && str.Contains(frontmatterBindingEnd)

        let tryExtractFromString (str: string) =
            let norm = str.ReplaceLineEndings("\n")
            if containsCommentFrontmatter norm then
                norm.Substring(
                    frontMatterCommentStart.Length, 
                    (norm.IndexOf(frontMatterCommentEnd, StringComparison.Ordinal) - frontMatterCommentStart.Length))
                |> Some
            elif containsBindingFrontmatter norm then
                norm.Substring(
                    frontmatterBindingStart.Length, 
                    (norm.IndexOf(frontmatterBindingEnd, StringComparison.Ordinal) - frontmatterBindingStart.Length))
                |> Some
            else 
                None

        let extractFromString (str: string) =
            match tryExtractFromString str with
            | Some frontmatter -> frontmatter
            | None -> failwith $"""
input 

{str}

has no correctly formatted FSharp frontmatter."""

    module Python =
        /// the frontmatter start string if the package uses yaml frontmatter as comment
        let [<Literal>] frontMatterCommentStart = "\"\"\"\n---"
        /// the frontmatter end string if the package uses yaml frontmatter as comment
        let [<Literal>] frontMatterCommentEnd = "---\n\"\"\""

        /// the frontmatter start string if the package uses yaml frontmatter as a string binding to be re-used in the package code
        let [<Literal>] frontmatterBindingStart = "PACKAGE_METADATA = \"\"\"\n---"
        /// the frontmatter end string if the package uses yaml frontmatter as a string binding to be re-used in the package code
        let [<Literal>] frontmatterBindingEnd = "---\n\"\"\""

        let containsCommentFrontmatter (str: string) =
            str.StartsWith(frontMatterCommentStart, StringComparison.Ordinal) && str.Contains(frontMatterCommentEnd)

        let containsBindingFrontmatter (str: string) =
            str.StartsWith(frontmatterBindingStart, StringComparison.Ordinal) && str.Contains(frontmatterBindingEnd)

        let tryExtractFromString (str: string) =
            let norm = str.ReplaceLineEndings("\n")
            if containsCommentFrontmatter norm then
                norm.Substring(
                    frontMatterCommentStart.Length, 
                    (norm.IndexOf(frontMatterCommentEnd, StringComparison.Ordinal) - frontMatterCommentStart.Length))
                |> Some
            elif containsBindingFrontmatter norm then
                norm.Substring(
                    frontmatterBindingStart.Length, 
                    (norm.IndexOf(frontmatterBindingEnd, StringComparison.Ordinal) - frontmatterBindingStart.Length))
                |> Some
            else 
                None

        let extractFromString (str: string) =
            match tryExtractFromString str with
            | Some frontmatter -> frontmatter
            | None -> failwith $"""
input 

{str}

has no correctly formatted Python frontmatter."""

    let tryExtractFromString (lang:FrontmatterLanguage) (str: string) =
        match lang with
        | FSharpFrontmatter -> FSharp.tryExtractFromString str
        | PythonFrontmatter -> Python.tryExtractFromString str

    let extractFromString (lang:FrontmatterLanguage) (str: string) =
        match lang with
        | FSharpFrontmatter -> FSharp.extractFromString str
        | PythonFrontmatter -> Python.extractFromString str

    let yamlDeserializer() =
        let builder =
            DeserializerBuilder()
                .WithNamingConvention(NamingConventions.PascalCaseNamingConvention.Instance)
            |> configureCommandInputConverters

        builder
            .IgnoreUnmatchedProperties() // forward-compat: tolerate frontmatter keys unknown to this version (e.g. fields added in newer releases)
            .Build()

    let yamlSerializer() =
        SerializerBuilder()
            .WithNamingConvention(NamingConventions.PascalCaseNamingConvention.Instance)
            |> configureCommandInputSerializerConverters
            |> fun builder -> builder.Build()

    type ValidationPackageMetadata with
        
        static member extractFromString (lang: FrontmatterLanguage) (str: string) =
            let frontmatter = tryExtractFromString lang str
            match frontmatter with
            | Some frontmatter ->
                let result = 
                    yamlDeserializer().Deserialize<ValidationPackageMetadata>(frontmatter)
                result.ProgrammingLanguage <- FrontmatterLanguage.toString lang
                result
            | None ->
                failwith $"""
string 

{str}

has no correctly formatted {lang}."""

        static member tryExtractFromString (lang: FrontmatterLanguage) (str: string) =
            try 
                let vpm = ValidationPackageMetadata.extractFromString lang str 
                Some vpm
            with e ->
                printfn $"error parsing package metadata: {e.Message}"
                None

        static member extractFromScript (scriptPath: string) =

            let lang = 
                match Path.GetExtension(scriptPath).ToLowerInvariant() with
                | ".fsx" -> FrontmatterLanguage.FSharpFrontmatter
                | ".py" -> FrontmatterLanguage.PythonFrontmatter
                | ext -> failwith $"unsupported script extension: {ext}"

            scriptPath
            |> File.ReadAllText
            |> ValidationPackageMetadata.extractFromString lang

        static member tryExtractFromScript (scriptPath: string) =
            try 
                ValidationPackageMetadata.extractFromScript scriptPath |> Some
            with e ->
                printfn $"error parsing package metadata: {e.Message}"
                None

    type ValidationPackageIndex with

        static member create (
            repoPath: string, 
            lastUpdated: System.DateTimeOffset
        ) = 
            ValidationPackageIndex.create(
                repoPath = repoPath,
                lastUpdated = lastUpdated,
                metadata = ValidationPackageMetadata.extractFromScript(repoPath)
            )
