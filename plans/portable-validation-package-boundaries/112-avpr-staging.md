# AVPR #112: replace `AVPRIndex` with focused staging infrastructure

## Status

**Done.** Implemented for
[AVPR #112](https://github.com/nfdi4plants/arc-validate-package-registry/issues/112).

The focused staging boundary and migrations of AVPRCI and service
initialization are implemented. AVPR #114 subsequently moved generated-client
interop to `AVPRClient.Interop`, and `AVPRIndex` is retired from the current
project graph.

## Objective

Replace the remaining repository-facing responsibilities of `AVPRIndex` with
an internal `AVPR.Staging` project that:

- discovers F# and Python scripts below `StagingArea/`;
- reads and normalizes package content to LF;
- preserves the existing uppercase MD5 content-hash contract;
- parses metadata through `ValidationPackage.Codecs`;
- represents repository state as `StagedValidationPackage`;
- owns staging constants and second-precision date normalization;
- contains no HTTP, generated-client, service persistence, or publication
  behavior.

`AVPR.Staging` targets .NET only and is not published as a NuGet package.

## Boundary and layout

```text
src/AVPR.Staging/
  StagingConstants.fs
  NormalizedContent.fs
  ContentHash.fs
  DateTimeOffsetNormalization.fs
  StagedValidationPackage.fs
  StagingRepository.fs
  AVPR.Staging.fsproj

tests/AVPR.Staging.Tests/
  NormalizedContentTests.fs
  StagedValidationPackageTests.fs
  StagingRepositoryTests.fs
  AVPR.Staging.Tests.fsproj
```

## Migration sequence

1. Add the focused project and parity tests against existing fixture hashes,
   metadata, LF normalization, language detection, and repository discovery.
2. Move AVPRCI publication input and publication-specific client mappings to
   `StagedValidationPackage`.
3. Move service initialization to `AVPR.Staging`; service-domain ownership
   changes remain coordinated with AVPR #113.
4. Move generated-client ↔ portable-model mappings out of the generated client
   project as defined by AVPR #114.
5. Move staging-area naming/frontmatter checks to the portable model/codecs and
   staging constants.
6. Remove all production and test project references to `AVPRIndex`.
7. Remove the project from the solution and document its retirement. Publish a
   deprecated compatibility release only if external consumers still require
   one.

## Compatibility requirements

- Normalize all text line endings to `\n` before UTF-8 conversion or hashing.
- Continue using MD5 and uppercase hexadecimal output for published content
  hashes.
- Store repository paths with `/` separators while retaining paths that can be
  opened on Windows and Unix.
- Preserve F#-then-Python discovery grouping until callers no longer depend on
  the current ordering.
- Preserve second precision and the local UTC offset for generated
  `LastUpdated` values.
- Preserve the exact frontmatter forms and default/unknown-field behavior
  already covered by `ValidationPackage.Codecs.Tests`.

## Cross-issue boundary

- AVPR #112 owns repository and staged-file infrastructure.
- AVPR #113 owns service API and Entity Framework nested types.
- AVPR #114 owns generated-client interop.
- arc-validate migrations consume the portable packages in their own roadmap
  issues; `AVPR.Staging` must never become an arc-validate dependency.

## Verification

- Run focused `AVPR.Staging.Tests`.
- Run the existing index tests during parity migration.
- Run `PackageStagingArea.slnx` when staging checks change.
- Run the main solution after every consumer migration.
- Before removal, verify no production project references `AVPRIndex`.
