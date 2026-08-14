module ValidationPackage.Model.Tests.ConfigTests

open Fable.Pyxpecto
open ValidationPackage.Model

let private expectFailure action message =
    let mutable failed = false

    try
        action ()
    with _ ->
        failed <- true

    Expect.isTrue failed message

let private declaration id primitive nullable position prefix =
    CommandInputParameter.create(
        id,
        CommandInputType.create(primitive, nullable),
        CommandInputBinding.create(Position = position, Prefix = prefix)
    )

let tests =
    testList "validation-packages config" [
        testCase "input values preserve kind and numeric lexeme" <| fun () ->
            let values =
                [|
                    ValidationPackageInputValue.nullValue()
                    ValidationPackageInputValue.boolean false
                    ValidationPackageInputValue.integer "-9223372036854775808"
                    ValidationPackageInputValue.integer "9223372036854775807"
                    ValidationPackageInputValue.floatingPoint "1.25e+2"
                    ValidationPackageInputValue.string "001"
                |]

            Expect.equal values[0].Kind ValidationPackageInputValueKind.Null "Null kind"
            Expect.equal values[1].Value "false" "Boolean invariant text"
            Expect.equal values[2].Value "-9223372036854775808" "Minimum int64 lexeme"
            Expect.equal values[3].Value "9223372036854775807" "Maximum int64 lexeme"
            Expect.equal values[4].Value "1.25e+2" "Floating lexeme"
            Expect.equal values[5].Kind ValidationPackageInputValueKind.String "Quoted numeric-like string"

        testCase "numeric factories reject non-JSON and non-finite spellings" <| fun () ->
            [ "01"; "+1"; "1_000"; "0x10"; "1.0" ]
            |> List.iter (fun value ->
                expectFailure
                    (fun () -> ValidationPackageInputValue.integer value |> ignore)
                    $"Integer '{value}' should fail"
            )

            [ "1"; ".5"; "1."; "1e"; "1e400"; ".nan"; ".inf" ]
            |> List.iter (fun value ->
                expectFailure
                    (fun () -> ValidationPackageInputValue.floatingPoint value |> ignore)
                    $"Floating value '{value}' should fail"
            )

        testCase "canonical config preserves source order and optional ARC specification" <| fun () ->
            let first =
                ValidationPackageSelection.create(
                    "first",
                    SemVer.tryParse "1.2.3" |> Option.get,
                    Inputs =
                        [|
                            ValidationPackageInput.create(
                                "title",
                                ValidationPackageInputValue.string "release"
                            )
                        |]
                )

            let second =
                ValidationPackageSelection.create(
                    "second",
                    SemVer.tryParse "2.0.0" |> Option.get,
                    RollForward = RollForwardPolicy.LatestMinor
                )

            let config =
                ValidationPackagesConfig.create(
                    [| first; second |],
                    ArcSpecification = (SemVer.tryParse "3.0.0-draft.2" |> Option.get)
                )

            Expect.isTrue config.HasArcSpecification "ARC specification is present"
            Expect.equal config.ValidationPackages[0].Name "first" "Package order"
            Expect.equal config.ValidationPackages[1].RollForward RollForwardPolicy.LatestMinor "Policy"

        testCase "declaration compatibility covers scalar kinds and int64 limits" <| fun () ->
            let declarations =
                [|
                    declaration "flag" CwlPrimitive.Boolean false 0 "--flag"
                    declaration "count" CwlPrimitive.Int false 1 "--count"
                    declaration "total" CwlPrimitive.Long false 2 "--total"
                    declaration "ratio" CwlPrimitive.Float false 3 "--ratio"
                    declaration "score" CwlPrimitive.Double false 4 "--score"
                    declaration "title" CwlPrimitive.String false 5 "--title"
                    declaration "optional" CwlPrimitive.String true 6 "--optional"
                |]

            let selection =
                ValidationPackageSelection.create(
                    "package",
                    SemVer.create(1, 0, 0),
                    Inputs =
                        [|
                            ValidationPackageInput.create("flag", ValidationPackageInputValue.boolean false)
                            ValidationPackageInput.create("count", ValidationPackageInputValue.integer "-2147483648")
                            ValidationPackageInput.create("total", ValidationPackageInputValue.integer "9223372036854775807")
                            ValidationPackageInput.create("ratio", ValidationPackageInputValue.integer "2")
                            ValidationPackageInput.create("score", ValidationPackageInputValue.floatingPoint "1.25e2")
                            ValidationPackageInput.create("title", ValidationPackageInputValue.string "release candidate")
                            ValidationPackageInput.create("optional", ValidationPackageInputValue.nullValue())
                        |]
                )

            ValidationPackageSelection.validateInputs(selection, declarations)

        testCase "materialization orders by position then ordinal id and omits false and null" <| fun () ->
            let declarations =
                [|
                    declaration "zeta" CwlPrimitive.String false 5 "--zeta"
                    declaration "flag" CwlPrimitive.Boolean false 1 "--flag"
                    declaration "alpha" CwlPrimitive.Int false 5 "--alpha"
                    declaration "optional" CwlPrimitive.String true 0 "--optional"
                |]

            let selection =
                ValidationPackageSelection.create(
                    "package",
                    SemVer.create(1, 0, 0),
                    Inputs =
                        [|
                            ValidationPackageInput.create("zeta", ValidationPackageInputValue.string "hello world")
                            ValidationPackageInput.create("flag", ValidationPackageInputValue.boolean false)
                            ValidationPackageInput.create("alpha", ValidationPackageInputValue.integer "2")
                            ValidationPackageInput.create("optional", ValidationPackageInputValue.nullValue())
                        |]
                )

            Expect.equal
                (ValidationPackageSelection.materializeArguments(selection, declarations))
                [| "--alpha"; "2"; "--zeta"; "hello world" |]
                "Logical argv tokens"

        testCase "validation rejects unknown, missing, incompatible, and out-of-range values" <| fun () ->
            let declarations =
                [|
                    declaration "required" CwlPrimitive.Boolean false 0 "--required"
                    declaration "count" CwlPrimitive.Int false 1 "--count"
                |]

            let selections =
                [|
                    ValidationPackageSelection.create(
                        "missing",
                        SemVer.create(1, 0, 0),
                        Inputs = [| ValidationPackageInput.create("count", ValidationPackageInputValue.integer "1") |]
                    )
                    ValidationPackageSelection.create(
                        "unknown",
                        SemVer.create(1, 0, 0),
                        Inputs =
                            [|
                                ValidationPackageInput.create("required", ValidationPackageInputValue.boolean true)
                                ValidationPackageInput.create("count", ValidationPackageInputValue.integer "1")
                                ValidationPackageInput.create("other", ValidationPackageInputValue.string "x")
                            |]
                    )
                    ValidationPackageSelection.create(
                        "type",
                        SemVer.create(1, 0, 0),
                        Inputs =
                            [|
                                ValidationPackageInput.create("required", ValidationPackageInputValue.boolean true)
                                ValidationPackageInput.create("count", ValidationPackageInputValue.floatingPoint "1.0")
                            |]
                    )
                    ValidationPackageSelection.create(
                        "range",
                        SemVer.create(1, 0, 0),
                        Inputs =
                            [|
                                ValidationPackageInput.create("required", ValidationPackageInputValue.boolean true)
                                ValidationPackageInput.create("count", ValidationPackageInputValue.integer "2147483648")
                            |]
                    )
                |]

            selections
            |> Array.iter (fun selection ->
                expectFailure
                    (fun () -> ValidationPackageSelection.validateInputs(selection, declarations))
                    $"Selection '{selection.Name}' should fail"
            )

            [
                CwlPrimitive.Long, ValidationPackageInputValue.integer "9223372036854775808"
                CwlPrimitive.Long, ValidationPackageInputValue.integer "-9223372036854775809"
                CwlPrimitive.Float, ValidationPackageInputValue.floatingPoint "3.5e38"
                CwlPrimitive.Double, ValidationPackageInputValue.integer (String.replicate 400 "9")
            ]
            |> List.iteri (fun index (primitive, value) ->
                let id = $"range-{index}"

                let selection =
                    ValidationPackageSelection.create(
                        "range",
                        SemVer.create(1, 0, 0),
                        Inputs = [| ValidationPackageInput.create(id, value) |]
                    )

                expectFailure
                    (fun () ->
                        ValidationPackageSelection.validateInputs(
                            selection,
                            [| declaration id primitive false 0 $"--{id}" |]
                        )
                    )
                    $"Out-of-range {primitive} should fail"
            )

        testCase "config rejects duplicate package and input ids" <| fun () ->
            let version = SemVer.create(1, 0, 0)
            let selection name = ValidationPackageSelection.create(name, version)

            expectFailure
                (fun () -> ValidationPackagesConfig.create([| selection "same"; selection "same" |]) |> ignore)
                "Duplicate package names should fail"

            expectFailure
                (fun () ->
                    ValidationPackageSelection.create(
                        "package",
                        version,
                        Inputs =
                            [|
                                ValidationPackageInput.create("same", ValidationPackageInputValue.string "one")
                                ValidationPackageInput.create("same", ValidationPackageInputValue.string "two")
                            |]
                    )
                    |> ignore
                )
                "Duplicate input ids should fail"

        testCase "config rejects non-canonical semantic-version objects" <| fun () ->
            let invalid = SemVer.create(-1, 0, 0)

            expectFailure
                (fun () -> ValidationPackageSelection.create("package", invalid) |> ignore)
                "Canonical selections require a full semantic version"

            expectFailure
                (fun () ->
                    ValidationPackagesConfig.create(
                        Array.empty,
                        ArcSpecification = SemVer.create(1, 0, 0, PreRelease = "01")
                    )
                    |> ignore
                )
                "ARC specification requires a full semantic version"

            expectFailure
                (fun () ->
                    LegacyValidationPackageSelection.create("package", Version = invalid)
                    |> ignore
                )
                "Versioned legacy selections require a full semantic version"
    ]
