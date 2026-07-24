(*
---
Name: valid
MajorVersion: 2
MinorVersion: 0
PatchVersion: 0
PreReleaseVersionSuffix: alpha.1
BuildMetadataVersionSuffix: build.1
Summary: My package does the thing.
Description: |
  My package does the thing.
  It does it very good, it does it very well.
  It does it very fast, it does it very swell.
Publish: true
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
  - Name: my-tag
    TermSourceREF: my-ontology
    TermAccessionNumber: MO:12345
ReleaseNotes: |
  - initial release
    - does the thing
    - does it well
CQCHookEndpoint: https://hook.com
Inputs:
  - id: input
    type: string?
    label: Input ARC
    doc: Input ARC path
    inputBinding:
      prefix: --input
  - id: verbose
    type: boolean?
    doc: Enable verbose logging
    inputBinding:
      prefix: --verbose
  - id: threads
    type: int
    inputBinding:
      position: 2
      prefix: --threads
  - id: output
    type: string
    label: Output file
    doc: Write output to this file
    inputBinding:
      position: 2
      prefix: --output=
      separate: false
  - id: mode
    type: string?
    inputBinding:
      position: 3
---
*)
