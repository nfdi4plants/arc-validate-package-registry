module ValidationPackage.Model.Tests.SemanticVersionTests

open Fable.Pyxpecto
open ValidationPackage.Model
open ValidationPackage.Model.Tests.ReferenceObjects

let private expectParsed expected input =
    let actual = SemVer.tryParse input
    Expect.isSome actual $"Expected '{input}' to parse"
    Expect.equal actual.Value expected $"Expected '{input}' to retain every SemVer component"

let tests =
    testList "semantic version" [
        testCase "create preserves mandatory fields" <| fun () ->
            let actual = SemVer.create(1, 0, 0)
            Expect.equal actual SemanticVersions.mandatory "Mandatory SemVer fields should match"

        testCase "create preserves prerelease and build metadata" <| fun () ->
            let actual =
                SemVer.create(
                    1,
                    0,
                    0,
                    PreRelease = "alpha.1",
                    BuildMetadata = "build.1"
                )

            Expect.equal
                actual
                SemanticVersions.prereleaseAndBuildMetadata
                "All SemVer fields should match"

        testCase "parse supports every suffix combination" <| fun () ->
            expectParsed SemanticVersions.mandatory "1.0.0"
            expectParsed SemanticVersions.prerelease "1.0.0-alpha.1"
            expectParsed SemanticVersions.buildMetadata "1.0.0+build.1"

            expectParsed
                SemanticVersions.prereleaseAndBuildMetadata
                "1.0.0-alpha.1+build.1"

        testCase "parse supports major version zero and portable identifiers" <| fun () ->
            Expect.isSome (SemVer.tryParse "0.1.0") "Major zero should be valid"
            Expect.isSome (SemVer.tryParse "1.0.0-0") "A single zero prerelease should be valid"
            Expect.isSome (SemVer.tryParse "1.0.0-01alpha") "Alphanumeric prerelease identifiers may start with zero"
            Expect.isSome (SemVer.tryParse "1.0.0+001") "Build identifiers may contain leading zeros"

        testCase "parse rejects invalid values" <| fun () ->
            let invalidValues =
                [
                    null
                    ""
                    "1"
                    "1.0"
                    "01.0.0"
                    "1.01.0"
                    "1.0.01"
                    "1.0.0-01"
                    "1.0.0-"
                    "1.0.0-alpha..1"
                    "1.0.0+"
                    "1.0.0+build..1"
                    "1.0.0+build+second"
                    "1.0.0 alpha"
                ]

            invalidValues
            |> List.iteri (fun index value ->
                Expect.isNone (SemVer.tryParse value) $"Expected invalid SemVer case {index} to be rejected"
            )

        testCase "format supports every suffix combination" <| fun () ->
            Expect.equal (SemVer.toString SemanticVersions.mandatory) "1.0.0" "Mandatory format"
            Expect.equal (SemVer.toString SemanticVersions.prerelease) "1.0.0-alpha.1" "Prerelease format"
            Expect.equal (SemVer.toString SemanticVersions.buildMetadata) "1.0.0+build.1" "Build format"

            Expect.equal
                (SemVer.toString SemanticVersions.prereleaseAndBuildMetadata)
                "1.0.0-alpha.1+build.1"
                "Combined suffix format"

        testCase "equal values have equal hashes" <| fun () ->
            let first = SemVer.create(1, 2, 3, PreRelease = "rc.1")
            let second = SemVer.create(1, 2, 3, PreRelease = "rc.1")
            let different = SemVer.create(1, 2, 3)

            Expect.equal first second "Equal semantic versions should compare equal"
            Expect.equal (SemVer.getHashCode first) (SemVer.getHashCode second) "Equal semantic versions should hash equally"
            Expect.isFalse (first = different) "Prerelease participates in equality"

        testCase "precedence follows SemVer 2.0 and ignores build metadata" <| fun () ->
            let ordered =
                [|
                    "1.0.0-alpha"
                    "1.0.0-alpha.1"
                    "1.0.0-alpha.beta"
                    "1.0.0-beta"
                    "1.0.0-beta.2"
                    "1.0.0-beta.11"
                    "1.0.0-rc.1"
                    "1.0.0"
                |]
                |> Array.map (SemVer.tryParse >> Option.get)

            ordered
            |> Array.pairwise
            |> Array.iter (fun (lower, higher) ->
                Expect.isTrue
                    (SemVer.comparePrecedence(lower, higher) < 0)
                    $"{SemVer.toString lower} should precede {SemVer.toString higher}"
            )

            let firstBuild = SemVer.tryParse "1.0.0+build.2" |> Option.get
            let secondBuild = SemVer.tryParse "1.0.0+build.10" |> Option.get
            Expect.equal (SemVer.comparePrecedence(firstBuild, secondBuild)) 0 "Build is ignored for precedence"
            Expect.isTrue (SemVer.compareIdentity(firstBuild, secondBuild) > 0) "Canonical text breaks identity ties"

            let hugeNumeric = SemVer.tryParse "1.0.0-999999999999999999999999999999999999" |> Option.get
            let largerHugeNumeric = SemVer.tryParse "1.0.0-1000000000000000000000000000000000000" |> Option.get

            Expect.isTrue
                (SemVer.comparePrecedence(hugeNumeric, largerHugeNumeric) < 0)
                "Numeric prerelease identifiers compare without fixed-width parsing"

        testCase "identity ordering compares names then total semantic identity" <| fun () ->
            let first = ValidationPackageIdentity.create("a", SemVer.tryParse "1.0.0+one" |> Option.get)
            let second = ValidationPackageIdentity.create("a", SemVer.tryParse "1.0.0+two" |> Option.get)
            let otherName = ValidationPackageIdentity.create("b", SemVer.tryParse "0.1.0" |> Option.get)

            Expect.isTrue (ValidationPackageIdentity.compare(first, second) < 0) "Build identity is deterministic"
            Expect.isTrue (ValidationPackageIdentity.compare(second, otherName) < 0) "Name sorts first"

            let supplementary = ValidationPackageIdentity.create("😀", SemVer.create(1, 0, 0))
            let privateUse = ValidationPackageIdentity.create("\uE000", SemVer.create(1, 0, 0))

            Expect.isTrue
                (ValidationPackageIdentity.compare(supplementary, privateUse) < 0)
                "Ordinal comparison follows UTF-16 code units on every target"
    ]
