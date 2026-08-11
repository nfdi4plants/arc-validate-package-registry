# Testing changes

## Contents

- [Common commands](#common-commands)
- [Portable model](#portable-model)
- [In-process registry test host](#in-process-registry-test-host)
- [Metadata-model changes](#metadata-model-changes)

Use focused tests while iterating, then run the affected solution before
handoff.

## Common commands

```shell
# Main libraries, service, CLI, and tests
./build.sh TestSolution

# Staging-area checks
./build.sh TestStagingArea

# Portable .NET/JavaScript/Python contracts and packed-package consumers
./build.sh TestPortableModel
./build.sh TestPortableCodecs
./build.sh TestNativeValidationPackages

# Focused suites
dotnet test tests/AVPR.Staging.Tests/AVPR.Staging.Tests.fsproj --configuration Release
dotnet test tests/ClientTests/ClientTests.fsproj --configuration Release
dotnet test tests/APITests/APITests.csproj --configuration Release
dotnet test StagingAreaTests/StagingAreaTests.fsproj --configuration Release
```

On Windows, use `.\build.cmd` in place of `./build.sh`. The individual
`dotnet test` commands remain useful for focused iteration.

## Portable model

`ValidationPackage.Model.Tests` runs the same behavioral contract on .NET,
JavaScript, and Python. It is a Pyxpecto executable, so invoke the .NET target
with `dotnet run` rather than `dotnet test`.

Run `./build.sh TestPortableModel` for the model and
`./build.sh TestPortableCodecs` for the codecs. Each target restores the pinned
tools and uv environment, runs the shared contract on .NET, JavaScript, and
Python, packs the NuGet, npm, and Python artifacts, and runs consumers restored
from local packages rather than project references. `TestNativeValidationPackages`
is the focused installed-consumer check for the npm tarballs and Python wheels.

Generated output belongs under ignored `artifacts/portable/`. After changing a
public portable type, inspect that output for attached class members, clean
backing-field names, and target-specific behavior. Compiling the packed-package
consumers through Fable verifies that the required ordered sources were placed
under the NuGet package's `fable/` path.

## In-process registry test host

`tests/PackageRegistryTestHost` is shared infrastructure rather than a test
suite. `PackageRegistryWebApplicationFactory` starts the real registry entry
point through `WebApplicationFactory<Program>`, so requests exercise production
endpoint mappings, handlers, filters, and JSON configuration.

The factory uses the `Testing` environment. It disables development migrations,
staging-data initialization, the external PostgreSQL health check, and HTTPS
redirection, then replaces Npgsql with a uniquely named EF in-memory database.
Create and dispose one factory per test or fixture.

Seed through `SeedPackageAsync` or `SeedPackagesAsync`. These helpers add the
matching integrity-hash and zero-download records required by the real handlers.

Use the host at two boundaries:

- API tests call `factory.CreateClient()` and assert status codes and exact raw
  JSON property/value shapes.
- Generated-client tests pass that same `HttpClient` to `AVPRClient.Client`,
  replace its production `BaseUrl`, invoke the generated operation, and assert
  the typed result.

The in-memory provider does not verify PostgreSQL migrations, `jsonb` layout,
backfills, Npgsql conversion, or relational behavior. Retain focused EF model
assertions and manually verify persistence changes against PostgreSQL.

## Metadata-model changes

A metadata field crosses multiple representations. A green build alone does
not prove parsing or wire behavior.

1. **Domain behavior:** update mandatory/default and all-fields reference values
   in `tests/ValidationPackage.Model.Tests/`. Cover factories, defaults,
   equality, and semantic-version behavior on .NET, JavaScript, and Python.
2. **Frontmatter:** update full source, extracted YAML, expected metadata, and
   fixtures in `tests/ValidationPackage.Codecs.Tests/` for F#/Python comment
   and binding forms. Preserve a no-field case for optional-field compatibility
   and add focused malformed cases.
3. **Hashes and staging:** recompute hashes whenever fixture bytes change and
   update `tests/AVPR.Staging.Tests/`. Verify both metadata and hash values.
4. **Generated client:** regenerate from the local service OpenAPI document,
   maintain equivalent client/model reference objects, and test both mapping
   directions, nested collections, null/empty behavior, CWL scalars, and full
   semantic-version suffixes.
5. **Staging compatibility:** run `PackageStagingArea.slnx` when submitted script
   syntax or checks are affected. Add a real staged package only when layout
   itself must be exercised, always under a new semantic version.
6. **Service boundaries:** test exact raw JSON and generated-client
   deserialization through the in-process host. For persistence changes, inspect
   migrations and backfills against PostgreSQL and check website rendering with
   both present and absent values.

Do not weaken expected values to make tests pass. Recompute them from the actual
fixtures and preserve older fixtures that demonstrate backwards compatibility.
