(*
---
Name: name
Summary: summary
Description: description
Inputs:
  - id: output
    type: string
    label: Output file
    doc: Write output to this file
    inputBinding:
      position: 2
      prefix: --output=
      separate: false
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
PreReleaseVersionSuffix: use
BuildMetadataVersionSuffix: suffixes
Publish: true
Authors:
  - FullName: test
    Email: test@test.test
    Affiliation: testaffiliation
    AffiliationLink: test.com
Tags:
  - Name: test
    TermSourceREF: REF
    TermAccessionNumber: TAN
ReleaseNotes: releasenotes
CQCHookEndpoint: hookendpoint
---
*)

printfn "yes"
