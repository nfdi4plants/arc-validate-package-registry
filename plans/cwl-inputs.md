# Plan: Replace custom `CLIArguments` with an exact CWL `inputs` subset

## Status and scope

Implementation status as of 2026-07-24:

**Current stage: implementation.** The index/domain/frontmatter slice is
complete; the registry service and persistence slice is next.

- [x] `AVPRIndex` domain model (`CwlPrimitive`, `CommandInputType`,
  `CommandInputBinding`, `CommandInputParameter`, and
  `ValidationPackageMetadata.Inputs`)
- [x] YamlDotNet converters and registration for scalar types and nested CWL
  mappings
- [x] shared CWL scalar conversion and the System.Text.Json wire contract
- [x] Index domain/frontmatter fixtures, reference objects, hashes, and tests
- [ ] Registry service, EF JSON mapping, and migration
- [ ] Website rendering
- [ ] generated client and bidirectional mappings
- [ ] staging package, release notes, README, and `AGENTS.md`

This plan supersedes only the configurable-command-line-input portion of [`cli-args.md`](cli-args.md). The dev-branch/container work in that plan remains unchanged.

The current working tree implements a custom `CLIArguments` array containing `Flags`, `Description`, and `Example`. That shape is not expressive enough for a consumer to validate structured values or construct a command line, and its custom fields cannot be interpreted as Common Workflow Language (CWL).

Replace it before release with an **actual subset of the CWL v1.2 `CommandLineTool.inputs` schema**. Do not add AVPR-only schema fields. In particular, do not add `Aliases`, `Required`, `Description`, or `Example`.

Normative references:

- CWL v1.2 `CommandLineTool`: <https://www.commonwl.org/v1.2/CommandLineTool.html>
- CWL input examples and binding behavior: <https://www.commonwl.org/user_guide/en/topics/inputs.html>

The compatibility claim is deliberately narrow:

- The AVPR metadata field remains PascalCase as `Inputs`, consistently with the
  rest of the existing frontmatter. Its value is valid CWL v1.2 array-form
  input syntax and can be copied into the lowercase `inputs` field of a complete
  CWL `CommandLineTool`.
- AVPR supports only the fields, value shapes, types, and semantics listed below.
- A general CWL `inputs` document is not necessarily accepted by AVPR.
- Additional fields on otherwise supported parameter/binding objects are
  tolerated, ignored, and not persisted. Unsupported `type` values and shapes
  still fail because they cannot be represented by `CommandInputType`.

This repository continues to describe and serve validation packages; it does not become a general CWL runner.

---

## Exact supported schema

Use CWL's array form as the value of AVPR `Inputs`. CWL permits either an array of `CommandInputParameter` objects or a map keyed by input ID. The array form is chosen because it maps directly to the repository's existing array/owned-JSON patterns.

```yaml
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
```

### Supported `CommandInputParameter` fields

| CWL field | Required by the AVPR subset | Supported value |
| --- | --- | --- |
| `id` | yes | non-empty string, unique within the package |
| `type` | yes | one supported scalar type string, optionally suffixed with `?` |
| `label` | no | string |
| `doc` | no | string |
| `inputBinding` | yes | one supported `CommandLineBinding` object |

### Supported `CommandLineBinding` fields

| CWL field | Required | Supported value |
| --- | --- | --- |
| `prefix` | no | string; omission describes a positional value |
| `position` | no | integer only |
| `separate` | no | boolean; CWL default is `true` |

### Supported `type` values

Support only scalar CWL primitive types in the first version:

- `boolean`
- `string`
- `int`
- `long`
- `float`
- `double`

Support CWL's nullable shorthand by accepting exactly one trailing `?`, for example `boolean?` or `string?`.

The shorthand is the only nullable wire form accepted and emitted by this
subset. Full CWL also permits a union such as `type: ["null", boolean]`, but
this subset deliberately has one scalar wire representation rather than a
second collection-shaped representation. Normalize `boolean?` immediately to
a single `CommandInputType` value.

Do not initially support:

- `File`, `Directory`, or `stdin` (these require additional path/staging semantics)
- arrays, records, or enums
- union arrays such as `["null", string]`; use the equivalent `string?` shorthand in this subset
- user-defined or IRI types

### Additional and unsupported CWL fields

Allow but ignore other fields inside otherwise supported input and binding
objects, including:

- `default`
- `secondaryFiles`
- `streamable`
- `format`
- `loadContents`
- `loadListing`
- binding `itemSeparator`
- binding `valueFrom`
- binding `shellQuote`

Ignored fields are not persisted, exposed through the API/client, rendered, or
honored by downstream argument construction. Documentation must not imply that
their presence makes the feature supported. A supported field with an invalid
value is still an error, and unsupported `type` values/shapes are still errors.

Top-level CWL fields such as `cwlVersion`, `class`, `baseCommand`, `arguments`, `outputs`, `requirements`, and `hints` are outside this metadata fragment. Do not add them to validation-package metadata as part of this change.

### No aliases

CWL `CommandLineBinding` has one `prefix`; it has no flag-alias field. The declared prefix is therefore canonical:

```yaml
inputBinding:
  prefix: --echo
```

A package script may independently accept `-e`, but that alias is not part of the published CWL subset and must not appear in the metadata.

### Requiredness and value-less flags

Do not add a `Required` field. Requiredness comes from CWL's type:

- `type: string` requires a non-null value.
- `type: string?` permits omission/null.

A command-line flag that takes no following value is represented as a boolean input:

- `true` emits `prefix`.
- `false` emits nothing.
- `null` emits nothing for a nullable boolean.
- It never emits `prefix true` or `prefix false`.

### Ordering and separation

Follow CWL behavior:

- Missing `position` has the default position `0`.
- Bindings are sorted by CWL position rules.
- Equal sort keys are resolved deterministically by input ID.
- `separate` defaults to `true`.
- For a non-boolean with a prefix and `separate: true`, emit two argv elements: `--echo`, `hello`.
- With `separate: false`, concatenate prefix and value: `--output=result.txt` when the prefix is `--output=`.
- A missing prefix emits the value as a positional argv element.

### AVPR scope rule

The fragment declares package-specific configurable inputs. Existing runner-owned ARC input/output arguments remain part of the `arc-validate` execution contract and are not duplicated in every package's `inputs` array in this change.

---

## Layer 1 — `AVPRIndex` domain and frontmatter

### `src/AVPRIndex/Domain.fs`

Add the replacement domain types. Keep the unshipped custom `CLIArgument` type
temporarily while downstream projects are migrated, then remove it during the
final cleanup:

- `CwlPrimitive`
  - a C#/EF-compatible F# enum with `Boolean`, `String`, `Int`, `Long`,
    `Float`, and `Double` cases
- `CommandInputType`
  - internal CLR/F# member `PrimitiveType : CwlPrimitive`
  - internal CLR/F# member `IsNullable : bool`
- `CommandInputBinding`
  - internal CLR/F# member `Prefix`
  - internal CLR/F# member `Position`
  - internal CLR/F# member `Separate`
- `CommandInputParameter`
  - internal CLR/F# member `Id`
  - internal CLR/F# member `Type`
  - internal CLR/F# member `Label`
  - internal CLR/F# member `Doc`
  - internal CLR/F# member `InputBinding`

Use non-null values and the CWL defaults to keep the F# model straightforward:

- `Position = 0`
- `Prefix = ""` means no prefix/positional input
- `Separate = true`

The model preserves effective behavior, not whether a default was explicitly
written. Canonical serialization omits these default-valued binding fields.

Add structural equality, hashing, and create helpers consistent with neighboring domain types.

`CommandInputParameter.Type` must be one normalized `CommandInputType`; do not
store a raw string or an array of type alternatives. This makes it impossible
for a value produced through the supported parsing/persistence paths to contain
zero primitive types, multiple primitive types, or `null` without one primitive
type. Validate that `PrimitiveType` is a defined enum value at construction and
before persistence rather than accepting arbitrary enum backing integers.

Use an EF/C#-friendly representation rather than an F# discriminated union.
Following the existing mutable domain style, a CLIMutable record or property
class is acceptable. The semantic shape is:

```fsharp
type CwlPrimitive =
    | Boolean = 0
    | String = 1
    | Int = 2
    | Long = 3
    | Float = 4
    | Double = 5

type CommandInputType =
    {
        PrimitiveType: CwlPrimitive
        IsNullable: bool
    }
```

The public type name is exactly `CommandInputType`; do not introduce a
`ParsedCwlType` type.

Add dedicated YamlDotNet converters for `CommandInputType`,
`CommandInputBinding`, `CommandInputParameter`, and the parameter array:

- `boolean` parses as `{ PrimitiveType = Boolean; IsNullable = false }`
- `boolean?` parses as `{ PrimitiveType = Boolean; IsNullable = true }`
- serialization emits the corresponding scalar `boolean` or `boolean?`
- malformed suffixes, unknown types, YAML sequences, and YAML mappings fail
  with a targeted diagnostic
- nested parameter/binding mappings use exact CWL lower-camel-case keys
- additional unsupported parameter/binding fields are skipped, including
  nested mapping and sequence values
- writing emits canonical CWL keys and omits default-valued optional fields

Register all three converters on both the frontmatter deserializer and the YAML
serializer. Keep the surrounding AVPR `Inputs` property under the existing
PascalCase naming convention.

Add the equivalent System.Text.Json converter for API request/response JSON.
The normalized CLR object must appear on the public wire as one exact CWL
string, never as `{ "primitiveType": ..., "isNullable": ... }`.

The OpenAPI schema must describe this converter-backed property as a string and
enumerate the twelve supported required/nullable values (`boolean`,
`boolean?`, and so on). A plain unconstrained string or the normalized storage
object schema is not sufficient.

On `ValidationPackageMetadata`:

- remove `CLIArguments`
- add `Inputs : CommandInputParameter[]`, defaulting to an empty array
- include `Inputs` in hashing, equality, and `ValidationPackageMetadata.create`

The AVPR wrapper name is `Inputs`. Nested public serialized names are the exact
CWL names:

- `id`
- `type`
- `label`
- `doc`
- `inputBinding`
- `prefix`
- `position`
- `separate`

Internal PascalCase CLR/F# names are used throughout. YAML converters preserve
the nested lower-camel-case CWL wire names.

### `src/AVPRIndex/Frontmatter.fs`

Parsing accepts uppercase AVPR `Inputs` with exact nested CWL casing. Do not
accept the superseded `CLIArguments` shape.

The registered converters must:

1. If `Inputs` is absent, leave `Inputs = [||]`.
2. Require `Inputs` to be a YAML sequence; CWL's map form is outside this AVPR
   representation.
3. Require each item to be a mapping with `id`, `type`, and `inputBinding`.
4. Read supported `label`, `doc`, `prefix`, `position`, and `separate` values.
5. Ignore all other parameter/binding fields and their complete YAML values.
6. Parse `type` through `CommandInputType`, accepting only the exact scalar
   whitelist with at most one trailing `?`; reject sequence/union and mapping
   forms.
7. Reject malformed known-field values with useful diagnostics.

Keep `.IgnoreUnmatchedProperties()` for unrelated top-level forward
compatibility.

### Release metadata

Because the custom feature has not shipped:

- keep the planned `AVPRIndex` version bump rather than adding another version
- rewrite the new release-note entry to describe the CWL v1.2 scalar `inputs` subset
- do not mention `CLIArguments`, aliases, descriptions, or examples

---

## Layer 2 — Registry service and database

### `src/PackageRegistryService/Models/ValidationPackage.cs`

Replace the unshipped `CLIArguments` property with:

```csharp
public ICollection<AVPRIndex.Domain.CommandInputParameter> Inputs { get; set; } = [];
```

Keep the public wrapper property as `Inputs`, consistently with the service's
existing PascalCase JSON policy. Nested parameter and binding properties must
serialize with their exact lower-camel-case CWL names.

Register the System.Text.Json `CommandInputType` converter for both reads and
writes. Ensure OpenAPI marks `id`, `type`, and `inputBinding` as required and
publishes `type` as a string constrained to the exact supported CWL scalars.
Because the CLR representation is a normalized object, add a focused NSwag
schema processor for `CommandInputType`; do not allow reflection to publish
`PrimitiveType` and `IsNullable` as the API schema.

### `src/PackageRegistryService/Models/ValidationPackageDb.cs`

Replace the `CLIArguments` owned JSON configuration with an `Inputs` owned JSON collection.

Configure both `Type` and `InputBinding` as nested owned JSON objects inside
each input item:

- `Type.PrimitiveType` is persisted as the lowercase CWL name (`boolean`,
  `string`, and so on) through an explicit EF value converter, never as the
  enum's numeric backing value.
- `Type.IsNullable` is persisted as a boolean.
- the owned `Type` value is required.
- use explicit EF JSON property names if needed so the persisted structure is
  stable and matches the documented internal shape below

Verify the generated model represents one `Inputs` JSON document rather than a
relational side table. A representative internal database value is:

```json
[
  {
    "id": "verbose",
    "type": {
      "primitiveType": "boolean",
      "isNullable": true
    },
    "inputBinding": {
      "prefix": "--verbose"
    }
  }
]
```

The database column name may follow the repository's CLR/database convention
(`Inputs`). The normalized nested `type` object is deliberately an internal
storage representation and is not CWL wire syntax. Frontmatter, public JSON,
OpenAPI, and the generated client must still expose `type` as a CWL scalar such
as `boolean?`. Add assertions or integration checks for both representations
so neither serializer accidentally leaks into the other.

### Migration

The `AddCLIArguments` migration is untracked/unpublished and must not survive as historical schema:

1. Remove the unshipped `20260723143326_AddCLIArguments.cs` and `.Designer.cs`.
2. Revert only its generated changes from `ValidationPackageDbModelSnapshot.cs`, preserving unrelated work.
3. Regenerate one migration named `AddCWLInputs` from the final model.
4. Inspect, do not hand-invent, the nested owned-JSON model generated by EF,
   including the required `CommandInputType` ownership and primitive enum
   conversion.
5. Backfill pre-existing package rows with an empty JSON array.
6. Confirm `Down` removes the one new column.

There must not be both `CLIArguments` and `Inputs` columns or a follow-up rename migration in the final branch.

### `src/PackageRegistryService/Data/DataInitializer.cs`

Replace the custom mapping with `Inputs = i.Metadata.Inputs`, null-guarding old index data to an empty collection.

---

## Layer 3 — Website

Replace `PackageCLIArguments.cs` with a component named for the new model, for example `PackageInputs.cs`.

Keep the section omitted when `inputs` is absent/empty. Render only declared CWL data; do not synthesize aliases or examples.

Recommended table:

| Column | Source |
| --- | --- |
| Input | `id` |
| Type | `type` |
| Prefix | `inputBinding.prefix`, or “positional” when absent |
| Documentation | `label` and/or `doc` |

Optionally show non-default binding details (`position`, `separate: false`) in the Prefix/binding cell. HTML-escape every value.

Update:

- `src/PackageRegistryService/Pages/Components/Package.cs`
- both render paths in `src/PackageRegistryService/Pages/Handlers/PackageHandlers.cs`

The user-facing heading may remain **Available Commands**; headings and markup are presentation, not CWL schema fields.

---

## Layer 4 — generated `AVPRClient` and mappings

Regenerate `src/AVPRClient/AVPRClient.cs` from the final service OpenAPI by running the updated service locally. Do not generate against the production endpoint, which will still expose the old schema until deployment.

Expected generated concepts:

- `ValidationPackage.Inputs`
- `CommandInputParameter`
- a generated string enum or otherwise constrained scalar representation for
  public `CommandInputType`
- `CommandInputBinding`
- JSON property annotations using exact CWL names

Update `src/AVPRClient/Extensions.cs` in both directions:

- index metadata → generated client
- generated client → index metadata

Map between the generated client's scalar/enum `CommandInputType` and the
normalized `AVPRIndex.Domain.CommandInputType` in both directions. Do not map
through a string array. Also map the nested `InputBinding` object and preserve
effective CWL defaults. Null-guard:

- the `Inputs` collection
- each string at external boundaries
- the optional `InputBinding` at the API boundary, even though valid frontmatter requires it

Remove every `CLIArgument` mapping helper.

Rewrite the pending AVPRClient release note to describe regenerated CWL input types and round-trip mappings. Keep the already planned version bump if no custom version was published.

---

## Layer 5 — tests and fixtures

Follow the metadata test process documented in `README.md` and `AGENTS.md`.

### Domain tests

In `tests/IndexTests/ReferenceObjects.fs` and `DomainTests.fs`, add mandatory/minimal and all-supported-fields values for:

- `CommandInputType`
- `CommandInputBinding`
- `CommandInputParameter`
- `ValidationPackageMetadata.Inputs`

Test construction, equality, hashing, and wire conversion for all six primitive
types in both required and nullable form. Also test equality/hash-relevant
differences, especially:

- nullable versus required `type`
- different primitive types
- default versus `separate: false`
- default versus non-zero `position`
- empty prefix (positional input) versus a declared prefix

### Frontmatter positive matrix

Replace the custom blocks in all canonical full-frontmatter fixtures:

- F# comment
- F# binding
- Python comment
- Python binding

Use at least:

- one nullable string with a prefix
- one nullable boolean flag
- one numeric input with `position`
- one input with `separate: false`
- one positional input without a prefix

Update full-source strings, extracted YAML strings, parsed metadata reference objects, and all affected hashes/index expectations.

### Converter behavior and negative tests

Add focused failures for:

- `Inputs` is a map rather than the supported array form
- missing `id`
- missing `type`
- missing `inputBinding`
- duplicate IDs
- unknown type and malformed nullable suffix
- long-form union syntax such as `type: ["null", boolean]`
- mapping/object syntax for `type`
- wrong nested casing such as `InputBinding` or `Prefix` when it causes a
  required supported field to be missing
- wrong scalar kinds for `position` and `separate`

Assert actionable diagnostics, not merely that an exception occurred.

Add positive cases proving unsupported fields such as `default`, `valueFrom`,
`itemSeparator`, `shellQuote`, and nested extension values are tolerated and
discarded.

Keep a package with no `Inputs` to prove backward compatibility.

### Client tests

Replace `CLIArgument` reference objects and tests with client/index equivalents for both CWL types.

Cover:

- complete index → client conversion
- complete client → index conversion
- all twelve `CommandInputType` wire values in both mapping directions
- nested binding conversion
- empty/null inputs
- default binding fields
- exact serialized JSON property names
- JSON serialization of normalized `CommandInputType` as a scalar such as
  `"boolean?"`, never as an object or array
- the generated client's exact supported `type` enum/constraint

Rename/replace `allFields_cliargsAddition.fsx` with a CWL-input fixture and recompute its content hash. Keep older no-input fixtures to prove backward compatibility.

### Staging package

Replace the untracked `CLIArguments` block in `StagingArea/test/test@7.0.0.fsx` with the AVPR `Inputs` wrapper and exact nested CWL subset syntax:

```yaml
Inputs:
  - id: test
    type: boolean?
    doc: Enable test mode
    inputBinding:
      prefix: --test
  - id: echo
    type: string?
    doc: Print the supplied text
    inputBinding:
      prefix: --echo
```

The script may continue accepting short aliases internally, but the metadata publishes only the canonical CWL prefixes.

Read the script before running it. The default staging gate must continue to parse and compile-check real packages without executing them.

### Service/manual verification

The API test project is currently a placeholder. Add focused automated tests if a suitable service/component test seam is introduced; otherwise explicitly perform:

- OpenAPI inspection for exact CWL JSON names
- OpenAPI inspection for required fields and the supported `type` values
- generated-client inspection
- an AVPRCI publish dry-run confirming the emitted JSON contains `Inputs` and the nested CWL names
- Postgres migration and `[]` backfill
- persisted nested JSON inspection confirming `CommandInputType` is one
  normalized object with a lowercase string primitive and boolean nullability
- API round-trip inspection confirming that the normalized DB object is exposed
  as a scalar CWL type and accepted back into the same normalized value
- website section present/absent checks
- HTML escaping checks

---

## Layer 6 — documentation

### `README.md`

Remove the custom `CLIArguments`/`CLIArgument` documentation and examples.

Add a section titled along the lines of **CWL command inputs** that:

- states the registry implements the documented scalar subset of CWL v1.2
- links the normative CWL specification
- shows the uppercase AVPR `Inputs` wrapper and explains that its value can be
  copied into lowercase CWL `inputs`
- lists supported fields and types
- explains that `?` is the only supported/documented nullable form, plus boolean
  flags, missing position, and `separate`
- clearly lists unsupported CWL features and states that additional fields are
  ignored and discarded
- states there are no declared aliases
- explains that package scripts remain responsible for parsing the generated argv until downstream execution support lands

Do not describe the entire package frontmatter as a complete CWL document.

### `AGENTS.md`

Extend the cross-cutting metadata guidance with two CWL-specific constraints:

- the AVPR wrapper remains `Inputs`; nested lower-camel-case CWL names and scalar
  `CommandInputType` values must survive frontmatter, public JSON, OpenAPI, and
  client generation; the database alone uses the documented normalized
  `CommandInputType` object
- additional unsupported parameter/binding fields are ignored and discarded,
  while unsupported or malformed `type` shapes still fail conversion

---

## Downstream — `arc-validate`

Implementation remains in the downstream repository, but this registry contract is designed so it can:

1. accept an input object keyed by CWL input `id`
2. validate scalar values against `type`
3. enforce nullability derived from `?`
4. apply CWL boolean/prefix/separate/position behavior
5. create argv without exposing AVPR-only schema semantics

It must not claim support for excluded CWL types or fields.

Package-specific aliases are intentionally unavailable through the structured contract. Direct invocations can still use any aliases implemented by the script itself.

---

## Implementation sequence

1. **Completed:** replace the index domain types, including normalized
   `CommandInputType`.
2. **Completed:** implement and register YamlDotNet converters with focused
   converter tests.
3. **Completed:** convert index fixtures/reference objects/hashes/tests and
   establish positive, permissive-extension, and negative parser coverage.
4. Replace the service model, EF JSON ownership, seeding, and unshipped migration.
5. Update website rendering.
6. Add the converter-aware OpenAPI schema, regenerate the client, and update
   both `CommandInputType` mapping directions.
7. Convert client fixtures/tests and the staging test package.
8. Replace README/release-note documentation.
9. Run focused tests, both solutions, and the manual DB/OpenAPI/page checks.
10. Search the implementation, tests, fixtures, and current documentation for superseded `CLIArguments`, `CLIArgument`, `Flags`, and `PackageCLIArguments` references; only historical/superseded plan text may remain.

Do not regenerate the client or migration until the domain and service wire schema is final; otherwise generated churn will need to be repeated.

---

## Verification

Automated:

```shell
dotnet test tests/IndexTests/IndexTests.fsproj --configuration Release
dotnet test tests/ClientTests/ClientTests.fsproj --configuration Release
dotnet build arc-validate-package-registry.sln --configuration Release
dotnet test arc-validate-package-registry.sln --configuration Release --no-build
dotnet build PackageStagingArea.sln --configuration Release
dotnet test PackageStagingArea.sln --configuration Release --no-build
```

Contract checks:

1. A canonical `Inputs` fixture parses identically from F#/Python comment/binding frontmatter.
2. Every accepted field and type is defined by CWL v1.2 with the same name and semantics.
3. Additional unsupported parameter/binding fields are ignored and discarded;
   malformed known fields and unsupported `type` shapes fail conversion.
4. A package without `Inputs` still parses to an empty collection.
5. API/OpenAPI/client JSON uses exact CWL names.
6. YAML and API JSON expose `CommandInputType` only as one scalar required or
   nullable CWL type; union arrays are rejected.
7. OpenAPI identifies required input fields and enumerates exactly the supported
   `type` strings.
8. Index↔client mappings preserve normalized primitive/nullability and effective
   binding default values.
9. An AVPRCI dry-run emits the AVPR `Inputs` wrapper with nested CWL fields and
   no custom nested fields.
10. The migration creates only the replacement `Inputs` JSON column, backfills
    old rows to `[]`, and persists each type as exactly one normalized
    `CommandInputType` object.
11. Reading that database value through the API emits `type: "boolean?"`, not
    the normalized object or a union array.
12. The website renders declared data and omits the section for empty inputs.
13. The staging package passes the non-executing sanity path.
14. No superseded custom-model references remain outside historical/superseded plan text.

Manual command-line semantics to verify in downstream work:

- `boolean?` true → prefix only
- `boolean?` false/null → nothing
- default `separate` → prefix and value as separate argv elements
- `separate: false` → concatenated prefix/value
- no prefix → positional value
- no position → CWL default position and deterministic ID tie-break
