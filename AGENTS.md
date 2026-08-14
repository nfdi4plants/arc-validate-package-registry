# AGENTS.md

## Repository purpose

This repository is both the source of the ARC validation package registry and the submission/staging area for validation packages served through <https://avpr.nfdi4plants.org>. The packages are consumed by `arc-validate` in DataHUB validation pipelines and run in CI jobs.

Validation packages are self-contained, single-file F# (`.fsx`) or Python (`.py`) scripts enriched with YAML frontmatter. Contributions enter through GitHub under `StagingArea/`, are checked by a strict test and publication pipeline, and become immutable once published. Updates to a published package must use a new semantic version.

## Repository map

- `StagingArea/<package-name>/<package-name>@<semver>.fsx|py`: submitted validation packages.
- `StagingAreaTests/`: package layout, metadata, naming, and script sanity checks.
- `src/ValidationPackage.Model/`: portable metadata, CWL inputs, package identity, and SemVer behavior for .NET and Fable.
- `src/ValidationPackage.Codecs/`: portable YAML frontmatter and JSON codecs for the shared model.
- `src/AVPR.Staging/`: internal staged-package discovery, normalized content, and content hashing built on the portable model/codecs.
- `src/AVPRClient/`: generated/consumer-facing .NET API client.
- `src/AVPRClient.Interop/`: explicit mappings between generated client DTOs and the portable model.
- `src/AVPRCI/`: CLI used to publish packages.
- `src/PackageRegistryService/`: ASP.NET Core registry API, website, database model, and migrations.
- `tests/`: tests for the portable model/codecs, staging infrastructure, client interop, and API.
- `build/`: FAKE/BlackFox build project containing local and CI build, test, and pack targets.
- `.github/workflows/ci.yml`: path-focused main solution tests for core libraries and clients.
- `.github/workflows/portable-ci.yml`: cross-target Model and Codecs contract and packed-consumer tests.
- `.github/workflows/staging-ci.yml`: path-focused Windows staging checks with `uv`.
- `.github/workflows/service-image.yml`: service tests and `release`/`dev` container publication.
- `.github/workflows/release-model.yml` and `release-codecs.yml`: package-oriented, manually dispatched NuGet/npm/PyPI trusted publication.
- `.github/workflows/release-client.yml` and `release-client-interop.yml`: manually dispatched client gates and NuGet publication.
- `.github/workflows/release-all.yml`: manually checks committed versions against their registries and dispatches only package workflows with missing artifacts.
- `.github/workflows/release-package.yml`: reusable NuGet trusted-publishing job used only by the client release workflows.
- `.github/workflows/verify-release.yml`: reusable core/portable release gate; it never publishes.
- `.github/workflows/run-build-target.yml`: reusable cross-platform build-target runner.
- `.github/actions/setup-portable/`: composite portable pack-toolchain setup; composite actions retain the caller workflow identity used by trusted publishing.

## Toolchain and common commands

The pinned SDK is in `global.json` (currently .NET 10). Python package execution/dependency resolution uses `uv`; package dependencies must be declared as PEP 723 inline script metadata.

```shell
# Main libraries, service, CLI, and their tests
.\build.cmd TestSolution

# Staging-area checks (compile-checks all packages and executes two fixtures)
.\build.cmd TestStagingArea

# Portable model/codecs across .NET, JavaScript, and Python, including packed consumers
.\build.cmd TestPortableModel
.\build.cmd TestPortableCodecs
.\build.cmd TestNativeValidationPackages

# Produce NuGet/npm/Python artifacts under artifacts/packages
.\build.cmd PackModel
.\build.cmd PackCodecs
.\build.cmd PackClient
.\build.cmd PackClientInterop

# Focused test projects
dotnet test tests/AVPR.Staging.Tests/AVPR.Staging.Tests.fsproj
dotnet test tests/ClientTests/ClientTests.fsproj
dotnet test tests/APITests/APITests.csproj
dotnet test StagingAreaTests/StagingAreaTests.fsproj

# Inspect a publication without pushing
dotnet run --project src/AVPRCI/AVPRCI.fsproj -- publish --api-key <key> --dry-run
```

Use `./build.sh` instead of `.\build.cmd` on Linux or macOS. Keep CI command
details in the build project; workflows should install platform toolchains and
invoke a named target rather than duplicate build logic inline.

Treat the Fable compiler and Python `fable-library` as a coordinated toolchain.
When either pin changes, update the `uv` lock and run both portable targets;
the Fable API packages have independent version lines and should use their
current compatible releases.

Prefer focused builds/tests while developing, then run the affected solution before handing off. Do not perform a non-dry-run publication unless the user explicitly requests it and provides the necessary authorization.

## Validation package rules

- Put each package exactly one directory below `StagingArea/`.
- Match directory, filename, and metadata name/version: `<name>/<name>@<major>.<minor>.<patch>.<fsx|py>`.
- Keep packages self-contained and single-file. Only `.fsx` and `.py` files are allowed in `StagingArea/`.
- Put valid YAML frontmatter at the start of the script using the language-specific form documented in `README.md`.
- Prefer binding the metadata to `PACKAGE_METADATA` so the script can reuse it.
- F# external dependencies must use `#r "nuget: ..."`; Python dependencies must use `uv` inline script metadata.
- Treat a package with `Publish: true` as release-sensitive. Never modify a version known to be published; add a higher version instead.
- Preserve semantic-version suffixes when a filename uses them, and keep metadata and filename versions identical.
- Package scripts may perform network access, traverse input data, or do substantial computation. Read a script before invoking it locally.

## Staging-test strategy

`StagingAreaTests/PackageSanityChecks.fs` performs non-executing checks for every real package. F# scripts are parsed and type-checked with `FSharp.Compiler.Service`; Python source is passed to the built-in `compile` function without importing it. Only the small `single fsharp package runs` and `single python package runs` fixtures execute script code.

When changing these checks:

- Keep structural, filename, frontmatter, and metadata validation separate from language syntax/type checks.
- Keep real staged packages on the non-executing path in the default PR gate.
- Do not describe `dotnet fsi` as compile-only: loading a script executes its top-level expressions.
- Preserve actionable diagnostics that identify the failing package.
- Add small positive and negative fixtures for check behavior instead of using the entire staging area to test the checker itself.

## Code and test conventions

- Follow the style already present in the touched project: F# modules and pipeline-oriented code in F# projects, conventional C# in the service/client projects.
- Keep F# source order in `.fsproj` files correct; F# files compile in listed order.
- Use xUnit for existing tests and give test names behavior-oriented descriptions consistent with neighboring tests.
- Avoid broad formatting or generated-file churn unrelated to the change.
- Do not edit `bin/`, `obj/`, `.vs/`, or other build output.
- Preserve unrelated working-tree changes.

## Portable Fable code and tests

`ValidationPackage.Model` is compiled once from F# for .NET, JavaScript, and
Python. Treat transpiled API shape and behavior as part of its public contract.

- Keep the portable model free of YAML/JSON implementations, STJ attributes,
  filesystem, hashing, EF, HTTP, OpenAPI, generated-client, and AVPR staging
  types. Those concerns belong to codecs or application-specific boundaries.
- Prefer public classes over records when values are intended for direct use
  from JavaScript or Python. Mark every public portable class with
  `[<AttachMembers>]` so instance and static members remain attached to the
  emitted class.
- Implement settable properties with an explicitly named mutable backing field
  such as `_name`. Do not use `member val ... with get, set` in portable public
  classes: Fable emits compiler-generated fields such as `Name@`, which leak an
  awkward native API. Do not shadow constructor parameters with backing fields.
- Prefer static members on public classes to modules when the behavior belongs
  to a domain type. Private implementation modules are fine.
- Avoid reflection and target-specific APIs in portable sources. When a
  standard-library API may differ across targets, cover its behavior in the
  shared cross-target suite.
- After changing a portable public type, transpile it and inspect the generated
  JavaScript and Python under `artifacts/portable/`. Check for detached
  functions, `@`-suffixed fields, mangled names, and target-only failures.
- Keep F# source order explicit in every portable `.fsproj`. Pack the project
  and ordered `.fs`/`.fsi` sources under the NuGet package's `fable/` path.
- `PackModel` and `PackCodecs` each produce a NuGet package, an npm tarball,
  and a Python wheel. Native Codecs artifacts must depend on the corresponding
  native Model artifact and must not bundle a private generated Model copy.

Portable contract tests use only `Fable.Pyxpecto` as their test framework. They
are a regular executable, not a VSTest project:

- Run the .NET suite with `dotnet run`, not `dotnet test`.
- Transpile the same suite with the pinned local Fable tool and run the emitted
  entry point with Node or the uv-managed Python interpreter.
- Keep the root `package.json` marked as `"type": "module"` so Node treats
  Fable output as ESM.
- Declare Python runtime dependencies in the root `pyproject.toml`, commit
  `uv.lock`, and run Python output with `uv run`. Do not install dependencies
  ad hoc into a system Python.
- `.venv` is platform-specific and ignored. If a workspace moves between
  operating systems, recreate that exact `.venv` before running `uv sync` or
  `uv run`; never reuse a foreign-platform environment.
- Generated JS/Python belongs under ignored `artifacts/` and must not be
  committed.

## Cross-cutting model changes

Validation package metadata changes commonly require coordinated edits in:

- `src/ValidationPackage.Model/`
- `src/ValidationPackage.Codecs/` when YAML or domain JSON is affected
- `src/PackageRegistryService/Models/ValidationPackage.cs`
- `src/PackageRegistryService/Models/ValidationPackageDb.cs` (register owned-JSON collection fields with `OwnsMany(...).ToJson()`)
- `src/PackageRegistryService/Data/DataInitializer.cs`
- Entity Framework migrations under `src/PackageRegistryService/Migrations/`
- generated client code in `src/AVPRClient/AVPRClient.cs` **and** portable mappings in `src/AVPRClient.Interop/Mappings.cs` (easy to miss)
- website rendering under `src/PackageRegistryService/Pages/Components/` when a field should be shown
- frontmatter/metadata tests and README documentation

Search for every use of the changed field before editing. Do not hand-author a migration unless the existing migration workflow requires it.

## CWL input contract

- Keep the AVPR metadata wrapper as PascalCase `Inputs`. Inside it, preserve the exact lower-camel-case CWL names (`id`, `type`, `label`, `doc`, `inputBinding`, `prefix`, and `position`) through frontmatter, public JSON, OpenAPI, and generated-client code.
- Public YAML/JSON and the generated client expose `CommandInputType` as one scalar CWL string such as `boolean?`. Only the database uses the normalized object with a lowercase primitive string and boolean nullability; never leak that storage shape through the API.
- `inputBinding.prefix` is required, non-empty, exact, and unique. Positional inputs and joined-prefix behavior are unsupported. `position` defaults to `0`; argument materialization orders by position and then ordinal input ID.
- Reject unknown parameter and binding fields. Validate declaration IDs, types, required bindings, prefix uniqueness, `--`, and collisions with `-i`, `-o`, `--source-branch`, and `--source-commit-hash` through the portable Model validator rather than reimplementing those rules.
- Do not add custom aliases or requiredness fields. CWL has one canonical binding prefix, and requiredness is represented by the scalar type with or without `?`.

## Validation-package YAML schemas and configuration

- Canonical validation-package frontmatter and `.arc/validation_packages.yml` documents carry the exact immutable v1 `$schema` URI. Dispatch that identifier through the portable offline allowlist; never fetch an unknown schema URI or fall back from one.
- Keep schema-less frontmatter/config handling in explicit legacy readers. Schema-less frontmatter cannot declare the unreleased `Inputs` shape, and canonical writers never emit legacy documents.
- Configuration input values use only quoted strings, lowercase `null`/booleans, and finite JSON-form numbers. Preserve numeric lexemes so JavaScript cannot round valid signed 64-bit integers.
- Keep the reviewed Draft 2020-12 documents under `schemas/` and package them byte-for-byte under `schemas/` in every Codecs artifact. JSON Schema complements rather than replaces the stricter YAMLicious lexical and semantic checks.

## Test coverage for metadata model changes

Trace a metadata field through every representation it affects; a green build alone does not prove parsing or mapping behavior.

- In `tests/ValidationPackage.Model.Tests/`, exercise factory/default/equality and semantic-version behavior across .NET, JavaScript, and Python.
- In `tests/ValidationPackage.Codecs.Tests/`, cover full and mandatory metadata, comment/binding frontmatter, F#/Python forms, optional-field backwards compatibility, malformed values, and unknown-key policy.
- In `tests/AVPR.Staging.Tests/`, verify normalized content and hashes. Fixture byte changes require recomputing expected hashes from the actual content.
- In `tests/ClientTests/`, keep equivalent portable-model and generated-client reference objects. Test both mapping directions, nested collection conversion, null/empty behavior, CWL scalar variants, and full SemVer suffixes in `TypeExtensionsTests.fs`.
- Run `PackageStagingArea.slnx` when submitted-package syntax or sanity checks are affected. Prefer small codec/staging fixtures; add a real staged package only when the real layout must be exercised, always as a new semantic version and only after reading it.
- For service/client contract behavior, follow the shared in-process test-host design below. Explicitly verify migrations and backfills against Postgres, along with seeded JSON persistence and both present/absent website rendering cases when those behaviors change.

Use focused model/codec/staging/client tests while iterating, then run the main solution and, when package syntax is affected, the staging solution. Recompute expected values from actual fixture content instead of weakening assertions.

## In-process registry test host

`tests/PackageRegistryTestHost` is shared infrastructure for API and generated-client contract tests. `PackageRegistryWebApplicationFactory` boots the real `PackageRegistryService` entry point and replaces Npgsql with a uniquely named EF in-memory database. The `Testing` environment must remain free of external PostgreSQL health checks, HTTPS redirection, migrations, and staging-data initialization.

When adding tests:

- Create and dispose a factory per test or fixture. Do not share mutable database state between unrelated tests.
- Seed through `SeedPackageAsync` or `SeedPackagesAsync`; the helpers deliberately create matching integrity-hash and zero-download records required by the real handlers.
- In `APITests`, use `factory.CreateClient()` to assert HTTP status and exact raw JSON property/value shapes.
- In `ClientTests`, pass the factory client to `AVPRClient.Client`, replace its production `BaseUrl` with `httpClient.BaseAddress.ToString()`, and assert the generated typed result.
- For a cross-cutting wire-model change, prefer both a raw API assertion and a generated-client assertion. The former identifies service serialization errors; the latter identifies generated-client route or deserialization drift.
- Do not treat the in-memory host as database integration coverage. It does not verify PostgreSQL migrations, `jsonb` storage, backfills, Npgsql conversions, or relational behavior. Retain EF model assertions and perform focused PostgreSQL verification when persistence changes.

## CI and release safety

- Each published library and the registry service owns its version in its
  project file (`PackageVersion` for packages and `Version` for the service).
  Its first versioned `RELEASE_NOTES.md` heading must be
  `## <version> - YYYY-MM-DD`; the service may append ` - <release title>`.
  Run `./build.cmd ValidateReleaseMetadata` after changing either side. Main
  builds, staging checks, and every package target enforce the same gate.
- During a prerelease line, keep one rolling top release-notes entry and bump
  its preview version in place. Preserve published non-preview entries as
  immutable history.
- `ci.yml` owns path-focused main solution tests for core libraries and clients. Downstream package compatibility belongs to the downstream repository's normal CI and release process; AVPR workflows must not clone and rebuild `arc-validate`.
- `portable-ci.yml` owns cross-target Model and Codecs tests. It runs both portable targets when either portable implementation, consumer test, or shared build tool changes.
- `staging-ci.yml` owns `StagingArea/**`, staging-checker, staging-model/codec, and relevant build-tool changes. It runs `TestStagingArea` on Windows with `uv` installed.
- Routine `dev`, manual, and package-release verification runs `TestSolution` on Ubuntu. Pushes to `release` and pull requests targeting `release` additionally run it on Windows and macOS; this is the repository's cross-platform gate.
- `service-image.yml` owns the service project, its local project references, bundled `StagingArea`/documentation content, and Docker build inputs. It runs the applicable `TestSolution` gate before publishing an image on a branch push. Pull requests and the default manual dry run never publish.
- `dev` is the default, long-lived integration branch whose service changes publish `ghcr.io/nfdi4plants/avpr:dev`; `release` is the production branch and publishes `ghcr.io/nfdi4plants/avpr:release`. Package publication is never triggered by a branch push.
- `release-model.yml` and `release-codecs.yml` each rerun core and portable gates, pack once, preserve that build as a workflow artifact, and publish it through independent NuGet, npm, and PyPI jobs from a protected `release` environment. A failed registry job must be retriable without rebuilding or rerunning successful registry jobs. Release Model before Codecs when both versions change.
- `release-all.yml` is an orchestration convenience only. It validates release metadata, checks the exact committed versions on NuGet/npm/PyPI, and dispatches the existing package-specific workflows for incomplete versions. It must never publish directly or replace their trusted-publisher identities.
- Model and Codecs NuGet trusted-publisher policies must name their direct workflow (`release-model.yml` or `release-codecs.yml`). Client and Interop policies must name the called reusable workflow `release-package.yml`, which NuGet observes through `job_workflow_ref`.
- npm trusted-publisher policies for the scoped Model and Codecs packages must name the corresponding direct package workflow and allow `npm publish`. These workflows use Node 24, npm with trusted-publishing support, and no npm token. They publish prerelease versions with the `preview` dist-tag and stable versions with `latest`.
- PyPI trusted-publisher policies for Model and Codecs must name the corresponding direct, top-level package workflow. Do not move PyPI publishing into a reusable workflow.
- NuGet trusted-publishing policies are owner-wide and cannot be scoped to an
  individual package. The current policy owner and package owner is the
  `Mutagene` nuget.org account, and `NUGET_USER` must contain that same profile
  name. Separate policies restrict the repository/workflow identity, but each
  successful policy can publish any NuGet package owned by that account. npm
  publishers are configured per scoped package and PyPI publishers per project,
  using the exact field matrix in `docs/operations/releases.md`.
- All package publish jobs need `id-token: write` and the `release` environment. `NuGet/login` exchanges OIDC for a temporary key; never restore a long-lived `NUGET_KEY`. `NUGET_USER` is a repository Actions variable containing only the public nuget.org profile name associated with the policy. Repository variables remain available to same-repository reusable workflows without forwarding secrets.
- A staged package marked for publication can be pushed to the production registry after checks pass.
- Workflow actions should remain pinned to deliberate versions/commits. Preserve least-privilege permissions and never print secrets.

Before completing a change, report which focused and solution-level checks were run, and call out anything skipped because it would require unavailable external services.
