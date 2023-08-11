# arc-validate-packages

A repository of validation packages for ARCs

This repo is indexes validation packages that can be consumed by [`arc-validate`](https://github.com/nfdi4plants/arc-validate) to validate ARCs.

## How to add packages

This repo runs a [custom pre-commit hook](.git/hooks/pre-commit) that will automatically add any `.fsx` file in the [arc-validate-packages folder](./arc-validate-packages/) to [the package index](./arc-validate-package-index.json) when it is commited to the repo.