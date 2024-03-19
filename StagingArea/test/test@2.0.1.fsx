(*
---
Name: test
MajorVersion: 2
MinorVersion: 0
PatchVersion: 1
Publish: false
Summary: this package is here for testing purposes only.
Description: this package is here for testing purposes only.
Authors:
  - FullName: John Doe
    Email: j@d.com
    Affiliation: University of Nowhere
    AffiliationLink: https://nowhere.edu
  - FullName: Jane Doe
    Email: jj@d.com
    Affiliation: University of Somewhere
    AffiliationLink: https://somewhere.edu
Tags:
  - Name: validation
  - Name: my-package
  - Name: thing
ReleaseNotes: "Trigger another CI run!"
---
*)
 
// this file is intended for testing purposes only.
printfn "If you can read this in your console, you successfully executed test package v2.0.1!" 

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