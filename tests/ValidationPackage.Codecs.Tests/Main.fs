module ValidationPackage.Codecs.Tests.Main

open Fable.Pyxpecto

let all =
    testSequenced <| testList "ValidationPackage.Codecs" [
        FrontmatterTests.tests
        YamlTests.tests
        ConfigYamlTests.tests
        JsonTests.tests
#if !FABLE_COMPILER
        SchemaTests.tests
        YamlFixtureTests.tests
#endif
    ]

#if !FABLE_COMPILER_JAVASCRIPT && !FABLE_COMPILER_TYPESCRIPT
let (!!) (value: 'a) = value
#endif

#if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
open Fable.Core.JsInterop
#endif

[<EntryPoint>]
let main _ =
    !!Pyxpecto.runTests [||] all
