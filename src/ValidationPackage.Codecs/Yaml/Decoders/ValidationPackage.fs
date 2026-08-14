namespace ValidationPackage.Codecs.Yaml.Decoders

open YAMLicious
open ValidationPackage.Model
open ValidationPackage.Codecs
open ValidationPackage.Codecs.Yaml

[<RequireQualifiedAccess>]
module internal ValidationPackage =

    let private modelDecoder =
        Decode.object (fun get ->
            let stringField name =
                get.Optional.Field name Decode.string
                |> Option.defaultValue ""

            let intField name =
                get.Optional.Field name Decode.int
                |> Option.defaultValue -1

            let metadata =
                ValidationPackageMetadata.create(
                    stringField "Name",
                    stringField "Summary",
                    stringField "Description",
                    intField "MajorVersion",
                    intField "MinorVersion",
                    intField "PatchVersion",
                    stringField "ProgrammingLanguage"
                )

            metadata.PreReleaseVersionSuffix <- stringField "PreReleaseVersionSuffix"
            metadata.BuildMetadataVersionSuffix <- stringField "BuildMetadataVersionSuffix"

            metadata.Publish <-
                get.Optional.Field "Publish" Decode.bool
                |> Option.defaultValue false

            metadata.Authors <-
                get.Optional.Field "Authors" (Decode.array Author.decoder)
                |> Option.defaultValue Array.empty

            metadata.Tags <-
                get.Optional.Field "Tags" (Decode.array OntologyAnnotation.decoder)
                |> Option.defaultValue Array.empty

            metadata.ReleaseNotes <- stringField "ReleaseNotes"
            metadata.CQCHookEndpoint <- stringField "CQCHookEndpoint"

            metadata.Inputs <-
                get.Optional.Field "Inputs" Cwl.commandInputParameters
                |> Option.defaultValue Array.empty

            metadata
        )

    let private validateNestedObjects rootEntries =
        Strict.tryField "Authors" rootEntries
        |> Option.iter (fun element ->
            element
            |> Strict.sequence "$.Authors"
            |> Array.iteri (fun index author ->
                let path = $"$.Authors[{index}]"
                let entries = Strict.mapping path author

                Strict.validateAllowedFields
                    path
                    [| "FullName"; "Email"; "Affiliation"; "AffiliationLink" |]
                    entries

                Strict.requiredField path "FullName" entries |> ignore
            )
        )

        Strict.tryField "Tags" rootEntries
        |> Option.iter (fun element ->
            element
            |> Strict.sequence "$.Tags"
            |> Array.iteri (fun index tag ->
                let path = $"$.Tags[{index}]"
                let entries = Strict.mapping path tag

                Strict.validateAllowedFields
                    path
                    [| "Name"; "TermSourceREF"; "TermAccessionNumber" |]
                    entries

                Strict.requiredField path "Name" entries |> ignore
            )
        )

    let private strictV1Decoder element =
        Strict.validateSyntax "$" element
        let entries = Strict.mapping "$" element

        Strict.validateAllowedFields
            "$"
            [|
                "$schema"
                "Name"
                "Summary"
                "Description"
                "MajorVersion"
                "MinorVersion"
                "PatchVersion"
                "PreReleaseVersionSuffix"
                "BuildMetadataVersionSuffix"
                "ProgrammingLanguage"
                "Publish"
                "Authors"
                "Tags"
                "ReleaseNotes"
                "CQCHookEndpoint"
                "Inputs"
            |]
            entries

        [| "Name"; "Summary"; "Description"; "MajorVersion"; "MinorVersion"; "PatchVersion" |]
        |> Array.iter (fun name -> Strict.requiredField "$" name entries |> ignore)

        validateNestedObjects entries

        let metadata = modelDecoder element
        CommandInputParameter.validate metadata.Inputs |> ignore
        metadata

    let legacyDecoder element =
        let entries = Strict.mapping "$" element

        if Strict.tryField "$schema" entries |> Option.isSome then
            invalidArg "yaml" "legacy validation-package frontmatter must not contain $schema"

        if Strict.tryField "Inputs" entries |> Option.isSome then
            invalidArg
                "yaml"
                "schema-less legacy validation-package frontmatter cannot declare Inputs"

        modelDecoder element

    let currentDecoder element =
        let entries = Strict.mapping "$" element

        match Strict.schemaValue entries with
        | Some schema when schema = SchemaUris.ValidationPackageFrontmatterV1 ->
            strictV1Decoder element
        | Some schema ->
            invalidArg "yaml" $"UnsupportedSchema: unsupported validation-package frontmatter schema '{schema}'"
        | None ->
            invalidArg "yaml" "canonical validation-package frontmatter requires $schema"

    let decoder element =
        let entries = Strict.mapping "$" element

        match Strict.schemaValue entries with
        | None -> legacyDecoder element
        | Some schema when schema = SchemaUris.ValidationPackageFrontmatterV1 ->
            strictV1Decoder element
        | Some schema ->
            invalidArg "yaml" $"UnsupportedSchema: unsupported validation-package frontmatter schema '{schema}'"
