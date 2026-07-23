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
- `src/PackageRegistryService/Models/ValidationPackageDb.cs` (register owned-JSON collection fields with `OwnsMany(...).ToJson()`)
- `src/PackageRegistryService/Data/DataInitializer.cs`
- Entity Framework migrations under `src/PackageRegistryService/Migrations/`
- generated client code in `src/AVPRClient/AVPRClient.cs` **and** the index↔client mapping in `src/AVPRClient/Extensions.cs` (easy to miss)
- website rendering under `src/PackageRegistryService/Pages/Components/` when a field should be shown
- frontmatter/metadata tests and README documentation

Search for every use of the changed field before editing. Do not hand-author a migration unless the existing migration workflow requires it.

## Test coverage for metadata model changes

Trace a metadata field through every representation it affects; a green build alone does not prove parsing or mapping behavior.

- In `tests/IndexTests/ReferenceObjects.fs`, maintain mandatory/default and all-fields domain objects, full source frontmatter, extracted YAML, expected parsed metadata, and content-hash constants.
- Exercise the field's factory/default/equality behavior in `DomainTests.fs`. In `MetadataTests.fs` and the fixtures under `tests/IndexTests/fixtures/Frontmatter/`, cover the applicable comment/binding and F#/Python combinations. Preserve a no-field case for optional-field backwards compatibility, and add focused negative or unknown-key cases when parser policy changes.
- Fixture byte changes require recomputing the corresponding MD5 values and updating expected packages in `ValidationPackageIndexTests.fs`. Verify both metadata and hashes.
- In `tests/ClientTests/`, keep equivalent index and generated-client reference objects. Test index-to-client and client-to-index mappings, nested collection conversion, and null/empty behavior in `TypeExtensionsTests.fs`. Add a dedicated serialized-content fixture when needed, but retain older fixtures without a new optional field.
- Run `PackageStagingArea.sln` when submitted-package syntax or sanity checks are affected. Prefer small index/staging-test fixtures; add a real staged package only when the real layout must be exercised, always as a new semantic version and only after reading it.
- The API test project is currently a placeholder. For service behavior, add focused API/component tests where practical; otherwise explicitly verify generated OpenAPI/client changes, EF migrations and backfills against Postgres, seeded JSON persistence, and both present/absent website rendering cases.

Use focused index/client tests while iterating, then run the main solution and, when package syntax is affected, the staging solution. Recompute expected values from actual fixture content instead of weakening assertions.

## CI and release safety

- Changes under `StagingArea/**` trigger the staging solution on Windows with `uv` installed.
- Changes under `tests/**` or the main `src` projects can trigger the main solution build/test job.
- On pushes to `main`, release-note changes can publish NuGet packages and service changes can publish the production container image (`ghcr.io/nfdi4plants/avpr:main`).
- `dev` is a long-lived integration branch: pushes there publish a separate `ghcr.io/nfdi4plants/avpr:dev` image for the development instance, but NuGet releases and production package publication are gated to `main` only.
- A staged package marked for publication can be pushed to the production registry after checks pass.
- Workflow actions should remain pinned to deliberate versions/commits. Preserve least-privilege permissions and never print secrets.

Before completing a change, report which focused and solution-level checks were run, and call out anything skipped because it would require unavailable external services.
