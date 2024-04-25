let [<Literal>]PACKAGE_METADATA = """(*
---
Name: test
MajorVersion: 999
MinorVersion: 999
PatchVersion: 999
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
ReleaseNotes: "publish a package version that will eternally be the latest preview version"
---
*)"""

printfn "If you can read this in your console, you successfully executed preview test package v999.999.999!" 