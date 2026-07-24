namespace CommandInputYamlTests

open System
open Xunit
open AVPRIndex
open AVPRIndex.Domain
open AVPRIndex.Frontmatter

module Converters =

    let private assertMetadataFailureContains (expectedMessage: string) (yaml: string) =
        let actualException =
            Assert.ThrowsAny<Exception>(fun () ->
                yamlDeserializer().Deserialize<ValidationPackageMetadata>(yaml)
                |> ignore
            )

        Assert.Contains(expectedMessage, actualException.ToString())

    [<Fact>]
    let ``command input type converter reads every supported scalar type`` () =
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

        let deserializer = yamlDeserializer()

        for yamlType, expectedPrimitive, expectedNullable in cases do
            let actual = deserializer.Deserialize<CommandInputType>(yamlType)
            Assert.Equal(expectedPrimitive, actual.PrimitiveType)
            Assert.Equal(expectedNullable, actual.IsNullable)
            Assert.Equal(yamlType, yamlSerializer().Serialize(actual).Trim())

    [<Fact>]
    let ``command input converters read CWL names and ignore unsupported fields`` () =
        let yaml =
            """Inputs:
  - id: verbose
    type: boolean?
    label: Verbose logging
    doc: Enable verbose logging
    default: false
    extension:
      nested: [one, two]
    inputBinding:
      prefix: --verbose
      itemSeparator: ','
      valueFrom: $(self)
      shellQuote: false
      ignoredBindingField:
        nested: value
  - id: output
    type: string
    inputBinding:
      position: 2
      prefix: --output=
      separate: false
"""

        let actual = yamlDeserializer().Deserialize<ValidationPackageMetadata>(yaml)

        Assert.Equal(2, actual.Inputs.Length)

        let verbose = actual.Inputs[0]
        Assert.Equal("verbose", verbose.Id)
        Assert.Equal(CwlPrimitive.Boolean, verbose.Type.PrimitiveType)
        Assert.True(verbose.Type.IsNullable)
        Assert.Equal("Verbose logging", verbose.Label)
        Assert.Equal("Enable verbose logging", verbose.Doc)
        Assert.Equal("--verbose", verbose.InputBinding.Prefix)
        Assert.Equal(0, verbose.InputBinding.Position)
        Assert.True(verbose.InputBinding.Separate)

        let output = actual.Inputs[1]
        Assert.Equal("output", output.Id)
        Assert.Equal(CwlPrimitive.String, output.Type.PrimitiveType)
        Assert.False(output.Type.IsNullable)
        Assert.Equal(2, output.InputBinding.Position)
        Assert.Equal("--output=", output.InputBinding.Prefix)
        Assert.False(output.InputBinding.Separate)

        let serialized = yamlSerializer().Serialize(actual)
        Assert.Contains("Inputs:", serialized)
        Assert.Contains("- id: verbose", serialized)
        Assert.Contains("  inputBinding:", serialized)
        Assert.DoesNotContain("default:", serialized)
        Assert.DoesNotContain("extension:", serialized)
        Assert.DoesNotContain("itemSeparator:", serialized)
        Assert.DoesNotContain("valueFrom:", serialized)
        Assert.DoesNotContain("shellQuote:", serialized)
        Assert.DoesNotContain("ignoredBindingField:", serialized)

    [<Fact>]
    let ``command input converters write canonical CWL names and omit defaults`` () =
        let input =
            CommandInputParameter.create(
                "verbose",
                CommandInputType.create(CwlPrimitive.Boolean, true),
                CommandInputBinding.create(Prefix = "--verbose"),
                Label = "Verbose logging"
            )

        let actual =
            yamlSerializer().Serialize(input).ReplaceLineEndings("\n")

        let expectedYaml =
            """id: verbose
type: boolean?
label: Verbose logging
inputBinding:
  prefix: --verbose
"""

        let expected = expectedYaml.ReplaceLineEndings("\n")

        Assert.Equal(expected, actual)

    [<Fact>]
    let ``metadata without Inputs retains the empty default`` () =
        let actual = yamlDeserializer().Deserialize<ValidationPackageMetadata>("Name: no-inputs")
        Assert.Empty(actual.Inputs)

    [<Fact>]
    let ``Inputs must use CWL array form`` () =
        let yaml =
            """Inputs:
  files:
    type: string
    inputBinding: {}
"""

        assertMetadataFailureContains "AVPR Inputs must use the CWL array form" yaml

    [<Fact>]
    let ``command input parameters require id type and inputBinding`` () =
        let cases =
            [
                "id", """Inputs:
  - type: string
    inputBinding: {}
"""
                "type", """Inputs:
  - id: value
    inputBinding: {}
"""
                "inputBinding", """Inputs:
  - id: value
    type: string
"""
            ]

        for field, yaml in cases do
            assertMetadataFailureContains $"missing required field(s): {field}" yaml

    [<Fact>]
    let ``command input parameters and bindings must be mappings`` () =
        let scalarParameterYaml =
            """Inputs:
  - value
"""

        assertMetadataFailureContains "each CWL command input parameter must be a mapping" scalarParameterYaml

        let scalarBindingYaml =
            """Inputs:
  - id: value
    type: string
    inputBinding: --value
"""

        assertMetadataFailureContains "inputBinding must be a mapping" scalarBindingYaml

    [<Fact>]
    let ``command input ids must be non-empty and unique`` () =
        let emptyIdYaml =
            """Inputs:
  - id: "   "
    type: string
    inputBinding: {}
"""

        assertMetadataFailureContains "requires a non-empty id" emptyIdYaml

        let duplicateIdYaml =
            """Inputs:
  - id: value
    type: string
    inputBinding: {}
  - id: value
    type: int
    inputBinding: {}
"""

        assertMetadataFailureContains "id must be unique, but was duplicated: value" duplicateIdYaml

    [<Fact>]
    let ``unsupported command input type scalars fail with their value`` () =
        for unsupportedType in [ "File"; "string??"; "STRING" ] do
            let yaml =
                $"""Inputs:
  - id: value
    type: {unsupportedType}
    inputBinding: {{}}
"""

            assertMetadataFailureContains $"unsupported CWL command input type: {unsupportedType}" yaml

    [<Fact>]
    let ``unsupported command input type collections and mappings fail conversion`` () =
        let unionYaml =
            """Inputs:
  - id: value
    type: ["null", boolean]
    inputBinding: {}
"""

        assertMetadataFailureContains "type must be one supported scalar type string" unionYaml

        let mappingYaml =
            """Inputs:
  - id: files
    type:
      type: array
      items: string
    inputBinding: {}
"""

        assertMetadataFailureContains "type must be one supported scalar type string" mappingYaml

    [<Fact>]
    let ``wrong nested casing does not satisfy required CWL fields`` () =
        let yaml =
            """Inputs:
  - id: value
    type: string
    InputBinding:
      Prefix: --value
"""

        assertMetadataFailureContains "missing required field(s): inputBinding" yaml

    [<Fact>]
    let ``invalid binding scalar values fail with actionable diagnostics`` () =
        let invalidPositionYaml =
            """Inputs:
  - id: value
    type: string
    inputBinding:
      position: first
"""

        assertMetadataFailureContains "position must be an integer, but was: first" invalidPositionYaml

        let invalidSeparateYaml =
            """Inputs:
  - id: value
    type: string
    inputBinding:
      separate: sometimes
"""

        assertMetadataFailureContains "separate must be a boolean, but was: sometimes" invalidSeparateYaml
