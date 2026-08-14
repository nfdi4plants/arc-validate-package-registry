module ValidationPackage.Codecs.Tests.YamlFixtureTests

open System.IO
open Fable.Pyxpecto
open ValidationPackage.Codecs

let private fixturePath kind name =
    Path.Combine(__SOURCE_DIRECTORY__, "Fixtures", "Yaml", kind, name)

let private read kind name =
    File.ReadAllText(fixturePath kind name)

let tests =
    testList "committed YAML fixtures" [
        testCase "positive canonical and legacy fixtures select their intended decoders" <| fun () ->
            let canonicalFrontmatter =
                read "positive" "canonical-frontmatter.yml"
                |> ValidationPackageYaml.decodeOrFail

            let legacyFrontmatter =
                read "positive" "legacy-frontmatter.yml"
                |> ValidationPackageYaml.decodeOrFail

            let canonicalConfig =
                read "positive" "canonical-validation-packages.yml"
                |> ValidationPackagesConfigYaml.decodeOrFail

            let legacyConfig =
                read "positive" "legacy-validation-packages.yml"
                |> ValidationPackagesConfigYaml.decodeOrFail

            Expect.equal canonicalFrontmatter.Name "fixture-package" "Canonical frontmatter"
            Expect.equal legacyFrontmatter.Name "historical-package" "Legacy frontmatter"
            Expect.isFalse canonicalConfig.IsLegacy "Canonical config"
            Expect.isTrue legacyConfig.IsLegacy "Legacy config"

        testCase "negative fixtures are rejected by the strict codecs" <| fun () ->
            [
                "ambiguous-scalar.yml"
                "anchor-and-alias.yml"
                "unknown-config-field.yml"
            ]
            |> List.iter (fun name ->
                match read "negative" name |> ValidationPackagesConfigYaml.decode with
                | Ok _ -> failwith $"Expected {name} to fail"
                | Error _ -> ()
            )

            [
                "separate-binding.yml"
                "positional-binding.yml"
            ]
            |> List.iter (fun name ->
                match read "negative" name |> ValidationPackageYaml.decode with
                | Ok _ -> failwith $"Expected {name} to fail"
                | Error _ -> ()
            )
    ]
