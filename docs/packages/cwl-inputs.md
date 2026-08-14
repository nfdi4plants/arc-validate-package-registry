# CWL command inputs

## Contents

- [Why a CWL subset](#why-a-cwl-subset)
- [Example](#example)
- [Supported input fields](#supported-input-fields)
- [Supported types](#supported-types)
- [Supported binding fields](#supported-binding-fields)
- [Deliberately unsupported features](#deliberately-unsupported-features)
- [Representation boundaries](#representation-boundaries)

`Inputs` declares package-specific configurable inputs using a deliberately
scoped subset of
[CWL v1.2 `CommandLineTool.inputs`](https://www.commonwl.org/v1.2/CommandLineTool.html).
The AVPR wrapper remains PascalCase `Inputs`, consistently with the other
[package metadata](metadata.md). Its value uses CWL's array form and can be
placed under the lowercase `inputs` field of a complete CWL
`CommandLineTool`.

AVPR publishes a compatible CWL fragment; package frontmatter is not a complete
CWL document, and the registry is not a general CWL runner.

## Why a CWL subset

Package inputs need more than documentation: downstream tools must be able to
validate values and eventually construct deterministic command lines. Reusing
CWL provides established names and semantics for types, nullability, prefixes,
and ordering instead of creating an AVPR-only schema.

The first release supports the configuration needed by current validation
scripts while keeping parsing, persistence, OpenAPI, generated clients, and
future argument construction unambiguous:

- Six scalar primitives map directly to portable command-line values.
- One nullable shorthand avoids multiple wire shapes for the same meaning.
- Two binding fields cover flags, options, and deterministic ordering.
- Array form maps directly to the registry's ordered collection and owned JSON
  model while retaining explicit input IDs.

Complex CWL types are deferred until the registry and downstream runner can
implement their staging, path, validation, and serialization semantics.

## Example

```yaml
$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Inputs:
  - id: echo
    type: string?
    label: Echo text
    doc: Print the supplied text
    inputBinding:
      prefix: --echo

  - id: verbose
    type: boolean?
    label: Verbose logging
    doc: Enable verbose logging
    inputBinding:
      prefix: --verbose

  - id: output
    type: string
    doc: Select the output file
    inputBinding:
      position: 2
      prefix: --output
```

## Supported input fields

| CWL field | Supported value | Mandatory |
| --- | --- | --- |
| `id` | Non-empty string, unique within the package | yes |
| `type` | Supported scalar string, optionally followed by one `?` | yes |
| `label` | string | no |
| `doc` | string | no |
| `inputBinding` | Supported binding object | yes |

Nested names remain exactly lower-camel-case in frontmatter, public JSON,
OpenAPI, and generated-client serialization.

## Supported types

- `boolean`
- `int`
- `long`
- `float`
- `double`
- `string`

Appending exactly one `?` makes a value nullable, for example `boolean?` or
`string?`. This shorthand is the only accepted nullable representation. CWL
also permits union arrays such as `type: ["null", "string"]`, but accepting
both would expose two public shapes for the same meaning and complicate schema
generation, clients, and storage.

Do not add a separate `Required` property. Requiredness is represented by the
type: `string` requires a value, while `string?` permits omission or null.

## Supported binding fields

| CWL field | Supported value | Default |
| --- | --- | --- |
| `prefix` | One non-empty canonical string | required |
| `position` | integer | `0` |

Binding behavior follows CWL:

- A boolean `true` emits its prefix; `false` emits nothing. A nullable boolean
  also emits nothing for null. Boolean flags never emit a trailing `true` or
  `false` value.
- A non-boolean always emits prefix and value as two argv elements.
- Missing `position` uses position `0`; equal positions are resolved
  deterministically by input ID.
- Prefixes are exact and unique. They cannot be `--`, `-i`, `-o`,
  `--source-branch`, or `--source-commit-hash`.

CWL defines one binding prefix, not aliases. A package script may independently
accept `-v` as well as `--verbose`, but only one canonical prefix belongs in
the structured contract.

## Deliberately unsupported features

The first subset does not support:

- `File`, `Directory`, or `stdin`, which need path and staging semantics;
- arrays, records, and enums;
- union-array syntax or general unions;
- user-defined or IRI types;
- CWL maps keyed by input ID.

Additional parameter or binding fields such as `default`, `secondaryFiles`,
`format`, `valueFrom`, `itemSeparator`, `shellQuote`, and `separate` are
rejected. Positional inputs are likewise rejected. Unsupported `type` values
and shapes fail with an actionable diagnostic.

The extracted metadata mapping carries
`$schema: https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json`.
The schema is also shipped as
`schemas/validation-package-frontmatter.schema.json` in every
`ValidationPackage.Codecs` artifact. Runtime parsing selects its decoder from
an offline allowlist and never fetches the URI.

## Representation boundaries

- Frontmatter and public API JSON expose `type` as one scalar such as
  `boolean?`.
- OpenAPI enumerates the twelve required/nullable scalar strings.
- The generated client maps that scalar to its client representation.
- PostgreSQL alone stores a normalized primitive/nullability object inside the
  owned `Inputs` JSON document.

The internal database object must never leak through the API. Existing packages
without `Inputs` are represented by an empty collection, including rows
backfilled during migration.

Declaring inputs does not install an argument parser in a package script.
Scripts remain responsible for interpreting argv until downstream execution
support lands in `arc-validate`.
