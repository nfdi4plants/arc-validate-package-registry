# Plan: Configurable validation packages (`CLIArguments`) + dev instance

## Context

Validation packages already accept ad-hoc command-line arguments — e.g. [`agdafair@0.0.1.fsx`](StagingArea/agdafair/agdafair@0.0.1.fsx) hand-parses `-h`/`-u`/`-i`/`-o` — but there is **no declared, discoverable contract** for them. Consumers (`arc-validate`, the website, package authors) can't know what flags a package supports without reading its source.

This change introduces an **optional** `CLIArguments` section in the YAML frontmatter where a package declares each accepted flag with a description and an example. The data flows through every layer (index → DB → API → client → website) and is rendered on the package page under a new **"Available Commands"** section. Everything must be **backwards compatible**: existing packages (no such section) and existing DB rows keep working unchanged.

The field is modeled exactly like the existing owned-JSON collections `Tags` and `Authors` (see [`ValidationPackageDb.cs`](src/PackageRegistryService/Models/ValidationPackageDb.cs) `OwnsMany(...).ToJson()`), which keeps the migration trivial and the code idiomatic.

Separately, we set up a **dev instance**: a new `dev` branch whose pushes publish a distinctly tagged `ghcr.io/nfdi4plants/avpr:dev` image (production keeps `:main`), documented in the repo. Actual dev-server deployment is handled outside this repo (**docs-only** here).

`arc-validate` is a downstream consumer of the `AVPRIndex`/`AVPRClient` NuGet packages and must be adapted too — **that implementation is intentionally omitted here** and will be drafted in that repo from a copy of this plan (see "Downstream" section).

### Decisions (confirmed with user)
- Frontmatter key / domain type: **`CLIArguments`** (array) of **`CLIArgument`** with sub-fields **`Flags`, `Description`, `Example`**, where **`Flags` is a string array** so one argument can declare multiple aliases (e.g. `-i` and `--input`).
- Website section heading: **"Available Commands"** (rendered only when the package declares at least one arg).
- Storage: owned JSON `jsonb` column, mirroring `Tags`/`Authors`.
- Backwards compat: optional field defaults to empty array; migration backfills existing rows to `[]`; code null-guards with `?? []`.
- Dev instance: pipeline change + docs only (no compose file committed).

---

## Layer 1 — `AVPRIndex` domain (F#) + NuGet release

**File: [`src/AVPRIndex/Domain.fs`](src/AVPRIndex/Domain.fs)**
- Add a `CLIArgument` type mirroring `OntologyAnnotation` (lines ~128-163): `member val Flags : string [] = Array.empty with get,set` plus `Description`/`Example` (default `""`); `GetHashCode`; `Equals`; and `static member create (flags, ?Description, ?Example)`. Array-valued structural comparison in `Equals`/`GetHashCode` is already the established pattern here — `ValidationPackageMetadata.Equals` compares its `Authors`/`Tags` arrays the same way (F# `=`/`hash` are structural over arrays).
- On `ValidationPackageMetadata` (lines ~165-271): add `member val CLIArguments : CLIArgument [] = Array.empty with get,set` (place with the optional fields, after `CQCHookEndpoint`). Add it to `GetHashCode`, to both tuples in `Equals`, and add an optional `?CLIArguments` param wired in `create` (mirror how `Authors`/`Tags` are handled).

**File: [`src/AVPRIndex/Frontmatter.fs`](src/AVPRIndex/Frontmatter.fs)**
- YAML deserialization is automatic (YamlDotNet PascalCase → `CLIArguments` key with `Flags`/`Description`/`Example`, `Flags` a YAML string sequence), no code change required for parsing.
- **Hardening for forward-compat:** in `yamlDeserializer()` (line ~123) add `.IgnoreUnmatchedProperties()` so frontmatter containing keys unknown to an older reader does not throw. This makes `CLIArguments` — and future additions — non-breaking for already-shipped consumers. Confirm current strict/lenient behavior with a quick unit test (parse frontmatter with an unknown key).

**Release:** bump [`src/AVPRIndex/RELEASE_NOTES.md`](src/AVPRIndex/RELEASE_NOTES.md) (new top entry, e.g. `v0.5.0 - Add optional CLIArguments metadata`) and the version in [`AVPRIndex.fsproj`](src/AVPRIndex/AVPRIndex.fsproj). A `RELEASE_NOTES.md` change on `main` triggers the NuGet publish (see [`pipeline.yml`](.github/workflows/pipeline.yml) `trigger-release-index`).

---

## Layer 2 — Registry service model, DB migration, seeding (C#)

**File: [`src/PackageRegistryService/Models/ValidationPackage.cs`](src/PackageRegistryService/Models/ValidationPackage.cs)**
- Add `public ICollection<AVPRIndex.Domain.CLIArgument> CLIArguments { get; set; } = Array.Empty<AVPRIndex.Domain.CLIArgument>().ToList();` (mirror `Authors` at line ~99 / `Tags` at line ~84).

**File: [`src/PackageRegistryService/Models/ValidationPackageDb.cs`](src/PackageRegistryService/Models/ValidationPackageDb.cs)**
- In `OnModelCreating` (lines ~16-27) chain `.OwnsMany(v => v.CLIArguments, c => c.ToJson())` alongside the existing `Authors`/`Tags` owners.

**Migration: `src/PackageRegistryService/Migrations/<timestamp>_AddCLIArguments.cs` (+ `.Designer.cs` + snapshot)**
- Generate with `dotnet ef migrations add AddCLIArguments` (auto-updates [`ValidationPackageDbModelSnapshot.cs`](src/PackageRegistryService/Migrations/ValidationPackageDbModelSnapshot.cs)). Expected `Up`: `AddColumn<string>(name:"CLIArguments", table:"ValidationPackages", type:"jsonb", nullable:true)`; `Down`: `DropColumn`. Pattern reference: [`AddProgrammingLanguage.cs`](src/PackageRegistryService/Migrations/20251203092043_AddProgrammingLanguage.cs).
- **Backfill existing rows** (user's "empty entry" requirement): append `migrationBuilder.Sql(@"UPDATE ""ValidationPackages"" SET ""CLIArguments"" = '[]' WHERE ""CLIArguments"" IS NULL;");` to `Up`.

**File: [`src/PackageRegistryService/Data/DataInitializer.cs`](src/PackageRegistryService/Data/DataInitializer.cs)**
- In the `new ValidationPackage { ... }` seeding block (lines ~35-52) add `CLIArguments = i.Metadata.CLIArguments`.

Null-handling elsewhere follows the existing `?? []` convention (Layer 3).

---

## Layer 3 — Website view ("Available Commands")

**New file: `src/PackageRegistryService/Pages/Components/PackageCLIArguments.cs`**
- `public static string Render(CLIArgument[] args)`; return `""` when `args` is null/empty (this is how the section is omitted).
- Otherwise return a full `<section>` + trailing `<hr />`, headed `<h2>Available Commands</h2>`, containing a `<table>` with `<thead>` columns **Flags / Description / Example** and one `<tr>` per arg. Follow the table markup in [`PackageAvailableVersion.RenderVersionTable`](src/PackageRegistryService/Pages/Components/PackageAvailableVersion.cs). Render the **Flags** cell by joining the aliases (e.g. `", "`), each wrapped in `<code>`; wrap `Example` in `<code>` too; HTML-escape all values via `System.Security.SecurityElement.Escape` (as [`Package.cs`](src/PackageRegistryService/Pages/Components/Package.cs) line ~58 does).

**File: [`src/PackageRegistryService/Pages/Components/Package.cs`](src/PackageRegistryService/Pages/Components/Package.cs)**
- Add parameter `CLIArgument[] packageCLIArguments` to `Render`.
- Insert `{PackageCLIArguments.Render(packageCLIArguments)}` immediately **after** the Description `</section>` + `<hr />` (after line ~48), before the Release-notes section.

**File: [`src/PackageRegistryService/Pages/Handlers/PackageHandlers.cs`](src/PackageRegistryService/Pages/Handlers/PackageHandlers.cs)**
- In both `Render` and `RenderLatest` `Package.Render(...)` calls (lines ~38 and ~81) pass `packageCLIArguments: (package.CLIArguments ?? []).ToArray()` (mirroring `packageAuthors`).

---

## Layer 4 — `AVPRClient` regeneration + mapping + NuGet release

**Regenerate [`src/AVPRClient/AVPRClient.cs`](src/AVPRClient/AVPRClient.cs)** from the updated OpenAPI (per [`src/AVPRClient/README.md`](src/AVPRClient/README.md)): running the service locally so the schema includes `CLIArguments`, then NSwag `openapi2csclient`. This adds a `CLIArgument` partial class and a `CLIArguments` property on `ValidationPackage` (like `OntologyAnnotation`/`Tags` at lines ~1128-1158).

**File: [`src/AVPRClient/Extensions.cs`](src/AVPRClient/Extensions.cs)** — the round-trip mapping (this is the easily-missed file):
- `toValidationPackage` (index → client, line ~58): add `CLIArguments = indexedPackage.Metadata.CLIArguments.Select(a => new AVPRClient.CLIArgument { Flags = a.Flags.ToList(), Description = a.Description, Example = a.Example }).ToList()` (mirror the `Tags`/`Authors` selects; the generated `CLIArgument.Flags` is an `ICollection<string>`).
- Add `AsIndexType(this CLIArgument)` (maps `Flags = (c.Flags ?? new List<string>()).ToArray()`, `Description`, `Example`) and `AsIndexType(this ICollection<CLIArgument>)` helpers (mirror the `Author`/`OntologyAnnotation` ones at lines ~146-188), null-guarding both collections.
- `toValidationPackageMetadata` (client → index, line ~190): pass `CLIArguments: (validationPackage.CLIArguments ?? new List<CLIArgument>()).AsIndexType()` to `ValidationPackageMetadata.create` (relies on the new `?CLIArguments` param from Layer 1).

**Release:** bump [`src/AVPRClient/RELEASE_NOTES.md`](src/AVPRClient/RELEASE_NOTES.md) and version in [`AVPRClient.csproj`](src/AVPRClient/AVPRClient.csproj) (triggers `trigger-release-client`).

---

## Layer 5 — Tests, fixtures, docs

**Index tests** ([`tests/IndexTests/`](tests/IndexTests/)):
- [`ReferenceObjects.fs`](tests/IndexTests/ReferenceObjects.fs): add a `module CLIArgument` (mandatory/all fields, e.g. `CLIArgument(Flags = [| "-i"; "--input" |], Description = "...", Example = "...")`); add `CLIArguments = [| ... |]` to `ValidationPackageMetadata.allFields`, `Metadata.FSharp.validFullFrontmatter`, and `Metadata.Python.validFullFrontmatter`; add a `CLIArguments:` block to the `Frontmatter.FSharp.Comment/Binding.validFullFrontmatter` **and** `...Extracted` strings (Python variants derive via `.Replace`). Recompute the affected MD5 constants under `Hash.Hashes.CommentFrontmatter/BindingFrontmatter.validFullFrontmatter` after editing fixtures.
- Fixtures `fixtures/Frontmatter/{Comment,Binding}/valid@2.0.0.{fsx,py}`: add a matching `CLIArguments:` section (with `Flags:` as a YAML sequence) so the "all fields" files stay in sync with `validFullFrontmatter`, e.g.:

  ```yaml
  CLIArguments:
    - Flags:
        - -i
        - --input
      Description: Input ARC path
      Example: ./my-arc
  ```
- [`DomainTests.fs`](tests/IndexTests/DomainTests.fs): add `CLIArgument` create/equality tests; the existing `ValidationPackageMetadata … all fields` create test (line ~153) must pass `CLIArguments`.

**Client/API tests**: update [`tests/ClientTests/ReferenceObjects.fs`](tests/ClientTests/ReferenceObjects.fs), [`TypeExtensionsTests.fs`](tests/ClientTests/TypeExtensionsTests.fs), fixtures `tests/ClientTests/fixtures/allFields_*.fsx`, and [`tests/APITests/UnitTest1.cs`](tests/APITests/UnitTest1.cs) wherever they assert on package field sets, to include `CLIArguments`.

**Docs**:
- [`README.md`](README.md): add `CLIArguments` to the Optional-fields table (line ~264), add a short subsection documenting the array shape (`Flags`/`Description`/`Example`) and the "Available Commands" rendering, and extend the "all fields" example (lines ~275-311). Do **not** modify any published `StagingArea` package to demo it (immutable) — use the docs + test fixtures.
- [`AGENTS.md`](AGENTS.md): the "Cross-cutting model changes" list (lines ~79-89) already enumerates the touched files; add `AVPRClient/Extensions.cs` if worth calling out.

---

## Layer 6 — Dev instance (pipeline + docs only)

**File: [`.github/workflows/pipeline.yml`](.github/workflows/pipeline.yml)**
- Add `dev` to `on.push.branches` (line ~5) and `on.pull_request.branches` (line ~15) → `['main', 'dev']`.
- Gate the **NuGet** releases to `main` only so a `dev` push never publishes libraries: add `github.ref == 'refs/heads/main' &&` to `trigger-release-index` and `trigger-release-client` (lines ~91-92).
- Leave `release-api-image` triggering on any qualifying push. `docker/metadata-action` (no explicit `tags:`) already tags by branch name via `type=ref,event=branch`, so `main` → `:main` and `dev` → `:dev` automatically — production tagging is unchanged. (Optional: make tags explicit with a `tags:` block if we later want `:latest` on `main`.)
- Reusable-workflow refs stay `@main` (stable).

**Branch:** create the `dev` branch (git operation, done at execution time, not now).

**Docs**: document in [`README.md`](README.md)/[`AGENTS.md`](AGENTS.md) the `dev` branch, the `ghcr.io/nfdi4plants/avpr:dev` image, and that the dev instance is deployed from that tag (deployment mechanics live outside this repo).

---

## Downstream — `arc-validate` (plan only; implement in that repo)

Copy this plan over and draft there. Expected work:
- Bump `AVPRIndex` and `AVPRClient` package references to the versions released here.
- Consume `Metadata.CLIArguments`: surface declared flags in CLI help / `package` inspection output, and/or validate & pass user-supplied config through to package execution.
- **Forward-compat note:** packages that use `CLIArguments` require an `arc-validate` bundling the new `AVPRIndex`. The `IgnoreUnmatchedProperties()` hardening (Layer 1) protects readers that have the new build but predate future fields; it cannot retroactively fix already-shipped older releases.

---

## Verification

1. **Build + unit tests (main solution):** `dotnet build arc-validate-package-registry.sln -c Release` then `dotnet test …`. Focused: `dotnet test tests/IndexTests/IndexTests.fsproj`, `tests/ClientTests/ClientTests.fsproj`, `tests/APITests/APITests.csproj`.
2. **Frontmatter round-trip:** IndexTests confirm `CLIArguments` parses from both comment & binding frontmatter (F# + Python) and survives `create`/equality; add an explicit unknown-key test proving `IgnoreUnmatchedProperties()` works.
3. **Staging solution:** `dotnet build PackageStagingArea.sln -c Release` + `dotnet test … --no-build` — ensure a package carrying a `CLIArguments:` section passes sanity checks.
4. **DB migration end-to-end:** run the stack ([`docker-compose.yml`](docker-compose.yml) postgres); apply migrations; verify (via adminer) that pre-existing rows show `CLIArguments = []` and a newly seeded package with args persists them as `jsonb`.
5. **Website:** load a package page locally — confirm the **"Available Commands"** table renders for a package with args and is **absent** for one without.
6. **Client regen:** confirm regenerated `AVPRClient.cs` contains `CLIArgument` + `CLIArguments`, and an AVPRCI dry-run (`dotnet run --project src/AVPRCI/AVPRCI.fsproj -- publish --api-key <key> --dry-run`) shows the field in the emitted JSON.
7. **Pipeline:** `workflow_dispatch` dry-run to sanity-check job gating; a no-op push to `dev` should build `:dev` while NOT triggering NuGet releases; `main` behavior unchanged.
