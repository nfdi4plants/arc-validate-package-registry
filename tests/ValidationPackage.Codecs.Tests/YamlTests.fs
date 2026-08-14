module ValidationPackage.Codecs.Tests.YamlTests

open Fable.Pyxpecto
open ValidationPackage.Codecs
open ValidationPackage.Codecs.Tests.ReferenceObjects

let private expectErrorContains expected message result =
    match result with
    | Ok _ -> failwith message
    | Error error -> Expect.stringContains error expected message

let tests =
    testList "YAML codecs" [
        testCase "all metadata fields decode" <| fun () ->
            let actual = ValidationPackageYaml.decodeOrFail yaml
            Expect.equal actual metadata "Decoded metadata"

        testCase "metadata round-trips through YAMLicious" <| fun () ->
            let encoded = ValidationPackageYaml.encode metadata
            let actual = ValidationPackageYaml.decodeOrFail encoded
            Expect.equal actual metadata "YAML round-trip"

        testCase "CWL names and scalar types remain canonical" <| fun () ->
            let encoded = ValidationPackageYaml.encode metadata
            Expect.isTrue (encoded.StartsWith("$schema:")) "$schema is first"
            Expect.stringContains encoded "Inputs:" "PascalCase Inputs wrapper"
            Expect.stringContains encoded "id: \"output\"" "lower-camel id"
            Expect.stringContains encoded "type: \"string\"" "scalar CWL type"
            Expect.stringContains encoded "inputBinding:" "canonical binding key"
            Expect.stringContains encoded "prefix: \"--output\"" "binding prefix"

        testCase "missing optional metadata fields keep model defaults" <| fun () ->
            let actual =
                ValidationPackageYaml.decodeOrFail
                    """Name: minimal
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
"""

            Expect.equal actual.Name "minimal" "Name"
            Expect.isEmpty actual.Authors "Authors default"
            Expect.isEmpty actual.Tags "Tags default"
            Expect.isEmpty actual.Inputs "Inputs default"
            Expect.isFalse actual.Publish "Publish default"

        testCase "schema-less legacy unknown fields retain historical behavior" <| fun () ->
            let actual =
                ValidationPackageYaml.decodeOrFail
                    """Name: compatible
FutureField:
  nested: value
"""

            Expect.equal actual.Name "compatible" "Known field"

        testCase "versioned frontmatter rejects unknown fields" <| fun () ->
            ValidationPackageYaml.decode
                """$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: compatible
Summary: summary
Description: description
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
FutureField: value
"""
            |> expectErrorContains "unknown field" "Unknown versioned field should fail"

        testCase "frontmatter extraction sets the source language" <| fun () ->
            let fsharp =
                ValidationPackageYaml.extractOrFail
                    FrontmatterLanguage.FSharp
                    fsharpComment

            let python =
                ValidationPackageYaml.extractOrFail
                    FrontmatterLanguage.Python
                    pythonBinding

            Expect.equal fsharp.ProgrammingLanguage "FSharp" "F# language"
            Expect.equal python.ProgrammingLanguage "Python" "Python language"

        testCase "Inputs must use the CWL array form" <| fun () ->
            ValidationPackageYaml.decode
                """$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: package
Summary: summary
Description: description
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Inputs:
  value:
    type: string
"""
            |> expectErrorContains
                "AVPR Inputs must use the CWL array form"
                "Mapping Inputs should fail"

        testCase "CWL parameters require id, type, and inputBinding" <| fun () ->
            ValidationPackageYaml.decode
                """$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: package
Summary: summary
Description: description
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Inputs:
  - id: value
    type: string
"""
            |> expectErrorContains
                "missing required field 'inputBinding'"
                "Missing inputBinding should fail"

        testCase "unsupported CWL scalar types fail" <| fun () ->
            ValidationPackageYaml.decode
                """$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: package
Summary: summary
Description: description
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Inputs:
  - id: file
    type: File
    inputBinding:
      prefix: --file
"""
            |> expectErrorContains
                "unsupported CWL command input type: File"
                "Unsupported type should fail"

        testCase "CWL parameter ids must be non-empty and unique" <| fun () ->
            ValidationPackageYaml.decode
                """$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: package
Summary: summary
Description: description
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Inputs:
  - id: duplicate
    type: string
    inputBinding:
      prefix: --first
  - id: duplicate
    type: string
    inputBinding:
      prefix: --second
"""
            |> expectErrorContains
                "id must be unique"
                "Duplicate id should fail"

        testCase "schema dispatch preserves legacy and rejects unsupported schemas offline" <| fun () ->
            let legacy =
                ValidationPackageYaml.decodeOrFail
                    """Name: historical
Summary: summary
Description: description
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
"""

            Expect.equal legacy.Name "historical" "Schema-less legacy frontmatter"

            ValidationPackageYaml.decode
                """$schema: "https://example.org/unknown.schema.json"
Name: package
"""
            |> expectErrorContains "UnsupportedSchema" "Unknown schemas fail without fallback"

            ValidationPackageYaml.decode
                """Name: historical
Inputs:
  - id: value
    type: string
    inputBinding:
      prefix: --value
"""
            |> expectErrorContains "cannot declare Inputs" "Legacy Inputs are unavailable"

        testCase "narrowed declarations reject separate, positional, and unknown fields" <| fun () ->
            let prefix =
                """$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: package
Summary: summary
Description: description
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Inputs:
  - id: value
    type: string
    inputBinding:
"""

            [
                prefix + "      separate: false\n"
                prefix + "      position: 1\n"
                prefix + "      prefix: --value\n      valueFrom: expression\n"
                prefix + "      prefix: --value\n    default: x\n"
            ]
            |> List.iter (fun invalid ->
                ValidationPackageYaml.decode invalid
                |> expectErrorContains "field" "Unsupported declaration field should fail"
            )
    ]
