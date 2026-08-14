# Validation-packages configuration contract

## Contents

- [Canonical document](#canonical-document)
- [Selection and input rules](#selection-and-input-rules)
- [Strict YAML scalar profile](#strict-yaml-scalar-profile)
- [Schema dispatch and packaging](#schema-dispatch-and-packaging)

AVPR Model and Codecs own the portable contract for
`.arc/validation_packages.yml`. The normative ARC specification update is
tracked separately; this page documents the executable v1 codec supplied by
this repository.

## Canonical document

```yaml
$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json"
arc_specification: 3.0.0-draft.2
validation_packages:
  - name: configurable-validation
    version: 1.2.3
    roll_forward: latest_patch
    inputs:
      strict: true
      minimum-files: 2
      report-title: "release candidate"
```

## Selection and input rules

`validation_packages` is required and may be empty. Every canonical selection
has a unique non-empty `name` and full semantic `version`. `roll_forward` is
optional and accepts `disable`, `latest_patch`, or `latest_minor`; omission is
`disable`. Input keys are exact package declaration IDs, not command-line
prefixes.

## Strict YAML scalar profile

Input values use a deliberately strict JSON-compatible YAML scalar profile:
quoted strings, lowercase `null`, `true`, and `false`, JSON-form integers, and
finite JSON-form floating-point numbers. Arrays, objects, YAML convenience
scalars, aliases, anchors, tags, merge keys, duplicate keys, and multiple
documents are rejected. Numeric lexemes are preserved so JavaScript does not
round valid signed 64-bit integers.

## Schema dispatch and packaging

The `$schema` value is an immutable wire identifier and editor association.
Runtime readers select a local decoder from an exact allowlist; they never
fetch an unknown URI. Schema-less legacy files remain read-only during the
migration window. Canonical writers always emit `$schema` first and never emit
legacy files.

The Draft 2020-12 document is included in every `ValidationPackage.Codecs`
artifact as `schemas/validation-packages.schema.json`. JSON Schema covers the
JSON-compatible structure; YAML lexical restrictions, uniqueness, and
declaration-driven value compatibility remain executable codec/model checks.
