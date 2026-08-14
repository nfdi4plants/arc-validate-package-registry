namespace ValidationPackage.Codecs.Yaml.Encoders

open ValidationPackage.Model
open ValidationPackage.Codecs
open ValidationPackage.Codecs.Yaml

[<RequireQualifiedAccess>]
module internal ValidationPackage =

    let encode (metadata: ValidationPackageMetadata) =
        Encoding.object [
            "$schema", Encoding.string SchemaUris.ValidationPackageFrontmatterV1
            "Name", Encoding.string metadata.Name
            "Summary", Encoding.string metadata.Summary
            "Description", Encoding.string metadata.Description
            "MajorVersion", Encoding.int metadata.MajorVersion
            "MinorVersion", Encoding.int metadata.MinorVersion
            "PatchVersion", Encoding.int metadata.PatchVersion
            "PreReleaseVersionSuffix", Encoding.string metadata.PreReleaseVersionSuffix
            "BuildMetadataVersionSuffix", Encoding.string metadata.BuildMetadataVersionSuffix
            "ProgrammingLanguage", Encoding.string metadata.ProgrammingLanguage
            "Publish", Encoding.bool metadata.Publish
            "Authors", metadata.Authors |> Array.map Author.encode |> Encoding.array
            "Tags", metadata.Tags |> Array.map OntologyAnnotation.encode |> Encoding.array
            "ReleaseNotes", Encoding.string metadata.ReleaseNotes
            "CQCHookEndpoint", Encoding.string metadata.CQCHookEndpoint
            "Inputs", Cwl.commandInputParameters metadata.Inputs
        ]
