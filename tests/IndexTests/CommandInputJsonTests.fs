namespace CommandInputJsonTests

open System
open System.Text.Json
open Xunit
open AVPRIndex.Domain

module Converter =

    let private assertTypeFailureContains (expectedMessage: string) (json: string) =
        let actualError =
            Assert.Throws<JsonException>(fun () ->
                JsonSerializer.Deserialize<CommandInputType>(json)
                |> ignore
            )

        Assert.Contains(expectedMessage, actualError.ToString())

    [<Fact>]
    let ``command input type JSON conversion supports every CWL scalar form`` () =
        let cases =
            [
                "boolean", CwlPrimitive.Boolean, false
                "boolean?", CwlPrimitive.Boolean, true
                "int", CwlPrimitive.Int, false
                "int?", CwlPrimitive.Int, true
                "long", CwlPrimitive.Long, false
                "long?", CwlPrimitive.Long, true
                "float", CwlPrimitive.Float, false
                "float?", CwlPrimitive.Float, true
                "double", CwlPrimitive.Double, false
                "double?", CwlPrimitive.Double, true
                "string", CwlPrimitive.String, false
                "string?", CwlPrimitive.String, true
            ]

        for cwlType, expectedPrimitive, expectedNullable in cases do
            let actual = JsonSerializer.Deserialize<CommandInputType>($"\"{cwlType}\"")

            Assert.Equal(expectedPrimitive, actual.PrimitiveType)
            Assert.Equal(expectedNullable, actual.IsNullable)
            Assert.Equal(cwlType, CommandInputType.toCwlString(actual))
            Assert.Equal(actual, CommandInputType.fromCwlString(cwlType))
            Assert.Equal($"\"{cwlType}\"", JsonSerializer.Serialize(actual))

    [<Fact>]
    let ``command input type JSON conversion rejects unsupported strings and shapes`` () =
        for unsupportedType in [ "File"; "string??"; "STRING" ] do
            assertTypeFailureContains
                $"unsupported CWL command input type: {unsupportedType}"
                $"\"{unsupportedType}\""

        for unsupportedShape in [ "null"; "[\"null\",\"boolean\"]"; "{\"type\":\"array\"}" ] do
            assertTypeFailureContains
                "CWL command input type must be one supported scalar type string"
                unsupportedShape

module Parameters =

    [<Fact>]
    let ``minimal command input parameter JSON retains sensible defaults`` () =
        let actual =
            JsonSerializer.Deserialize<CommandInputParameter>(
                """{"id":"value","type":"string","inputBinding":{}}"""
            )

        Assert.Equal("value", actual.Id)
        Assert.Equal(CwlPrimitive.String, actual.Type.PrimitiveType)
        Assert.False(actual.Type.IsNullable)
        Assert.Equal("", actual.Label)
        Assert.Equal("", actual.Doc)
        Assert.Equal(0, actual.InputBinding.Position)
        Assert.Equal("", actual.InputBinding.Prefix)
        Assert.True(actual.InputBinding.Separate)

    [<Fact>]
    let ``command input parameter JSON uses exact CWL property names and scalar type`` () =
        let input =
            CommandInputParameter.create(
                "output",
                CommandInputType.create(CwlPrimitive.String),
                CommandInputBinding.create(
                    Position = 2,
                    Prefix = "--output=",
                    Separate = false
                ),
                Label = "Output file",
                Doc = "Write output to this file"
            )

        let actual = JsonSerializer.Serialize(input)
        let expected =
            """{"id":"output","type":"string","label":"Output file","doc":"Write output to this file","inputBinding":{"position":2,"prefix":"--output=","separate":false}}"""

        Assert.Equal(expected, actual)

    [<Fact>]
    let ``command input parameter JSON reads defaults and discards unsupported CWL fields`` () =
        let json =
            """{
  "id": "verbose",
  "type": "boolean?",
  "label": "Verbose logging",
  "doc": "Enable verbose logging",
  "default": false,
  "extension": { "nested": ["one", "two"] },
  "inputBinding": {
    "prefix": "--verbose",
    "valueFrom": "$(self)",
    "shellQuote": false
  }
}"""

        let actual = JsonSerializer.Deserialize<CommandInputParameter>(json)

        Assert.Equal("verbose", actual.Id)
        Assert.Equal(CwlPrimitive.Boolean, actual.Type.PrimitiveType)
        Assert.True(actual.Type.IsNullable)
        Assert.Equal("Verbose logging", actual.Label)
        Assert.Equal("Enable verbose logging", actual.Doc)
        Assert.Equal(0, actual.InputBinding.Position)
        Assert.Equal("--verbose", actual.InputBinding.Prefix)
        Assert.True(actual.InputBinding.Separate)

        let serialized = JsonSerializer.Serialize(actual)
        Assert.DoesNotContain("default", serialized)
        Assert.DoesNotContain("extension", serialized)
        Assert.DoesNotContain("valueFrom", serialized)
        Assert.DoesNotContain("shellQuote", serialized)

    [<Fact>]
    let ``command input parameter JSON requires id type and inputBinding`` () =
        let cases =
            [
                "id", """{"type":"string","inputBinding":{}}"""
                "type", """{"id":"value","inputBinding":{}}"""
                "inputBinding", """{"id":"value","type":"string"}"""
            ]

        for field, json in cases do
            let actualError =
                Assert.Throws<JsonException>(fun () ->
                    JsonSerializer.Deserialize<CommandInputParameter>(json)
                    |> ignore
                )

            Assert.Contains(field, actualError.ToString())

    [<Fact>]
    let ``metadata JSON keeps uppercase Inputs wrapper and nested CWL names`` () =
        let input =
            CommandInputParameter.create(
                "verbose",
                CommandInputType.create(CwlPrimitive.Boolean, true),
                CommandInputBinding.create(Prefix = "--verbose")
            )
        let metadata = ValidationPackageMetadata(Inputs = [| input |])
        let options = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
        let json = JsonSerializer.Serialize(metadata, options)

        use document = JsonDocument.Parse(json)
        let root = document.RootElement
        let mutable inputs = Unchecked.defaultof<JsonElement>
        let mutable lowercaseInputs = Unchecked.defaultof<JsonElement>

        Assert.True(root.TryGetProperty("Inputs", &inputs))
        Assert.False(root.TryGetProperty("inputs", &lowercaseInputs))
        Assert.Equal(JsonValueKind.Array, inputs.ValueKind)

        let serializedInput = inputs[0]
        Assert.Equal("verbose", serializedInput.GetProperty("id").GetString())
        Assert.Equal("boolean?", serializedInput.GetProperty("type").GetString())
        Assert.Equal("--verbose", serializedInput.GetProperty("inputBinding").GetProperty("prefix").GetString())

        let mutable pascalId = Unchecked.defaultof<JsonElement>
        let mutable pascalBinding = Unchecked.defaultof<JsonElement>
        Assert.False(serializedInput.TryGetProperty("Id", &pascalId))
        Assert.False(serializedInput.TryGetProperty("InputBinding", &pascalBinding))

        let metadataWithoutInputs = JsonSerializer.Deserialize<ValidationPackageMetadata>("{}")
        Assert.Empty(metadataWithoutInputs.Inputs)
