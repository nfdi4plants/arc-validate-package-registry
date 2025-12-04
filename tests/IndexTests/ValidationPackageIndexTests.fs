namespace ValidationPackageIndexTests

open System
open System.IO
open Xunit
open AVPRIndex
open AVPRIndex.Domain
open AVPRIndex.Frontmatter
open Utils
open ReferenceObjects

module StaticMethods = 

    module FSharp =

        [<Fact>]
        let ``create function for mandatory fields``() =
            let actual =
                ValidationPackageIndex.create(
                    repoPath = "fixtures/Frontmatter/Comment/valid@1.0.0.fsx",
                    fileName = "valid@1.0.0.fsx",
                    lastUpdated = testDate,
                    contentHash = "2A29D85A29D908C7DE214D56119DE207",
                    metadata = ValidationPackageMetadata.create(
                        name = "valid",
                        majorVersion = 1,
                        minorVersion = 0,
                        patchVersion = 0,
                        summary = "My package does the thing.",
                        description = """My package does the thing.
It does it very good, it does it very well.
It does it very fast, it does it very swell.
""".ReplaceLineEndings("\n"),
                        programmingLanguage = "FSharp")
                )
            Assert.Equivalent(ValidationPackageIndex.FSharp.CommentFrontmatter.validMandatoryFrontmatter, actual)
            Assert.Equal(ValidationPackageIndex.FSharp.CommentFrontmatter.validMandatoryFrontmatter, actual)

        [<Fact>]
        let ``create function for all fields``() =
            let actual =
                ValidationPackageIndex.create(
                    repoPath = "fixtures/Frontmatter/Comment/valid@2.0.0.fsx",
                    fileName = "valid@2.0.0.fsx",
                    lastUpdated = testDate,
                    contentHash = "E2BE9000A07122842FC805530DDC9FDA",
                    metadata = ValidationPackageMetadata.create(
                        name = "valid",
                        majorVersion = 2,
                        minorVersion = 0,
                        patchVersion = 0,
                        summary = "My package does the thing.",
                        description = """My package does the thing.
It does it very good, it does it very well.
It does it very fast, it does it very swell.
""".ReplaceLineEndings("\n"),
                        programmingLanguage = "FSharp",
                        PreReleaseVersionSuffix = "alpha.1",
                        BuildMetadataVersionSuffix = "build.1",
                        Publish = true,
                        Authors = [|
                            Author(
                                FullName = "John Doe",
                                Email = "j@d.com",
                                Affiliation = "University of Nowhere",
                                AffiliationLink = "https://nowhere.edu"
                            )
                            Author(
                                FullName = "Jane Doe",
                                Email = "jj@d.com",
                                Affiliation = "University of Somewhere",
                                AffiliationLink = "https://somewhere.edu"
                            )
                        |],
                        Tags = [|
                            OntologyAnnotation(Name = "validation")
                            OntologyAnnotation(Name = "my-tag", TermSourceREF = "my-ontology", TermAccessionNumber = "MO:12345")
                        |],
                        ReleaseNotes = """- initial release
  - does the thing
  - does it well
""".ReplaceLineEndings("\n"),
                        CQCHookEndpoint = "https://hook.com"
                    )
                )
            Assert.Equivalent(ValidationPackageIndex.FSharp.CommentFrontmatter.validFullFrontmatter, actual)
            Assert.Equal(ValidationPackageIndex.FSharp.CommentFrontmatter.validFullFrontmatter, actual)

        [<Fact>]
        let ``tryGetSemanticVersion from valid package with mandatory fields``() =
            let actual = ValidationPackageIndex.FSharp.CommentFrontmatter.validMandatoryFrontmatter |> ValidationPackageIndex.tryGetSemanticVersion
            Assert.True(actual.IsSome)
            Assert.Equivalent(SemVer.SemVers.mandatory, actual.Value)
            Assert.Equal(SemVer.SemVers.mandatory, actual.Value)

        [<Fact>]
        let ``getSemanticVersion from valid package with mandatory fields``() =
            let actual = ValidationPackageIndex.FSharp.CommentFrontmatter.validMandatoryFrontmatter |> ValidationPackageIndex.getSemanticVersion
            Assert.Equivalent(SemVer.SemVers.mandatory, actual)
            Assert.Equal(SemVer.SemVers.mandatory, actual)

        [<Fact>]
        let ``tryGetSemanticVersionString from valid package with mandatory fields``() =
            let actual = ValidationPackageIndex.FSharp.CommentFrontmatter.validMandatoryFrontmatter |> ValidationPackageIndex.tryGetSemanticVersionString
            Assert.True(actual.IsSome)
            Assert.Equal(SemVer.Strings.mandatory, actual.Value)

        [<Fact>]
        let ``getSemanticVersionString from valid package with mandatory fields``() =
            let actual = ValidationPackageIndex.FSharp.CommentFrontmatter.validMandatoryFrontmatter |> ValidationPackageIndex.getSemanticVersionString
            Assert.Equal(SemVer.Strings.mandatory, actual)

        [<Fact>]
        let ``tryGetSemanticVersion from valid package with all fields``() =
            let actual = ValidationPackageIndex.FSharp.CommentFrontmatter.validFullFrontmatter |> ValidationPackageIndex.tryGetSemanticVersion
            Assert.True(actual.IsSome)
            Assert.Equivalent(SemVer.SemVers.fixtureFile, actual.Value)
            Assert.Equal(SemVer.SemVers.fixtureFile, actual.Value)

        [<Fact>]
        let ``getSemanticVersion from valid package with all fields``() =
            let actual = ValidationPackageIndex.FSharp.CommentFrontmatter.validFullFrontmatter |> ValidationPackageIndex.getSemanticVersion
            Assert.Equivalent(SemVer.SemVers.fixtureFile, actual)
            Assert.Equal(SemVer.SemVers.fixtureFile, actual)

        [<Fact>]
        let ``tryGetSemanticVersionString from valid package with all fields``() =
            let actual = ValidationPackageIndex.FSharp.CommentFrontmatter.validFullFrontmatter |> ValidationPackageIndex.tryGetSemanticVersionString
            Assert.True(actual.IsSome)
            Assert.Equal(SemVer.Strings.fixtureFile, actual.Value)

        [<Fact>]
        let ``getSemanticVersionString from valid package with all fields``() =
            let actual = ValidationPackageIndex.FSharp.CommentFrontmatter.validFullFrontmatter |> ValidationPackageIndex.getSemanticVersionString
            Assert.Equal(SemVer.Strings.fixtureFile, actual)

        [<Fact>]
        let ``identityEquals returns true for identical versions``() =
            Assert.True(
                ValidationPackageIndex.identityEquals
                    ValidationPackageIndex.FSharp.CommentFrontmatter.validMandatoryFrontmatter
                    ValidationPackageIndex.FSharp.CommentFrontmatter.validMandatoryFrontmatter
            )

        [<Fact>]
        let ``identityEquals returns false for non-identical versions``() =
            Assert.False(
                ValidationPackageIndex.identityEquals
                    ValidationPackageIndex.FSharp.CommentFrontmatter.validMandatoryFrontmatter
                    ValidationPackageIndex.FSharp.CommentFrontmatter.validFullFrontmatter
            )

        [<Fact>]
        let ``identityEquals returns false for same version with suffixes``() =
            let a = ValidationPackageIndex.create(
                repoPath = "",
                fileName = "",
                lastUpdated = testDate,
                contentHash = "",
                metadata = ValidationPackageMetadata.create(
                    name = "",
                    majorVersion = 1,
                    minorVersion = 0,
                    patchVersion = 0,
                    summary = "",
                    description = "",
                    programmingLanguage = "FSharp",
                    PreReleaseVersionSuffix = "some",
                    BuildMetadataVersionSuffix = "suffix"
                )
            )
            let b = ValidationPackageIndex.create(
                repoPath = "",
                fileName = "",
                lastUpdated = testDate,
                contentHash = "",
                metadata = ValidationPackageMetadata.create(
                    name = "",
                    majorVersion = 1,
                    minorVersion = 0,
                    patchVersion = 0,
                    summary = "",
                    description = "",
                    programmingLanguage = "FSharp"
                )
            )
            Assert.False(ValidationPackageIndex.identityEquals a b)

        [<Fact>]
        let ``identityEquals returns true for identical version with suffixes``() =
            let a = ValidationPackageIndex.create(
                repoPath = "",
                fileName = "",
                lastUpdated = testDate,
                contentHash = "",
                metadata = ValidationPackageMetadata.create(
                    name = "",
                    majorVersion = 1,
                    minorVersion = 0,
                    patchVersion = 0,
                    summary = "",
                    description = "",
                    programmingLanguage = "FSharp",
                    PreReleaseVersionSuffix = "some",
                    BuildMetadataVersionSuffix = "suffix"
                )
            )
            let b = ValidationPackageIndex.create(
                repoPath = "",
                fileName = "",
                lastUpdated = testDate,
                contentHash = "",
                metadata = ValidationPackageMetadata.create(
                    name = "",
                    majorVersion = 1,
                    minorVersion = 0,
                    patchVersion = 0,
                    summary = "",
                    description = "",
                    programmingLanguage = "FSharp",
                    PreReleaseVersionSuffix = "some",
                    BuildMetadataVersionSuffix = "suffix"
                )
            )
            Assert.True(ValidationPackageIndex.identityEquals a b)

    module Python =

        [<Fact>]
        let ``create function for mandatory fields``() =
            let actual =
                ValidationPackageIndex.create(
                    repoPath = "fixtures/Frontmatter/Comment/valid@1.0.0.py",
                    fileName = "valid@1.0.0.py",
                    lastUpdated = testDate,
                    contentHash = "EB3E5827C147B660D4AE7F5560A7CFBA",
                    metadata = ValidationPackageMetadata.create(
                        name = "valid",
                        majorVersion = 1,
                        minorVersion = 0,
                        patchVersion = 0,
                        summary = "My package does the thing.",
                        description = """My package does the thing.
It does it very good, it does it very well.
It does it very fast, it does it very swell.
""".ReplaceLineEndings("\n"),
                        programmingLanguage = "Python")
                )
            Assert.Equivalent(ValidationPackageIndex.Python.CommentFrontmatter.validMandatoryFrontmatter, actual)
            Assert.Equal(ValidationPackageIndex.Python.CommentFrontmatter.validMandatoryFrontmatter, actual)

        [<Fact>]
        let ``create function for all fields``() =
            let actual =
                ValidationPackageIndex.create(
                    repoPath = "fixtures/Frontmatter/Comment/valid@2.0.0.py",
                    fileName = "valid@2.0.0.py",
                    lastUpdated = testDate,
                    contentHash = "AA2EC3E8DF93D50469C22C5894A1007D",
                    metadata = ValidationPackageMetadata.create(
                        name = "valid",
                        majorVersion = 2,
                        minorVersion = 0,
                        patchVersion = 0,
                        summary = "My package does the thing.",
                        description = """My package does the thing.
It does it very good, it does it very well.
It does it very fast, it does it very swell.
""".ReplaceLineEndings("\n"),
                        programmingLanguage = "Python",
                        PreReleaseVersionSuffix = "alpha.1",
                        BuildMetadataVersionSuffix = "build.1",
                        Publish = true,
                        Authors = [|
                            Author(
                                FullName = "John Doe",
                                Email = "j@d.com",
                                Affiliation = "University of Nowhere",
                                AffiliationLink = "https://nowhere.edu"
                            )
                            Author(
                                FullName = "Jane Doe",
                                Email = "jj@d.com",
                                Affiliation = "University of Somewhere",
                                AffiliationLink = "https://somewhere.edu"
                            )
                        |],
                        Tags = [|
                            OntologyAnnotation(Name = "validation")
                            OntologyAnnotation(Name = "my-tag", TermSourceREF = "my-ontology", TermAccessionNumber = "MO:12345")
                        |],
                        ReleaseNotes = """- initial release
  - does the thing
  - does it well
""".ReplaceLineEndings("\n"),
                        CQCHookEndpoint = "https://hook.com"
                    )
                )
            Assert.Equivalent(ValidationPackageIndex.Python.CommentFrontmatter.validFullFrontmatter, actual)
            Assert.Equal(ValidationPackageIndex.Python.CommentFrontmatter.validFullFrontmatter, actual)

        [<Fact>]
        let ``tryGetSemanticVersion from valid package with mandatory fields``() =
            let actual = ValidationPackageIndex.Python.CommentFrontmatter.validMandatoryFrontmatter |> ValidationPackageIndex.tryGetSemanticVersion
            Assert.True(actual.IsSome)
            Assert.Equivalent(SemVer.SemVers.mandatory, actual.Value)
            Assert.Equal(SemVer.SemVers.mandatory, actual.Value)

        [<Fact>]
        let ``getSemanticVersion from valid package with mandatory fields``() =
            let actual = ValidationPackageIndex.Python.CommentFrontmatter.validMandatoryFrontmatter |> ValidationPackageIndex.getSemanticVersion
            Assert.Equivalent(SemVer.SemVers.mandatory, actual)
            Assert.Equal(SemVer.SemVers.mandatory, actual)

        [<Fact>]
        let ``tryGetSemanticVersionString from valid package with mandatory fields``() =
            let actual = ValidationPackageIndex.Python.CommentFrontmatter.validMandatoryFrontmatter |> ValidationPackageIndex.tryGetSemanticVersionString
            Assert.True(actual.IsSome)
            Assert.Equal(SemVer.Strings.mandatory, actual.Value)

        [<Fact>]
        let ``getSemanticVersionString from valid package with mandatory fields``() =
            let actual = ValidationPackageIndex.Python.CommentFrontmatter.validMandatoryFrontmatter |> ValidationPackageIndex.getSemanticVersionString
            Assert.Equal(SemVer.Strings.mandatory, actual)

        [<Fact>]
        let ``tryGetSemanticVersion from valid package with all fields``() =
            let actual = ValidationPackageIndex.Python.CommentFrontmatter.validFullFrontmatter |> ValidationPackageIndex.tryGetSemanticVersion
            Assert.True(actual.IsSome)
            Assert.Equivalent(SemVer.SemVers.fixtureFile, actual.Value)
            Assert.Equal(SemVer.SemVers.fixtureFile, actual.Value)

        [<Fact>]
        let ``getSemanticVersion from valid package with all fields``() =
            let actual = ValidationPackageIndex.Python.CommentFrontmatter.validFullFrontmatter |> ValidationPackageIndex.getSemanticVersion
            Assert.Equivalent(SemVer.SemVers.fixtureFile, actual)
            Assert.Equal(SemVer.SemVers.fixtureFile, actual)

        [<Fact>]
        let ``tryGetSemanticVersionString from valid package with all fields``() =
            let actual = ValidationPackageIndex.Python.CommentFrontmatter.validFullFrontmatter |> ValidationPackageIndex.tryGetSemanticVersionString
            Assert.True(actual.IsSome)
            Assert.Equal(SemVer.Strings.fixtureFile, actual.Value)

        [<Fact>]
        let ``getSemanticVersionString from valid package with all fields``() =
            let actual = ValidationPackageIndex.Python.CommentFrontmatter.validFullFrontmatter |> ValidationPackageIndex.getSemanticVersionString
            Assert.Equal(SemVer.Strings.fixtureFile, actual)

        [<Fact>]
        let ``identityEquals returns true for identical versions``() =
            Assert.True(
                ValidationPackageIndex.identityEquals
                    ValidationPackageIndex.Python.CommentFrontmatter.validMandatoryFrontmatter
                    ValidationPackageIndex.Python.CommentFrontmatter.validMandatoryFrontmatter
            )

        [<Fact>]
        let ``identityEquals returns false for non-identical versions``() =
            Assert.False(
                ValidationPackageIndex.identityEquals
                    ValidationPackageIndex.Python.CommentFrontmatter.validMandatoryFrontmatter
                    ValidationPackageIndex.Python.CommentFrontmatter.validFullFrontmatter
            )

        [<Fact>]
        let ``identityEquals returns false for same version with suffixes``() =
            let a = ValidationPackageIndex.create(
                repoPath = "",
                fileName = "",
                lastUpdated = testDate,
                contentHash = "",
                metadata = ValidationPackageMetadata.create(
                    name = "",
                    majorVersion = 1,
                    minorVersion = 0,
                    patchVersion = 0,
                    summary = "",
                    description = "",
                    programmingLanguage = "Python",
                    PreReleaseVersionSuffix = "some",
                    BuildMetadataVersionSuffix = "suffix"
                )
            )
            let b = ValidationPackageIndex.create(
                repoPath = "",
                fileName = "",
                lastUpdated = testDate,
                contentHash = "",
                metadata = ValidationPackageMetadata.create(
                    name = "",
                    majorVersion = 1,
                    minorVersion = 0,
                    patchVersion = 0,
                    summary = "",
                    description = "",
                    programmingLanguage = "Python"
                )
            )
            Assert.False(ValidationPackageIndex.identityEquals a b)

        [<Fact>]
        let ``identityEquals returns true for identical version with suffixes``() =
            let a = ValidationPackageIndex.create(
                repoPath = "",
                fileName = "",
                lastUpdated = testDate,
                contentHash = "",
                metadata = ValidationPackageMetadata.create(
                    name = "",
                    majorVersion = 1,
                    minorVersion = 0,
                    patchVersion = 0,
                    summary = "",
                    description = "",
                    programmingLanguage = "Python",
                    PreReleaseVersionSuffix = "some",
                    BuildMetadataVersionSuffix = "suffix"
                )
            )
            let b = ValidationPackageIndex.create(
                repoPath = "",
                fileName = "",
                lastUpdated = testDate,
                contentHash = "",
                metadata = ValidationPackageMetadata.create(
                    name = "",
                    majorVersion = 1,
                    minorVersion = 0,
                    patchVersion = 0,
                    summary = "",
                    description = "",
                    programmingLanguage = "Python",
                    PreReleaseVersionSuffix = "some",
                    BuildMetadataVersionSuffix = "suffix"
                )
            )
            Assert.True(ValidationPackageIndex.identityEquals a b)


module CommentFrontmatterIO =

    module FSharp =

        open System.IO

        [<Fact>]
        let ``valid indexed package is extracted from valid mandatory field test file`` () =

            let actual = 
                ValidationPackageIndex.create(
                    repoPath = "fixtures/Frontmatter/Comment/valid@1.0.0.fsx",
                    lastUpdated = testDate
                )
            Assert.Equivalent(ValidationPackageIndex.FSharp.CommentFrontmatter.validMandatoryFrontmatter, actual)


        [<Fact>]
        let ``valid indexed package is extracted from all fields test file`` () =

            let actual = 
                ValidationPackageIndex.create(
                    repoPath = "fixtures/Frontmatter/Comment/valid@2.0.0.fsx",
                    lastUpdated = testDate
                )
            Assert.Equivalent(ValidationPackageIndex.FSharp.CommentFrontmatter.validFullFrontmatter, actual)

        [<Fact>]
        let ``invalid indexed package is extracted from testfile with missing fields`` () =

            let actual = 
                ValidationPackageIndex.create(
                    repoPath = "fixtures/Frontmatter/Comment/invalid@0.0.fsx",
                    lastUpdated = testDate
                )
            Assert.Equivalent(ValidationPackageIndex.FSharp.CommentFrontmatter.invalidMissingMandatoryFrontmatter, actual)

        [<Fact>]
        let ``CRLF: correct content hash (with line endings replaced) is extracted from valid mandatory field test file`` () =
            let tmp_path = Path.GetTempFileName().Replace(".tmp", ".fsx")
            File.WriteAllText(
                tmp_path,
                Frontmatter.FSharp.Comment.validMandatoryFrontmatter.ReplaceLineEndings("\r\n")
            )
            let actual = 
                ValidationPackageIndex.create(
                    repoPath = tmp_path,
                    lastUpdated = testDate
                )
            let expected = {
                ValidationPackageIndex.FSharp.CommentFrontmatter.validMandatoryFrontmatter with 
                    RepoPath = tmp_path
                    FileName = Path.GetFileName(tmp_path)
            }
            Assert.Equivalent(expected, actual)


        [<Fact>]
        let ``CRLF: correct content hash (with line endings replaced) is extracted from all fields test file`` () =
            let tmp_path = Path.GetTempFileName().Replace(".tmp", ".fsx")
            File.WriteAllText(
                tmp_path,
                Frontmatter.FSharp.Comment.validFullFrontmatter.ReplaceLineEndings("\r\n")
            )
            let actual = 
                ValidationPackageIndex.create(
                    repoPath = tmp_path,
                    lastUpdated = testDate
                )
            let expected = {
                ValidationPackageIndex.FSharp.CommentFrontmatter.validFullFrontmatter with 
                    RepoPath = tmp_path
                    FileName = Path.GetFileName(tmp_path)
            }
            Assert.Equivalent(expected, actual)

        [<Fact>]
        let ``CRLF: correct content hash (with line endings replaced) is extracted from testfile with missing fields`` () =
            let tmp_path = Path.GetTempFileName().Replace(".tmp", ".fsx")
            File.WriteAllText(
                tmp_path,
                Frontmatter.FSharp.Comment.invalidMissingMandatoryFrontmatter.ReplaceLineEndings("\r\n")
            )
            let actual = 
                ValidationPackageIndex.create(
                    repoPath = tmp_path,
                    lastUpdated = testDate
                )
            let expected = {
                ValidationPackageIndex.FSharp.CommentFrontmatter.invalidMissingMandatoryFrontmatter with 
                    RepoPath = tmp_path
                    FileName = Path.GetFileName(tmp_path)
            }
            Assert.Equivalent(expected, actual)


        module BindingFrontmatterIO =

            open System.IO

            [<Fact>]
            let ``valid indexed package is extracted from valid mandatory field test file`` () =

                let actual = 
                    ValidationPackageIndex.create(
                        repoPath = "fixtures/Frontmatter/Binding/valid@1.0.0.fsx",
                        lastUpdated = testDate
                    )
                Assert.Equivalent(ValidationPackageIndex.FSharp.BindingFrontmatter.validMandatoryFrontmatter, actual)


            [<Fact>]
            let ``valid indexed package is extracted from all fields test file`` () =

                let actual = 
                    ValidationPackageIndex.create(
                        repoPath = "fixtures/Frontmatter/Binding/valid@2.0.0.fsx",
                        lastUpdated = testDate
                    )
                Assert.Equivalent(ValidationPackageIndex.FSharp.BindingFrontmatter.validFullFrontmatter, actual)

            [<Fact>]
            let ``invalid indexed package is extracted from testfile with missing fields`` () =

                let actual = 
                    ValidationPackageIndex.create(
                        repoPath = "fixtures/Frontmatter/Binding/invalid@0.0.fsx",
                        lastUpdated = testDate
                    )
                Assert.Equivalent(ValidationPackageIndex.FSharp.BindingFrontmatter.invalidMissingMandatoryFrontmatter, actual)

            [<Fact>]
            let ``CRLF: correct content hash (with line endings replaced) is extracted from valid mandatory field test file`` () =
                let tmp_path = Path.GetTempFileName().Replace(".tmp", ".fsx")
                File.WriteAllText(
                    tmp_path,
                    Frontmatter.FSharp.Binding.validMandatoryFrontmatter.ReplaceLineEndings("\r\n")
                )
                let actual = 
                    ValidationPackageIndex.create(
                        repoPath = tmp_path,
                        lastUpdated = testDate
                    )
                let expected = {
                    ValidationPackageIndex.FSharp.BindingFrontmatter.validMandatoryFrontmatter with 
                        RepoPath = tmp_path
                        FileName = Path.GetFileName(tmp_path)
                }
                Assert.Equivalent(expected, actual)


            [<Fact>]
            let ``CRLF: correct content hash (with line endings replaced) is extracted from all fields test file`` () =
                let tmp_path = Path.GetTempFileName().Replace(".tmp", ".fsx")
                File.WriteAllText(
                    tmp_path,
                    Frontmatter.FSharp.Binding.validFullFrontmatter.ReplaceLineEndings("\r\n")
                )
                let actual = 
                    ValidationPackageIndex.create(
                        repoPath = tmp_path,
                        lastUpdated = testDate
                    )
                let expected = {
                    ValidationPackageIndex.FSharp.BindingFrontmatter.validFullFrontmatter with 
                        RepoPath = tmp_path
                        FileName = Path.GetFileName(tmp_path)
                }
                Assert.Equivalent(expected, actual)

            [<Fact>]
            let ``CRLF: correct content hash (with line endings replaced) is extracted from testfile with missing fields`` () =
                let tmp_path = Path.GetTempFileName().Replace(".tmp", ".fsx")
                File.WriteAllText(
                    tmp_path,
                    Frontmatter.FSharp.Binding.invalidMissingMandatoryFrontmatter.ReplaceLineEndings("\r\n")
                )
                let actual = 
                    ValidationPackageIndex.create(
                        repoPath = tmp_path,
                        lastUpdated = testDate
                    )
                let expected = {
                    ValidationPackageIndex.FSharp.BindingFrontmatter.invalidMissingMandatoryFrontmatter with 
                        RepoPath = tmp_path
                        FileName = Path.GetFileName(tmp_path)
                }
                Assert.Equivalent(expected, actual)

    module Python =

        open System.IO

        [<Fact>]
        let ``valid indexed package is extracted from valid mandatory field test file`` () =

            let actual = 
                ValidationPackageIndex.create(
                    repoPath = "fixtures/Frontmatter/Comment/valid@1.0.0.py",
                    lastUpdated = testDate
                )
            Assert.Equivalent(ValidationPackageIndex.Python.CommentFrontmatter.validMandatoryFrontmatter, actual)


        [<Fact>]
        let ``valid indexed package is extracted from all fields test file`` () =

            let actual = 
                ValidationPackageIndex.create(
                    repoPath = "fixtures/Frontmatter/Comment/valid@2.0.0.py",
                    lastUpdated = testDate
                )
            Assert.Equivalent(ValidationPackageIndex.Python.CommentFrontmatter.validFullFrontmatter, actual)

        [<Fact>]
        let ``invalid indexed package is extracted from testfile with missing fields`` () =

            let actual = 
                ValidationPackageIndex.create(
                    repoPath = "fixtures/Frontmatter/Comment/invalid@0.0.py",
                    lastUpdated = testDate
                )
            Assert.Equivalent(ValidationPackageIndex.Python.CommentFrontmatter.invalidMissingMandatoryFrontmatter, actual)

        [<Fact>]
        let ``CRLF: correct content hash (with line endings replaced) is extracted from valid mandatory field test file`` () =
            let tmp_path = Path.GetTempFileName().Replace(".tmp", ".py")
            File.WriteAllText(
                tmp_path,
                Frontmatter.Python.Comment.validMandatoryFrontmatter.ReplaceLineEndings("\r\n")
            )
            let actual = 
                ValidationPackageIndex.create(
                    repoPath = tmp_path,
                    lastUpdated = testDate
                )
            let expected = {
                ValidationPackageIndex.Python.CommentFrontmatter.validMandatoryFrontmatter with 
                    RepoPath = tmp_path
                    FileName = Path.GetFileName(tmp_path)
            }
            Assert.Equivalent(expected, actual)


        [<Fact>]
        let ``CRLF: correct content hash (with line endings replaced) is extracted from all fields test file`` () =
            let tmp_path = Path.GetTempFileName().Replace(".tmp", ".py")
            File.WriteAllText(
                tmp_path,
                Frontmatter.Python.Comment.validFullFrontmatter.ReplaceLineEndings("\r\n")
            )
            let actual = 
                ValidationPackageIndex.create(
                    repoPath = tmp_path,
                    lastUpdated = testDate
                )
            let expected = {
                ValidationPackageIndex.Python.CommentFrontmatter.validFullFrontmatter with 
                    RepoPath = tmp_path
                    FileName = Path.GetFileName(tmp_path)
            }
            Assert.Equivalent(expected, actual)

        [<Fact>]
        let ``CRLF: correct content hash (with line endings replaced) is extracted from testfile with missing fields`` () =
            let tmp_path = Path.GetTempFileName().Replace(".tmp", ".py")
            File.WriteAllText(
                tmp_path,
                Frontmatter.Python.Comment.invalidMissingMandatoryFrontmatter.ReplaceLineEndings("\r\n")
            )
            let actual = 
                ValidationPackageIndex.create(
                    repoPath = tmp_path,
                    lastUpdated = testDate
                )
            let expected = {
                ValidationPackageIndex.Python.CommentFrontmatter.invalidMissingMandatoryFrontmatter with 
                    RepoPath = tmp_path
                    FileName = Path.GetFileName(tmp_path)
            }
            Assert.Equivalent(expected, actual)


        module BindingFrontmatterIO =

            open System.IO

            [<Fact>]
            let ``valid indexed package is extracted from valid mandatory field test file`` () =

                let actual = 
                    ValidationPackageIndex.create(
                        repoPath = "fixtures/Frontmatter/Binding/valid@1.0.0.py",
                        lastUpdated = testDate
                    )
                Assert.Equivalent(ValidationPackageIndex.Python.BindingFrontmatter.validMandatoryFrontmatter, actual)


            [<Fact>]
            let ``valid indexed package is extracted from all fields test file`` () =

                let actual = 
                    ValidationPackageIndex.create(
                        repoPath = "fixtures/Frontmatter/Binding/valid@2.0.0.py",
                        lastUpdated = testDate
                    )
                Assert.Equivalent(ValidationPackageIndex.Python.BindingFrontmatter.validFullFrontmatter, actual)

            [<Fact>]
            let ``invalid indexed package is extracted from testfile with missing fields`` () =

                let actual = 
                    ValidationPackageIndex.create(
                        repoPath = "fixtures/Frontmatter/Binding/invalid@0.0.py",
                        lastUpdated = testDate
                    )
                Assert.Equivalent(ValidationPackageIndex.Python.BindingFrontmatter.invalidMissingMandatoryFrontmatter, actual)

            [<Fact>]
            let ``CRLF: correct content hash (with line endings replaced) is extracted from valid mandatory field test file`` () =
                let tmp_path = Path.GetTempFileName().Replace(".tmp", ".py")
                File.WriteAllText(
                    tmp_path,
                    Frontmatter.Python.Binding.validMandatoryFrontmatter.ReplaceLineEndings("\r\n")
                )
                let actual = 
                    ValidationPackageIndex.create(
                        repoPath = tmp_path,
                        lastUpdated = testDate
                    )
                let expected = {
                    ValidationPackageIndex.Python.BindingFrontmatter.validMandatoryFrontmatter with 
                        RepoPath = tmp_path
                        FileName = Path.GetFileName(tmp_path)
                }
                Assert.Equivalent(expected, actual)


            [<Fact>]
            let ``CRLF: correct content hash (with line endings replaced) is extracted from all fields test file`` () =
                let tmp_path = Path.GetTempFileName().Replace(".tmp", ".py")
                File.WriteAllText(
                    tmp_path,
                    Frontmatter.Python.Binding.validFullFrontmatter.ReplaceLineEndings("\r\n")
                )
                let actual = 
                    ValidationPackageIndex.create(
                        repoPath = tmp_path,
                        lastUpdated = testDate
                    )
                let expected = {
                    ValidationPackageIndex.Python.BindingFrontmatter.validFullFrontmatter with 
                        RepoPath = tmp_path
                        FileName = Path.GetFileName(tmp_path)
                }
                Assert.Equivalent(expected, actual)

            [<Fact>]
            let ``CRLF: correct content hash (with line endings replaced) is extracted from testfile with missing fields`` () =
                let tmp_path = Path.GetTempFileName().Replace(".tmp", ".py")
                File.WriteAllText(
                    tmp_path,
                    Frontmatter.Python.Binding.invalidMissingMandatoryFrontmatter.ReplaceLineEndings("\r\n")
                )
                let actual = 
                    ValidationPackageIndex.create(
                        repoPath = tmp_path,
                        lastUpdated = testDate
                    )
                let expected = {
                    ValidationPackageIndex.Python.BindingFrontmatter.invalidMissingMandatoryFrontmatter with 
                        RepoPath = tmp_path
                        FileName = Path.GetFileName(tmp_path)
                }
                Assert.Equivalent(expected, actual)