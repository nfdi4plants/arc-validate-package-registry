# ARC validation package registry documentation

## Contents

- [About AVPR](#about-avpr)
  - [Package browser](#package-browser)
- [Package authors](#package-authors)
- [Contributors](#contributors)
- [Maintainers](#maintainers)
- [API reference](#api-reference)
- [Frequently asked questions](#frequently-asked-questions)
  - [What is an ARC validation package?](#what-is-an-arc-validation-package)
  - [How can I use a validation package?](#how-can-i-use-a-validation-package)
  - [What is Continuous Quality Control?](#what-is-continuous-quality-control)
  - [How can I contribute a validation package?](#how-can-i-contribute-a-validation-package)
  - [How can I validate my ARC locally?](#how-can-i-validate-my-arc-locally)

This documentation covers authoring and publishing validation packages,
developing the registry and its client libraries, and operating service
releases.

## About AVPR

The ARC validation package registry (AVPR) indexes and hosts validation
packages for [Annotated Research Contexts
(ARCs)](https://nfdi4plants.org/nfdi4plants.knowledgebase/docs/implementation/AnnotatedResearchContext.html).
It has two primary uses:

1. Make validation packages discoverable and inspectable for people building
   Continuous Quality Control (CQC) workflows locally or on the
   [DataHUB](https://git.nfdi4plants.org/explore).
2. Give downstream tools programmatic access to query, verify, and download
   immutable validation packages through the public API.

### Package browser

The deployed [package browser](https://avpr.nfdi4plants.org/packages) lists all
published validation packages and shows details including tags, authors,
release notes, declared inputs, and validation code. Each package also links to
its staged source so contributors can propose changes as a new version.

## Package authors

- [Submit and version a validation package](packages/submission.md)
- [Validation package metadata](packages/metadata.md)
- [CWL command inputs](packages/cwl-inputs.md)
- [Validation-packages configuration contract](packages/validation-packages-config.md)

## Contributors

- [Development setup and repository projects](development/overview.md)
- [Testing and cross-cutting metadata changes](development/testing.md)

## Maintainers

- [CI/CD, releases, migrations, and publication](operations/releases.md)

## API reference

The deployed [Swagger UI](https://avpr.nfdi4plants.org/swagger) is the canonical
endpoint-level API reference. The website's
[service release history](https://avpr.nfdi4plants.org/releases) shows which
service version and source revision are currently running.

Resolver and publication clients should use the lightweight
`/api/v1/package-index`, `/api/v1/packages/{name}/versions`, and
`/api/v1/packages/{name}/{version}/metadata` routes. They expose identities and
metadata without transferring executable package content or incrementing
download statistics. The legacy all-content package collection remains
available for compatibility and is deprecated.

Published package versions are immutable. Endpoints that modify registry data
therefore require authorization and are not intended for user-facing tools. If
you have a valid integration use case that requires an API key, open an issue
in the [repository issue tracker](https://github.com/nfdi4plants/arc-validate-package-registry/issues).

## Frequently asked questions

### What is an ARC validation package?

A validation package bundles validation cases that an ARC must pass to qualify
as valid for that package, together with the instructions needed to run the
checks and summarize their results. Validation packages are part of the
[ARC specification 2.0 draft](https://github.com/nfdi4plants/ARC-specification/blob/v2.0.0/ARC%20specification.md).
Packages hosted by AVPR can be implemented with the
[ARCExpect reference library](https://nfdi4plants.github.io/arc-validate/ARCExpect/design.html).

### How can I use a validation package?

Although package scripts can be downloaded from the browser, use
[`arc-validate`](https://github.com/nfdi4plants/arc-validate) to install and run
them locally, or use a CQC pipeline on the DataPLANT DataHUB.

### What is Continuous Quality Control?

Continuous Quality Control continuously collects and reports selected quality
metrics throughout an ARC's lifecycle. It can cover metadata annotations,
exportability to endpoint repositories, and other project-specific checks by
repeatedly validating the ARC against selected packages. The
[PLANTDataHUB paper](https://doi.org/10.1111/tpj.16474) provides further
background.

### How can I contribute a validation package?

Submit the package to the repository staging area for review. Once approved,
it can be published for others to install. Follow the
[package-submission workflow](packages/submission.md) for layout, metadata,
versioning, and publication requirements. The
[ARCExpect documentation](https://nfdi4plants.github.io/arc-validate/ARCExpect/design.html)
covers authoring packages with the reference implementation.

### How can I validate my ARC locally?

Use [`arc-validate`](https://github.com/nfdi4plants/arc-validate) to manage
validation packages and execute them against a local ARC.
