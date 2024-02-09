# arc-validate-package-registry

This repository contains:

- a staging area for authoring official validation packages intended for use with [`arc-validate`](). 
- a web API for serving validation packages. This API is consumed by `arc-validate` to install and sync validation packages.

## Table of contents

- [Validation package staging area](#validation-package-staging-area)
  - [The package index](#the-package-index)
  - [Automated package testing](#automated-package-testing)
- [Web API (PackageRegistryService)](#web-api-packageregistryservice)
  - [Local development](#local-development)
  - [OpenAPI endpoint documentation via Swagger UI](#openapi-endpoint-documentation-via-swagger-ui)
- [Setup](#setup)
  - [package indexing](#package-indexing)
  - [local development](#local-development-1)
- [How to add packages](#how-to-add-packages)
  - [Package metadata](#package-metadata)
    - [Mandatory fields](#mandatory-fields)
    - [Optional fields](#optional-fields)
  - [Package publication workflow](#package-publication-workflow)
  - [Versioning packages](#versioning-packages)


## Validation package staging area

### The package index

This repo runs a [custom pre-commit hook](pre-commit.sh) that will run a [script](./update-index.fsx) automatically add any `.fsx` file in the [staging area](src/PackageRegistryService/StagingArea/) to [the package index](src/PackageRegistryService/Data/arc-validate-package-index.json) when it is commited to the repo.

### Automated package testing

Tests located at [./tests](./tests) are run on every package in the index. Only if all packages pass these tests, the docker container will be built and pushed to the registry.

## Web API (PackageRegistryService)

The `PackageRegistryService` project located in `/src` is a simple ASP.NET Core (8) web API that serves validation packages and/or associated metadata via a few endpoints.

It is developed specifically for containerization and use in a docker environment. 

The service will eventually be continuously deployed to a public endpoint on the nfdi4plants infrastructure.

### Local development

To run the `PackageRegistryService` locally, ideally use VisualStudio and run the `Docker Compose` project in Debug mode. This will launch the stack defined at [`docker-compose.yml`](docker-compose.yml), which includes:

- the containerized `PackageRegistryService` application 
- a `postgres` database seeded with the [latest indexed packages](src/PackageRegistryService/Data/arc-validate-package-index.json)
- an `adminer` instance for database management (will maybe be replaced by pgAdmin in the future)

### OpenAPI endpoint documentation via Swagger UI

The `PackageRegistryService` has a built-in Swagger UI endpoint for API documentation. It is served at `/swagger/index.html`.

## Setup

### package indexing

To install the pre-commit hook needed for automatic package indexing, run either `setup.cmd` or `setup.sh` depending on your platform to install the pre-commit hook.

### local development

install the following prerequisites:
- .NET 8 SDK
- Docker
- Docker Compose

## How to add packages

To add a package to the staging area, make sure that you installed the pre-commit hook as described in the [Setup](#setup) section. 

Then, simply add a new `.fsx` file to the [staging area](src/PackageRegistryService/StagingArea/), and commit it to the repo. The pre-commit hook will automatically add the new package to the package index.

All packages in the staging area are automatically tested on every commit. Additionally, all packages set to `publish: true` in their yml frontmatter will be pushed to the registry service if they pass all tests and are not already present in the registry.

### Package metadata

Package metadata is extracted from **yml frontmatter** at the start of the `.fsx` file, indicated by a multiline comment (`(* ... *)`)containing the frontmatter fenced by `---` at its start and end:
  
```fsharp
(*
---
<yaml frontmatter here>
---
*)
```

#### Mandatory fields

| Field | Type | Description |
| --- | --- | --- |
| Name | string | the name of the package |
| MajorVersion | int | the major version of the package |
| MinorVersion | int | the minor version of the package |
| PatchVersion | int | the patch version of the package |
| Description | string | a short description of the package |

example (only mandatory fields):

```fsharp
(*
---
Name: my-package
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Description: My package does the thing.
---
*)
let doSomeValidation () = ()
doSomeValidation ()
```

#### Optional fields

| Field | Type | Description |
| --- | --- | --- |
| Publish | bool | a boolean value indicating whether the package should be published to the registry. If set to `true`, the package will be built and pushed to the registry. If set to `false` (or not present), the package will be ignored. |
| Authors | author[] | the authors of the package. Authors have a mandatory **FullName** field and optional **Email**, **Affiliation** and **AffiliationLink** fields |
| Tags | string[] | a list of tags that describe the package |
| ReleaseNotes | string[] | a list of release notes for the package indicating changes from previous versions |

example (all fields):

```fsharp
(*
---
Name: my-package
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Description: My package does the thing.
Publish: true
Authors:
  - FullName: John Doe
    Email: j@d.com
    Affiliation: University of Nowhere
    AffiliationLink: https://nowhere.edu
  - FullName: Jane Doe
    Email: jj@d.com
    Affiliation: University of Somewhere
    AffiliationLink: https://somewhere.edu
Tags:
  - validation
  - my-package
  - thing
ReleaseNotes: " <- note that these must be a single string, not a list of strings
  - initial release
    - does the thing
    - does it well"
---
*)

let doSomeValidation () = ()
doSomeValidation ()
```
### Package publication workflow

Publishing a package to the registry is a multi-step process:

Suppose you want to develop version 1.0.0 of a package called `my-package`.

1. Add a new blank `my-package@1.0.0.fsx` file to the [staging area](./src/PackageRegistryService/StagingArea/) in the folder `my-package`.
2. Develop the package, using this repositories CI to perform automates integrity tests on it.
3. Once the package is ready, add `publish: true` to the yml frontmatter of the package file. This will trigger the CI to build and push the package to the registry.
4. Once a package is published, it cannot be unpublished or changed. To update a package, create a new script with the same name and a higher version number.

| stage | availability | mutability |
| --- | --- | --- |
| staging: development in this repo | version of current HEAD commit in this repo via github API-based execution in `arc-validate` CLI | any changes are allowed |
| published: available in the registry | version of the published package via the registry API | no changes are allowed |

### Versioning packages