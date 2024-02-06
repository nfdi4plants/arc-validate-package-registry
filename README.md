# arc-validate-package-registry

This repository contains:

- a staging area for authoring official validation packages intended for use with [`arc-validate`](). 
- a web API for serving validation packages. This API is consumed by `arc-validate` to install and sync validation packages.

## Table of contents

- [arc-validate-package-registry](#arc-validate-package-registry)
  - [Table of contents](#table-of-contents)
- [Validation package staging area](#validation-package-staging-area)
  - [the package index](#the-package-index)
- [Web API (PackageRegistryService)](#web-api-packageregistryservice)
  - [Local development](#local-development)
  - [OpenAPI endpoint documentation via Swagger UI](#openapi-endpoint-documentation-via-swagger-ui)
- [Setup](#setup)
  - [package indexing](#package-indexing)
  - [local development](#local-development-1)
- [How to add packages](#how-to-add-packages)


# Validation package staging area

## the package index

This repo runs a [custom pre-commit hook](pre-commit.sh) that will run a [script](./update-index.fsx) automatically add any `.fsx` file in the [staging area](src/PackageRegistryService/StagingArea/) to [the package index](src/PackageRegistryService/Data/arc-validate-package-index.json) when it is commited to the repo.

# Web API (PackageRegistryService)

The `PackageRegistryService` project located in `/src` is a simple ASP.NET Core (8) web API that serves validation packages and/or associated metadata via a few endpoints.

It is developed specifically for containerization and use in a docker environment. 

The service will eventually be continuously deployed to a public endpoint on the nfdi4plants infrastructure.

## Local development

To run the `PackageRegistryService` locally, ideally use VisualStudio and run the `Docker Compose` project in Debug mode. This will launch the stack defined at [`docker-compose.yml`](docker-compose.yml), which includes:

- the containerized `PackageRegistryService` application 
- a `postgres` database seeded with the [latest indexed packages](src/PackageRegistryService/Data/arc-validate-package-index.json)
- an `adminer` instance for database management (will maybe be replaced by pgAdmin in the future)

## OpenAPI endpoint documentation via Swagger UI

The `PackageRegistryService` has a built-in Swagger UI endpoint for API documentation. It is served at `/swagger/index.html`.

# Setup

## package indexing

To install the pre-commit hook needed for automatic package indexing, run either `setup.cmd` or `setup.sh` depending on your platform to install the pre-commit hook.

## local development

install the following prerequisites:
- .NET 8 SDK
- Docker
- Docker Compose

# How to add packages

To add a package to the staging area, make sure that you installed the pre-commit hook as described in the [Setup](#setup) section. 

Then, simply add a new `.fsx` file to the [staging area](src/PackageRegistryService/StagingArea/), and commit it to the repo. The pre-commit hook will automatically add the new package to the package index.

If all packages on the index pass a set of tests, the docker container will be built and pushed to the registry, and can from there be deployed to the public endpoint.