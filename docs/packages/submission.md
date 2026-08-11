# Submitting a validation package

## Contents

- [Staging-area layout](#staging-area-layout)
- [Submission workflow](#submission-workflow)
- [Package versioning](#package-versioning)
- [Automated checks](#automated-checks)

The repository's staging area is the development and review path for validation
packages that will be published through AVPR.

## Staging-area layout

Put each package exactly one directory below `StagingArea/`. The directory,
filename, and metadata name and version must agree:

```text
StagingArea/
├── some-package/
│   ├── some-package@1.0.0.fsx
│   ├── some-package@2.0.0.fsx
│   └── some-package@2.1.0.fsx
└── some-python-package/
    ├── some-python-package@1.0.0.py
    └── some-python-package@2.0.0.py
```

Validation packages must be self-contained, single-file scripts:

- F# packages use `.fsx`. External dependencies must use `#r "nuget: ..."`
  directives and packages run through `dotnet fsi`.
- Python packages use `.py`. Dependencies must use
  [PEP 723 inline script metadata](https://docs.astral.sh/uv/guides/scripts/#declaring-script-dependencies)
  and packages run through `uv run`.

Only `.fsx` and `.py` files are allowed in `StagingArea/`.

## Submission workflow

Suppose you want to develop version `1.0.0` of a package named `my-package`:

1. Fork the repository.
2. Add `StagingArea/my-package/my-package@1.0.0.fsx` or the corresponding
   `.py` file.
3. Develop the package in a work-in-progress pull request so CI continuously
   checks its layout, metadata, and syntax.
4. When the package is ready and reviewed, set `Publish: true` in its
   [frontmatter](metadata.md).
5. Merge the pull request. The publication pipeline publishes eligible
   packages after all staging and pre-publication checks pass.

Published packages are immutable. Never change a version already published to
the registry; submit a higher semantic version instead.

| Stage | Availability | Mutability |
| --- | --- | --- |
| Staging | Current repository revision | Changes are allowed |
| Published | Production registry API | Immutable |

## Package versioning

Use [Semantic Versioning](https://semver.org/):

- Increment the major version for incompatible behavior changes.
- Increment the minor version for backwards-compatible functionality.
- Increment the patch version for backwards-compatible fixes.

Filename suffixes and metadata suffixes must also match when using prerelease or
build metadata.

## Automated checks

Changes below `StagingArea/` trigger `StagingAreaTests`:

- Directory, filename, and metadata identities must match.
- Frontmatter and required metadata are validated.
- F# scripts are parsed and type-checked with FSharp.Compiler.Service.
- Python source is compiled without importing or executing it.

The default gate does not execute real staged package code. Runtime execution is
limited to small repository-owned fixtures. Package scripts can access the
network, traverse input data, or perform substantial computation, so read a
script before invoking it locally.

Run the staging checks with:

```shell
dotnet build PackageStagingArea.slnx --configuration Release
dotnet test PackageStagingArea.slnx --configuration Release --no-build
```

See [validation package metadata](metadata.md) for the required script header.
