(*
---
Name: test
Summary: this package is here for testing purposes only.
Description: this package is here for testing purposes only.
MajorVersion: 1
MinorVersion: 0
PatchVersion: 1
Publish: true
---
*)
 
// this file is intended for testing purposes only.
printfn "If you can read this in your console, you successfully executed test package v1.0.1!" 

#r "nuget: ARCExpect, 1.0.1"

open ARCExpect
open Expecto

let validationCases = testList "test" [
    test "yes" {Expect.equal 1 1 "yes"}
]

validationCases
|> Execute.ValidationPipeline(
    basePath = System.Environment.CurrentDirectory,
    packageName = "test"
)