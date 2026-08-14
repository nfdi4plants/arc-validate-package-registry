module ValidationPackage.Codecs.Tests.SchemaTests

open Fable.Pyxpecto

#if !FABLE_COMPILER
open System.IO
open System.Text.Json
open Json.Schema
open ValidationPackage.Codecs
open ValidationPackage.Codecs.Tests.ReferenceObjects

let private repositoryRoot =
    Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, "..", ".."))

let private schemaPath fileName =
    Path.Combine(repositoryRoot, "schemas", fileName)

let private fixturePath fileName =
    Path.Combine(__SOURCE_DIRECTORY__, "Fixtures", fileName)

let private read path = File.ReadAllText path

let private evaluate (schema: JsonSchema) (json: string) =
    use document = JsonDocument.Parse json
    schema.Evaluate(document.RootElement)

let private expectValid message (results: EvaluationResults) =
    Expect.isTrue results.IsValid $"{message}: {results}"

let private expectInvalid message (results: EvaluationResults) =
    Expect.isFalse results.IsValid message

let private frontmatterSchema =
    lazy JsonSchema.FromText(read (schemaPath "validation-package-frontmatter.schema.json"))

let private configSchema =
    lazy JsonSchema.FromText(read (schemaPath "validation-packages.schema.json"))

let private loadSchema fileName =
    match fileName with
    | "validation-package-frontmatter.schema.json" -> frontmatterSchema.Value
    | "validation-packages.schema.json" -> configSchema.Value
    | _ -> failwith $"Unknown schema fixture: {fileName}"

let tests =
    testList "JSON Schema contracts" [
        testCase "both schemas validate against the Draft 2020-12 meta-schema" <| fun () ->
            [
                "validation-package-frontmatter.schema.json"
                "validation-packages.schema.json"
            ]
            |> List.iter (fun fileName ->
                read (schemaPath fileName)
                |> evaluate MetaSchemas.Draft202012
                |> expectValid $"{fileName} meta-schema"
            )

        testCase "schema ids and closed root contracts are exact" <| fun () ->
            let cases =
                [
                    "validation-package-frontmatter.schema.json",
                    SchemaUris.ValidationPackageFrontmatterV1
                    "validation-packages.schema.json",
                    SchemaUris.ValidationPackagesConfigV1
                ]

            cases
            |> List.iter (fun (fileName, expectedId) ->
                use document = JsonDocument.Parse(read (schemaPath fileName))
                let root = document.RootElement
                Expect.equal (root.GetProperty("$schema").GetString()) "https://json-schema.org/draft/2020-12/schema" "Dialect"
                Expect.equal (root.GetProperty("$id").GetString()) expectedId "$id"
                Expect.isFalse (root.GetProperty("additionalProperties").GetBoolean()) "Closed root"
                Expect.equal (root.GetProperty("properties").GetProperty("$schema").GetProperty("const").GetString()) expectedId "Instance const"
            )

        testCase "canonical frontmatter fixture passes schema and codec" <| fun () ->
            let schema = loadSchema "validation-package-frontmatter.schema.json"
            read (fixturePath "canonical-frontmatter.json")
            |> evaluate schema
            |> expectValid "Frontmatter fixture"

            let decoded = ValidationPackageYaml.decodeCurrentOrFail yaml
            Expect.equal decoded metadata "YAML codec fixture parity"

        testCase "canonical config fixture passes schema and codec with matching order" <| fun () ->
            let schema = loadSchema "validation-packages.schema.json"
            let fixture = read (fixturePath "canonical-validation-packages.json")
            fixture |> evaluate schema |> expectValid "Config fixture"

            let decoded = ValidationPackagesConfigYaml.decodeCurrentOrFail ConfigYamlTests.canonicalYaml
            use document = JsonDocument.Parse fixture
            let packages = document.RootElement.GetProperty("validation_packages")
            Expect.equal decoded.ValidationPackages.Length (packages.GetArrayLength()) "Package count"
            Expect.equal decoded.ValidationPackages[0].Name (packages[0].GetProperty("name").GetString()) "First package"
            Expect.equal decoded.ValidationPackages[1].Name (packages[1].GetProperty("name").GetString()) "Second package"

        testCase "schemas reject wrong ids, missing fields, invalid enums, SemVer, and unknown fields" <| fun () ->
            let configSchema = loadSchema "validation-packages.schema.json"

            [
                """{"$schema":"https://example.org/wrong","validation_packages":[]}"""
                """{"$schema":"https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json"}"""
                """{"$schema":"https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json","validation_packages":[{"name":"p","version":"1.2"}]}"""
                """{"$schema":"https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json","validation_packages":[{"name":"p","version":"1.2.3","roll_forward":"major"}]}"""
                """{"$schema":"https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json","validation_packages":[],"unknown":true}"""
            ]
            |> List.iteri (fun index json ->
                json
                |> evaluate configSchema
                |> expectInvalid $"Invalid config schema case {index}"
            )

            let frontmatterSchema = loadSchema "validation-package-frontmatter.schema.json"

            [
                """{"$schema":"https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json","Name":"p","Summary":"s","Description":"d","MajorVersion":1,"MinorVersion":0,"PatchVersion":0,"Inputs":[{"id":"v","type":"File","inputBinding":{"prefix":"--v"}}]}"""
                """{"$schema":"https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json","Name":"p","Summary":"s","Description":"d","MajorVersion":1,"MinorVersion":0,"PatchVersion":0,"Inputs":[{"id":"v","type":"string","inputBinding":{}}]}"""
                """{"$schema":"https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json","Name":"p","Summary":"s","Description":"d","MajorVersion":1,"MinorVersion":0,"PatchVersion":0,"unknown":true}"""
            ]
            |> List.iteri (fun index json ->
                json
                |> evaluate frontmatterSchema
                |> expectInvalid $"Invalid frontmatter schema case {index}"
            )
    ]
#endif
