# Plan: Modular ARC validation specifications

## Status

**Accepted in design on 2026-08-21. This document records the replacement
architecture and implementation sequence only. No specification, schema,
implementation, release, or deployment change has started under this plan.**

This plan supersedes the monolithic specification work described as Step 5 in
[`validation-packages-config.md`](validation-packages-config.md). Steps 1–4 of
that plan remain completed implementation history. Its DataHUB and production
rollout steps remain blocked until the specification work and the later
implementation amendments described here are accepted.

## 1. Goals and boundaries

Move stable validation contracts into `nfdi4plants/ARC-specification` as
independently versioned specification modules. Keep the ARC core specification
focused on intent and general concepts, and keep concrete profiles separate.

The resulting documents must make these boundaries explicit:

- A validation package contains executable validation logic and descriptive
  metadata. The general concept does not prescribe a programming language,
  registry, runner, process model, or helper library.
- AVPR's single-file F#/Python packages, `arc-validate`, and ARCExpect are
  reference implementations, not requirements for another conforming
  implementation.
- AVPR is authoritative for the package identities, versions, metadata records,
  immutable artifacts, and publication state it serves. ARC-specification is
  authoritative for the public formats and schemas moved under this plan.
- The existing AVPR frontmatter shape is the concrete AVPR validation-package
  profile, including registry concerns such as `Publish`; this plan does not
  invent a second, supposedly neutral metadata schema.
- A CQC branch is one profile for recording validation provenance in an ARC,
  not the universal definition of validation provenance.

ISA-XLSX and its specification document are entirely out of scope and must
remain untouched. AVPR-dev, DataHUB, and production deployment work is also out
of scope until the specification phase and its later implementation follow-up
are complete.

## 2. ARC-specification structure

Reduce validation-specific detail in `ARC specification.md` to descriptions of
intent, the relationships between the concepts, and links to these separate
normative documents:

```text
specifications/validation-packages.md
specifications/validation-package-configuration.md
specifications/validation-execution-plan.md
specifications/validation-summary.md
profiles/cqc-validation-provenance.md
```

The validation-package introduction must explain the role of metadata and the
division of authority between the format specification and AVPR's registry
records. Concrete F#, Python, ARCExpect, arc-validate, and DataHUB behavior may
appear only in clearly marked non-normative implementation notes or examples.

The existing monolithic ARC-specification commit `0839e49` and PR #184 are not
the acceptance vehicle for this structure. Close that PR as superseded and
carry forward only material that fits the modular, implementation-neutral
documents. ARC-specification issue #183 may remain as the umbrella issue, with
the replacement work linked from it.

## 3. Canonical schemas

Seed ARC-specification with the three schemas that already exist in the
implementation repositories:

```text
arc-validate-package-registry/schemas/validation-package-frontmatter.schema.json
arc-validate-package-registry/schemas/validation-packages.schema.json
arc-validate/schemas/validation_plan.schema.json
```

Do not redesign their wire shapes. Relocation may change their canonical `$id`,
the corresponding instance `$schema` constant, descriptions, and examples, but
must preserve their field names, casing, requiredness, validation rules, and
compatibility behavior.

Create only the currently missing
`schemas/validation-summary.schema.json`. Derive it from ARCExpect's current
`ValidationSummary` JSON codec rather than the obsolete inline Draft-04 schema.
Preserve the existing PascalCase result fields and their current optionality,
add the canonical `$schema` discriminator, and add a required top-level
`Inputs` object. `Inputs` is `{}` when no configured inputs were supplied and
has these semantics:

- keys are exact package input declaration IDs, never CLI prefixes;
- values are the validated configuration string, boolean, finite JSON number,
  or `null`, with signed 64-bit integers represented losslessly;
- explicit `false` and `null` are preserved, while absent configuration keys
  remain absent; and
- values are provenance and may be published in CQC branches, so package and
  configuration guidance must warn authors not to place secrets in them.

Recording `Inputs` is specified here first and implemented later. It is not
part of the already completed safe-execution acceptance claim in Step 4 of the
old plan.

## 4. Persistent identifiers and releases

Use the existing `https://w3id.org/arc/` namespace rather than creating a new
project prefix:

```text
https://w3id.org/arc/specification/<name>/<version>
https://w3id.org/arc/profile/<name>/<version>
https://w3id.org/arc/schema/<name>/<version>
```

Start each new document and schema module at `1.0.0-draft.1`. Modules version
independently. Once a versioned PID is published, its content is immutable;
further draft or stable changes receive a new version.

Add a versioned ARC-specification documentation build whose published output
contains the rendered documents and raw JSON schemas. Extend the existing
`/arc` W3ID configuration to resolve those PIDs and add `kMutagene` to its
contacts through a `perma-id/w3id.org` PR. W3ID provides persistent redirects,
not W3C endorsement. Continue using Zenodo releases for archival citations.

## 5. Implementation-repository copies

Do not introduce Git submodules, Git subtrees, build-time downloads, schema
packages, lock manifests, or automated synchronization tooling. Normal builds
and runtime schema dispatch must remain offline.

Finish and accept the entire ARC-specification reorganization first. Then copy
the final canonical files once into the repositories that need them as tracked,
non-authoritative runtime snapshots:

- AVPR receives the validation-package frontmatter and package-configuration
  schemas for offline Codecs use and NuGet/npm/PyPI packaging.
- `arc-validate` receives the execution-plan and validation-summary schemas for
  CLI, container, and ARCExpect distribution.

Future schema changes repeat this as a deliberate coordinated update after the
corresponding specification module receives a new version. Repository history
and the schema's own PID record provenance; no automatic upstream fetch occurs.

After that copy, update canonical writers, offline URI allowlists, examples,
package contents, and tests to use the W3ID identifiers. The old identifiers
and AVPR schema routes were introduced only on development branches and
prerelease artifacts, so they do not become a second stable identity. Remove
the dev-only AVPR routes as canonical endpoints while continuing to bundle the
local schema copies where offline consumers need them.

## 6. Later validation-summary implementation

In a separate post-specification ARCExpect/arc-validate change, emit the new
summary `$schema` and the validated `Inputs` map. The reference implementation
may pass that map to the child through a permission-restricted temporary JSON
sidecar identified by a reserved environment variable and delete it after
execution. This transport is an internal implementation detail and must not
appear as a normative requirement.

Publish affected implementation packages only as new prerelease versions; do
not replace the already published preview.4 artifacts.

## 7. Acceptance gates

- Validate all four schemas against JSON Schema Draft 2020-12 and cover
  representative valid and invalid documents.
- Validate normative examples with generic schema tooling; ARCExpect,
  `arc-validate`, AVPR, F#, and Python must not become normative dependencies of
  ARC-specification.
- Build the versioned documentation and verify every W3ID resolves to the
  intended immutable document or schema.
- Prove ISA-XLSX is unchanged.
- After the one-time copy, run AVPR Codecs cross-target contracts, offline
  schema dispatch tests, package-content tests, and affected solution tests.
- Run `arc-validate` hermetic CLI tests, ARCExpect .NET/JavaScript/Python
  contracts, packed-consumer tests, and schema packaging checks.
- For the later summary implementation, cover empty and omitted inputs,
  strings, Unicode, leading dashes, `false`, `null`, numeric boundaries,
  canonical schema identity, and CQC serialization.
- Do not require an AVPR-dev live check or any deployment for specification
  acceptance.

## 8. Implementation-alignment review — 2026-08-24

> **Review checkpoint:** These notes compare the ARC-specification WIP on
> `spec-reference-split` with the current AVPR and arc-validate/ARCExpect
> implementations. They clarify the intended direction but authorize no code,
> schema, specification, release, or deployment change. Review them before
> approving implementation commits.

### Public validation-plan boundary

A non-nullable `requested_version` is not inherently a problem if the public
plan contract describes canonical selections only. The current mismatch exists
because arc-validate emits `requested_version: null` and
`roll_forward: legacy_latest_stable` in a plan carrying the same canonical
`$schema` identifier.

Setting `additionalProperties: true` does not relax a declared property's type
or enum. If `requested_version` and `roll_forward` are tooling-internal
diagnostics, remove them from the public schema's declared and required
properties. Require only interoperable entry fields such as `name` and
`resolved_version`, and allow additional entry properties so the current
arc-validate annotations remain valid without becoming normative. The
arc-validate plan decoder currently rejects unknown properties and must later
be aligned if open plan entries become part of the public contract.

### Frontmatter implementation versus the WIP schema

The implementation-repository schema copies are scheduled for removal and are
not themselves an ownership problem. Comparing runtime behavior to the WIP
schema still identifies these differences:

- AVPR's canonical decoder accepts `ProgrammingLanguage`, and its canonical
  writer emits it. Frontmatter extraction then overwrites the value from the
  actual `.fsx` or `.py` format. If the WIP schema remains authoritative, keep
  the language in internal/API metadata but remove it from canonical
  frontmatter encoding and accepted canonical wire fields.
- The schema constrains non-empty names, non-negative version components,
  valid SemVer suffixes, and non-empty author/tag names. The frontmatter codec
  is looser for several of these rules; staging validation catches some, but
  the canonical codec boundary does not enforce all schema constraints.
- The prose's 50-word `Summary` limit is enforced by neither the schema nor the
  implementation and therefore needs either removal or matching validation.
- Canonical validation-package configuration parsing otherwise matches its WIP
  schema materially, with intentional stricter YAML lexical checks beyond what
  JSON Schema can express.

### Reserved standard prefixes

The WIP correctly reserves `--arc-directory` and `--out-directory`, but the
current shared AVPR `CommandInputParameter.validate` implementation permits
them. This also affects arc-validate/ARCExpect: ARCExpect recognizes the long
forms as standard aliases but checks package declarations first, allowing a
package-defined input to shadow an alias, and current cross-repository fixtures
exercise that behavior.

Fix the invariant centrally in AVPR Model, publish a new compatible prerelease,
then update arc-validate's pins and affected ARCExpect fixtures/tests. The
arc-validate CLI option parser does not need an independent reservation rule,
but its shared model dependency and package-side behavior must be updated.

### Legacy frontmatter behavior

Legacy frontmatter is already documented by the superseded ARC specification
and need not be repeated as a new normative format. Current parsers behave as
follows:

- an absent `$schema` automatically selects the legacy decoder;
- legacy frontmatter cannot declare `Inputs`;
- historical fields are decoded and unknown fields retain permissive legacy
  behavior;
- script extraction derives and overwrites `ProgrammingLanguage` from the
  selected F#/Python frontmatter format;
- a recognized `$schema` selects the strict canonical decoder; and
- an unknown or malformed `$schema` fails without network access or fallback.

Callers that require only the new format can use the explicit current decoder.
Moving the canonical schema document does not require rewriting immutable
legacy packages or removing this compatibility path.

### Installed metadata authority

Do not require every metadata consumer to parse package frontmatter. Retain a
stronger requirement specifically for a runner about to execute an installed
package. The recommended reference flow is:

1. use registry metadata for discovery and parent preflight;
2. during installation, parse the downloaded script's embedded metadata and
   compare its identity and input declarations with the registry response;
3. before execution, parse the installed script again to detect stale or
   modified cache metadata; and
4. validate configured values against those embedded declarations.

Current arc-validate writes API metadata into its cache next to the downloaded
script and later trusts that cached metadata without reparsing the script. The
recommended change keeps reference packages self-describing and detects
registry, artifact, or cache disagreement without making the transport a
requirement for unrelated implementations.

### Validation-summary input provenance

There is no current schema/implementation mismatch: the WIP summary schema
describes ARCExpect's present output, and neither currently contains `Inputs`.
This is the agreed next specification feature rather than an existing defect.

Before declaring summary schema `1.0.0` stable, draft the top-level `Inputs`
contract and retain a draft-version PID while it changes. `$id` identifies the
schema document but does not add `$schema` to summary instances. An instance
`$schema` property is optional; it is recommended because persisted summaries
are interoperability artifacts that may outlive the producing tool.

ARCExpect cannot reconstruct exact provenance from materialized argv: explicit
`false` and `null` values produce no argument tokens, and omitted values are
also absent. The later implementation must therefore pass the already
validated input map from arc-validate to the package/ARCExpect output path
without reconstructing it from process arguments.

### Expected rollout gaps

The current DataHUB pipeline is not yet plan-driven. This is expected future
rollout work, not an inconsistency that blocks drafting or reviewing the
modular specifications. No DataHUB or AVPR-dev work is authorized here.
