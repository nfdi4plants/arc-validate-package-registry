# AGENTS.md

## Repository purpose

This repository is both the source of the ARC validation package registry and the submission/staging area for validation packages served through <https://avpr.nfdi4plants.org>. The packages are consumed by `arc-validate` in DataHUB validation pipelines and run in CI jobs.

Validation packages are self-contained, single-file F# (`.fsx`) or Python (`.py`) scripts enriched with YAML frontmatter. Contributions enter through GitHub under `StagingArea/`, are checked by a strict test and publication pipeline, and become immutable once published. Updates to a published package must use a new semantic version.

## Repository map

- `StagingArea/<package-name>/<package-name>@<semver>.fsx|py`: submitted validation packages.
- `StagingAreaTests/`: package layout, metadata, naming, and script sanity checks.
- `src/AVPRIndex/`: F# domain types and utilities for package metadata, frontmatter, hashes, and indexes.
- `src/AVPRClient/`: generated/consumer-facing .NET API client.
- `src/AVPRCI/`: CLI used to publish packages.
- `src/PackageRegistryService/`: ASP.NET Core registry API, website, database model, and migrations.
- `tests/`: tests for the index, client, and API.
- `.github/workflows/pipeline.yml`: change detection and orchestration for tests and releases.
- `.github/workflows/build-and-test-solution.yml`: reusable .NET build/test workflow.

## Toolchain and common commands

The pinned SDK is in `global.json` (currently .NET 10). Python package execution/dependency resolution uses `uv`; package dependencies must be declared as PEP 723 inline script metadata.

```shell
# Main libraries, service, CLI, and their tests
dotnet build arc-validate-package-registry.sln --configuration Release
dotnet test arc-validate-package-registry.sln --configuration Release

# Staging-area checks (compile-checks all packages and executes two fixtures)
dotnet build PackageStagingArea.sln --configuration Release
dotnet test PackageStagingArea.sln --configuration Release --no-build

# Focused test projects
dotnet test tests/IndexTests/IndexTests.fsproj
dotnet test tests/ClientTests/ClientTests.fsproj
dotnet test StagingAreaTests/StagingAreaTests.fsproj

# Inspect a publication without pushing
dotnet run --project src/AVPRCI/AVPRCI.fsproj -- publish --api-key <key> --dry-run
```

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

## Cross-cutting model changes

Validation package metadata changes commonly require coordinated edits in:

- `src/AVPRIndex/Domain.fs`
- `src/PackageRegistryService/Models/ValidationPackage.cs`
- `src/PackageRegistryService/Data/DataInitializer.cs`
- Entity Framework migrations under `src/PackageRegistryService/Migrations/`
- generated client code or its generation inputs in `src/AVPRClient/`
- frontmatter/metadata tests and README documentation

Search for every use of the changed field before editing. Do not hand-author a migration unless the existing migration workflow requires it.

## CI and release safety

- Changes under `StagingArea/**` trigger the staging solution on Windows with `uv` installed.
- Changes under `tests/**` or the main `src` projects can trigger the main solution build/test job.
- On pushes to `main`, release-note changes can publish NuGet packages and service changes can publish a container image.
- A staged package marked for publication can be pushed to the production registry after checks pass.
- Workflow actions should remain pinned to deliberate versions/commits. Preserve least-privilege permissions and never print secrets.

Before completing a change, report which focused and solution-level checks were run, and call out anything skipped because it would require unavailable external services.
