# ValidationPackage.Model

Portable validation-package domain types for .NET and Fable targets.

The package contains metadata, authors, ontology tags, the supported CWL
command-input subset, semantic versions, package identity, and the canonical
validation-packages configuration/value contracts. It intentionally
contains no YAML, JSON, filesystem, hashing, HTTP, EF, OpenAPI, or AVPR staging
logic.

String codecs are intentionally out of scope here and will be provided
separately by `ValidationPackage.Codecs`.

Configuration input values preserve validated numeric lexemes so all targets,
including JavaScript, retain signed 64-bit boundaries exactly. The Model owns
declaration/value compatibility and deterministic logical argv materialization;
it performs no process execution.
