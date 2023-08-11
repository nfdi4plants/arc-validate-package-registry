# arc-validate-packages

A repository of validation packages for ARCs

This repo is indexes validation packages that can be consumed by [`arc-validate`](https://github.com/nfdi4plants/arc-validate) to validate ARCs.

Once released as v2.0, The `arc-validate` tool will be able to sync a local copy of any validation package indexed in this repo, and perform the contained validation tests on an ARC. 

This has the advantage that any new validation tests added to this repo will be automatically available to all users of `arc-validate` without requiring a new release of the tool, while also providing clearly separated tests for various endpoints, e.g. a validation package that only tests wether an ARC can be exported to SRA.

## How to add packages

This repo runs a [custom pre-commit hook](update-index.fsx) that will automatically add any `.fsx` file in the [arc-validate-packages folder](./arc-validate-packages/) to [the package index](./arc-validate-package-index.json) when it is commited to the repo.

A tutorial on authoring validation packages will be released here once the arc-validate 2.0 API is stable.
