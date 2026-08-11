# AVPR #111, phase 1: extract `ValidationPackage.Model`

## Status

Implemented delivery plan for the first delivery slice of
[AVPR #111](https://github.com/nfdi4plants/arc-validate-package-registry/issues/111).

This phase creates and proves the portable model package. The subsequent
`ValidationPackage.Codecs` slice is implemented separately; production
consumer migration remains outside this phase.

## Objective

Create a publishable `ValidationPackage.Model` F# library containing the
validation-package domain contract and domain behavior, with no dependency on
AVPR indexing or serialization infrastructure.

The package must:

- compile and run its contract tests on .NET, JavaScript, and Python through
  Fable;
- remain convenient for both F# and C# consumers;
- preserve the current metadata defaults, factories, equality, CWL scalar
  behavior, and semantic-version behavior;
- own package identity independently of repository paths, timestamps, and
  content hashes;
- contain the F# sources and project metadata required for consumption from a
  packed NuGet package by Fable.

## Boundary

### Model-owned types and behavior

Move the portable equivalents of these types from
`src/AVPRIndex/Domain.fs`:

- `SemVer`
- `Author`
- `OntologyAnnotation`
- `CwlPrimitive`
- `CommandInputType`
- `CommandInputBinding`
- `CommandInputParameter`
- `ValidationPackageMetadata`

Add an explicit portable identity:

```fsharp
[<AttachMembers>]
type ValidationPackageIdentity(name: string, version: SemVer) =
    member _.Name = name
    member _.Version = version
```

Expose identity construction from metadata through a validated
`ValidationPackageMetadata.tryGetIdentity` function and a throwing
`getIdentity` counterpart, following the existing semantic-version helper
pattern. Identity equality is based only on package name and the complete
semantic version, including prerelease and build metadata. Changes to summary,
description, authors, tags, inputs, release notes, publication state, language,
or CQC hook do not change identity.

Keep these as domain behavior:

- semantic-version construction, parsing, validation, and formatting;
- extraction of a semantic version or package identity from metadata;
- conversion between `CwlPrimitive`/nullability and the canonical CWL scalar
  strings such as `boolean?`;
- defaults, factory functions, equality, and hashing.

The CWL scalar conversion is part of the CWL value contract. YAML and JSON
tree traversal around that scalar belongs to the codec package.

### Explicitly excluded

`ValidationPackage.Model` must not contain or reference:

- YAMLicious, YamlDotNet, Thoth.Json, System.Text.Json, or serializer
  attributes/converters;
- frontmatter delimiters, `FrontmatterLanguage`, YAML extraction, or document
  encoding/decoding;
- file or directory access;
- repository paths, file names, timestamps, normalized script content, or
  `ValidationPackageIndex`;
- hashing or cryptography;
- HTTP, EF Core, ASP.NET, OpenAPI, or generated-client types;
- AVPR staging/publication constants such as `STAGING_AREA_RELATIVE_PATH`.

## Proposed project layout

```text
src/
  ValidationPackage.Model/
    ValidationPackage.Model.fsproj
    PortableHash.fs
    SemanticVersion.fs
    ValidationPackageIdentity.fs
    Cwl.fs
    Author.fs
    OntologyAnnotation.fs
    ValidationPackageMetadata.fs
    README.md
    RELEASE_NOTES.md

tests/
  ValidationPackage.Model.Tests/
    ValidationPackage.Model.Tests.fsproj
    ReferenceObjects.fs
    SemanticVersionTests.fs
    CwlTests.fs
    DomainTests.fs
    Main.fs
```

Use the namespace `ValidationPackage.Model`. Keep the existing public type and
member names in this first extraction so downstream migration is primarily a
namespace/package-reference change. A broader immutable-domain redesign is
not part of this phase.

Target `netstandard2.0` for broad .NET consumption unless a concrete Fable or
compiler limitation discovered during the spike requires a higher target.
Use `Fable.Package.SDK` as a private build dependency and mark the project as a
Fable library. Pack the `.fsproj`, `.fs`, and any `.fsi` files under the
NuGet package's `fable/` path.

The runtime dependency surface should otherwise be limited to FSharp.Core,
Fable.Core, and the target framework. The model must not reference
`AVPRIndex`.

## Portability adjustments during extraction

Copy behavior, not infrastructure-specific implementation details.

- Replace the current named-group semantic-version regex with an implementation
  whose syntax and capture handling are supported consistently by Fable on
  JavaScript and Python. Preserve the existing accepted and rejected strings,
  including leading-zero rules and prerelease/build suffixes.
- Replace reflection-dependent enum validation such as `Enum.IsDefined` with
  an exhaustive `CwlPrimitive` check if cross-target compilation or execution
  shows inconsistent behavior.
- Avoid target-specific null helpers in portable paths. Retain the current
  argument-error behavior and verify it on all three targets.
- Do not assert exact hash-code integers across targets. Assert equality
  semantics and that equal values have equal hashes within each runtime.
- Keep arrays and current defaults in the public surface for compatibility;
  verify structural equality for nested arrays on every target.

## Migration strategy

Make this first phase additive.

Do not remove or replace the model types currently compiled into `AVPRIndex`.
Doing that in isolation would:

- change the CLR identity of EF-owned nested types and potentially create
  unintended model/migration changes;
- remove STJ type/property attributes that currently preserve public API JSON;
- force coordinated changes to the service, generated-client mappings,
  staging infrastructure, and arc-validate before the portable package has
  been proven.

The temporary duplicate model is an intentional transition boundary:

1. This phase publishes and validates `ValidationPackage.Model`.
2. The codec phase targets only `ValidationPackage.Model`.
3. ARCExpect switches to the model and codecs together.
4. AVPR staging, service models, and client interop switch in issues
   #112–#114.
5. The old `AVPRIndex.Domain` types are then removed with `AVPRIndex`.

Do not add bidirectional compatibility mappings in the model. Mappings belong
to the consumer boundary that needs them.

To limit drift while both models exist, derive the new contract tests from the
current `IndexTests/DomainTests.fs` cases and reuse equivalent reference values.
Any metadata change made during the transition must update both contracts until
the old one is retired.

## Implementation sequence

### 1. Record the baseline

- Run the focused `IndexTests` project and the main solution before editing.
- Pack the current `AVPRIndex` once and record its public model API with a
  deterministic API-surface tool or a checked-in textual baseline.
- Confirm the existing SemVer and CWL positive/negative cases that must remain
  compatible.

This makes accidental behavior changes visible without treating serializer
attributes as part of the new model.

### 2. Scaffold the portable package

- Create `src/ValidationPackage.Model/ValidationPackage.Model.fsproj`.
- Add package metadata, README, release notes, license metadata, and an initial
  package version.
- Configure Fable package metadata and source packing.
- Add the project to `arc-validate-package-registry.slnx`.
- Add the project path to CI change detection so model-only changes run the
  relevant build and tests.

Do not add YAML or JSON package references to make an early consumer compile.

### 3. Extract domain sources

- Put the shared internal cross-target hash implementation in `PortableHash.fs`.
- Put semantic-version types/functions in `SemanticVersion.fs` and package
  identity in `ValidationPackageIdentity.fs`.
- Put CWL primitive, type, binding, and parameter types/functions in `Cwl.fs`.
- Put `Author`, `OntologyAnnotation`, and `ValidationPackageMetadata` in their
  own correspondingly named files.
- Preserve existing constructor defaults, `create` signatures, equality, and
  semantic-version helper names where practical.
- Add `ValidationPackageIdentity` and metadata identity helpers.
- Remove all STJ attributes, `CommandInputTypeJsonConverter`, JSON options,
  path/hash overloads, and `ValidationPackageIndex` behavior from the copied
  implementation.
- Keep F# compile order explicit in the project file.

At the end of this step, a dependency search within the project should find no
codec, filesystem, HTTP, EF, ASP.NET, OpenAPI, client, staging, or hashing
references.

### 4. Add one cross-target contract suite

Use `Fable.Pyxpecto` for a single executable test suite compiled and run on all
three targets. Port the domain-only coverage from `IndexTests/DomainTests.fs`
and add identity coverage.

Cover:

- mandatory and all-field factories for every model type;
- default values and structural equality/hashing;
- every required and nullable CWL scalar form;
- rejection of unsupported CWL primitives and malformed scalar strings;
- CWL input binding defaults and nested parameter equality;
- mandatory and all-field metadata;
- valid SemVer values with prerelease and/or build metadata;
- invalid leading zeros and malformed SemVer values;
- metadata-to-SemVer helpers;
- package identity equality, including prerelease and build metadata;
- proof that non-identity metadata changes do not affect identity.

Keep YAML/frontmatter and JSON cases out of this project. They belong in
`ValidationPackage.Codecs.Tests`.

Add pinned Fable tooling and the minimal JavaScript/Python runtime manifests
needed to execute the generated tests. Generated JS/Python output goes under an
ignored artifacts directory.

### 5. Add CI and package verification

Add a focused Ubuntu CI job that:

1. restores pinned .NET/Fable, JavaScript, and Python dependencies;
2. builds the model in Release mode;
3. runs the contract suite on .NET;
4. transpiles and runs the same suite on JavaScript;
5. transpiles and runs the same suite on Python;
6. packs `ValidationPackage.Model`;
7. verifies that the package contains its `fable/` project and ordered source
   files;
8. compiles a minimal smoke consumer against the packed package rather than a
   project reference.

Keep the existing multi-OS main-solution build. The focused job proves Fable
behavior; the solution build proves ordinary .NET integration.

Wire a model release job into `.github/workflows/pipeline.yml`, gated in the
same way as the existing package releases: push to `release`, successful tests,
and a deliberate `ValidationPackage.Model/RELEASE_NOTES.md` change. Do not
publish from pull requests or dry runs.

### 6. Document the handoff to the codec phase

- Update the repository map and development/release documentation with the new
  project and test commands.
- Document that model values alone have no implicit JSON or YAML shape.
- Record the exact model package version expected by the codec implementation.
- Leave current authoring documentation on `AVPRIndex` until the codec and
  consumer migration is available; avoid advertising an incomplete metadata
  parsing path.

## Verification commands

Exact Fable output paths can follow the chosen tool manifest, but the completed
phase should provide repository commands equivalent to:

```shell
dotnet tool restore
uv sync --locked

dotnet build src/ValidationPackage.Model/ValidationPackage.Model.fsproj --configuration Release
dotnet run --project tests/ValidationPackage.Model.Tests/ValidationPackage.Model.Tests.fsproj --configuration Release

dotnet fable tests/ValidationPackage.Model.Tests/ValidationPackage.Model.Tests.fsproj --outDir artifacts/model-tests/js --lang javascript --noCache
npm run test:model:js

dotnet fable tests/ValidationPackage.Model.Tests/ValidationPackage.Model.Tests.fsproj --outDir artifacts/model-tests/py --lang python --noCache
uv run --locked python artifacts/model-tests/py/main.py

dotnet pack src/ValidationPackage.Model/ValidationPackage.Model.fsproj --configuration Release --output artifacts/packages
dotnet build arc-validate-package-registry.slnx --configuration Release
dotnet test arc-validate-package-registry.slnx --configuration Release --no-build
```

`PackageStagingArea.slnx` is not required for this additive phase because staged
package parsing and sanity checks remain on `AVPRIndex`. It becomes required
when the codecs or staging consumers switch to the new model.

## Phase acceptance criteria

This phase is complete when:

- `ValidationPackage.Model` builds without runtime package dependencies beyond
  FSharp.Core, Fable.Core, and framework libraries;
- its source contains none of the excluded codec or infrastructure concerns;
- the packed NuGet artifact contains usable Fable sources/project metadata;
- one contract suite passes on .NET, JavaScript, and Python;
- SemVer, CWL, metadata defaults/equality, and package identity have explicit
  cross-target coverage;
- the main AVPR solution remains green with no API JSON, OpenAPI, EF model, or
  staged-package behavior changes;
- CI detects model-only changes and can deliberately release the package.

AVPR #111 itself is not complete at this point. It still requires
`ValidationPackage.Codecs`, equivalent F#/Python frontmatter behavior, and an
ARCExpect migration that removes its `AVPRIndex` reference.

## Non-goals

- Implement YAMLicious or Thoth.Json codecs.
- Move frontmatter extraction or file reading.
- Rename or retire `AVPRIndex`.
- Change service-owned API/EF models or generate migrations.
- Change `AVPRClient` or add client interop.
- Update arc-validate/ARCExpect package references before codecs exist.
- Redesign the model as immutable records or change existing metadata
  semantics.
- Publish a package outside the normal reviewed release workflow.
