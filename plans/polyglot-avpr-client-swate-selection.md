# Plan: Polyglot AVPR client and Swate package selection

## Status

**Proposed as a strict follow-up to the accepted validation-package
configuration plan. Implementation and issue creation have not started.**

Predecessor:

- [Safe, configurable validation-package execution](validation-packages-config.md)
- Tracking epic: [AVPR #119](https://github.com/nfdi4plants/arc-validate-package-registry/issues/119)

This plan begins only after:

- All seven predecessor steps and their acceptance gates have completed.
- AVPR #119 and its rollout issue #122 are complete.
- The final service is deployed to `dev`.
- The generated AVPRClient and `AVPRClient.Interop` previews have been
  published.
- `arc-validate`, AVPRCI, and DataHUB are operating with the completed
  configuration flow.

Throughout this document, **post-epic `dev`** means the future state of the
`dev` branch and deployed `dev` service after every issue in AVPR #119,
including rollout issue #122, has completed. It does not mean today's `dev`
state or the production environment.

This plan does not amend, replace, reorder, or add requirements to any
predecessor step. In particular, generating AVPRClient and publishing Interop
remain required predecessor deliverables. They provide the functioning baseline
that this follow-up will subsequently replace.

The follow-up has two outcomes:

1. Replace the generated .NET-only registry client and Interop layer with a full
   polyglot client matching the completed `dev` OpenAPI.
2. Add a Swate Electron GUI for preparing an in-memory package/version selection
   without taking ownership of `.arc/validation_packages.yml`.

No GitHub issues should be created until this plan is accepted.

---

## 1. Preamble: moving parts and ownership boundaries

### Completed predecessor system

The predecessor plan owns the configuration format, schemas, registry
endpoints, version resolution, execution plan, safe child execution, ARC
specification, DataHUB orchestration, and initial rollout.

This follow-up consumes those completed contracts unchanged:

- The OpenAPI generated from post-epic `dev` is the HTTP source of truth.
- AVPR Model and Codecs are the canonical portable contract packages.
- The generated AVPRClient and Interop previews provide a behavior and
  compatibility baseline.
- `arc-validate` already uses lightweight package discovery and exact metadata
  retrieval.
- DataHUB already consumes `validation_plan.json`.
- No service or configuration-format redesign is part of this follow-up.

If client implementation reveals an ambiguity or defect in the completed
OpenAPI, it must be reported as a separate service bug. The client must not
silently invent or change wire behavior.

### `DataHubClient`

Repository: `nfdi4plants/DataHubClient`

Relevant locations:

- `src/DataHubClient`
- The .NET, JavaScript, and Python project files.
- `build/PackageTasks.fs`
- Packed-consumer tests and `docs/samples`.

DataHubClient is the architectural reference for the new AVPRClient. It
defines:

- one ordered F# source tree;
- parallel .NET, JavaScript, and Python projects;
- transport-independent requests and responses;
- .NET, Fetch, and httpx transports;
- resource-oriented APIs;
- hand-written Thoth codecs;
- portable attached classes;
- target-native asynchronous APIs; and
- packed-artifact testing.

DataHubClient is not a runtime dependency of AVPRClient.

This follow-up first adds manual Fable NuGet source packaging and TypeScript
declarations to DataHubClient. AVPRClient then adopts that completed
distribution pattern exactly.

### AVPR Model and Codecs

Repository: `nfdi4plants/arc-validate-package-registry`

Relevant locations:

- `src/ValidationPackage.Model`
- `src/ValidationPackage.Codecs`
- Portable contract suites.

These completed predecessor packages remain the canonical owners of identities,
semantic versions, metadata, declarations, configuration types, and YAML/JSON
schemas.

This follow-up does not change their domain contracts. It adds TypeScript
declarations to their npm artifacts because AVPRClient's TypeScript API exposes
their public types.

### `PackageRegistryService`

Repository: `nfdi4plants/arc-validate-package-registry`

Relevant locations:

- `src/PackageRegistryService/API`
- `src/PackageRegistryService/OpenAPI`
- Service contract tests.

The service is read-only scope for this plan. Its post-epic `dev` OpenAPI
defines which client operations must exist, including:

- lightweight discovery and metadata;
- legacy package retrieval and publication;
- package-content verification; and
- download statistics.

The client preserves OpenAPI operation behavior, JSON casing, status codes,
authentication, and deprecation metadata.

The two schema routes, health route, version route, and website routes remain
outside the client because the predecessor design intentionally excludes them
from OpenAPI.

### `AVPRClient`

Repository: `nfdi4plants/arc-validate-package-registry`

Relevant locations:

- `src/AVPRClient`
- New shared and target-specific client test projects.
- Existing build, package, documentation, and release targets.

AVPRClient becomes the maintained polyglot HTTP client for:

- .NET/F#;
- JavaScript/TypeScript;
- Python; and
- Fable applications consuming its NuGet package.

It owns:

- HTTP abstractions;
- target-specific transports;
- optional API-key authentication;
- registry resources;
- JSON codecs;
- registry-only wrapper models;
- typed errors; and
- NuGet, npm, and PyPI artifacts.

The generated implementation is replaced only after parity with the completed
OpenAPI is proven.

### `AVPRClient.Interop`

Repository: `nfdi4plants/arc-validate-package-registry`

Relevant location:

- `src/AVPRClient.Interop`

Interop remains intact throughout the predecessor plan and while the new client
is developed.

It is retired in this follow-up after:

- the new AVPRClient is published;
- all current consumers are migrated;
- equivalent canonical model behavior is tested; and
- no remaining consumer references it.

### Existing AVPR consumers

Repositories and locations:

- `nfdi4plants/arc-validate-package-registry`, primarily `src/AVPRCI`.
- `nfdi4plants/arc-validate`, in the registry-resolution and package-management
  modules delivered by the predecessor plan.

These consumers migrate from generated DTOs and Interop to the canonical types
returned directly by the polyglot client.

DataHUB and ARCExpect do not consume AVPRClient directly and require no changes.

### Swate reusable components

Repository: `nfdi4plants/Swate`

Relevant location:

- `src/Components/src/Page/ValidationPackages`
- Colocated Storybook stories and behavioral tests.

The Components project owns the application-agnostic selection UI. It receives
available package identities and versions through injected properties or
loaders.

It contains no AVPRClient, Electron, IPC, ARC state, or file-persistence logic.

### Swate Electron

Repository: `nfdi4plants/Swate`

Relevant locations:

- `src/Electron/src/Main`
- `src/Electron/src/Preload`
- `src/Electron/src/Swate.Electron.Shared`
- `src/Electron/src/Renderer`

Electron owns:

- the AVPRClient instance;
- network access;
- application-lifetime caching;
- typed IPC;
- navigation;
- page-local draft state; and
- confirmation before discarding a draft.

Ownership of `.arc/validation_packages.yml` readers and writers remains outside
this follow-up.

### End-to-end target flow

1. The completed predecessor system remains deployed and operational.
2. DataHubClient establishes the reusable Fable and TypeScript distribution
   pattern.
3. AVPRClient is rewritten against the completed `dev` OpenAPI.
4. The new client is tested alongside the existing generated-client baseline.
5. New NuGet, npm, and PyPI previews are published.
6. `arc-validate` and AVPRCI migrate to the new client.
7. Generated code and AVPRClient.Interop are retired.
8. Swate Electron loads the lightweight package index through the new client.
9. Electron maps identities into a renderer-specific catalog.
10. The reusable page lets the user create an in-memory package/version draft.
11. No ARC configuration file is read or written.

---

## 2. Proposed changes and owners

| Change | Owner |
| --- | --- |
| Reference Fable NuGet and TypeScript distribution | DataHubClient |
| TypeScript declarations for canonical AVPR packages | AVPR Model/Codecs |
| Completed HTTP/OpenAPI contract | Existing predecessor system; unchanged |
| Polyglot HTTP client and target transports | AVPRClient |
| Registry wrapper types and HTTP codecs | AVPRClient |
| Migration from generated DTOs and Interop | `arc-validate`, AVPRCI |
| Removal of generated client infrastructure | AVPR repository |
| Reusable package/version selection UI | Swate Components |
| Network, caching, IPC, route, and draft state | Swate Electron |
| YAML configuration reading/writing | Deferred |

---

## 3. Target client contract

### OpenAPI parity

The client targets every operation present in the post-epic `dev` OpenAPI:

| Resource | Operation |
| --- | --- |
| Packages | Get lightweight package index |
| Packages | Get versions for a package |
| Packages | Get metadata for an exact identity |
| Packages | Get all packages with content |
| Packages | Get the latest stable package by name |
| Packages | Get an exact package artifact |
| Packages | Create a package |
| Verification | Verify a package-content hash |
| Statistics | Get all download statistics |
| Statistics | Get download statistics by name |
| Statistics | Get download statistics by exact identity |

The heavy all-content collection operation remains available and deprecated.
Other legacy operations retain the deprecation status present in the completed
OpenAPI.

A parity test records the completed operation-ID inventory and deprecation
flags. Every operation must have a compiled client invocation and an in-process
behavioral test.

### Client facade

```fsharp
AVPRClient(baseUrl: string, ?authentication: Authentication)
```

The facade exposes:

- settable `.Http`;
- `.Packages`;
- `.Verification`; and
- `.Statistics`.

`Authentication.ApiKey(key)` contributes `X-API-KEY`. Authentication is
optional because read and verification operations are public.

### Packages resource

```fsharp
ListAsync()
ListVersionsAsync(name)
GetMetadataAsync(name, version)
ListAllWithContentAsync()
GetLatestAsync(name)
GetAsync(name, version)
CreateAsync(package)
```

`ListAllWithContentAsync` is marked obsolete consistently across target
documentation and TypeScript declarations.

### Verification and statistics resources

```fsharp
Verification.VerifyContentAsync(contentHash)

Statistics.ListDownloadsAsync()
Statistics.ListDownloadsByNameAsync(name)
Statistics.GetDownloadsAsync(name, version)
```

### Portable client representations

Reuse completed Model types for semantic versions, identities, metadata,
authors, ontology annotations, and input declarations.

Add AVPRClient-owned HTTP representations:

- `RegisteredValidationPackage`
  - canonical metadata;
  - ISO-8601 release date;
  - computed identity.
- `ValidationPackageArtifact`
  - registered package;
  - lossless Base64 package content.
- `PackageContentHash`
  - canonical identity;
  - hash.
- `PackageDownloadStatistics`
  - canonical identity;
  - download count.

Hand-written codecs map these composed types to the completed service's flat
PascalCase JSON without changing the wire format.

---

## 4. Follow-up implementation plan

### Step 1 - Establish the distribution reference in DataHubClient

Repository: `nfdi4plants/DataHubClient`

Implement manual YAMLicious-style Fable source packing:

- Package the JavaScript project's ordered source list under
  `fable/DataHubClient.fsproj`.
- Include the Fetch transport.
- Exclude .NET and Python transports from the Fable project view.
- Continue shipping the normal .NET assembly.

Add TypeScript declaration generation and npm metadata.

Acceptance gate:

- Packed NuGet works from external .NET and Fable projects.
- Packed npm works from JavaScript and strict TypeScript projects.
- Existing Python and native tests remain green.
- Executable documentation samples consume packed artifacts.

### Step 2 - Add TypeScript declarations to AVPR Model and Codecs

Repository: `nfdi4plants/arc-validate-package-registry`

Do not modify the completed domain contracts or schemas.

Add:

- declaration generation;
- npm `types` and export metadata;
- import-path normalization; and
- strict TypeScript packed-consumer tests.

Acceptance gate:

- Existing .NET, JavaScript, Python, schema, and codec suites remain unchanged
  and green.
- Strict TypeScript consumers can use the completed canonical types.
- NuGet and PyPI artifacts remain compatible.

### Step 3 - Implement the full polyglot AVPRClient

Repository: `nfdi4plants/arc-validate-package-registry`

Replace the implementation under `src/AVPRClient` with the DataHubClient
architecture:

- one ordered F# source tree;
- .NET, JavaScript, and Python projects;
- transport abstraction;
- .NET, Fetch, and httpx transports;
- optional API-key authentication;
- package, verification, and statistics resources;
- hand-written codecs;
- typed status/body errors; and
- host-native async APIs.

Use the completed OpenAPI and generated client as behavioral baselines. Do not
change the service.

Acceptance gate:

- Every completed OpenAPI operation is covered.
- Shared suites pass on .NET, Node, and Python.
- URL encoding, authentication, SemVer, Base64, PascalCase codecs, and typed
  errors are tested.
- Legacy and deprecated operations remain functional.
- Python remains async-only.

### Step 4 - Package and publish the new AVPRClient

Repository: `nfdi4plants/arc-validate-package-registry`

Add:

- manual Fable source packing under `fable/AVPRClient.fsproj`;
- TypeScript declarations;
- NuGet, npm, and PyPI packages;
- packed-consumer tests; and
- executable documentation samples.

Publish a coordinated preview without removing the previous generated-client
versions.

Acceptance gate:

- External .NET, Fable, JavaScript, strict TypeScript, and Python consumers
  pass.
- Published previews are indexed on all target registries.
- Package contents and versions are recorded for consumer migration.

### Step 5 - Migrate consumers and retire generated infrastructure

Repositories:

- `nfdi4plants/arc-validate`
- `nfdi4plants/arc-validate-package-registry`

Migrate:

- `arc-validate` registry resolution.
- AVPRCI discovery and publication.
- Any remaining in-repository client consumers.

After all consumers pass:

- remove NSwag generation;
- remove generated client source;
- remove `AVPRClient.Interop`;
- remove Interop release workflows; and
- document the preview breaking change.

Acceptance gate:

- `arc-validate` resolves the same plans against AVPR `dev`.
- AVPRCI performs lightweight discovery and authenticated publication.
- No maintained project references Interop or generated DTOs.
- The original configuration and DataHUB integration suites remain green.

### Step 6 - Build the reusable Swate selector

Repository: `nfdi4plants/Swate`

Location:

- `src/Components/src/Page/ValidationPackages`

Implement:

- searchable package selection;
- dependent version selection;
- multiple addable/removable rows;
- highest-stable default with prerelease fallback;
- test-package filtering and a "Show test packages" toggle;
- loading, empty, retry, duplicate, keyboard, and accessibility states; and
- injected catalog loading.

Acceptance gate:

- Storybook documents all meaningful states.
- Interaction tests cover selection, defaults, filtering, duplicates, removal,
  and retry.
- The component has no Electron, AVPRClient, IPC, or persistence dependency.

### Step 7 - Integrate the selector into Swate Electron

Repository: `nfdi4plants/Swate`

Locations:

- `src/Electron/src/Main`
- `src/Electron/src/Preload`
- `src/Electron/src/Swate.Electron.Shared`
- `src/Electron/src/Renderer`

Implement:

- AVPRClient NuGet consumption.
- Main-process package-index loading.
- Concurrent-request coalescing.
- Application-lifetime successful-result caching.
- Retryable failures.
- Typed IPC and preload exposure.
- Renderer catalog mapping.
- Route and left action-rail entry.
- Page-local draft state.
- Confirmation before discarding a non-empty draft.

Acceptance gate:

- Packaged Electron loads choices from the completed AVPR `dev` service.
- Cache, coalescing, failure retry, mapping, and IPC are behaviorally tested.
- The selector works without creating or changing
  `validation_packages.yml`.

---

## 5. GitHub issue structure

This follow-up has its own tracking hierarchy. It is not added to the delivery
checklist of #119 and does not block closure of the predecessor epic.

No issues should be created until this plan is accepted.

### Cross-repository dependency order

```text
Completed AVPR #119 and #122
  -> DataHubClient distribution reference
    -> AVPR Model/Codecs TypeScript declarations
      -> full polyglot AVPRClient
        -> AVPRClient preview publication
          -> arc-validate + AVPRCI migration
            -> generated client and Interop retirement
              -> Swate live Electron integration

Swate reusable UI may proceed against fixtures after this follow-up begins.
```

### Follow-up umbrella

**New AVPR epic: `[Epic] Replace the generated AVPR client with a polyglot
client`**

- Blocked by completed AVPR #119 and #122.
- References the predecessor plan without becoming part of it.
- Tracks DataHubClient, AVPRClient, and downstream migration issues.
- Links the Swate epic as related follow-up functionality.

### DataHubClient repository

**New feature: `[Feature] Add Fable NuGet support and TypeScript declarations to
DataHubClient`**

Suggested subissues:

1. `[Feature] Package DataHubClient F# sources for downstream Fable applications`
2. `[Feature] Publish TypeScript declarations for DataHubClient`

### `arc-validate-package-registry` repository

Children of the new AVPR follow-up epic:

1. `[Feature] Publish TypeScript declarations for ValidationPackage Model and Codecs`
2. `[Feature] Implement a full DataHubClient-style polyglot AVPRClient`
3. `[Feature] Package AVPRClient for NuGet, npm, PyPI, and Fable`
4. `[Feature] Migrate AVPRCI to the polyglot AVPRClient`
5. `[Cleanup] Retire NSwag-generated AVPRClient and AVPRClient.Interop`

### `arc-validate` repository

Related child of the new AVPR follow-up epic:

- `[Feature] Migrate registry resolution to the polyglot AVPRClient`

This migration must preserve the completed predecessor behavior and its exact
`validation_plan.json` output.

### Swate repository

**New epic: `[Epic] Select ARC validation packages in Swate Electron`**

Use `Type: Enhancement`. Electron-specific issues select the "Electron Swate
App" host.

Children:

1. `[Feature Request]: Add reusable validation-package selection components`
2. `[Feature Request]: Add AVPR discovery and typed IPC to Electron`
3. `[Feature Request]: Integrate validation-package selection into Electron navigation`

The Swate epic is related to, but does not block, the AVPR client replacement
epic. YAML persistence remains a separate future feature.
