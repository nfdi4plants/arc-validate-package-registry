let [<Literal>]PACKAGE_METADATA = """(*
---
Name: test
MajorVersion: 5
MinorVersion: 0
PatchVersion: 0
PreRelease: use
BuildMetadata: suffixes
Publish: true
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
ReleaseNotes: Use ARCExpect v3
CQCHookEndpoint: https://avpr.nfdi4plants.org
---
*)"""

printfn "If you can read this in your console, you successfully executed test package v5.0.0-use+suffixes!" 

#r "nuget: ARCExpect, 3.0.0"

open ARCExpect
open Expecto
let test_package =
    Setup.ValidationPackage(
        metadata = Setup.Metadata(PACKAGE_METADATA),
        CriticalValidationCases = [
            test "yes" {Expect.equal 1 1 "yes"}
        ]
    )

test_package
|> Execute.ValidationPipeline(
    basePath = System.Environment.CurrentDirectory
)