## v0.0.8
- Fix content hash being dependent on line endings (now, all content is normalized to \n before hashing)
- Fix code duplication in create functions for `ValidationPackageIndex`
- Unify `create` functions for Domain types

## v0.0.7

fix preview index download url

## v0.0.6

- Refactor and expose parsing & convenience functions:
  - Frontmatter
    - containsFrontmatter
    - tryExtractFromString
    - extractFromString
  - ValidationPackageMetadata
    - tryExtractFromString
    - extractFromString
    - tryExtractFromScript
    - extractFromScript

all frontmatter/metadata extraction functions will replace line endings with "\n", as YamlDotNet will replace any line endings with new line when presented the string anyways.

that way, the extracted frontmatter/metadata (especially field values, which caused problems due to YamlDotNet's default behavior) will be consistent across different platforms.

## v0.0.5

- Add `getPreviewIndex` function that downloads the currently released preview index from the github release.

## v0.0.4

- Replace line endings when parsing frontmatter

## v0.0.3

- Add create function to Author and OntologyAnnotation (https://github.com/nfdi4plants/arc-validate-package-registry/pull/27) 

## v0.0.2

- Add qol Domain functions (https://github.com/nfdi4plants/arc-validate-package-registry/pull/26)

## v0.0.1

- Initial release for AVPR API v1