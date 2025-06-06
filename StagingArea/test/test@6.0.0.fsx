let [<Literal>]PACKAGE_METADATA = """(*
---
Name: test
MajorVersion: 6
MinorVersion: 0
PatchVersion: 0
Publish: true
Summary: this package is here for testing purposes only.
Description: this package is here for testing purposes only - now with payload in json output.
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
ReleaseNotes: Use ARCExpect v5 with payload in json output
CQCHookEndpoint: https://aaas.nfdi4plants.org/
---
*)"""

printfn "If you can read this in your console, you are executing test package v6.0.0!" 

#r "nuget: ARCExpect, 5.0.0"

open ARCExpect
open Expecto

let test_package =
    Setup.ValidationPackage(
        metadata = Setup.Metadata(PACKAGE_METADATA),
        CriticalValidationCases = [
            test "yes" {Expect.equal 1 1 "yes"}
        ]
    )

open System.Collections.Generic

test_package
|> Execute.ValidationPipeline(
    basePath = System.Environment.CurrentDirectory,
    Payload = Dictionary<string,obj>([
        KeyValuePair("some", box "payload")
        KeyValuePair(
            "inner", 
            box (
                Dictionary<string,obj>([
                    KeyValuePair("inner?", box "yes")
                ])
            )
        )
        KeyValuePair("integer", box 42)
    ])
)

printfn "If you can read this in your console, you successfully executed test package v6.0.0!" 