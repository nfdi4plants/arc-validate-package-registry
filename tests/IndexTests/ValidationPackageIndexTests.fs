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

    module CommentFrontmatter =

        open System.IO

        [<Fact>]
        let ``valid indexed package is extracted from valid mandatory field test file`` () =

            let actual = 
                ValidationPackageIndex.create(
                    repoPath = "fixtures/Frontmatter/Comment/valid@1.0.0.fsx",
                    lastUpdated = testDate
                )
            Assert.Equivalent(ValidationPackageIndex.CommentFrontmatter.validMandatoryFrontmatter, actual)


        [<Fact>]
        let ``valid indexed package is extracted from all fields test file`` () =

            let actual = 
                ValidationPackageIndex.create(
                    repoPath = "fixtures/Frontmatter/Comment/valid@2.0.0.fsx",
                    lastUpdated = testDate
                )
            Assert.Equivalent(ValidationPackageIndex.CommentFrontmatter.validFullFrontmatter, actual)

        [<Fact>]
        let ``invalid indexed package is extracted from testfile with missing fields`` () =

            let actual = 
                ValidationPackageIndex.create(
                    repoPath = "fixtures/Frontmatter/Comment/invalid@0.0.fsx",
                    lastUpdated = testDate
                )
            Assert.Equivalent(ValidationPackageIndex.CommentFrontmatter.invalidMissingMandatoryFrontmatter, actual)

        [<Fact>]
        let ``CRLF: correct content hash (with line endings replaced) is extracted from valid mandatory field test file`` () =
            let tmp_path = Path.GetTempFileName()
            File.WriteAllText(
                tmp_path,
                Frontmatter.Comment.validMandatoryFrontmatter.ReplaceLineEndings("\r\n")
            )
            let actual = 
                ValidationPackageIndex.create(
                    repoPath = tmp_path,
                    lastUpdated = testDate
                )
            let expected = {
                ValidationPackageIndex.CommentFrontmatter.validMandatoryFrontmatter with 
                    RepoPath = tmp_path
                    FileName = Path.GetFileName(tmp_path)
            }
            Assert.Equivalent(expected, actual)


        [<Fact>]
        let ``CRLF: correct content hash (with line endings replaced) is extracted from all fields test file`` () =
            let tmp_path = Path.GetTempFileName()
            File.WriteAllText(
                tmp_path,
                Frontmatter.Comment.validFullFrontmatter.ReplaceLineEndings("\r\n")
            )
            let actual = 
                ValidationPackageIndex.create(
                    repoPath = tmp_path,
                    lastUpdated = testDate
                )
            let expected = {
                ValidationPackageIndex.CommentFrontmatter.validFullFrontmatter with 
                    RepoPath = tmp_path
                    FileName = Path.GetFileName(tmp_path)
            }
            Assert.Equivalent(expected, actual)

        [<Fact>]
        let ``CRLF: correct content hash (with line endings replaced) is extracted from testfile with missing fields`` () =
            let tmp_path = Path.GetTempFileName()
            File.WriteAllText(
                tmp_path,
                Frontmatter.Comment.invalidMissingMandatoryFrontmatter.ReplaceLineEndings("\r\n")
            )
            let actual = 
                ValidationPackageIndex.create(
                    repoPath = tmp_path,
                    lastUpdated = testDate
                )
            let expected = {
                ValidationPackageIndex.CommentFrontmatter.invalidMissingMandatoryFrontmatter with 
                    RepoPath = tmp_path
                    FileName = Path.GetFileName(tmp_path)
            }
            Assert.Equivalent(expected, actual)