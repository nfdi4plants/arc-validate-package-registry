module ValidationPackage.Codecs.Tests.ReferenceObjects

open ValidationPackage.Model

let input =
    CommandInputParameter.create(
        "output",
        CommandInputType.create(CwlPrimitive.String),
        CommandInputBinding.create(
            Position = 2,
            Prefix = "--output"
        ),
        Label = "Output file",
        Doc = "Write output to this file"
    )

let metadata =
    ValidationPackageMetadata.create(
        name = "test-package",
        summary = "A portable package",
        description = "Validates an ARC.",
        majorVersion = 1,
        minorVersion = 2,
        patchVersion = 3,
        programmingLanguage = "FSharp",
        PreReleaseVersionSuffix = "beta.1",
        BuildMetadataVersionSuffix = "build.5",
        Publish = true,
        Authors =
            [|
                Author.create(
                    "Ada Example",
                    Email = "ada@example.org",
                    Affiliation = "DataPLANT",
                    AffiliationLink = "https://nfdi4plants.org"
                )
            |],
        Tags =
            [|
                OntologyAnnotation.create(
                    "validation",
                    TermSourceRef = "AVPR",
                    TermAccessionNumber = "AVPR:validation"
                )
            |],
        ReleaseNotes = "Initial portable release",
        CQCHookEndpoint = "https://example.org/cqc",
        Inputs = [| input |]
    )

let yaml =
    """$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: test-package
Summary: A portable package
Description: Validates an ARC.
MajorVersion: 1
MinorVersion: 2
PatchVersion: 3
PreReleaseVersionSuffix: beta.1
BuildMetadataVersionSuffix: build.5
ProgrammingLanguage: FSharp
Publish: true
Authors:
  - FullName: Ada Example
    Email: ada@example.org
    Affiliation: DataPLANT
    AffiliationLink: https://nfdi4plants.org
Tags:
  - Name: validation
    TermSourceREF: AVPR
    TermAccessionNumber: AVPR:validation
ReleaseNotes: Initial portable release
CQCHookEndpoint: https://example.org/cqc
Inputs:
  - id: output
    type: string
    label: Output file
    doc: Write output to this file
    inputBinding:
      position: 2
      prefix: --output
"""

let fsharpComment =
    $"(*\n---{yaml}---\n*)\nprintfn \"validation\""

let fsharpBinding =
    $"let [<Literal>]PACKAGE_METADATA = \"\"\"(*\n---{yaml}---\n*)\"\"\"\nprintfn PACKAGE_METADATA"

let pythonComment =
    $"\"\"\"\n---{yaml}---\n\"\"\"\nprint('validation')"

let pythonBinding =
    $"PACKAGE_METADATA = \"\"\"\n---{yaml}---\n\"\"\"\nprint(PACKAGE_METADATA)"
