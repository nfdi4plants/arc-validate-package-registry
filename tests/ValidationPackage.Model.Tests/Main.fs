module ValidationPackage.Model.Tests.Main

open Fable.Pyxpecto

let all =
    testSequenced <| testList "ValidationPackage.Model" [
        SemanticVersionTests.tests
        CwlTests.tests
        ConfigTests.tests
        DomainTests.tests
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
