namespace DomainTests

open System
open System.IO
open Xunit
open AVPRIndex
open AVPRIndex.Domain
open ReferenceObjects

module SemVer =

    [<Fact>]
    let ``create function for mandatory fields``() =
        let actual = SemVer.create(major=1, minor=0, patch=0)
        Assert.Equivalent(SemVer.SemVers.mandatory, actual)
        Assert.Equal(SemVer.SemVers.mandatory, actual)

    [<Fact>]
    let ``create function for all fields``() =
        let actual = SemVer.create(major=1, minor=0, patch=0, PreRelease="alpha.1", BuildMetadata="build.1")
        Assert.Equivalent(SemVer.SemVers.prereleaseAndBuildmetadata, actual)
        Assert.Equal(SemVer.SemVers.prereleaseAndBuildmetadata, actual)

    [<Fact>]
    let ``Can parse mandatory``() =
        let actual = SemVer.Strings.mandatory |> SemVer.tryParse
        Assert.True(actual.IsSome)
        Assert.Equivalent(SemVer.SemVers.mandatory, actual.Value)
        Assert.Equal(SemVer.SemVers.mandatory, actual.Value)

    [<Fact>]
    let ``Can parse prerelease``() =
        let actual = SemVer.Strings.prerelease |> SemVer.tryParse
        Assert.True(actual.IsSome)
        Assert.Equivalent(SemVer.SemVers.prerelease, actual.Value)
        Assert.Equal(SemVer.SemVers.prerelease, actual.Value)

    [<Fact>]
    let ``Can parse buildmetadata``() =
        let actual = SemVer.Strings.buildmetadata |> SemVer.tryParse
        Assert.True(actual.IsSome)
        Assert.Equivalent(SemVer.SemVers.buildmetadata, actual.Value)
        Assert.Equal(SemVer.SemVers.buildmetadata, actual.Value)

    [<Fact>]
    let ``Can parse prereleaseAndBuildmetadata``() =
        let actual = SemVer.Strings.prereleaseAndBuildmetadata |> SemVer.tryParse
        Assert.True(actual.IsSome)
        Assert.Equivalent(SemVer.SemVers.prereleaseAndBuildmetadata, actual.Value)
        Assert.Equal(SemVer.SemVers.prereleaseAndBuildmetadata, actual.Value)

    [<Fact>]
    let ``Can parse major version 0``() =
        let actual = SemVer.Strings.majorZero |> SemVer.tryParse
        Assert.True(actual.IsSome)
        Assert.Equivalent(SemVer.SemVers.majorZero, actual.Value)
        Assert.Equal(SemVer.SemVers.majorZero, actual.Value)

    [<Fact>]
    let ``Cannot parse leading 0 in major``() =
        let actual = SemVer.Strings.invalidLeadingZero |> SemVer.tryParse
        Assert.True(actual.IsNone)

    [<Fact>]
    let ``Cannot parse leading 0 in minor``() =
        let actual = SemVer.Strings.invalidLeadingZeroMinor |> SemVer.tryParse
        Assert.True(actual.IsNone)

    [<Fact>]
    let ``Cannot parse leading 0 in patch``() =
        let actual = SemVer.Strings.invalidLeadingZeroPatch |> SemVer.tryParse
        Assert.True(actual.IsNone)

    [<Fact>]
    let ``Cannot parse leading 0 in prerelease``() =
        let actual = SemVer.Strings.invalidLeadingZeroPrerelease |> SemVer.tryParse
        Assert.True(actual.IsNone)

    [<Fact>]
    let ``Can write mandatory``() =
        let actual = SemVer.SemVers.mandatory |> SemVer.toString
        Assert.Equivalent(SemVer.Strings.mandatory, actual)
        Assert.Equal(SemVer.Strings.mandatory, actual)

    [<Fact>]
    let ``Can write prerelease``() =
        let actual = SemVer.SemVers.prerelease |> SemVer.toString
        Assert.Equivalent(SemVer.Strings.prerelease, actual)
        Assert.Equal(SemVer.Strings.prerelease, actual)

    [<Fact>]
    let ``Can write buildmetadata``() =
        let actual = SemVer.SemVers.buildmetadata |> SemVer.toString
        Assert.Equivalent(SemVer.Strings.buildmetadata, actual)
        Assert.Equal(SemVer.Strings.buildmetadata, actual)

    [<Fact>]
    let ``Can write prereleaseAndBuildmetadata``() =
        let actual = SemVer.SemVers.prereleaseAndBuildmetadata |> SemVer.toString
        Assert.Equivalent(SemVer.Strings.prereleaseAndBuildmetadata, actual)
        Assert.Equal(SemVer.Strings.prereleaseAndBuildmetadata, actual)


module Author =
    
    [<Fact>]
    let ``create function for mandatory fields``() =
        let actual = Author.create(fullName = "test")
        Assert.Equivalent(Author.mandatoryFields, actual)

    [<Fact>]
    let ``create function for all fields``() =
        let actual = Author.create(
            fullName = "test", 
            Email = "test@test.test",
            Affiliation = "testaffiliation",
            AffiliationLink = "test.com"
        )
        Assert.Equivalent(Author.allFields, actual)

module OntologyAnnotation =

    [<Fact>]
    let ``create function for mandatory fields``() =
        let actual = OntologyAnnotation.create(name = "test")
        Assert.Equivalent(OntologyAnnotation.mandatoryFields, actual)

    [<Fact>]
    let ``create function for all fields``() =
        let actual = OntologyAnnotation.create(
            name = "test",
            TermSourceREF = "REF",
            TermAccessionNumber = "TAN"
        )
        Assert.Equivalent(OntologyAnnotation.allFields, actual)

module CommandInputType =

    [<Fact>]
    let ``create function uses required primitive type by default`` () =
        let actual = CommandInputType.create(CwlPrimitive.String)
        Assert.Equivalent(CommandInputType.requiredString, actual)

    [<Fact>]
    let ``create function supports nullable primitive type`` () =
        let actual = CommandInputType.create(CwlPrimitive.Boolean, true)
        Assert.Equivalent(CommandInputType.nullableBoolean, actual)

    [<Fact>]
    let ``primitive type setter rejects undefined enum values`` () =
        let actual = CommandInputType()
        Assert.Throws<ArgumentException>(fun () ->
            actual.PrimitiveType <- enum<CwlPrimitive> 999
        )
        |> ignore

    [<Fact>]
    let ``equality and hashing include primitive type and nullability`` () =
        let requiredString = CommandInputType.create(CwlPrimitive.String)
        let sameRequiredString = CommandInputType.create(CwlPrimitive.String)
        let nullableString = CommandInputType.create(CwlPrimitive.String, true)
        let requiredBoolean = CommandInputType.create(CwlPrimitive.Boolean)

        Assert.Equal(requiredString, sameRequiredString)
        Assert.Equal(requiredString.GetHashCode(), sameRequiredString.GetHashCode())
        Assert.NotEqual(requiredString, nullableString)
        Assert.NotEqual(requiredString, requiredBoolean)

module CommandInputBinding =

    [<Fact>]
    let ``create function uses CWL binding defaults`` () =
        let actual = CommandInputBinding.create()
        Assert.Equivalent(CommandInputBinding.defaultFields, actual)

    [<Fact>]
    let ``create function supports all binding fields`` () =
        let actual =
            CommandInputBinding.create(
                Position = 2,
                Prefix = "--output=",
                Separate = false
            )
        Assert.Equivalent(CommandInputBinding.allFields, actual)

    [<Fact>]
    let ``equality and hashing include every binding field`` () =
        let defaults = CommandInputBinding.create()
        let sameDefaults = CommandInputBinding.create()

        Assert.Equal(defaults, sameDefaults)
        Assert.Equal(defaults.GetHashCode(), sameDefaults.GetHashCode())
        Assert.NotEqual(defaults, CommandInputBinding.create(Position = 1))
        Assert.NotEqual(defaults, CommandInputBinding.create(Prefix = "--value"))
        Assert.NotEqual(defaults, CommandInputBinding.create(Separate = false))

module CommandInputParameter =

    [<Fact>]
    let ``create function supports mandatory fields`` () =
        let actual =
            CommandInputParameter.create(
                "input",
                CommandInputType.create(CwlPrimitive.String, true),
                CommandInputBinding(Prefix = "--input")
            )
        Assert.Equivalent(CommandInputParameter.mandatoryFields, actual)

    [<Fact>]
    let ``create function supports all fields`` () =
        let actual =
            CommandInputParameter.create(
                "output",
                CommandInputType.requiredString,
                CommandInputBinding.allFields,
                Label = "Output file",
                Doc = "Write output to this file"
            )
        Assert.Equivalent(CommandInputParameter.allFields, actual)

    [<Fact>]
    let ``equality and hashing include nested type and binding`` () =
        let input =
            CommandInputParameter.create(
                "value",
                CommandInputType.create(CwlPrimitive.String),
                CommandInputBinding.create()
            )
        let sameInput =
            CommandInputParameter.create(
                "value",
                CommandInputType.create(CwlPrimitive.String),
                CommandInputBinding.create()
            )
        let nullableInput =
            CommandInputParameter.create(
                "value",
                CommandInputType.create(CwlPrimitive.String, true),
                CommandInputBinding.create()
            )
        let prefixedInput =
            CommandInputParameter.create(
                "value",
                CommandInputType.create(CwlPrimitive.String),
                CommandInputBinding.create(Prefix = "--value")
            )

        Assert.Equal(input, sameInput)
        Assert.Equal(input.GetHashCode(), sameInput.GetHashCode())
        Assert.NotEqual(input, nullableInput)
        Assert.NotEqual(input, prefixedInput)

module ValidationPackageMetadata =
    
    [<Fact>]
    let ``create function for mandatory fields``() =
        let actual = ValidationPackageMetadata.create(
            name = "name",
            summary = "summary" ,
            description = "description" ,
            majorVersion = 1,
            minorVersion = 0,
            patchVersion = 0,
            programmingLanguage = "FSharp"
        )
        Assert.Equivalent(ValidationPackageMetadata.mandatoryFields, actual)

    [<Fact>]
    let ``create function for all fields``() =
        let actual = ValidationPackageMetadata.create(
            name = "name",
            summary = "summary" ,
            description = "description" ,
            majorVersion = 1,
            minorVersion = 0,
            patchVersion = 0,
            programmingLanguage = "FSharp",
            PreReleaseVersionSuffix = "alpha.1",
            BuildMetadataVersionSuffix = "build.1",
            Publish = true,
            Authors = [|
                Author.create(
                    fullName = "test", 
                    Email = "test@test.test",
                    Affiliation = "testaffiliation",
                    AffiliationLink = "test.com"
                )
            |],
            Tags = [|
            OntologyAnnotation.create(
                    name = "test",
                    TermSourceREF = "REF",
                    TermAccessionNumber = "TAN"
                )
            |],
            ReleaseNotes = "releasenotes",
            CQCHookEndpoint = "hookendpoint",
            Inputs = CommandInputParameter.canonicalInputs
        )
        Assert.Equivalent(ValidationPackageMetadata.allFields, actual)

    [<Fact>]
    let ``tryGetSemanticVersion from valid package metadata with mandatory fields``() =
        let actual = ValidationPackageMetadata.mandatoryFields |> ValidationPackageMetadata.tryGetSemanticVersion
        Assert.True(actual.IsSome)
        Assert.Equivalent(SemVer.SemVers.mandatory, actual.Value)
        Assert.Equal(SemVer.SemVers.mandatory, actual.Value)

    [<Fact>]
    let ``getSemanticVersion from valid package metadata with mandatory fields``() =
        let actual = ValidationPackageMetadata.mandatoryFields |> ValidationPackageMetadata.getSemanticVersion
        Assert.Equivalent(SemVer.SemVers.mandatory, actual)
        Assert.Equal(SemVer.SemVers.mandatory, actual)

    [<Fact>]
    let ``tryGetSemanticVersionString from valid package metadata with mandatory fields``() =
        let actual = ValidationPackageMetadata.mandatoryFields |> ValidationPackageMetadata.tryGetSemanticVersionString
        Assert.True(actual.IsSome)
        Assert.Equal(SemVer.Strings.mandatory, actual.Value)

    [<Fact>]
    let ``getSemanticVersionString from valid package metadata with mandatory fields``() =
        let actual = ValidationPackageMetadata.mandatoryFields |> ValidationPackageMetadata.getSemanticVersionString
        Assert.Equal(SemVer.Strings.mandatory, actual)

    [<Fact>]
    let ``tryGetSemanticVersion from valid package metadata with all fields``() =
        let actual = ValidationPackageMetadata.allFields |> ValidationPackageMetadata.tryGetSemanticVersion
        Assert.True(actual.IsSome)
        Assert.Equivalent(SemVer.SemVers.prereleaseAndBuildmetadata, actual.Value)
        Assert.Equal(SemVer.SemVers.prereleaseAndBuildmetadata, actual.Value)

    [<Fact>]
    let ``getSemanticVersion from valid package metadata with all fields``() =
        let actual = ValidationPackageMetadata.allFields |> ValidationPackageMetadata.getSemanticVersion
        Assert.Equivalent(SemVer.SemVers.prereleaseAndBuildmetadata, actual)
        Assert.Equal(SemVer.SemVers.prereleaseAndBuildmetadata, actual)

    [<Fact>]
    let ``tryGetSemanticVersionString from valid package metadata with all fields``() =
        let actual = ValidationPackageMetadata.allFields |> ValidationPackageMetadata.tryGetSemanticVersionString
        Assert.True(actual.IsSome)
        Assert.Equal(SemVer.Strings.prereleaseAndBuildmetadata, actual.Value)

    [<Fact>]
    let ``getSemanticVersionString from valid package metadata with all fields``() =
        let actual = ValidationPackageMetadata.allFields |> ValidationPackageMetadata.getSemanticVersionString
        Assert.Equal(SemVer.Strings.prereleaseAndBuildmetadata, actual)
