# Roadmap: Portable validation-package boundaries across AVPR and arc-validate

## Status and planning policy

**Status: Done.** The five roadmap milestones are implemented. AVPR owns canonical
cross-boundary fixtures and verifies its NuGet/npm/Python artifacts. Downstream
ARCExpect compatibility belongs to arc-validate's normal contract, packed-
consumer, and release checks; AVPR does not clone or rebuild arc-validate.
ARCExpect exposes its shared authoring and execution API from .NET, JavaScript,
and Python using target-native model and codec dependencies. Post-roadmap API
hardening continues without reopening the completed extraction milestones.

GitHub tracking:

- Epic: [AVPR #110](https://github.com/nfdi4plants/arc-validate-package-registry/issues/110)
- AVPR sub-issues:
  [#111](https://github.com/nfdi4plants/arc-validate-package-registry/issues/111),
  [#112](https://github.com/nfdi4plants/arc-validate-package-registry/issues/112),
  [#113](https://github.com/nfdi4plants/arc-validate-package-registry/issues/113),
  [#114](https://github.com/nfdi4plants/arc-validate-package-registry/issues/114), and
  [#115](https://github.com/nfdi4plants/arc-validate-package-registry/issues/115)
- arc-validate sub-issues:
  [#242](https://github.com/nfdi4plants/arc-validate/issues/242),
  [#243](https://github.com/nfdi4plants/arc-validate/issues/243),
  [#244](https://github.com/nfdi4plants/arc-validate/issues/244), and
  [#245](https://github.com/nfdi4plants/arc-validate/issues/245)

This document intentionally stays at roadmap level. Do not add detailed
implementation plans for individual sub-issues until explicitly requested.
When requested, place each detailed plan in this folder as a separate document
named for its GitHub issue.

---

## Goals

- Establish one portable validation-package domain model that can be consumed
  from .NET and, through Fable, JavaScript and Python.
- Remove registry indexing, YAML, JSON, filesystem, EF, and generated-client
  concerns from that model.
- Retire `AVPRIndex` by moving its remaining responsibilities to accurately
  named owners.
- Prepare ARCExpect for transpilation without blocking near-term work on
  changes to the widely consumed `Fable.Pyxpecto` library.
- Move package caching, installation, and process execution into the
  `arc-validate` CLI, where they are actually consumed.
- Make contract drift fail through each repository's normal package and
  consumer checks without coupling their CI implementations.

---

## Confirmed architectural decisions

### Portable domain and codecs

- `ValidationPackage.Model` owns metadata, authors, tags, CWL inputs, package
  identity, and semantic-version behavior.
- The model contains no YAML/JSON implementation, STJ attributes, filesystem,
  hashing, EF, HTTP, generated-client, or AVPR staging types.
- `ValidationPackage.Codecs` owns string-to-model and model-to-string codecs:
  YAML/frontmatter through YAMLicious and domain JSON through Thoth.Json.
- File reading, directory traversal, and content hashing are application
  infrastructure, not codec responsibilities.
- The model and codecs must eventually be published as matching target-native
  artifacts: NuGet packages for .NET, npm packages for JavaScript, and PyPI
  packages for Python. A Fable source NuGet package may bootstrap transpilation,
  but bundling generated model/codec code into downstream packages is only a
  transition state, not the final dependency boundary.

### AVPRIndex retirement

- `AVPRIndex` will be decomposed and retired rather than retained as a renamed
  collection of unrelated infrastructure.
- Repository traversal, normalized package content, hashing, and staged package
  state move to focused AVPR staging infrastructure.
- `ValidationPackageIndex` becomes `StagedValidationPackage`; it is not part of
  the portable domain.

### Registry service model

- `PackageRegistryService` keeps one service-owned model used for both
  ASP.NET/OpenAPI transport and EF persistence.
- The service replaces nested `AVPRIndex.Domain` types with service-owned
  author, ontology, and command-input types under
  `src/PackageRegistryService/Models/`.
- Explicit mappings convert between `ValidationPackage.Model` and the
  service-owned model.
- STJ attributes/converters and EF fluent configuration remain service
  implementation details.
- Separate API DTOs and persistence entities are not introduced now. Revisit
  that split only if the public and stored shapes materially diverge.

### Generated client

- `AVPRClient` contains generated code only and has no dependency on the model,
  codecs, staging infrastructure, or the former index project.
- Portable model/client mappings move to `AVPRClient.Interop`.
- Staging and publication-specific conversions move to `AVPRCI`.

### ARCExpect and Pyxpecto

- ARCExpect owns a portable result contract consisting of case outcomes,
  per-case results, and run summaries.
- Portable summary, JUnit, and badge generation consumes that result contract,
  not `Expecto.TestRunSummary`.
- ARCExpect production package authoring and execution use Fable.Pyxpecto on
  every target. Expecto may remain in .NET test projects as their test harness,
  but is not an ARCExpect production dependency.
- `ARCExpect.Setup`, `ARCValidationPackage`, and the top-level
  `ARCExpect.Execute` facade are required public APIs on .NET, JavaScript, and
  Python. Output writers alone are not a complete transpiled ARCExpect API.
- Portable validation packages use Fable.Pyxpecto test cases. ARCExpect owns a
  focused structured-result adapter until Pyxpecto exposes an equivalent API.
- An internal output pipeline combines critical and non-critical results and
  creates summary JSON, JUnit XML, and badge SVG content without adding a
  public orchestration type or requiring authors to call individual writers in
  the standard flow. Individual `Execute` output operations and pure content
  writers remain public for custom workflows.
- Filesystem writes are target boundaries: .NET owns its adapter under
  `DotNet/`, Python owns its native package facade, and JavaScript package/file
  conventions remain deliberately undesigned.
- Each ARCExpect build consumes the matching target-native
  ValidationPackage.Model and ValidationPackage.Codecs artifacts. The .NET
  project uses NuGet, the JavaScript package uses npm dependencies, and the
  Python package uses PyPI dependencies.

### CLI package management

- `ARCValidationPackages` is not treated as a general-purpose authoring
  library.
- Registry access, configuration, caching, and installation move into internal
  `arc-validate/PackageManagement` modules.
- FSI and `uv run` execution move into internal
  `arc-validate/PackageRunner` modules.
- The published package is deprecated before removal if external usage
  requires a compatibility window.

---

## Target structure

```text
arc-validate-package-registry/
  src/
    ValidationPackage.Model/       portable domain
    ValidationPackage.Codecs/      YAMLicious + Thoth.Json codecs
    AVPR.Staging/                   repository scan, normalized content, hashes
    AVPRClient/                     generated HTTP client only
    AVPRClient.Interop/             client DTO <-> portable domain
    AVPRCI/                         publication orchestration and conversions
    PackageRegistryService/
      Models/                       service-owned API + EF types
      Mappings/                     portable domain <-> service model

arc-validate/
  src/
    ARCExpect/
      ARCExpect.fsproj              .NET library and compatibility adapters
      ARCExpect.Javascript.fsproj   Fable JavaScript build
      ARCExpect.Python.fsproj       Fable Python build
      Common/                       shared contracts, Setup, Execute, writers
      DotNet/                       .NET runtime, filesystem, CV/ARCTokenization
      Python/                       Python runtime and native top-level facade
      Javascript/                   JavaScript runtime and native top-level facade
    arc-validate/
      PackageManagement/            registry, cache, config, install/uninstall
      PackageRunner/                FSI and Python execution
```

The three ARCExpect project files compile the same ordered shared source list.
Only `ARCExpect.fsproj` additionally compiles the .NET compatibility sources.
The public identity is `ARCExpect` on NuGet and `arcexpect` on npm/PyPI; project
suffixes and source folders must not become public namespaces.

---

## Roadmap

### Milestone 1 — Extract the portable contract and retire AVPRIndex

Issues:

- [AVPR #111 — Extract ValidationPackage.Model and portable metadata codecs](https://github.com/nfdi4plants/arc-validate-package-registry/issues/111)
- [AVPR #112 — Replace AVPRIndex with focused AVPR staging infrastructure](https://github.com/nfdi4plants/arc-validate-package-registry/issues/112)
- [AVPR #113 — Move registry API and EF nested models under service ownership](https://github.com/nfdi4plants/arc-validate-package-registry/issues/113)
- [AVPR #114 — Make AVPRClient generated-only and add portable model interop](https://github.com/nfdi4plants/arc-validate-package-registry/issues/114)

Primary moves:

- `src/AVPRIndex/Domain.fs` and the SemVer portion of `Globals.fs`
  → `src/ValidationPackage.Model/`
- `src/AVPRIndex/Frontmatter.fs`
  → `src/ValidationPackage.Codecs/`, replacing YamlDotNet with YAMLicious
- Domain JSON conversion
  → Thoth.Json codecs in `src/ValidationPackage.Codecs/`
- `AVPRRepo.fs`, `BinaryContent.fs`, `MD5Hash.fs`, staging constants, and
  `ValidationPackageIndex`
  → focused `src/AVPR.Staging/` modules and `StagedValidationPackage`
- Nested shared types currently used by
  `PackageRegistryService/Models/ValidationPackage.cs`
  → service-owned types in `PackageRegistryService/Models/`
- `src/AVPRClient/Extensions.cs`
  → portable mappings in `AVPRClient.Interop` and publication mappings in
  `AVPRCI`

Outcome:

- Neither ARCExpect nor arc-validate references AVPR indexing infrastructure.
- `AVPRClient` no longer brings YAML or staging dependencies transitively.
- The service owns its public and stored representations while the portable
  model stays infrastructure-free.
- `AVPRIndex` is removed.

### Milestone 2 — Prepare ARCExpect outputs without waiting for Pyxpecto

Issue:

- [arc-validate #242 — Prepare ARCExpect result and output contracts for transpilation](https://github.com/nfdi4plants/arc-validate/issues/242)

Primary moves:

- AVPR metadata/frontmatter dependencies in `ARCValidationPackage.fs`,
  `ValidationSummary.fs`, and `TopLevelAPI.fs`
  → `ValidationPackage.Model` and `ValidationPackage.Codecs`
- Framework-neutral result types
  → portable ARCExpect source/project boundary
- Summary, JUnit, and badge content generation
  → portable writers consuming ARCExpect `RunSummary`
- result combination and three-format output orchestration
  → internal portable pipeline, with target-specific filesystem adapters
- ARCExpect release ownership
  → registered in `build/ProjectInfo.fs` with package metadata, release notes,
  and dedicated tests

Outcome:

- Output schemas and writers can be exercised independently of the runner.
- Immutable published packages retain their historical exact ARCExpect pins;
  new package versions use Pyxpecto.
- The top-level pipeline restores the established one-call output behavior
  without exposing result combination or writer orchestration to authors.

### Milestone 3 — Absorb ARCValidationPackages into the CLI

Issue:

- [arc-validate #243 — Absorb ARCValidationPackages into the arc-validate CLI](https://github.com/nfdi4plants/arc-validate/issues/243)

Primary moves:

- `ARCValidationPackages` config, cache, domain, registry, and install logic
  → `src/arc-validate/PackageManagement/`
- `ARCValidationPackages/ScriptExecution.fs`
  → `src/arc-validate/PackageRunner/`
- `tests/ARCValidationPackages.Tests/`
  → package-management and runner coverage under
  `tests/arc-validate.Tests/`

Defects fixed while moving:

- Honor `Config.PackageCacheFolder`.
- Make cache writes atomic.
- Make registry endpoints configurable.
- Use asynchronous HTTP and status-based error classification.
- Remove FAKE runtime process dependencies.
- Replace live-production API tests with injected or local test endpoints.

Outcome:

- Package management is application infrastructure owned by its only in-repo
  production consumer.
- Existing cache paths and formats remain compatible.
- CLI handlers retain only arguments, orchestration, and presentation.

### Milestone 4 — Enforce compatibility and prepare ARC-specific Fable work

Issues:

- [AVPR #115 — Add AVPR contract fixtures and downstream arc-validate compatibility testing](https://github.com/nfdi4plants/arc-validate-package-registry/issues/115)
- [arc-validate #244 — Consolidate ARCExpect into one portable package with a .NET-specific API boundary](https://github.com/nfdi4plants/arc-validate/issues/244)

Primary work:

- AVPR owns canonical model, frontmatter, API JSON, and CWL fixtures.
- AVPR CI verifies model, codec, client, and interop artifacts in AVPR;
  arc-validate verifies its exact dependency pins and packed consumers in its
  own normal CI and release process.
- ARCExpect is consolidated into one source tree with three parallel .NET,
  JavaScript, and Python project files following the DataHubClient/ARCtrl
  project layout.
- Unused ARCGraph, OBO, Graphoscope, and Cytoscape functionality is retired
  rather than ported. ARCTokenization and ControlledVocabulary helpers remain
  available only through the .NET compatibility boundary.
- AVPR produces target-native ValidationPackage.Model and
  ValidationPackage.Codecs artifacts so ARCExpect's npm and PyPI packages can
  declare real runtime dependencies instead of vendoring generated copies.
- Affected staged packages and compatibility requirements are inventoried
  before migration.

Outcome:

- Cross-repository drift fails in the owning repository's artifact and
  downstream-consumer checks without a cross-repository clone/build harness.
- ARCExpect has one public identity, three target builds, and no public
  `Core`, `Portable`, or target-suffixed namespace.
- ARCExpect's shared authoring and execution boundary is implemented, and AVPR
  #115 enforces its model, codec, client, interop, API JSON, frontmatter, SemVer,
  and CWL compatibility before release.

### Milestone 5 — Provide portable Pyxpecto execution

Issue:

- [arc-validate #245 — Replace the ARCExpect Expecto boundary after structured Pyxpecto results are available](https://github.com/nfdi4plants/arc-validate/issues/245)

Primary moves:

- Structured case results
  → obtained from `Fable.Pyxpecto`; prefer an upstream API, with a focused
  ARCExpect adapter allowed when the released API cannot yet provide them
- `Expecto.Test` in `ARCValidationPackage`
  → Pyxpecto's portable test representation
- Expecto result conversion
  → Pyxpecto result-to-ARCExpect-`RunSummary` conversion
- `Setup`, `ARCValidationPackage`, and `Execute`
  → shared top-level ARCExpect API compiled by all three projects
- output filesystem writes
  → .NET and Python target-specific adapters over the internal portable output
  pipeline; JavaScript remains “Coming soon”

Outcome:

- ARCExpect authoring and execution use one test framework on .NET, JavaScript,
  and Python.
- Packed consumers on all three targets can construct a package and invoke the
  top-level `Execute` API.
- The already-stable result and output contracts do not change during runner
  replacement.
- Expecto and its temporary production compatibility layer are removed in the
  ARCExpect 7 preview line.
- arc-validate #245 is complete using the permitted focused ARCExpect adapter;
  a future structured Pyxpecto result API can replace that adapter without
  changing the public result or output contracts.

---

## Dependency order

```text
ValidationPackage.Model
        |
        +--> ValidationPackage.Codecs
        |         |
        |         +--> AVPR.Staging
        |         +--> ARCExpect metadata setup
        |
        +--> PackageRegistryService mappings/models
        +--> AVPRClient.Interop
        +--> ARCExpect portable results/output

Published AVPR model/client artifacts
        |
        +--> pinned arc-validate dependency and packed-consumer verification

ARCExpect portable result/output contract
        |
        +--> shared Setup/ARCValidationPackage/Execute facade
                    |
                    +--> Pyxpecto structured runner
                    +--> internal portable output pipeline
                              |
                              +--> .NET filesystem adapter
                              +--> Python filesystem adapter
```

Milestone 3 can proceed alongside much of Milestones 1 and 2, provided its
model/client dependencies are updated at an agreed integration point.
Milestone 5 is intentionally non-blocking for Milestones 1–4.

---

## Compatibility gates

### Public API and generated client

- Preserve lower-camel-case CWL nested fields and the PascalCase `Inputs`
  metadata wrapper.
- Preserve scalar public command-input types such as `boolean?`.
- Verify exact raw API JSON separately from generated-client deserialization.
- Keep `AVPRClient` regeneration deterministic and generated-only.

### Database

- Verify that service-model changes do not create unintended pending EF model
  changes.
- Test intentional migrations and backfills against PostgreSQL; the in-memory
  test host is not sufficient database coverage.
- Preserve the normalized database representation of command-input types.

### Validation outputs

- Preserve `.arc-validate-results/<name>@<version>/`.
- Preserve `validation_summary.json`, `validation_report.xml`, and `badge.svg`
  paths and documented schema semantics.
- Prefer semantic/schema equivalence across targets over incidental JSON/XML
  formatting equality.

### CLI cache and execution

- Preserve the existing application-data cache location and readable cache
  format.
- Cover install, list, uninstall, and validation without relying on the live
  production registry.
- Use safe process argument handling for FSI and `uv run`.

---

## Deferred decisions

These are related but not part of this roadmap unless explicitly promoted into
their own issues:

- Split service API DTOs from EF persistence entities. The current decision is
  one service-owned model for both; reconsider only when the shapes need to
  evolve independently.
- Publish ARC specification validation as a validation package.
- Move `StagingArea/` into a separate repository.
- Retarget all non-portable projects to a newer .NET target framework.
- Define version synchronization and release automation across each library's
  NuGet, npm, and PyPI artifacts.

---

## Roadmap completion

The epic is complete when:

- `AVPRIndex` has no remaining production consumer and is retired.
- The portable model and codecs are available to AVPR, ARCExpect, and
  arc-validate without registry infrastructure dependencies.
- The registry service owns its ASP.NET/EF model and maps explicitly to the
  portable domain.
- `AVPRClient` is generated-only.
- ARCValidationPackages has been absorbed or deliberately retained based on
  verified external usage.
- AVPR artifacts are verified in AVPR and exact published pins are exercised
  by arc-validate's normal contract and packed-consumer checks.
- ARCExpect has a portable result/output contract and a documented path from
  the temporary Expecto boundary to Pyxpecto.
- ARCExpect exposes `Setup`, `ARCValidationPackage`, and `Execute` from its
  .NET, JavaScript, and Python packages, using target-native model and codec
  dependencies.

All completion criteria above are satisfied by the tracked AVPR and
arc-validate issue set.
