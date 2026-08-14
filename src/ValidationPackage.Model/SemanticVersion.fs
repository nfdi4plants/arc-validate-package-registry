namespace ValidationPackage.Model

open System
open Fable.Core

module internal PortableString =

    let private utf16CodeUnits (value: string) =
        let units = ResizeArray<int>()

        for character in value do
            let codePoint = int character

            if codePoint > 0xFFFF then
                let scalar = codePoint - 0x10000
                units.Add(0xD800 + (scalar / 0x400))
                units.Add(0xDC00 + (scalar % 0x400))
            else
                units.Add codePoint

        units.ToArray()

    let compareOrdinal (first: string) (second: string) =
        let firstUnits = utf16CodeUnits first
        let secondUnits = utf16CodeUnits second
        let mutable index = 0
        let mutable comparison = 0

        while comparison = 0 && index < firstUnits.Length && index < secondUnits.Length do
            comparison <- compare firstUnits[index] secondUnits[index]
            index <- index + 1

        if comparison <> 0 then
            comparison
        else
            compare firstUnits.Length secondUnits.Length

module private SemanticVersionParsing =

    let isAsciiDigit (character: char) =
        character >= '0' && character <= '9'

    let isAsciiLetter (character: char) =
        (character >= 'a' && character <= 'z')
        || (character >= 'A' && character <= 'Z')

    let isIdentifierCharacter (character: char) =
        isAsciiDigit character
        || isAsciiLetter character
        || character = '-'

    let isNonEmptyIdentifier (value: string) =
        value.Length > 0 && value |> Seq.forall isIdentifierCharacter

    let hasValidNumericLeadingZero (value: string) =
        not (
            value.Length > 1
            && value |> Seq.forall isAsciiDigit
            && value[0] = '0'
        )

    let isValidPreReleaseIdentifier (value: string) =
        isNonEmptyIdentifier value && hasValidNumericLeadingZero value

    let isValidBuildIdentifier (value: string) =
        isNonEmptyIdentifier value

    let trySplitOnce (separator: string) (value: string) =
        let firstIndex = value.IndexOf(separator, StringComparison.Ordinal)

        if firstIndex < 0 then
            Some(value, "")
        elif value.IndexOf(separator, firstIndex + separator.Length, StringComparison.Ordinal) >= 0 then
            None
        else
            Some(
                value.Substring(0, firstIndex),
                value.Substring(firstIndex + separator.Length)
            )

    let splitAtFirst (separator: string) (value: string) =
        let index = value.IndexOf(separator, StringComparison.Ordinal)

        if index < 0 then
            value, ""
        else
            value.Substring(0, index), value.Substring(index + separator.Length)

    let tryParseCoreNumber (value: string) =
        if
            value.Length = 0
            || value |> Seq.exists (isAsciiDigit >> not)
            || (value.Length > 1 && value[0] = '0')
        then
            None
        else
            match Int32.TryParse(value) with
            | true, parsed -> Some parsed
            | false, _ -> None

    let hasValidIdentifiers validator (value: string) =
        value.Split('.')
        |> Array.forall validator

    let compareNumericIdentifiers (first: string) (second: string) =
        let lengthComparison = compare first.Length second.Length

        if lengthComparison <> 0 then
            lengthComparison
        else
            PortableString.compareOrdinal first second

    let comparePreReleaseIdentifiers (first: string) (second: string) =
        let firstIsNumeric = first |> Seq.forall isAsciiDigit
        let secondIsNumeric = second |> Seq.forall isAsciiDigit

        match firstIsNumeric, secondIsNumeric with
        | true, true -> compareNumericIdentifiers first second
        | true, false -> -1
        | false, true -> 1
        | false, false -> PortableString.compareOrdinal first second

    let rec comparePreReleaseParts index (first: string array) (second: string array) =
        if index = first.Length && index = second.Length then
            0
        elif index = first.Length then
            -1
        elif index = second.Length then
            1
        else
            let comparison = comparePreReleaseIdentifiers first[index] second[index]

            if comparison <> 0 then
                comparison
            else
                comparePreReleaseParts (index + 1) first second

[<AttachMembers>]
type SemVer() =

    let mutable _major = -1
    let mutable _minor = -1
    let mutable _patch = -1
    let mutable _preRelease = ""
    let mutable _buildMetadata = ""

    member _.Major
        with get () = _major
        and set value = _major <- value

    member _.Minor
        with get () = _minor
        and set value = _minor <- value

    member _.Patch
        with get () = _patch
        and set value = _patch <- value

    member _.PreRelease
        with get () = _preRelease
        and set value = _preRelease <- value

    member _.BuildMetadata
        with get () = _buildMetadata
        and set value = _buildMetadata <- value

    override this.GetHashCode() =
        SemVer.getHashCode(this)

    static member getHashCode(semVer: SemVer) =
        PortableHash.combineValues [
            semVer.Major
            semVer.Minor
            semVer.Patch
            PortableHash.stringValue semVer.PreRelease
            PortableHash.stringValue semVer.BuildMetadata
        ]

    override this.Equals(other) =
        match other with
        | :? SemVer as semVer ->
            (
                this.Major,
                this.Minor,
                this.Patch,
                this.PreRelease,
                this.BuildMetadata
            ) = (
                semVer.Major,
                semVer.Minor,
                semVer.Patch,
                semVer.PreRelease,
                semVer.BuildMetadata
            )
        | _ -> false

    static member create (
        major: int,
        minor: int,
        patch: int,
        ?PreRelease: string,
        ?BuildMetadata: string
    ) =
        let semVer =
            SemVer(
                Major = major,
                Minor = minor,
                Patch = patch
            )

        PreRelease |> Option.iter (fun value -> semVer.PreRelease <- value)
        BuildMetadata |> Option.iter (fun value -> semVer.BuildMetadata <- value)
        semVer

    static member tryParse(version: string) =
        if
            isNull version
            || version.EndsWith("-", StringComparison.Ordinal)
            || version.EndsWith("+", StringComparison.Ordinal)
        then
            None
        else
            match SemanticVersionParsing.trySplitOnce "+" version with
            | None -> None
            | Some(versionWithoutBuild, buildMetadata) ->
                let coreVersion, preRelease =
                    SemanticVersionParsing.splitAtFirst "-" versionWithoutBuild

                let coreParts = coreVersion.Split('.')

                let validPreRelease =
                    preRelease = ""
                    ||
                    SemanticVersionParsing.hasValidIdentifiers
                        SemanticVersionParsing.isValidPreReleaseIdentifier
                        preRelease

                let validBuildMetadata =
                    buildMetadata = ""
                    ||
                    SemanticVersionParsing.hasValidIdentifiers
                        SemanticVersionParsing.isValidBuildIdentifier
                        buildMetadata

                if coreParts.Length <> 3 || not validPreRelease || not validBuildMetadata then
                    None
                else
                    match
                        SemanticVersionParsing.tryParseCoreNumber coreParts[0],
                        SemanticVersionParsing.tryParseCoreNumber coreParts[1],
                        SemanticVersionParsing.tryParseCoreNumber coreParts[2]
                    with
                    | Some major, Some minor, Some patch ->
                        Some(
                            SemVer.create(
                                major,
                                minor,
                                patch,
                                preRelease,
                                buildMetadata
                            )
                        )
                    | _ -> None

    static member toString(semVer: SemVer) =
        match semVer.PreRelease, semVer.BuildMetadata with
        | preRelease, buildMetadata when preRelease <> "" && buildMetadata <> "" ->
            $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}-{preRelease}+{buildMetadata}"
        | preRelease, _ when preRelease <> "" ->
            $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}-{preRelease}"
        | _, buildMetadata when buildMetadata <> "" ->
            $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}+{buildMetadata}"
        | _ ->
            $"{semVer.Major}.{semVer.Minor}.{semVer.Patch}"

    static member comparePrecedence(first: SemVer, second: SemVer) =
        if isNull (box first) then
            nullArg "first"

        if isNull (box second) then
            nullArg "second"

        let coreComparison =
            if first.Major <> second.Major then
                compare first.Major second.Major
            elif first.Minor <> second.Minor then
                compare first.Minor second.Minor
            else
                compare first.Patch second.Patch

        if coreComparison <> 0 then
            coreComparison
        else
            match first.PreRelease, second.PreRelease with
            | "", "" -> 0
            | "", _ -> 1
            | _, "" -> -1
            | firstPreRelease, secondPreRelease ->
                SemanticVersionParsing.comparePreReleaseParts
                    0
                    (firstPreRelease.Split('.'))
                    (secondPreRelease.Split('.'))

    static member compareIdentity(first: SemVer, second: SemVer) =
        let precedenceComparison = SemVer.comparePrecedence(first, second)

        if precedenceComparison <> 0 then
            precedenceComparison
        else
            PortableString.compareOrdinal (SemVer.toString first) (SemVer.toString second)
