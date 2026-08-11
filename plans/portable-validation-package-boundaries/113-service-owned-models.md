# AVPR #113: move registry API and EF models under service ownership

## Status

Implemented for
[AVPR #113](https://github.com/nfdi4plants/arc-validate-package-registry/issues/113).

## Objective

Remove the registry service's dependency on nested `AVPRIndex.Domain` types
without changing its public JSON/OpenAPI contract or PostgreSQL storage shape.
The service continues to use one model for ASP.NET transport and EF
persistence, while the portable model remains free of STJ and EF concerns.

## Implementation slices

1. Add separate service-owned files for `Author`, `OntologyAnnotation`, and
   the CWL input types under `PackageRegistryService/Models/`.
2. Keep lower-camel CWL member names and scalar command-input type strings in
   the public JSON contract through service-local STJ attributes and a
   `CommandInputType` converter.
3. Keep the normalized `primitiveType`/`isNullable` object in EF-owned JSON
   through service-local fluent configuration and the primitive storage
   converter.
4. Add explicit, bidirectional mappings for portable nested types and package
   metadata. Map `StagedValidationPackage` to the service entity only at the
   service initialization boundary.
5. Replace service SemVer, normalized-content, and hash calls with
   `ValidationPackage.Model` and `AVPR.Staging`, then remove the direct
   `AVPRIndex` project reference.
6. Verify full and empty metadata mappings, every CWL primitive, exact API
   JSON, OpenAPI, page rendering, and the EF model shape.

## Compatibility gates

- The public wrapper remains PascalCase `Inputs`.
- CWL members remain `id`, `type`, `label`, `doc`, and `inputBinding`.
- Public command-input types remain scalar strings such as `boolean?`.
- EF continues to store the command-input type as a normalized object with a
  lowercase primitive and boolean nullability.
- Missing optional collections continue to map to empty collections.
- EF must report no pending model changes; no migration is added when the
  generated relational model is unchanged.

## Verification

- `dotnet test tests/APITests/APITests.csproj --configuration Release`
- `dotnet ef migrations has-pending-model-changes` for the service project
- `dotnet test arc-validate-package-registry.slnx --configuration Release`

PostgreSQL migration/backfill verification is required only if EF reports an
intentional relational change. This implementation is designed to preserve
the existing relational and owned-JSON schema.
