## 0.3.0-preview.4 - 2026-08-14
- Add generated clients for package-index, version-list, and metadata-only
  discovery endpoints.
- Mark the legacy all-content package collection operation as deprecated.
- Regenerate the client for the supported CWL v1.2 scalar `Inputs` contract.
- Move all handwritten model and staging mappings to `AVPRClient.Interop` and `AVPRCI`, leaving this package generated-only.
- Replace token-based publication with a manually approved, retry-safe NuGet
  trusted-publishing workflow.

## v0.2.1
- fix : Correct `ProgrammingLanguage` field mapping in `ValidationPackage` type extension.

## v0.2.0
- Regen client for `ProgrammingLanguage` field 

## v0.1.2
- Add more `IdentityEquals` extension methods to compare between `ValidationPackage` and `ValidationPackageIndex`

## v0.1.1
- Add `IdentityEquals` extension method for `ValidationPackage`

## v0.1.0
- Regen client for full semVer support (AVPRIndex >= 0.2.0)

## v0.0.9
- Use AVPRIndex for all package binary content extractions

## v0.0.8
- Use AVPRIndex for all package hash calculations

## v0.0.7
- Further TypeExtension improvements and fixes

## v0.0.6
- Fix some missing fields in type extensions

## v0.0.5
- Regen Client with new `CQCHookEndpoint` field in package metadata

## v0.0.4

- Add more interop extensions between AVPRClient and AVPRIndex
- Regen Client with new statistics API Endpoints
 
## v0.0.3

- Add extensions that connect AVPRIndex and AVPRClient types

## v0.0.2

- Regen with for additional metadata fields

## v0.0.1

- Initial release for AVPR API v1
