module ValidationPackage.Codecs.Tests.ConfigYamlTests

open Fable.Pyxpecto
open ValidationPackage.Codecs
open ValidationPackage.Model

let private expectErrorContains expected message result =
    match result with
    | Ok _ -> failwith message
    | Error error -> Expect.stringContains error expected message

let canonicalYaml =
    """$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json"
arc_specification: 3.0.0-draft.2
validation_packages:
  - name: configurable-validation
    version: 1.2.3
    roll_forward: latest_patch
    inputs:
      strict: false
      minimum-files: -2147483648
      maximum-files: 9223372036854775807
      ratio: 1.25e+2
      report-title: "release candidate"
      numeric-title: "01"
      optional-note: null
  - name: exact-validation
    version: 2.0.0+build.1
"""

let tests =
    testList "validation-packages YAML" [
        testCase "canonical config decodes every scalar and preserves source order" <| fun () ->
            let decoded = ValidationPackagesConfigYaml.decodeOrFail canonicalYaml
            Expect.isFalse decoded.IsLegacy "Canonical path"
            let config = decoded.Canonical
            Expect.isTrue config.HasArcSpecification "ARC specification"
            Expect.equal config.ValidationPackages[0].Name "configurable-validation" "Package order"
            Expect.equal config.ValidationPackages[1].Name "exact-validation" "Package order"

            let inputs = config.ValidationPackages[0].Inputs
            Expect.equal inputs[0].Value.Kind ValidationPackageInputValueKind.Boolean "Boolean"
            Expect.equal inputs[1].Value.Value "-2147483648" "Int lexeme"
            Expect.equal inputs[2].Value.Value "9223372036854775807" "Int64 lexeme"
            Expect.equal inputs[3].Value.Value "1.25e+2" "Floating lexeme"
            Expect.equal inputs[4].Value.Kind ValidationPackageInputValueKind.String "Quoted string"
            Expect.equal inputs[5].Value.Kind ValidationPackageInputValueKind.String "Quoted numeric-looking string"
            Expect.equal inputs[5].Value.Value "01" "Quoted numeric-looking lexeme"
            Expect.equal inputs[6].Value.Kind ValidationPackageInputValueKind.Null "Explicit null"

        testCase "canonical writer emits schema first and omits defaults" <| fun () ->
            let decoded = ValidationPackagesConfigYaml.decodeOrFail canonicalYaml
            let encoded = ValidationPackagesConfigYaml.encode decoded.Canonical
            Expect.isTrue (encoded.StartsWith("$schema:")) "$schema is first"
            Expect.stringContains encoded "9223372036854775807" "Int64 lexeme is not rounded"
            Expect.stringContains encoded "\"report-title\": \"release candidate\"" "Strings and arbitrary input ids are quoted"
            Expect.stringContains encoded "\"numeric-title\": \"01\"" "Numeric-looking strings stay quoted"
            Expect.isFalse (encoded.Contains("roll_forward: \"disable\"")) "Default policy omitted"
            Expect.isFalse (encoded.Contains("inputs: {}")) "Empty inputs omitted"

            let roundTrip = ValidationPackagesConfigYaml.decodeOrFail encoded
            Expect.equal roundTrip.Canonical decoded.Canonical "Canonical round-trip"

        testCase "legacy config is read-only and preserves optional versions" <| fun () ->
            let decoded =
                ValidationPackagesConfigYaml.decodeOrFail
                    """arc_specification: 2.0.0
validation_packages:
  - name: latest-stable
  - name: exact
    version: 1.2.3
"""

            Expect.isTrue decoded.IsLegacy "Legacy dispatch"
            Expect.isTrue decoded.Legacy.HasArcSpecification "Legacy ARC specification"
            Expect.isFalse decoded.Legacy.ValidationPackages[0].HasVersion "Name-only selection"
            Expect.isTrue decoded.Legacy.ValidationPackages[1].HasVersion "Exact selection"

        testCase "schema dispatch rejects unknown and malformed references" <| fun () ->
            [
                "$schema: \"https://example.org/unknown.json\"\nvalidation_packages: []\n"
                "$schema: 1\nvalidation_packages: []\n"
                "$schema: true\nvalidation_packages: []\n"
            ]
            |> List.iter (fun yaml ->
                ValidationPackagesConfigYaml.decode yaml
                |> expectErrorContains "UnsupportedSchema" "Unsupported schema should fail"
            )

        testCase "unsafe and ambiguous YAML constructs fail" <| fun () ->
            let documentWith value =
                $"""$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json"
validation_packages:
  - name: package
    version: 1.0.0
    inputs:
      value: {value}
"""

            [
                "yes"
                "no"
                "on"
                "off"
                "~"
                "01"
                "0x10"
                "1_000"
                ".nan"
                ".inf"
                "[1, 2]"
                "{nested: true}"
                ""
            ]
            |> List.iteri (fun index value ->
                ValidationPackagesConfigYaml.decode (documentWith value)
                |> expectErrorContains "input" $"Unsafe scalar case {index} should fail"
            )

            ValidationPackagesConfigYaml.decode (documentWith "&shared 1")
            |> expectErrorContains "anchors" "Anchor should fail"

            ValidationPackagesConfigYaml.decode
                ((documentWith "&shared 1") + "      other: *shared\n")
            |> expectErrorContains "anchors" "Alias should fail"

            ValidationPackagesConfigYaml.decode (documentWith "!!str value")
            |> expectErrorContains "tags" "Explicit tag should fail"

        testCase "duplicates, unknown fields, and multiple documents fail" <| fun () ->
            let schema = "$schema: \"https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json\"\n"

            [
                schema + "validation_packages: []\nvalidation_packages: []\n"
                schema + "unknown: true\nvalidation_packages: []\n"
                schema + "validation_packages:\n  - name: package\n    name: duplicate\n    version: 1.0.0\n"
                schema + "validation_packages:\n  - name: package\n    version: 1.0.0\n    unknown: true\n"
                schema + "validation_packages:\n  - name: package\n    version: 1.0.0\n    inputs:\n      same: 1\n      same: 2\n"
                schema + "validation_packages:\n  - name: same\n    version: 1.0.0\n  - name: same\n    version: 2.0.0\n"
                schema + "validation_packages: []\n---\nvalidation_packages: []\n"
            ]
            |> List.iteri (fun index yaml ->
                match ValidationPackagesConfigYaml.decode yaml with
                | Ok _ -> failwith $"Invalid structural case {index} should fail"
                | Error _ -> ()
            )

        testCase "canonical selections require full versions" <| fun () ->
            ValidationPackagesConfigYaml.decode
                """$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json"
validation_packages:
  - name: package
"""
            |> expectErrorContains "version" "Missing version should fail"

            ValidationPackagesConfigYaml.decode
                """$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json"
validation_packages:
  - name: package
    version: 1.2
"""
            |> expectErrorContains "semantic version" "Partial version should fail"
    ]
