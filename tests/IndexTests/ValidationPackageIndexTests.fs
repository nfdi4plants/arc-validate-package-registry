namespace ValidationPackageIndexTests

open System
open System.IO
open Xunit
open AVPRIndex
open AVPRIndex.Domain
open AVPRIndex.Frontmatter
open Utils
open ReferenceObjects

module IO =

    open System.IO

    [<Fact>]
    let ``valid indexed package is extracted from valid mandatory field test file`` () =

        let actual = 
            ValidationPackageIndex.create(
                repoPath = "fixtures/valid@1.0.0.fsx",
                lastUpdated = testDate
            )
        Assert.Equivalent(ValidationPackageIndex.validMandatoryFrontmatter, actual)


    [<Fact>]
    let ``valid indexed package is extracted from all fields test file`` () =

        let actual = 
            ValidationPackageIndex.create(
                repoPath = "fixtures/valid@2.0.0.fsx",
                lastUpdated = testDate
            )
        Assert.Equivalent(ValidationPackageIndex.validFullFrontmatter, actual)

    [<Fact>]
    let ``invalid indexed package is extracted from testfile with missing fields`` () =

        let actual = 
            ValidationPackageIndex.create(
                repoPath = "fixtures/invalid@0.0.fsx",
                lastUpdated = testDate
            )
        Assert.Equivalent(ValidationPackageIndex.invalidMissingMandatoryFrontmatter, actual)

    [<Fact>]
    let ``CRLF: correct content hash (with line endings replaced) is extracted from valid mandatory field test file`` () =
        let tmp_path = Path.GetTempFileName()
        File.WriteAllText(
            tmp_path,
            Frontmatter.validMandatoryFrontmatter.ReplaceLineEndings("\r\n")
        )
        let actual = 
            ValidationPackageIndex.create(
                repoPath = tmp_path,
                lastUpdated = testDate
            )
        let expected = {
            ValidationPackageIndex.validMandatoryFrontmatter with 
                RepoPath = tmp_path
                FileName = Path.GetFileName(tmp_path)
        }
        Assert.Equivalent(expected, actual)


    [<Fact>]
    let ``CRLF: correct content hash (with line endings replaced) is extracted from all fields test file`` () =
        let tmp_path = Path.GetTempFileName()
        File.WriteAllText(
            tmp_path,
            Frontmatter.validFullFrontmatter.ReplaceLineEndings("\r\n")
        )
        let actual = 
            ValidationPackageIndex.create(
                repoPath = tmp_path,
                lastUpdated = testDate
            )
        let expected = {
            ValidationPackageIndex.validFullFrontmatter with 
                RepoPath = tmp_path
                FileName = Path.GetFileName(tmp_path)
        }
        Assert.Equivalent(expected, actual)

    [<Fact>]
    let ```CRLF: correct content hash (with line endings replaced) is extracted from testfile with missing fields`` () =
        let tmp_path = Path.GetTempFileName()
        File.WriteAllText(
            tmp_path,
            Frontmatter.invalidMissingMandatoryFrontmatter.ReplaceLineEndings("\r\n")
        )
        let actual = 
            ValidationPackageIndex.create(
                repoPath = tmp_path,
                lastUpdated = testDate
            )
        let expected = {
            ValidationPackageIndex.invalidMissingMandatoryFrontmatter with 
                RepoPath = tmp_path
                FileName = Path.GetFileName(tmp_path)
        }
        Assert.Equivalent(expected, actual)