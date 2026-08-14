# AVPRClient.Interop

`AVPRClient.Interop` maps between the generated `AVPRClient` transport types
and the portable types in `ValidationPackage.Model`.

The lightweight `ValidationPackageIdentity` and `ValidationPackageMetadata`
transport DTOs map to the portable identity and metadata types. Identity and
metadata conversions parse the endpoint's canonical full-SemVer string rather
than duplicating semantic-version logic in the client boundary.

The generated client deliberately has no dependency on the portable model,
YAML codecs, or AVPR staging infrastructure. Applications that need model
conversion can reference this package explicitly and import
`AVPRClient.Interop` to use the `ToModel`, `ToClient`, and `IdentityEquals`
extension methods.

Converting portable metadata to a client validation package requires callers
to provide the package bytes and release date because those transport fields
are not part of the portable metadata model.
