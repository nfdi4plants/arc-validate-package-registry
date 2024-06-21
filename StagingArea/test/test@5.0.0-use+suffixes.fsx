(*
---
Name: test
MajorVersion: 5
MinorVersion: 0
PatchVersion: 0
PreReleaseVersionSuffix: use
BuildMetadataVersionSuffix: suffixes
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
ReleaseNotes: Use pre-release and build metadata version suffixes 
CQCHookEndpoint: https://avpr.nfdi4plants.org
---
*)

printfn "If you can read this in your console, you successfully executed test package v5.0.0-use+suffixes!" 