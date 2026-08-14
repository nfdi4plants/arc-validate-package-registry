module ValidationPackage.Model.Tests.CwlTests

open System
open Fable.Pyxpecto
open ValidationPackage.Model
open ValidationPackage.Model.Tests.ReferenceObjects

let private expectArgumentFailure action message =
    let mutable failed = false

    try
        action ()
    with
    | _ ->
        failed <- true

    Expect.isTrue failed message

let tests =
    testList "CWL command inputs" [
        testCase "every supported scalar round-trips" <| fun () ->
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

            cases
            |> List.iter (fun (cwlType, primitive, nullable) ->
                let actual = CommandInputType.fromCwlString cwlType
                Expect.equal actual.PrimitiveType primitive $"Primitive for {cwlType}"
                Expect.equal actual.IsNullable nullable $"Nullability for {cwlType}"
                Expect.equal (CommandInputType.toCwlString actual) cwlType $"Round-trip for {cwlType}"
            )

        testCase "unsupported scalar strings fail" <| fun () ->
            [ "File"; "string??"; "STRING"; ""; "?" ]
            |> List.iter (fun value ->
                expectArgumentFailure
                    (fun () -> CommandInputType.fromCwlString value |> ignore)
                    $"Expected '{value}' to fail"
            )

        testCase "undefined primitive values fail" <| fun () ->
            expectArgumentFailure
                (fun () ->
                    let inputType = CommandInputType()
                    inputType.PrimitiveType <- enum<CwlPrimitive> 999
                )
                "Undefined primitives should fail"

        testCase "type equality includes primitive and nullability" <| fun () ->
            let requiredString = CommandInputType.create(CwlPrimitive.String)
            let sameRequiredString = CommandInputType.create(CwlPrimitive.String)
            let nullableString = CommandInputType.create(CwlPrimitive.String, true)

            Expect.equal requiredString sameRequiredString "Equivalent types should compare equal"
            Expect.equal (CommandInputType.getHashCode requiredString) (CommandInputType.getHashCode sameRequiredString) "Equivalent types should hash equally"
            Expect.isFalse (requiredString = nullableString) "Nullability participates in equality"
            Expect.isFalse (requiredString = CommandInputs.nullableBoolean) "Primitive participates in equality"

        testCase "binding factory uses CWL defaults" <| fun () ->
            let actual = CommandInputBinding.create()
            Expect.equal actual CommandInputs.defaultBinding "Default bindings should match"
            Expect.equal actual.Position 0 "Default position"
            Expect.equal actual.Prefix "" "Default prefix"

        testCase "binding equality includes every field" <| fun () ->
            let defaults = CommandInputBinding.create()
            Expect.equal defaults (CommandInputBinding.create()) "Equivalent bindings"
            Expect.isFalse (defaults = CommandInputBinding.create(Position = 1)) "Position participates"
            Expect.isFalse (defaults = CommandInputBinding.create(Prefix = "--value")) "Prefix participates"

        testCase "parameter factory supports mandatory and optional fields" <| fun () ->
            let mandatory =
                CommandInputParameter.create(
                    "input",
                    CommandInputType.create(CwlPrimitive.String, true),
                    CommandInputBinding.create(Prefix = "--input")
                )

            let allFields =
                CommandInputParameter.create(
                    "output",
                    CommandInputs.requiredString,
                    CommandInputs.allFieldsBinding,
                    Label = "Output file",
                    Doc = "Write output to this file"
                )

            Expect.equal mandatory CommandInputs.mandatoryParameter "Mandatory parameter"
            Expect.equal allFields CommandInputs.allFieldsParameter "All-fields parameter"

        testCase "parameter equality includes nested values" <| fun () ->
            let first =
                CommandInputParameter.create(
                    "value",
                    CommandInputType.create(CwlPrimitive.String),
                    CommandInputBinding.create()
                )

            let second =
                CommandInputParameter.create(
                    "value",
                    CommandInputType.create(CwlPrimitive.String),
                    CommandInputBinding.create()
                )

            let nullable =
                CommandInputParameter.create(
                    "value",
                    CommandInputType.create(CwlPrimitive.String, true),
                    CommandInputBinding.create()
                )

            Expect.equal first second "Equivalent parameters"
            Expect.equal (CommandInputParameter.getHashCode first) (CommandInputParameter.getHashCode second) "Equivalent parameters hash equally"
            Expect.isFalse (first = nullable) "Nested input type participates"

        testCase "declaration validation accepts the narrowed binding contract" <| fun () ->
            let declarations =
                [|
                    CommandInputParameter.create(
                        "strict",
                        CommandInputType.create(CwlPrimitive.Boolean),
                        CommandInputBinding.create(Prefix = "--strict")
                    )
                    CommandInputParameter.create(
                        "title",
                        CommandInputType.create(CwlPrimitive.String, true),
                        CommandInputBinding.create(Position = 10, Prefix = "--title")
                    )
                |]

            Expect.equal
                (CommandInputParameter.validate declarations)
                declarations
                "Valid declarations retain source order"

        testCase "declaration validation rejects positional, duplicate, and reserved prefixes" <| fun () ->
            let declaration id prefix =
                CommandInputParameter.create(
                    id,
                    CommandInputType.create(CwlPrimitive.String),
                    CommandInputBinding.create(Prefix = prefix)
                )

            [
                [| declaration "value" "" |]
                [| declaration "value" "--" |]
                [| declaration "value" "-i" |]
                [| declaration "value" "-o" |]
                [| declaration "value" "--source-branch" |]
                [| declaration "value" "--source-commit-hash" |]
                [| declaration "same" "--first"; declaration "same" "--second" |]
                [| declaration "first" "--same"; declaration "second" "--same" |]
            ]
            |> List.iteri (fun index declarations ->
                expectArgumentFailure
                    (fun () -> CommandInputParameter.validate declarations |> ignore)
                    $"Invalid declaration case {index} should fail"
            )
    ]
