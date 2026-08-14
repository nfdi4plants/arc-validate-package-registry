namespace ValidationPackage.Model

open Fable.Core

[<AttachMembers>]
type ValidationPackageIdentity(name: string, version: SemVer) =

    member _.Name = name
    member _.Version = version

    override this.GetHashCode() =
        ValidationPackageIdentity.getHashCode(this)

    static member getHashCode(identity: ValidationPackageIdentity) =
        PortableHash.combineValues [
            PortableHash.stringValue identity.Name
            SemVer.getHashCode identity.Version
        ]

    override this.Equals(other) =
        match other with
        | :? ValidationPackageIdentity as identity ->
            (this.Name, this.Version) = (identity.Name, identity.Version)
        | _ -> false

    static member create(name: string, version: SemVer) =
        ValidationPackageIdentity(name, version)

    static member compare(first: ValidationPackageIdentity, second: ValidationPackageIdentity) =
        if isNull (box first) then
            nullArg "first"

        if isNull (box second) then
            nullArg "second"

        let nameComparison = PortableString.compareOrdinal first.Name second.Name

        if nameComparison <> 0 then
            nameComparison
        else
            SemVer.compareIdentity(first.Version, second.Version)
