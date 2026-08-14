# Validation package metadata

## Contents

- [F# frontmatter](#f-frontmatter)
- [Python frontmatter](#python-frontmatter)
- [Mandatory fields](#mandatory-fields)
- [Optional fields](#optional-fields)
- [Nested objects](#nested-objects)
  - [Author](#author)
  - [Ontology annotation](#ontology-annotation)
- [Complete example](#complete-example)

Every validation package starts with YAML frontmatter. The script language
determines how that YAML is enclosed; the metadata contract is otherwise the
same. New canonical frontmatter carries the immutable v1 schema URI. Readers
select that schema entirely offline; the URI is not fetched during parsing.

## F# frontmatter

An F# script may place frontmatter in a multiline comment:

```fsharp
(*
---
$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: my-package
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Summary: My package validates an ARC.
Description: A longer explanation of the validation behavior.
---
*)
```

Binding it to the literal `PACKAGE_METADATA` is recommended so the script can
reuse the same metadata:

```fsharp
let [<Literal>] PACKAGE_METADATA = """(*
---
$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: my-package
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Summary: My package validates an ARC.
Description: A longer explanation of the validation behavior.
---
*)"""
```

For example, a script using the portable model and codecs can extract it
without repeating the package identity:

```fsharp
#r "nuget: ValidationPackage.Model"
#r "nuget: ValidationPackage.Codecs"

open ValidationPackage.Codecs

let metadata =
    ValidationPackageYaml.extractOrFail
        FrontmatterLanguage.FSharp
        PACKAGE_METADATA
```

The binding must be at the start of the file and use the exact name
`PACKAGE_METADATA` with the `[<Literal>]` attribute.

## Python frontmatter

A Python script uses its initial triple-quoted string:

```python
"""
---
$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: my-package
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Summary: My package validates an ARC.
Description: A longer explanation of the validation behavior.
---
"""
```

Binding the string is likewise recommended:

```python
PACKAGE_METADATA = """
---
$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: my-package
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Summary: My package validates an ARC.
Description: A longer explanation of the validation behavior.
---
"""
```

## Mandatory fields

Canonical documents also require `$schema` with the exact value shown above.
The codec retains an explicit schema-less legacy reader for immutable published
packages, but canonical writers always emit `$schema` first.

| Field | Type | Description |
| --- | --- | --- |
| `Name` | string | Package name; must match its directory and filename |
| `MajorVersion` | integer | Semantic major version |
| `MinorVersion` | integer | Semantic minor version |
| `PatchVersion` | integer | Semantic patch version |
| `Summary` | string | Single-sentence description, at most 50 words |
| `Description` | string | Unconstrained longer description |

## Optional fields

| Field | Type | Description |
| --- | --- | --- |
| `PreReleaseVersionSuffix` | string | Semantic-version prerelease suffix |
| `BuildMetadataVersionSuffix` | string | Semantic-version build suffix |
| `Publish` | boolean | Marks an eligible staged package for publication |
| `Authors` | `Author[]` | Package authors and maintainers |
| `Tags` | `OntologyAnnotation[]` | Search and ontology tags |
| `ReleaseNotes` | string | Human-readable changes for this package version |
| `CQCHookEndpoint` | string | Optional CQC hook URL |
| `Inputs` | `CommandInputParameter[]` | Supported CWL command inputs; see [CWL command inputs](cwl-inputs.md) |

`Publish` defaults to `false`; collection and string fields default to empty
values. Omitting `Inputs` is backwards compatible and produces an empty input
collection.

## Nested objects

### Author

| Field | Type | Mandatory | Description |
| --- | --- | --- | --- |
| `FullName` | string | yes | Author's full name |
| `Email` | string | no | Contact email |
| `Affiliation` | string | no | Institution or organization |
| `AffiliationLink` | string | no | Link to the affiliation |

### Ontology annotation

| Field | Type | Mandatory | Description |
| --- | --- | --- | --- |
| `Name` | string | yes | Display name |
| `TermSourceREF` | string | no | Controlled-vocabulary source |
| `TermAccessionNumber` | string | no | Accession in that vocabulary |

## Complete example

```yaml
$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: my-package
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Summary: My package validates an ARC.
Description: |
  A longer explanation of the package and its validation behavior.
Publish: true
Authors:
  - FullName: Jane Doe
    Email: jane@example.org
    Affiliation: Example University
    AffiliationLink: https://example.org
Tags:
  - Name: validation
  - Name: my-tag
    TermSourceREF: my-ontology
    TermAccessionNumber: MO:12345
ReleaseNotes: Initial release.
CQCHookEndpoint: https://example.org/cqc-hook
Inputs:
  - id: verbose
    type: boolean?
    label: Verbose logging
    doc: Enable verbose logging
    inputBinding:
      prefix: --verbose
```

Wrap this YAML using the F# or Python form above. See
[submitting a validation package](submission.md) for naming, versioning, and
publication rules. The standalone Draft 2020-12 schema is shipped at
`schemas/validation-package-frontmatter.schema.json` in every
`ValidationPackage.Codecs` package.
