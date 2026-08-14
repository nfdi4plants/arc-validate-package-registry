(*
---
$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: canonical-contract
Summary: Canonical portable contract
Description: Exercises every public metadata and CWL boundary.
MajorVersion: 1
MinorVersion: 2
PatchVersion: 3
PreReleaseVersionSuffix: rc.1
BuildMetadataVersionSuffix: build.7
ProgrammingLanguage: FSharp
Publish: false
Authors:
  - FullName: Ada Example
    Email: ada@example.org
    Affiliation: DataPLANT
    AffiliationLink: https://nfdi4plants.org
Tags:
  - Name: validation
    TermSourceREF: AVPR
    TermAccessionNumber: AVPR:validation
ReleaseNotes: Canonical cross-repository contract.
CQCHookEndpoint: https://example.org/hooks/cqc
Inputs:
  - id: arc-directory
    type: string
    label: ARC directory
    doc: Path to the ARC.
    inputBinding:
      position: 1
      prefix: --arc-directory
  - id: verbose
    type: boolean?
    label: Verbose output
    doc: Enable detailed output.
    inputBinding:
      position: 2
      prefix: --verbose
---
*)
printfn "canonical contract"
