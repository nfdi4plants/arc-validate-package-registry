# Plan: Safe, configurable validation-package execution

## Status

**Accepted in design on 2026-08-13, including the machine-readable schema
revision. As of 2026-08-20, Steps 1–4 are DONE and their issues are closed.
Step 5 is CURRENT/NEXT and has not been implemented. Steps 6–7 have not
started.**

This plan records implementation state but does not itself authorize package
publication, service deployment, issue closure, or DataHUB changes. Begin each
remaining step only through its linked issue and after its predecessor
acceptance gate is complete.

GitHub tracking:

- [EPIC — AVPR #119](https://github.com/nfdi4plants/arc-validate-package-registry/issues/119)
- [Step 1 — AVPR #120](https://github.com/nfdi4plants/arc-validate-package-registry/issues/120)
- [Step 2 — AVPR #121](https://github.com/nfdi4plants/arc-validate-package-registry/issues/121)
- [Step 3 — arc-validate #253](https://github.com/nfdi4plants/arc-validate/issues/253)
- [Step 4 — arc-validate #254](https://github.com/nfdi4plants/arc-validate/issues/254)
- [Step 5 — ARC-specification #183](https://github.com/nfdi4plants/ARC-specification/issues/183)
- [Step 6 — DataHUB #73](https://github.com/nfdi4plants/DataHUB/issues/73)
- [Step 7 — AVPR #122](https://github.com/nfdi4plants/arc-validate-package-registry/issues/122)

Current implementation state:

| Step | State on 2026-08-20 | Evidence and remaining gate |
| --- | --- | --- |
| Step 1 — AVPR #120 | **DONE — issue closed** | Commit `c0b9ccb` implements the portable contracts, schemas, compatibility readers, cross-target tests, and package checks. Model and Codecs preview.4 are indexed on NuGet/npm and as PyPI `0.1.0a4`; final release runs were [Model 32394298070](https://github.com/nfdi4plants/arc-validate-package-registry/actions/runs/32394298070) and [Codecs 32394301300](https://github.com/nfdi4plants/arc-validate-package-registry/actions/runs/32394301300). |
| Step 2 — AVPR #121 | **DONE — issue closed** | Commit `5d3ff2d` implements the service/client/Interop/AVPRCI work; `2da28f0` corrected the PyPI publisher. Client `0.3.0-preview.4` and Interop `0.1.0-preview.4` were published by [32393190585](https://github.com/nfdi4plants/arc-validate-package-registry/actions/runs/32393190585) and [32393194304](https://github.com/nfdi4plants/arc-validate-package-registry/actions/runs/32393194304). An isolated PostgreSQL 16 migration/backfill/storage/round-trip gate and local service-image build passed. Every AVPR-dev deployment/live check is **SCRATCHED by user instruction — not passed**. |
| Step 3 — arc-validate #253 | **DONE — issue closed** | Commit `3cb0aa4` implements exact/patch/minor/legacy resolution, bounded metadata preflight, strict `validation_plan.json`, its schema, and stable exit codes. `eded32a` has green [build/test 32397177695](https://github.com/nfdi4plants/arc-validate/actions/runs/32397177695), [documentation 32397177573](https://github.com/nfdi4plants/arc-validate/actions/runs/32397177573), and [container 32397177548](https://github.com/nfdi4plants/arc-validate/actions/runs/32397177548) gates. The AVPR-dev integration check is **SCRATCHED by user instruction — not passed**. |
| Step 4 — arc-validate #254 | **DONE — issue closed** | Commit `0d25a4b` implements safe configured child execution and ARCExpect alignment; `eded32a` fixes CLI publication and produced `ghcr.io/nfdi4plants/arc-validate:sha-eded32a` at immutable digest `sha256:8f14e791723dfa187d66f93574b536318143967171f0d6f3afe553e9d1b9d665`. `ce5f9d2` fixes the Linux packed-wheel smoke. [Release run 32399194175, attempt 2](https://github.com/nfdi4plants/arc-validate/actions/runs/32399194175/attempts/2) reused the verified artifacts and published ARCExpect `7.0.0-preview.4` to NuGet/npm and `7.0.0a4` to PyPI; only the previously failed NuGet job was rerun after `Mutagene` became a package owner. Configured execution against AVPR dev is **SCRATCHED by user instruction — not passed**. |
| Step 5 — ARC-specification #183 | **CURRENT / NEXT — NOT IMPLEMENTED** | The issue remains open. The normative specification update is now the active implementation step. |
| Step 6 — DataHUB #73 | **NOT STARTED** | The issue remains open and blocked by Step 5. No DataHUB template or pipeline change has been made. |
| Step 7 — AVPR #122 | **NOT STARTED** | The production rollout issue remains open and waits for Steps 5–6. |

The AVPR preview releases were orchestrated through
[release-all 32394250238](https://github.com/nfdi4plants/arc-validate-package-registry/actions/runs/32394250238).
The focused PostgreSQL fixture/container cleanup completed, and local AVPR
image `avpr-local-check:2da28f06539c` built successfully with image ID
`sha256:ae00bbdb877519a14170da6501a961f88dd65e4a53f5258dd34a5537d289b51f`;
it was not pushed. Every AVPR-dev deployment, route verification, or live
integration check in this plan is **SCRATCHED by explicit user instruction
because `avpr-dev.nfdi4plants.org` is down — it was not attempted and did not
pass**. This status update does not mark those checks successful.

The design extends `.arc/validation_packages.yml` with typed package input
values and replaces shell/YAML processing in DataHUB CI with an explicit,
validated resolution and execution contract. It deliberately follows the CWL
separation between input declarations and a job object containing input
values, while supporting only the scalar functionality required by validation
packages.

Normative and related references:

- ARC specification `validation_packages.yml` section:
  <https://github.com/nfdi4plants/ARC-specification/blob/release/ARC%20specification.md#the-validation_packagesyml-file>
- CWL command-line input declarations:
  <https://www.commonwl.org/v1.2/CommandLineTool.html>
- CWL job/input values:
  <https://www.commonwl.org/user_guide/en/topics/inputs.html>
- ARCtrl ownership follow-up:
  <https://github.com/nfdi4plants/ARCtrl/issues/634>

---

## 1. Preamble: moving parts and ownership boundaries

### ARC specification repository

`nfdi4plants/ARC-specification` is the normative owner of the
`.arc/validation_packages.yml` wire format. It defines what an ARC author may
write and the meaning of package selection, version policies, and configured
input values. The specification should describe the contract, but should not
contain the parser or execution implementation.

The existing prose has an inconsistency between `specification` and the
implemented/example key `arc_specification`. The canonical key remains
`arc_specification` and the prose must be corrected as part of this work.

### `arc-validate-package-registry` (AVPR)

AVPR owns the contracts shared by package authors, the registry service, and
`arc-validate`:

- `ValidationPackage.Model` owns portable validation-package metadata, CWL
  input declarations, semantic versions, and the new portable configuration
  value model.
- `ValidationPackage.Codecs` owns the YAMLicious parser and writer for the
  configuration format and the machine-readable JSON Schemas for both YAML
  contracts. It performs no file I/O.
- `PackageRegistryService` owns persistence and HTTP endpoints for discovering
  available package identities and retrieving metadata without downloading
  package script content.
- `AVPRClient` remains generated-only, while `AVPRClient.Interop` maps generated
  DTOs to the portable model.
- `AVPRCI` owns package publication and must use lightweight discovery rather
  than downloading every package artifact.

The CWL-style `Inputs` declaration in package metadata and the CWL-job-style
`inputs` values in `validation_packages.yml` are different halves of one
contract. Both portable halves therefore belong in AVPR Model/Codecs.

### `arc-validate`

The `arc-validate` repository owns application behavior:

- registry queries and version-policy resolution;
- reading configuration files from disk and verifying their digest;
- producing a stable JSON execution plan for CI orchestration;
- owning the execution plan's JSON Schema and canonical persisted filename;
- selecting an exact installed package version from the local cache;
- validating configured values against that package version's declarations;
- converting values to process argument-list elements without a shell; and
- executing the selected package.

ARCExpect remains the polyglot package-side API. Its `PackageArguments` parser
must understand the same narrowed declaration semantics so F#, JavaScript, and
Python packages observe the same values. The existing manual raw-argument mode
after the `--` boundary remains supported.

### DataHUB CI scripts

The DataHUB runner/template scripts own orchestration, not configuration
semantics. Today they read `validation_packages.yml` with `yq` and generate one
child job per package. The target flow preserves that parent/child structure,
but the scripts consume only a versioned JSON plan produced by `arc-validate`.
They never parse package input values or construct their argv.

The exact DataHUB CI repository is not present in this workspace and was not
identified in the public repositories inspected for this plan. Its repository
URL and script paths must be attached to the DataHUB subissue before that issue
is started; this is tracking information, not an unresolved design choice.

### ARCtrl

ARCtrl currently exposes a validation-packages model and YAML implementation.
That ownership moves conceptually to AVPR Model/Codecs as described in ARCtrl
#634. ARCtrl migration is intentionally deferred until the new AVPR artifacts,
registry API, CLI, and DataHUB flow have shipped. No ARCtrl compatibility layer
or dual-write path is part of this plan.

### End-to-end target flow

1. An ARC author writes package selections and typed values in
   `.arc/validation_packages.yml`.
2. The DataHUB parent job calls `arc-validate config resolve` once.
3. `arc-validate` safely parses the YAML, queries lightweight AVPR endpoints,
   resolves exact versions, validates every value, and writes the JSON plan
   persisted by CI as `validation_plan.json`.
4. DataHUB creates one child job per plan entry and passes only package name,
   exact resolved version, config path, and config digest.
5. The child installs that exact version and invokes `arc-validate validate`.
6. `validate` loads the exact cached package metadata, rechecks the unchanged
   config, materializes argv, and starts the package with `ArgumentList`.
7. ARCExpect parses the resulting standard and package-defined arguments using
   the same portable declarations.

---

## 2. Proposed changes and owners

| Change | Owner |
| --- | --- |
| Normative `.arc/validation_packages.yml` schema and semantics | `ARC-specification` |
| Portable config types, SemVer ordering, declaration validation, YAML parser/writer | AVPR Model/Codecs |
| JSON Schema for extracted validation-package YAML frontmatter | AVPR Codecs |
| JSON Schema for `.arc/validation_packages.yml` | AVPR Codecs; referenced normatively by `ARC-specification` |
| Lightweight identity/version/metadata HTTP endpoints | AVPR registry service |
| Generated HTTP surface and portable mappings | AVPRClient + AVPRClient.Interop |
| Version resolution, preflight, JSON plan, digest verification, config-to-argv | `arc-validate` CLI |
| JSON Schema and canonical filename `validation_plan.json` for the execution plan | `arc-validate` CLI |
| Package-side typed argument parsing on .NET/JavaScript/Python | ARCExpect |
| Parent/child job orchestration using the JSON plan | DataHUB CI scripts |
| Removal/migration of the old public ARCtrl implementation | Deferred to ARCtrl #634 |

The old heavy `GET /api/v1/packages` endpoint remains temporarily available and
is marked deprecated. The new resolver requires the new lightweight API and
does not silently fall back to downloading every package.

---

## 3. Implementation plan

### Step 1 — Finalize the portable declaration and configuration contracts in AVPR

Status: **DONE**

Tracking issue: [AVPR #120](https://github.com/nfdi4plants/arc-validate-package-registry/issues/120)

Implement this step in `arc-validate-package-registry`, primarily in
`ValidationPackage.Model`, `ValidationPackage.Codecs`, and their shared
cross-target contract suites.

#### 1.1 Narrow package input declarations

The current CWL-inspired metadata is unreleased/dev-only, so change it directly
without a compatibility or deprecation representation.

Keep this exact supported declaration shape:

```yaml
$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Inputs:
  - id: strict
    type: boolean
    label: Strict validation
    doc: Fail on every non-conforming value
    inputBinding:
      prefix: --strict
      position: 10
```

Rules:

- A parameter accepts exactly `id`, `type`, optional `label`, optional `doc`,
  and required `inputBinding`. Unknown fields fail.
- A binding accepts required `prefix` and optional integer `position`. Unknown
  fields fail.
- Remove `separate` from every portable type, codec, persistence mapping,
  OpenAPI/client shape, interop mapping, fixture, and documentation location.
- Remove positional inputs. `prefix` must be non-empty and non-whitespace.
- Retain only `boolean`, `int`, `long`, `float`, `double`, and `string`, plus one
  trailing `?` for nullable values.
- IDs are non-empty, exact, case-sensitive, and unique within the package. No
  additional ID grammar is imposed.
- Prefixes are exact and unique. Reject `--` and collisions with the standard
  package-process arguments `-i`, `-o`, `--source-branch`, and
  `--source-commit-hash`. A leading `-` is recommended but not required.
- Missing `position` means `0`. Materialization order is ascending position,
  then input ID using ordinal comparison.
- Non-boolean values always emit two argv elements: prefix, then value.
- Boolean `true` emits the prefix; `false` and nullable `null` emit nothing.

Put the structural and collision checks in one portable declaration validator.
Use it at metadata decoding/publication, registry mapping/persistence,
resolver preflight, child materialization, and ARCExpect parsing instead of
reimplementing rules at those boundaries.

#### 1.2 Add the canonical configuration model

Add Fable-friendly public types with intentional JavaScript/Python APIs:

- `ValidationPackagesConfig`
- `ValidationPackageSelection`
- `RollForwardPolicy` with `Disable`, `LatestPatch`, and `LatestMinor`
- `ValidationPackageInput`
- `ValidationPackageInputValue`
- `ValidationPackageInputValueKind`

Use `[<AttachMembers>]` public classes, explicit mutable backing fields, ordered
arrays, and class-owned factories/methods. Do not expose F# maps or unions as
the native public representation. Preserve source order for package selections
and input entries.

The canonical YAML shape is:

```yaml
$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json"
arc_specification: 3.0.0-draft.2
validation_packages:
  - name: configurable-validation
    version: 1.2.3
    roll_forward: latest_patch
    inputs:
      strict: true
      minimum-files: 2
      report-title: "release candidate"
```

Canonical model rules:

- `$schema` is required in newly written canonical documents and must exactly
  match the supported v1 configuration-schema URI. It is codec envelope
  metadata used for decoder dispatch, not a mutable domain-model property.
- `validation_packages` is required but may be an empty sequence.
- Package names are non-empty and unique within the file.
- `version` is required and is a full semantic version.
- `roll_forward` accepts `disable`, `latest_patch`, or `latest_minor`; omission
  means `disable`.
- `inputs` is optional and is a mapping from declared input ID to scalar value.
- `arc_specification` is optional. When present, parse it as a semantic version;
  preserve its canonical string for writing and plan output.
- Reject unknown root fields, selection fields, and duplicate mapping keys.
- Preserve explicit input `null`; distinguish it from an omitted input.
- The writer emits `$schema` as the first root key, omits
  `roll_forward: disable`, empty `inputs`, and an absent `arc_specification`,
  but never omits canonical package `version`.

#### 1.3 Implement the strict YAMLicious scalar profile

Use YAMLicious, consistent with the other portable YAML implementations in the
workspace. No second YAML parser is introduced.

Accept only a JSON-compatible YAML scalar subset for input values:

- quoted YAML scalar -> string;
- lowercase `null`, `true`, and `false`;
- JSON-form decimal integer;
- finite JSON-form floating-point number, including exponent notation; and
- ordinary quoted strings, including strings that resemble booleans/numbers.

Reject YAML conveniences and unsafe/ambiguous constructs:

- `yes`/`no`, `on`/`off`, `~`, and empty implicit null;
- hexadecimal, octal, numeric separators, and leading-zero integers;
- `.nan`, `.inf`, and every non-finite numeric value;
- sequences or mappings as input values;
- aliases, anchors, explicit tags, merge keys, multiple documents, and
  duplicate keys.

Detect forbidden syntax before it can be normalized away by an object mapper.
Retain a validated numeric lexeme in the portable value representation so a
JavaScript round trip cannot lose a valid 64-bit integer. The writer emits
invariant JSON-compatible numeric text and always quotes string input values.

#### 1.4 Define value/declaration compatibility

The selected package version's metadata is authoritative:

- `int` accepts only an integer fitting signed 32-bit range.
- `long` accepts only an integer fitting signed 64-bit range.
- `float` accepts an integer or floating scalar representable as a finite
  IEEE-754 single-precision value.
- `double` accepts an integer or floating scalar representable as a finite
  IEEE-754 double-precision value.
- `string` accepts only a YAML string, and `boolean` only a YAML boolean.
- Integer-to-floating widening is allowed. Floating-to-integer and all
  string/boolean coercion are forbidden.
- Every non-nullable input, including boolean, must be explicitly present in a
  config file. A required `false` is valid and emits no argv token.
- A nullable input may be omitted or explicitly set to `null`.
- Unknown input IDs, missing required IDs, type/range failures, and invalid
  declarations fail with a path-oriented diagnostic.

Keep config-value validation and argv materialization as pure portable model
behavior where possible. The model produces ordered logical tokens; only the
CLI performs process execution.

#### 1.5 Add machine-readable schemas for both YAML contracts

Commit two standalone [JSON Schema Draft
2020-12](https://json-schema.org/draft/2020-12) documents under AVPR's
top-level `schemas/` directory:

```text
schemas/validation-package-frontmatter.schema.json
schemas/validation-packages.schema.json
```

JSON Schema is used for both because editor/YAML integrations validate the
JSON-compatible data model produced from YAML. These schemas complement rather
than replace YAMLicious. JSON Schema cannot detect YAML representation details
such as duplicate keys, anchors, aliases, explicit tags, multiple documents,
or whether a numeric-looking string was quoted; the strict codec remains
authoritative for those checks.

Both schemas must:

- declare `"$schema": "https://json-schema.org/draft/2020-12/schema"`;
- have immutable versioned `$id` values beneath
  `https://avpr.nfdi4plants.org/schemas/v1/`;
- be self-contained, using local `$defs` rather than network `$ref` values;
- carry titles, descriptions, examples, defaults where behavior has a default,
  and deprecation annotations only for genuinely deprecated fields;
- use the same tested SemVer pattern and CWL scalar enumeration;
- constrain required fields, property names, scalar types, arrays, and known
  object shapes as far as Draft 2020-12 permits; and
- use `$comment` to identify invariants that require executable validation,
  including package-name uniqueness, declaration-driven input types/ranges,
  prefix collisions, and the stricter YAML lexical profile.

In addition to the schema document's own `$schema` keyword declaring the
Draft 2020-12 dialect, each schema defines a required **instance property**
named `$schema`, constrained with `const` to that document's `$id`. The shared
spelling is intentional: YAML Language Server recognizes it in YAML documents
and common JSON editors recognize it in JSON documents, while
AVPR/arc-validate also define it as their explicit wire-version discriminator.
An embedded F#/Python frontmatter block is not automatically a standalone YAML
editor document; there the property still supplies the version/link and can be
used by extraction-aware tooling.

`validation-package-frontmatter.schema.json` describes the extracted YAML
metadata mapping, not the surrounding F#/Python comment or binding syntax. It
requires its `$schema` instance property and contains `$defs` for authors,
ontology annotations, command input parameters, bindings, and semantic-version
components. It follows the documented publication requiredness and closes the
root and nested objects with `additionalProperties: false`; forward
compatibility comes from explicit schema-directed decoders, not silently
discarding unknown fields in a versioned document.

`validation-packages.schema.json` describes the canonical configuration only:
required constant `$schema`, optional `arc_specification`, required
`validation_packages`, required package name/version, the roll-forward enum,
and scalar/null values under `inputs`. It does not describe the read-only legacy
form. Dynamic validation against a selected package's declarations remains
executable behavior rather than being pretended into a static schema.

Treat these committed files as reviewed contract artifacts, not reflection or
OpenAPI output. Package both files unchanged in the NuGet, npm, and PyPI
`ValidationPackage.Codecs` artifacts under `schemas/`, and document their
package-relative locations. Add a non-portable test-only Draft 2020-12
validator at the test boundary; do not add a JSON Schema runtime dependency to
portable Model or Codecs.

#### 1.6 Add semantic-version ordering and schema-directed legacy reading

Extend portable `SemVer` with:

- SemVer 2.0 precedence comparison, ignoring build metadata for precedence;
- a deterministic total identity comparison using precedence followed by the
  canonical version text when build metadata differs; and
- cross-target tests for numeric/alphanumeric prerelease ordering and builds.

Decode both YAML formats by inspecting the root `$schema` scalar before domain
decoding:

- An exact recognized URI selects its corresponding strict decoder.
- An absent `$schema` selects the schema-less legacy decoder.
- A present but unknown/non-string `$schema` fails as `UnsupportedSchema`; it
  never falls back and is never fetched from the network.
- Keep the recognized URI table in portable Codecs and dispatch entirely
  offline. A schema URI is an identifier, not permission for HTTP access.
- Canonical writers always emit the newest URI they implement. Schema URI is
  excluded from metadata/config equality and hashing because it selects the
  wire decoder rather than changing package semantics.
- Once a schema URI ships, retain its decoder for the documented compatibility
  window. A future incompatible shape gets a new immutable schema URI instead
  of mutating the v1 schema.

For package frontmatter, the schema-less legacy decoder preserves the existing
metadata behavior needed by immutable already-published packages; those source
files are not rewritten merely to add `$schema`. Because configurable `Inputs`
are unreleased, a schema-less frontmatter document cannot declare them:
frontmatter using the narrowed inputs must carry the v1 URI and pass its strict
decoder. The canonical frontmatter writer emits `$schema` as its first extracted
YAML key, but the codec strips the discriminator before constructing metadata,
API DTOs, persistence values, equality, or metadata semantic hashes. As usual,
the raw package-content hash still reflects every source byte.

For validation configuration, add read-only legacy types named exactly
`LegacyValidationPackagesConfig` and `LegacyValidationPackageSelection`. A
schema-less legacy root accepts optional `arc_specification` and
`validation_packages`; each selection accepts `name` and optional `version`,
but not `roll_forward` or `inputs`, and unknown fields fail. An entry with a
version means exact/disable and an entry without one means highest stable across
all majors. Do not add a legacy encoder. The canonical v1 decoder never accepts
a missing package version. Legacy execution semantics are defined in Step 3.

#### Validation — Step 1 acceptance gate

- Model and Codecs focused tests pass on .NET.
- The same portable contract suites pass after transpilation on Node and
  Python, including public API-shape inspection.
- Positive round trips cover all scalar kinds, explicit null, int64 limits,
  quoting, package/input order, and writer omission defaults.
- Negative fixtures cover every forbidden YAML feature, duplicate/unknown
  fields, duplicate package/input IDs, invalid declarations, type/range errors,
  and missing required values.
- Declaration tests prove `separate` and positional bindings are rejected and
  prefixes cannot shadow standard arguments.
- Legacy fixtures are accepted only by the explicit legacy decoder and can
  never be written by the canonical writer.
- Dispatch tests cover recognized, absent, malformed, and unknown `$schema`
  values; unknown URIs perform no HTTP/DNS access and cannot fall back.
- New canonical frontmatter/config writers emit the correct `$schema` first;
  immutable schema-less historical frontmatter remains decodable and its model
  equality/hash is unaffected by the envelope discriminator.
- Both schemas validate against the Draft 2020-12 meta-schema. Canonical
  frontmatter/config fixtures pass their corresponding schema and codec;
  structurally invalid fixtures fail both where the schema can express the
  rule.
- Schema contract tests assert exact `$id`, required fields, enums, SemVer
  constraints, `additionalProperties` policy, and schema/codec fixture parity.
- NuGet, npm, and wheel package-content tests find byte-identical schema files
  at the documented `schemas/` paths.
- `PackModel`, `PackCodecs`, `TestPortableModel`, `TestPortableCodecs`, and
  `TestNativeValidationPackages` pass without generated files entering Git.

---

### Step 2 — Add efficient AVPR discovery/metadata APIs and release the shared artifacts

Status: **DONE**

Tracking issue: [AVPR #121](https://github.com/nfdi4plants/arc-validate-package-registry/issues/121)

Implement this step in the AVPR registry service, generated client, interop,
AVPRCI, persistence model, website where applicable, and API/client tests.

#### 2.1 Add additive v1 endpoints

Retain `GET /api/v1/packages` for compatibility, document it as deprecated in
OpenAPI, and do not change its response shape in place.

Add:

```text
GET /api/v1/package-index
GET /api/v1/packages/{name}/versions
GET /api/v1/packages/{name}/{version}/metadata
GET /schemas/v1/validation-package-frontmatter.schema.json
GET /schemas/v1/validation-packages.schema.json
```

Contracts:

- `package-index` returns a JSON array of `{ "Name": string, "Version":
  string }`, ordered by name ascending and then semantic version descending
  using the deterministic total comparison.
- `versions` returns all canonical full-SemVer strings for the requested name,
  in descending order.
- `metadata` returns the full metadata needed for preflight without package
  content: `Name`, canonical `Version`, `Summary`, `Description`, `ReleaseDate`,
  `Tags`, `ReleaseNotes`, `CQCHookEndpoint`, `Authors`,
  `ProgrammingLanguage`, and `Inputs`.
- Keep the established PascalCase v1 JSON convention. New endpoint identity
  versions are canonical strings even though internal service/portable models
  may retain split SemVer fields.
- Unknown names/versions return the service's normal typed 404 response.
- The schema routes return the exact committed Step 1 files as
  `application/schema+json`; their URLs equal the documents' `$id` values.
  They are documentation/tooling resources, not generated OpenAPI components.

Use no-tracking database projections that never load package script bytes.
Only artifact/content endpoints validate content hashes and increment download
statistics. Index, version, and metadata requests do neither.

Fix the existing latest-stable query so it checks both prerelease and build
suffixes rather than checking the build suffix twice.

Pagination, ETags, and additional cache headers are explicitly deferred.

#### 2.2 Synchronize service storage and public contracts

Apply the narrowed declaration model from Step 1 throughout the service:

- remove persisted/public `separate` and positional-prefix behavior;
- make binding prefix required;
- apply centralized declaration validation before accepting or serving
  metadata;
- update the current unreleased migration/model snapshot directly rather than
  layering a compatibility migration for the dev-only shape; and
- confirm no unintended unrelated EF changes are generated.

The existing service-owned persistence model remains the database boundary.
The portable model is not made into an EF entity.

Update AVPR package-author documentation to link the versioned frontmatter
schema route and show editor association for the extracted YAML mapping. Make
clear that the schema validates metadata YAML only; language-specific
frontmatter extraction and package publication checks remain separate stages.

#### 2.3 Regenerate and map the client

Regenerate `AVPRClient` from the local final OpenAPI document. Do not hand-edit
domain helpers into generated code and do not generate against production.

Update `AVPRClient.Interop` to map:

- package-index identity DTOs to portable `ValidationPackageIdentity`;
- canonical version strings through portable `SemVer` parsing;
- metadata endpoint DTOs to portable `ValidationPackageMetadata`; and
- narrowed input declarations in both required directions.

Update AVPRCI and existing integration consumers to use the lightweight index
instead of the deprecated all-content collection endpoint.

#### 2.4 Produce the integration artifacts needed downstream

After all AVPR gates pass:

1. publish coordinated preview versions of Model and Codecs for NuGet, npm,
   and PyPI;
2. publish preview versions of AVPRClient and AVPRClient.Interop;
3. **SCRATCHED by user instruction — not passed:** deploy the compatible
   registry service to the AVPR dev environment; and
4. record the exact artifact versions. Recording a deployed dev API
   image/version is likewise **SCRATCHED by user instruction — not passed**.

Release Model before Codecs; release generated Client before Interop. The
former AVPR-dev reachability/deployment sequencing check is **SCRATCHED by
user instruction — not passed**. Production ordering remains part of Step 7.

#### Validation — Step 2 acceptance gate

- Raw API tests assert exact routes, PascalCase JSON, canonical version strings,
  deterministic ordering, 404 behavior, and the absence of script content.
- Schema route tests assert content type, `$id`, Draft 2020-12 validity, and
  byte-for-byte equality with the committed/package-shipped schema artifacts.
- Generated-client tests call the in-process registry host and round-trip every
  new DTO through Interop.
- Query tests prove index/versions/metadata requests do not load content,
  validate hashes, or increment download counts; artifact downloads still do.
- Stable-version tests cover prerelease and build exclusion and reproduce the
  corrected latest query.
- EF model assertions pass, the migration/snapshot diff contains only intended
  declaration changes, and a focused PostgreSQL check validates JSON storage.
- AVPRCI publication discovery works through `package-index` without fetching
  every package.
- `TestSolution`, portable targets, client packs, release-metadata validation,
  and a service container build pass.
- Published previews are indexed on every required package registry.
- **SCRATCHED by user instruction — not passed:** verify that the deployed
  AVPR-dev service serves the three new API endpoints and two schema routes.

---

### Step 3 — Implement configuration resolution and the versioned execution plan in `arc-validate`

Status: **DONE**

Tracking issue: [arc-validate #253](https://github.com/nfdi4plants/arc-validate/issues/253)

Implement this step in the `arc-validate` CLI and its internal package
management/configuration modules. Pin the exact AVPR preview artifacts produced
by Step 2; do not duplicate portable config or metadata types locally.

#### 3.1 Implement version-policy resolution

Canonical selection rules:

- Omitted `roll_forward`/`disable`: the exact full identity must exist.
- `latest_patch`: select the highest suffix-free version with the same major
  and minor as the requested floor and with precedence greater than or equal
  to it.
- `latest_minor`: select the highest suffix-free version with the same major
  as the requested floor and with precedence greater than or equal to it.
- Rolling never crosses a major version.
- The requested floor does not itself need to exist for a rolling policy.
- A requested version containing prerelease or build metadata is valid only
  with `disable`.
- Rolling candidates contain neither prerelease nor build metadata.
- Legacy name-only entries resolve to the highest stable suffix-free version
  across all majors. A document decoded through the legacy path emits one
  migration warning to stderr; a versioned entry within that document keeps
  exact/disable semantics.

Fetch `package-index` once, resolve all selections locally, then retrieve exact
metadata for the selected identities with bounded parallelism (maximum four
in-flight requests). Validate the complete configuration, all selected
declarations, and all input values before emitting any plan. The registry is
authoritative during this parent preflight; the local cache is not.

A missing eligible version is a configuration error. Network failures,
unsupported/404 discovery endpoints, or invalid registry responses are
registry errors. Do not fall back to deprecated `GET /api/v1/packages`.

#### 3.2 Add the resolver command

Add:

```bash
arc-validate config resolve \
  --validation-config .arc/validation_packages.yml \
  > validation_plan.json
```

Behavior:

- Read exact bytes as strict UTF-8. Accept a leading UTF-8 BOM, strip it for
  parsing, and include it in the digest because the digest covers file bytes.
- Calculate lowercase SHA-256 over the exact file bytes.
- Dispatch through the offline `$schema` rules from Step 1: recognized v1 is
  canonical, absence is legacy, and an unknown URI is an unsupported-schema
  configuration error with no fallback or network fetch.
- Write only one complete JSON document to stdout. Diagnostics, deprecation
  warnings, and verbose logging go to stderr.
- Emit no partial JSON if any selection or input fails preflight.
- JSON is the only initial output format; do not add a format switch.
- An explicitly supplied missing/unreadable file is a configuration error.
- Whenever stdout is persisted, the canonical filename is exactly
  `validation_plan.json`. DataHUB uses that name for the generated artifact;
  alternate filenames are not part of the documented interchange contract.

The CLI owns this internal JSON contract, its strict encoder/decoder, and
contract tests. It is not added to AVPR's public portable packages.

Canonical v1 plan:

```json
{
  "$schema": "https://nfdi4plants.github.io/arc-validate/schemas/v1/validation_plan.schema.json",
  "config_sha256": "<lowercase-sha256>",
  "arc_specification": "3.0.0-draft.2",
  "validation_packages": [
    {
      "name": "configurable-validation",
      "requested_version": "1.2.3",
      "roll_forward": "latest_patch",
      "resolved_version": "1.2.7"
    }
  ]
}
```

Rules:

- Preserve source package order. This does not imply execution order because
  DataHUB child jobs may run concurrently.
- Omit `arc_specification` when it was absent.
- Keep input values and generated argv out of the plan; CI must never quote or
  reconstruct them.
- For a legacy name-only entry, emit `requested_version: null` and
  `roll_forward: "legacy_latest_stable"`. For a versioned entry in the same
  legacy document, emit its canonical requested version and `disable`.
- An empty canonical package list emits a valid plan with an empty array.

#### 3.3 Add the execution-plan JSON Schema

Commit the CLI-owned standalone Draft 2020-12 schema as:

```text
schemas/validation_plan.schema.json
```

It has the immutable `$id`
`https://nfdi4plants.github.io/arc-validate/schemas/v1/validation_plan.schema.json`
and describes exactly plan schema v1:

- a closed root object with required `$schema` fixed to the schema's `$id`;
- a lowercase 64-character `config_sha256`;
- optional SemVer `arc_specification`;
- required ordered `validation_packages` array;
- closed selection objects with name, nullable requested version,
  roll-forward enum, and resolved full SemVer; and
- conditional rules requiring `requested_version: null` exactly for
  `legacy_latest_stable`, and a SemVer string for all canonical policies.

Use local `$defs`, the same tested SemVer pattern used by the AVPR schemas, and
`additionalProperties: false` on the root and selection objects. Document in a
`$comment` that uniqueness of package names and relationship of requested to
resolved versions are semantic resolver invariants not expressible by the
schema alone.

Do not emit a separate numeric `schema_version`. Readers dispatch on the exact
`$schema` URI using a local allowlist and fail on an unsupported URI without
fetching it. The URL remains resolvable for humans/editors, but runtime parsing
does not depend on network availability.

Treat the schema as a committed contract artifact, not generated serializer
output. Include it unchanged in the CLI distribution/container, copy it into
the documentation build so its `$id` resolves after release, and link it from
the CLI documentation beside the `validation_plan.json` example. Keep schema
validation at tests/tooling boundaries; the production resolver must not gain
a JSON Schema runtime dependency merely to validate JSON it just encoded.

#### 3.4 Add stable failure classification

Keep existing structural CLI misuse as exit code `2` and unexpected defects as
exit code `3`. Add:

- exit code `4`, `ConfigurationError`: malformed/unsafe YAML, digest mismatch,
  duplicate/unknown/missing/type/range errors, invalid policy, no eligible
  version, unsupported config/plan schema URI, missing exact cached selection,
  or incompatible child selection;
- exit code `5`, `RegistryError`: transport/status failures, unsupported API,
  or an invalid registry response.

Errors must identify the config path and package/input path where applicable,
without echoing secrets or turning values into shell text.

#### Validation — Step 3 acceptance gate

- Pure policy tests cover exact, patch, minor, absent floors, major boundaries,
  prerelease/build rejection, deterministic ordering, legacy resolution, and no
  eligible candidate.
- Local/injected HTTP tests prove one index request, bounded exact metadata
  requests, no heavy endpoint fallback, and correct registry error mapping.
- Resolver contract tests assert byte-for-byte JSON shape, source ordering,
  exact `$schema`, null legacy fields, optional ARC specification, empty plans,
  and stdout/stderr separation.
- `schemas/validation_plan.schema.json` passes the Draft 2020-12 meta-schema;
  every resolver fixture validates against it, and focused invalid documents
  fail its `$schema` const, hash, SemVer, enum, conditional, required-field, or
  closed-object constraints as appropriate.
- CLI pack/container and documentation-output tests contain the byte-identical
  plan schema at their documented paths, and the canonical persisted fixture is
  named `validation_plan.json`.
- UTF-8/BOM and SHA-256 fixtures prove the digest is calculated over exact
  bytes and unsafe YAML produces no partial plan.
- Full preflight rejects every declaration/value mismatch before plan output.
- Focused package-management and CLI test projects plus
  `RunAutomatedTests` pass without a live registry dependency.
- **SCRATCHED by user instruction — not passed:** an integration test against
  AVPR dev resolves known fixtures using the exact preview client/model pins.

---

### Step 4 — Implement safe config-driven child execution and align ARCExpect

Status: **DONE**

Tracking issue: [arc-validate #254](https://github.com/nfdi4plants/arc-validate/issues/254)

Implement this step in the `arc-validate` CLI execution path, ARCExpect's
portable `PackageArguments`, documentation, and shared .NET/JavaScript/Python
contracts.

#### 4.1 Extend `validate` without replacing its existing selectors

Add these `validate` options:

```text
--validation-config <path>
--validation-config-sha256 <lowercase-sha256>
```

Config-driven validation requires the existing selectors as well:

```bash
arc-validate validate \
  --arc-directory . \
  --package configurable-validation \
  --package-version 1.2.7 \
  --validation-config .arc/validation_packages.yml \
  --validation-config-sha256 <digest>
```

`--package-version` belongs to `validate`, not `config resolve`. The parent
resolver calculates `1.2.7`; the child passes it to `validate` so the CLI loads
that exact installed cache entry and its version-specific input declarations.
Without it, the current CLI would select whichever installed stable version is
latest and could drift from the parent plan.

Mode rules:

- Config-driven validation requires `--package`, `--package-version`, and
  `--validation-config` together.
- It is mutually exclusive with the manual `-- <raw package arguments>`
  boundary.
- `--validation-config-sha256` is optional for direct interactive use but is
  mandatory in DataHUB-generated jobs.
- Do not auto-detect `.arc/validation_packages.yml`; explicit mode selection is
  deterministic and leaves existing calls unchanged.
- Require the exact package/version to be present in the existing cache,
  preserving install-then-validate behavior.

#### 4.2 Recheck intent without re-resolving “latest”

The child:

1. reads the config and verifies the supplied digest before using its values;
2. locates exactly one source selection by package name;
3. confirms the supplied resolved version equals an exact `disable` selection,
   falls within and above the requested rolling band, or is stable for a legacy
   entry;
4. does **not** ask the registry whether it is still the highest version,
   because a newer package may have appeared since parent resolution;
5. loads the exact cached package metadata and reruns declaration/value
   validation; and
6. materializes the ordered package arguments and executes the script.

The cached package metadata is sufficient for child validation; do not add an
extra metadata HTTP request. The digest binds the parent's resolution to the
same config bytes used by the child.

#### 4.3 Materialize argv safely

For configured values, order declarations by position and ID and then apply:

- boolean true -> one prefix token;
- boolean false or null -> no token;
- non-boolean -> prefix token followed by one invariant value token.

Append these tokens after the existing standard package-process arguments.
Use `ProcessStartInfo.ArgumentList` throughout. Never construct a shell command,
and never pass configured input values through DataHUB variables or the JSON
plan.

#### 4.4 Align ARCExpect and preserve manual mode

Update the portable `PackageArguments` parser and typed getters for the Step 1
declaration subset:

- remove joined-prefix/`separate: false` parsing;
- remove positional parsing;
- match prefixes as exact tokens only;
- reuse centralized declaration validation and invariant scalar behavior; and
- preserve deterministic diagnostics on .NET, Node, and Python.

Manual raw-argument mode remains available:

```bash
arc-validate validate -p configurable-validation -v 1.2.7 -- --strict
```

In manual mode, the existing boolean convention remains useful: an omitted
required boolean is observed as false. The stricter requirement that a boolean
key be explicitly present applies to config preflight, where author intent can
be validated before a CI child is created.

#### 4.5 Release the CLI integration artifact

After validation, pin final compatible AVPR preview versions, update release
notes/docs, and produce a preview CLI/container that DataHUB staging can use.
Record its exact version/image digest in the DataHUB issue. Do not update
production DataHUB yet.

#### Validation — Step 4 acceptance gate

- CLI parsing tests cover every legal/illegal combination of config path,
  digest, package, version, and raw boundary.
- Cache tests prove exact version selection when multiple package versions are
  installed and prove no registry call is made by the child.
- Digest tests cover match, mismatch, file modification between parent and
  child, BOM, and interactive omission.
- Materialization tests cover spaces, empty strings, leading dashes, Unicode,
  numeric boundaries, false/null omission, and deterministic position/ID order.
- Process tests capture argv elements and prove no shell interpolation or token
  joining occurs.
- Existing raw argument forwarding remains compatible except for the deliberate
  unreleased removal of positional and joined CWL declarations.
- The shared ARCExpect contract suite passes on .NET, Node, and Python, and the
  generated native APIs are inspected for portable shape regressions.
- Focused CLI/package-runner tests, `TestPortableARCExpect`, packed-consumer
  checks, docs samples, and `RunAutomatedTests` pass.
- **SCRATCHED by user instruction — not passed:** the preview CLI/container
  executes a configured package successfully against AVPR dev.

---

### Step 5 — Update the normative ARC specification

Status: **CURRENT / NEXT — NOT IMPLEMENTED**

Tracking issue: [ARC-specification #183](https://github.com/nfdi4plants/ARC-specification/issues/183)

Implement this step in `nfdi4plants/ARC-specification` after Steps 1–4 have
stabilized the executable behavior, so normative prose and examples describe
the tested contract exactly.

#### 5.1 Specify the canonical file

Update the `validation_packages.yml` section to define:

- canonical location `.arc/validation_packages.yml`;
- optional `arc_specification` as a semantic version;
- required, possibly empty `validation_packages` sequence;
- unique non-empty package `name`;
- required full-SemVer `version`;
- optional `roll_forward` with `disable`, `latest_patch`, and `latest_minor`;
- optional CWL-job-style `inputs` mapping keyed by declared input ID; and
- the exact safe scalar subset and declaration compatibility rules from Step 1.

Identify the machine-readable companion by its immutable `$id`,
`https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json`, and
record the AVPR Codecs version that supplies it. Link the raw schema and include
the required top-level `$schema` property in every canonical example. Explain
that this property both enables editor association and selects the offline wire
decoder; tools must not dereference arbitrary schema values during parsing.
State explicitly that the normative prose and strict YAML codec cover
duplicate-key, alias/tag, numeric-lexeme, declaration lookup, uniqueness, and
roll-resolution rules that JSON Schema cannot fully express.

Document version policies precisely, including stable-only rolling, no major
roll-forward, floor-not-required behavior, and exact prerelease/build support
only under `disable`.

Do not make name-only selections canonical. Add a non-normative migration note
stating that tooling may temporarily read them and will warn. There is no
independent numeric config schema-version field: the immutable `$schema` URI is
the wire discriminator, and new incompatible shapes receive a new URI in
coordination with the ARC specification.

#### 5.2 Correct and clarify existing prose

- Replace the inconsistent `specification` field name with
  `arc_specification`.
- Explain that `inputs` keys are package declaration IDs, not raw flags or
  `inputBinding.prefix` values.
- Explain that `label` belongs to package metadata as optional human-readable
  documentation; it is not a configuration key. Use an input such as
  `report-title` in config examples.
- State that the executable reference model/codecs live in AVPR and that
  DataHUB/other consumers should not reinterpret YAML through shell tools.
- Link to the AVPR declaration documentation rather than duplicating all
  package-metadata authoring guidance.

#### Validation — Step 5 acceptance gate

- Repository Markdown/link/spelling checks pass.
- Every normative YAML example parses through the released preview
  `ValidationPackage.Codecs` implementation.
- Every canonical normative example validates against the exact versioned
  `validation-packages.schema.json` linked by the specification.
- **SCRATCHED by user instruction — not passed:** verify that the AVPR-dev
  route returns that file as `application/schema+json`.
- Positive and negative examples agree with the Step 1 codec fixtures and the
  Step 3 resolver behavior.
- Review confirms the document uses only `arc_specification`, never presents a
  missing `$schema` or package version as canonical, and never describes input
  keys as CLI flags.
- The specification change is approved before production DataHUB begins
  accepting the new fields.

---

### Step 6 — Replace DataHUB YAML processing with plan-driven child jobs

Status: **NOT STARTED**

Tracking issue: [DataHUB #73](https://github.com/nfdi4plants/DataHUB/issues/73)

Implement this step in the DataHUB CI runner/template repository identified on
the tracking issue. Use the exact preview `arc-validate` artifact first. Using
the preview AVPR-dev service is **SCRATCHED by user instruction — not passed**.

#### 6.1 Parent job

- Preserve the current behavior for an absent config file; invoke the resolver
  only when `.arc/validation_packages.yml` exists. An explicitly present but
  invalid file fails the parent job.
- Run `arc-validate config resolve --validation-config
  .arc/validation_packages.yml > validation_plan.json` once and retain that
  exact file as the plan artifact.
- Treat exit codes 4 and 5 as configuration and registry failures with distinct
  user-facing diagnostics.
- Validate that `$schema` exactly matches a locally supported plan-schema URI
  before generating child jobs; do not fetch or execute content from that URI.
- Read optional `arc_specification` from the JSON plan wherever the current
  pipeline selects built-in ARC specification validation. Preserve the current
  default when it is absent; this plan does not otherwise change built-in
  specification-validation behavior.
- Generate one child job per `validation_packages` plan entry, passing only
  `name`, `resolved_version`, config path, and `config_sha256`.
- An empty plan succeeds and generates no validation-package child jobs.
- Preserve current parallel child-job behavior; plan order is informational.

The parent may use a normal JSON tool to read the CLI-owned
`validation_plan.json`. Remove all `yq` access to package selection or input
values. Do not use shell `eval`, join arguments into a string, or embed
configured values into generated CI YAML.

#### 6.2 Child job

Each generated child performs the exact handoff:

```bash
arc-validate package install configurable-validation --version 1.2.7

arc-validate validate \
  --arc-directory . \
  --package configurable-validation \
  --package-version 1.2.7 \
  --validation-config .arc/validation_packages.yml \
  --validation-config-sha256 <digest>
```

Use the actual CI shell's array/quoted-variable facilities for the four scalar
handoff values. The child does not consume `requested_version`, decide a roll
policy, query for a newer version, or handle package input values.

Retain `validation_plan.json` as a job artifact for diagnosis, taking care that
it contains no configured input values. Keep existing validation result
artifact collection unchanged.

#### Validation — Step 6 acceptance gate

- A staging DataHUB project exercises exact, latest-patch, latest-minor, legacy,
  multiple-package, and empty-package-list configurations.
- Every captured artifact is named `validation_plan.json` and validates against
  the exact plan schema shipped by the selected CLI version before CI-template
  fixtures are accepted.
- Packages receive correctly typed values containing whitespace, quotes,
  Unicode, and leading dashes without CI/YAML/shell reinterpretation.
- A config edit after parent resolution is rejected by every child through the
  digest check.
- Publishing a newer eligible package after parent resolution does not change
  the exact version executed by an existing child.
- Invalid YAML/values fail before child generation; registry unavailability is
  distinguishable from a user configuration failure.
- The DataHUB scripts contain no remaining `yq` parsing of
  `validation_packages.yml` and no dynamic input-argument construction.
- Existing `arc_specification` selection/default behavior is preserved through
  the JSON plan rather than a second YAML read.
- Existing validation outputs and one-child-per-package behavior remain intact.

---

### Step 7 — Roll out, observe, and close compatibility work

Status: **NOT STARTED**

Tracking issue: [AVPR #122](https://github.com/nfdi4plants/arc-validate-package-registry/issues/122)

Perform rollout only after all preceding acceptance gates pass.

#### 7.1 Deployment order

1. Merge/release AVPR Model and Codecs.
2. Merge/release AVPRClient and Interop and deploy the compatible AVPR service.
3. Verify production lightweight endpoints before releasing the dependent CLI.
4. Release `arc-validate`/ARCExpect and the DataHUB-consumed container.
5. Merge the ARC specification update according to its release process.
6. Promote the tested DataHUB runner/template changes from staging to
   production.

The resolver intentionally reports `RegistryError` against an old registry;
deployment order is the compatibility mechanism. Do not add a heavy-endpoint
fallback.

#### 7.2 Compatibility and follow-ups

- Keep deprecated `GET /api/v1/packages` until AVPRCI and known external
  consumers have migrated. Removing it is a separate breaking API issue.
- Keep the read-only legacy config decoder and warning for an explicitly
  documented transition window. Canonical writers and specification examples
  never produce legacy files.
- Retain schema-less historical frontmatter decoding for immutable published
  packages, and retain each recognized versioned decoder for its declared
  compatibility lifetime. New writers always emit the current `$schema` URI.
- Monitor resolver exit-code counts, registry endpoint failures, child digest
  mismatches, and package execution failures through existing service/CI logs;
  do not add a telemetry platform as part of this work.
- After production stabilization, resume ARCtrl #634 to remove or redirect its
  duplicate model/codecs to AVPR. That follow-up must not block this rollout.
- ETags, pagination, richer CWL types, arrays/files/directories, aliases,
  expressions, enums, and general CWL execution remain out of scope.

#### Validation — Step 7 acceptance gate

- Production AVPR serves the index, versions, and metadata contracts before
  the production CLI/DataHUB flow calls them.
- Both production AVPR schema `$id` URLs and the released arc-validate plan
  schema `$id` URL resolve with `application/schema+json` and match the schema
  bytes shipped in their owning package/CLI artifacts.
- A production-like smoke ARC resolves, installs, and executes an exact package
  with boolean, numeric, and string inputs and produces the expected existing
  result artifacts.
- A second smoke run proves roll-forward resolution is stable within one plan
  and digest tampering fails safely.
- Registry metrics/logs show discovery no longer downloads all scripts or
  increments package download counts.
- Legacy configuration produces one warning and executes the selected highest
  stable version; canonical configuration produces no legacy warning.
- Release notes and exact artifact/image versions are recorded on the EPIC,
  and every repository subissue links its completed acceptance evidence.

---

## 4. GitHub issue structure

The cross-repository hierarchy is tracked by [AVPR
#119](https://github.com/nfdi4plants/arc-validate-package-registry/issues/119).
Its seven child issues map one-to-one to the implementation steps above and
carry GitHub blocker relationships matching their dependency order:

```text
AVPR #120
  -> AVPR #121
    -> arc-validate #253
      -> arc-validate #254
        -> ARC-specification #183
          -> DataHUB #73

AVPR #122 rollout waits for AVPR #121, arc-validate #254,
ARC-specification #183, and DataHUB #73.
```

Each child issue contains the corresponding behavioral requirements,
acceptance gate, and out-of-scope boundary. Release and acceptance evidence is
recorded on the child issue and summarized on the EPIC before closure.
