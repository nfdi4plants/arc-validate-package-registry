# AVPR #111, phase 2: extract `ValidationPackage.Codecs`

## Status

**Done.** Implemented delivery plan for the codec slice of
[AVPR #111](https://github.com/nfdi4plants/arc-validate-package-registry/issues/111).

This phase creates and proves the portable codec package. It is additive:
`AVPRIndex.Frontmatter` and current production consumers remain unchanged
until their owning migration issues move them to the portable boundary.

## Objective

Create a publishable `ValidationPackage.Codecs` F# library that converts
in-memory strings to and from `ValidationPackage.Model` on .NET, JavaScript,
and Python.

The package must:

- preserve the existing F# and Python frontmatter boundary forms;
- preserve PascalCase YAML metadata names;
- preserve the public JSON contract, including camel-case metadata,
  PascalCase `Inputs`, lower-camel CWL fields, and scalar CWL types;
- retain defaults, unknown-field tolerance, and actionable CWL failures;
- contain no filesystem access, repository traversal, content hashing, HTTP,
  package execution, or staging behavior;
- work from packed NuGet sources, not only through project references.

## Boundary and layout

```text
src/ValidationPackage.Codecs/
  FrontmatterLanguage.fs
  Frontmatter.fs
  Yaml/
    Encoding.fs
    CwlValidation.fs
    Encoders/
      Cwl.fs
      Author.fs
      OntologyAnnotation.fs
      ValidationPackage.fs
    Decoders/
      Cwl.fs
      Author.fs
      OntologyAnnotation.fs
      ValidationPackage.fs
    CwlYaml.fs
    AuthorYaml.fs
    OntologyAnnotationYaml.fs
    ValidationPackageYaml.fs
  Json/
    Runtime.fs
    Encoders/
      Cwl.fs
      Author.fs
      OntologyAnnotation.fs
      ValidationPackage.fs
    Decoders/
      Cwl.fs
      Author.fs
      OntologyAnnotation.fs
      ValidationPackage.fs
    CwlJson.fs
    AuthorJson.fs
    OntologyAnnotationJson.fs
    ValidationPackageJson.fs
  targets/
    ValidationPackage.Codecs.Javascript.fsproj
    ValidationPackage.Codecs.Python.fsproj

tests/ValidationPackage.Codecs.Tests/
  ReferenceObjects.fs
  FrontmatterTests.fs
  YamlTests.fs
  JsonTests.fs
  Main.fs
  javascript/
  python/

tests/ValidationPackage.Codecs.PackageSmoke/
```

The source layout separates formats first and encoding direction second.
Within each direction, CWL may stay grouped while author, ontology annotation,
and top-level metadata concerns live in separate files. The format-root modules
are compatibility facades over the split implementations.

## Codec decisions

### YAML and frontmatter

- Use YAMLicious to parse, decode, and write its portable YAML syntax tree.
- Keep frontmatter extraction as pure string logic with exact F#/Python
  comment and binding delimiters and newline normalization.
- Use Fable 5.11.0, which contains the Python name-generation fix reported
  against the YAMLicious writer in
  [YAMLicious #20](https://github.com/CSBiology/YAMLicious/issues/20).
- Keep the compiler and Python runtime aligned at 5.11.0; use the current
  Fable 5 API packages (`Fable.Core` 5.2.0 and `Fable.Python` 5.4.0).
- Quote emitted string scalars so metadata containing YAML punctuation,
  whitespace, or line breaks remains unambiguous.

### JSON

- Define every domain encoder and decoder with `Thoth.Json.Core`.
- Keep encoder/decoder values at module scope rather than as model static
  members, avoiding Fable Python self-reference initialization defects.
- Use a small portable `Thoth.Json.Core.Json` string runtime so the package
  does not require mutually exclusive Newtonsoft, JavaScript, and Python
  runtime dependencies.
- Reject the database-only object representation of CWL input types.

### CWL compatibility

- Decode and encode the twelve supported required/nullable scalar strings.
- Require `id`, `type`, and `inputBinding` for each parameter.
- Default binding `position` to `0`, `prefix` to `""`, and `separate` to
  `true`.
- Require `Inputs` to use the sequence form and enforce non-empty unique ids.
- Ignore unsupported extra parameter and binding fields.

## Verification sequence

1. Build and run one 22-case Pyxpecto contract suite on .NET.
2. Transpile and run the same cases with Fable JavaScript.
3. Transpile and run the same cases with Fable Python through the locked uv
   environment.
4. Pack both `ValidationPackage.Model` and `ValidationPackage.Codecs`.
5. Restore a smoke project from those local packages and run it on all three
   targets.
6. Verify the NuGet archive includes the codec project and every ordered F#
   source under `fable/`.
7. Build and test the main solution to prove the additive slice does not alter
   current registry behavior.

## Deferred work

- Replacing `AVPRIndex.Frontmatter` and moving script file reading belongs to
  AVPR staging migration work.
- Switching ARCExpect metadata setup to the new package belongs to the
  arc-validate roadmap.
- Canonical cross-repository fixtures remain part of AVPR #115.
