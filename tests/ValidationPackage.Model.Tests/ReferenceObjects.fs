module ValidationPackage.Model.Tests.ReferenceObjects

open ValidationPackage.Model

module SemanticVersions =

    let mandatory = SemVer.create(1, 0, 0)
    let prerelease = SemVer.create(1, 0, 0, PreRelease = "alpha.1")
    let buildMetadata = SemVer.create(1, 0, 0, BuildMetadata = "build.1")

    let prereleaseAndBuildMetadata =
        SemVer.create(
            1,
            0,
            0,
            PreRelease = "alpha.1",
            BuildMetadata = "build.1"
        )

module CommandInputs =

    let requiredString = CommandInputType.create(CwlPrimitive.String)
    let nullableBoolean = CommandInputType.create(CwlPrimitive.Boolean, true)
    let defaultBinding = CommandInputBinding.create()

    let allFieldsBinding =
        CommandInputBinding.create(
            Position = 2,
            Prefix = "--output"
        )

    let mandatoryParameter =
        CommandInputParameter.create(
            "input",
            CommandInputType.create(CwlPrimitive.String, true),
            CommandInputBinding.create(Prefix = "--input")
        )

    let allFieldsParameter =
        CommandInputParameter.create(
            "output",
            requiredString,
            allFieldsBinding,
            Label = "Output file",
            Doc = "Write output to this file"
        )

module Metadata =

    let mandatory =
        ValidationPackageMetadata.create(
            name = "name",
            summary = "summary",
            description = "description",
            majorVersion = 1,
            minorVersion = 0,
            patchVersion = 0,
            programmingLanguage = "FSharp"
        )

    let allFields =
        ValidationPackageMetadata.create(
            name = "name",
            summary = "summary",
            description = "description",
            majorVersion = 1,
            minorVersion = 0,
            patchVersion = 0,
            programmingLanguage = "FSharp",
            PreReleaseVersionSuffix = "alpha.1",
            BuildMetadataVersionSuffix = "build.1",
            Publish = true,
            Authors =
                [|
                    Author.create(
                        fullName = "Test Author",
                        Email = "test@example.org",
                        Affiliation = "DataPLANT",
                        AffiliationLink = "https://nfdi4plants.org"
                    )
                |],
            Tags =
                [|
                    OntologyAnnotation.create(
                        name = "validation",
                        TermSourceRef = "AVPR",
                        TermAccessionNumber = "AVPR:validation"
                    )
                |],
            ReleaseNotes = "Initial release",
            CQCHookEndpoint = "https://example.org/hooks/cqc",
            Inputs = [| CommandInputs.allFieldsParameter |]
        )
