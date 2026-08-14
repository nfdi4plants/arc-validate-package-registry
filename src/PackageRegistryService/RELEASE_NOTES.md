# AVPR service release notes

All notable changes to the package registry service are documented here. The
service follows semantic versioning independently of the `/api/v1` compatibility
contract.

## 1.2.0 - 2026-08-14 - Lightweight package discovery

Adds content-free package-index, version-list, and exact-version metadata
endpoints for resolver preflight. These queries use no-tracking projections,
do not validate artifact hashes, and do not increment download statistics.
The legacy all-content collection remains compatible but is deprecated.

The service now exposes the immutable v1 frontmatter and validation-package
configuration schemas at their canonical URLs. It also corrects latest-stable
selection to exclude both prerelease and build-qualified versions.

## 1.1.0 - 2026-07-28 - CWL command inputs

Adds a deliberately scoped CWL v1.2 command-input contract throughout the
package registry, giving packages machine-readable input metadata without
introducing an AVPR-specific command-line schema.

### Motivation

Validation packages increasingly need to declare package-specific options that
downstream tools can validate and eventually turn into deterministic command
lines. A custom flags-and-examples model would describe documentation rather
than executable semantics and would require every consumer to learn an
AVPR-only format. CWL already defines names and behavior for command inputs,
including types, nullability, prefixes, positional values, ordering, and value
separation, so AVPR now publishes a compatible fragment of that established
contract.

The compatibility claim is intentionally narrow: an AVPR `Inputs` value uses
CWL's array-form input syntax and can be placed under the lowercase `inputs`
field of a complete CWL `CommandLineTool`, but AVPR is not becoming a general
CWL runner and does not accept every valid CWL input document.

### Why the first release supports a scalar subset

The initial subset covers the package configuration needed by current
validation scripts while keeping validation, persistence, generated clients,
and future argument construction unambiguous:

- Supported types are `boolean`, `int`, `long`, `float`, `double`, and `string`.
  These map directly to portable command-line scalar values without requiring
  file staging or compound-value encoding.
- Appending one `?` is the sole nullable representation. CWL also permits union
  arrays such as `["null", "string"]`, but accepting both shapes would expose
  two public representations for the same meaning and complicate OpenAPI,
  generated clients, and normalized storage.
- `prefix`, `position`, and `separate` provide the essential CWL binding
  behavior for value-less boolean flags, prefixed options, concatenated
  options, and positional values.
- CWL's array form was selected instead of its map form because it preserves an
  explicit `id`, maps directly to the registry's ordered collection and owned
  JSON model, and supports deterministic binding order.
- Requiredness comes from the type's nullability instead of a custom
  `Required` field. Likewise, CWL defines one canonical `prefix`, so the
  contract does not add an AVPR-specific aliases field.

`File`, `Directory`, `stdin`, arrays, records, enums, user-defined types, and
general unions remain unsupported because they require additional staging,
path, validation, or serialization semantics that the registry and downstream
runner do not yet implement. Additional unknown parameter or binding fields
are tolerated and discarded for forward compatibility, but malformed known
fields and unsupported `type` values or shapes are rejected rather than being
published with unclear semantics.

### Public contract and compatibility

- The surrounding metadata property remains PascalCase `Inputs`, consistent
  with existing AVPR frontmatter and API metadata.
- Nested fields preserve the CWL lower-camel-case names `id`, `type`, `label`,
  `doc`, `inputBinding`, `prefix`, `position`, and `separate` in frontmatter,
  public JSON, OpenAPI, and the generated client.
- Public YAML and JSON expose each type as one CWL scalar such as `boolean?`.
  The database alone uses a normalized primitive/nullability object; that
  storage representation never leaks through the API.
- Packages without `Inputs` continue to parse and serialize with an empty
  collection, preserving compatibility with previously published packages.
- Declaring inputs publishes their contract but does not install an argument
  parser in the package script. Scripts remain responsible for interpreting
  their arguments until downstream execution support is implemented.

### Added

- Parse and validate the supported input subset from F# and Python YAML
  frontmatter.
- Expose the scalar CWL representation through the existing v1 API and its
  OpenAPI document.
- Regenerate the .NET client and add bidirectional mappings for inputs, types,
  and bindings.
- Persist inputs as owned JSON in PostgreSQL with an empty-array backfill for
  existing packages.
- Render declared command inputs and their effective binding behavior on
  package pages.
- Split contributor and operator guidance into GitHub-readable Markdown pages
  and render those same sources on the service at `/docs`.
- Cover required and nullable primitives, binding defaults, ignored extension
  fields, invalid shapes, public JSON, normalized storage, client conversion,
  and website rendering with automated tests.
- Report the exact running service revision and release channel.
- Initialize and seed a schema-less database on `dev`-channel deployments while
  leaving existing schemas and every other deployment channel untouched.

### Database

- Apply migration `20260724094518_AddCWLInputs` before starting this image.

### Documentation

The rationale, supported-field tables, and frontmatter example are maintained
in the [CWL command-input documentation](../../docs/packages/cwl-inputs.md).
The deployed response schema and example values are available through the
[Swagger UI](https://avpr.nfdi4plants.org/swagger), avoiding a second copy of those contract examples in
the release history.
