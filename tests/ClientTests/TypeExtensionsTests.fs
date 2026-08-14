namespace TypeExtensionsTests

open System
open Xunit
open AVPRClient.Interop
open Newtonsoft.Json.Linq

module ValidationPackage =

    [<Fact>]
    let ``client identities include full semantic-version suffixes`` () =
        Assert.True(
            ReferenceObjects.ValidationPackage.allFields.IdentityEquals(
                ReferenceObjects.ValidationPackage.allFields
            )
        )

        Assert.False(
            ReferenceObjects.ValidationPackage.allFields.IdentityEquals(
                ReferenceObjects.ValidationPackage.differentVersion
            )
        )

    [<Fact>]
    let ``client and model identities compare in both directions`` () =
        Assert.True(
            ReferenceObjects.ValidationPackage.allFields.IdentityEquals(
                ReferenceObjects.Metadata.allFields
            )
        )

        Assert.True(
            ReferenceObjects.Metadata.allFields.IdentityEquals(
                ReferenceObjects.ValidationPackage.allFields
            )
        )

    [<Fact>]
    let ``client package maps all portable metadata fields`` () =
        let actual = ReferenceObjects.ValidationPackage.allFields.ToModel()
        Assert.Equivalent(ReferenceObjects.Metadata.allFields, actual)
        Assert.False(actual.Publish)

    [<Fact>]
    let ``portable metadata maps to a complete client package`` () =
        let actual =
            ReferenceObjects.Metadata.allFields.ToClient(
                ReferenceObjects.packageContent,
                ReferenceObjects.releaseDate
            )

        Assert.Equivalent(ReferenceObjects.ValidationPackage.allFields, actual)

    [<Fact>]
    let ``null generated values use portable model defaults`` () =
        let package =
            AVPRClient.ValidationPackage(
                Name = null,
                Summary = null,
                Description = null,
                PreReleaseVersionSuffix = null,
                BuildMetadataVersionSuffix = null,
                ProgrammingLanguage = null,
                Authors = null,
                Tags = null,
                ReleaseNotes = null,
                CQCHookEndpoint = null,
                Inputs = null
            )

        let actual = package.ToModel()
        Assert.Equal("", actual.Name)
        Assert.Equal("", actual.Summary)
        Assert.Equal("", actual.Description)
        Assert.Equal("", actual.PreReleaseVersionSuffix)
        Assert.Equal("", actual.BuildMetadataVersionSuffix)
        Assert.Equal("", actual.ProgrammingLanguage)
        Assert.Empty(actual.Authors)
        Assert.Empty(actual.Tags)
        Assert.Empty(actual.Inputs)
        Assert.Equal("", actual.ReleaseNotes)
        Assert.Equal("", actual.CQCHookEndpoint)

module NestedCollections =

    [<Fact>]
    let ``authors map in both directions`` () =
        let model = ReferenceObjects.Author.allFieldsClient.ToModel()
        Assert.Equivalent(ReferenceObjects.Author.allFieldsModel, model)
        Assert.Equivalent(ReferenceObjects.Author.allFieldsClient, model.ToClient())

    [<Fact>]
    let ``ontology annotations map in both directions`` () =
        let model = ReferenceObjects.OntologyAnnotation.allFieldsClient.ToModel()
        Assert.Equivalent(ReferenceObjects.OntologyAnnotation.allFieldsModel, model)
        Assert.Equivalent(ReferenceObjects.OntologyAnnotation.allFieldsClient, model.ToClient())

    [<Fact>]
    let ``null and empty collections map to empty collections`` () =
        let nullAuthors: System.Collections.Generic.ICollection<AVPRClient.Author> = null
        let emptyTags = ResizeArray<AVPRClient.OntologyAnnotation>()
        let nullInputs: System.Collections.Generic.ICollection<AVPRClient.CommandInputParameter> = null

        Assert.Empty(nullAuthors.ToModel())
        Assert.Empty(emptyTags.ToModel())
        Assert.Empty(nullInputs.ToModel())

module CommandInput =

    [<Fact>]
    let ``parameters map in both directions`` () =
        let model = ReferenceObjects.CommandInput.allFieldsClient.ToModel()
        Assert.Equivalent(ReferenceObjects.CommandInput.allFieldsModel, model)
        Assert.Equivalent(ReferenceObjects.CommandInput.allFieldsClient, model.ToClient())

    [<Fact>]
    let ``all supported scalar types map in both directions`` () =
        let cases = [|
            AVPRClient.CommandInputType.Boolean, ValidationPackage.Model.CwlPrimitive.Boolean, false
            AVPRClient.CommandInputType.Boolean_, ValidationPackage.Model.CwlPrimitive.Boolean, true
            AVPRClient.CommandInputType.Int, ValidationPackage.Model.CwlPrimitive.Int, false
            AVPRClient.CommandInputType.Int_, ValidationPackage.Model.CwlPrimitive.Int, true
            AVPRClient.CommandInputType.Long, ValidationPackage.Model.CwlPrimitive.Long, false
            AVPRClient.CommandInputType.Long_, ValidationPackage.Model.CwlPrimitive.Long, true
            AVPRClient.CommandInputType.Float, ValidationPackage.Model.CwlPrimitive.Float, false
            AVPRClient.CommandInputType.Float_, ValidationPackage.Model.CwlPrimitive.Float, true
            AVPRClient.CommandInputType.Double, ValidationPackage.Model.CwlPrimitive.Double, false
            AVPRClient.CommandInputType.Double_, ValidationPackage.Model.CwlPrimitive.Double, true
            AVPRClient.CommandInputType.String, ValidationPackage.Model.CwlPrimitive.String, false
            AVPRClient.CommandInputType.String_, ValidationPackage.Model.CwlPrimitive.String, true
        |]

        for clientType, primitiveType, isNullable in cases do
            let modelType = clientType.ToModel()
            Assert.Equal(primitiveType, modelType.PrimitiveType)
            Assert.Equal(isNullable, modelType.IsNullable)
            Assert.Equal(clientType, modelType.ToClient())

    [<Fact>]
    let ``unsupported generated scalar type is rejected`` () =
        let invalidType = enum<AVPRClient.CommandInputType> 999
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> invalidType.ToModel() |> ignore)
        |> ignore

    [<Fact>]
    let ``null bindings are rejected by centralized declaration validation`` () =
        let input =
            AVPRClient.CommandInputParameter(
                Id = "value",
                Type = AVPRClient.CommandInputType.String,
                InputBinding = null
            )

        Assert.Throws<ArgumentException>(fun () -> input.ToModel() |> ignore)
        |> ignore

    [<Fact>]
    let ``generated parameter JSON keeps the public CWL wire shape`` () =
        let input =
            AVPRClient.CommandInputParameter(
                Id = "verbose",
                Type = AVPRClient.CommandInputType.Boolean_,
                Label = "Verbose",
                Doc = "Enable verbose logging",
                InputBinding = AVPRClient.CommandInputBinding(Prefix = "--verbose")
            )

        let json = JObject.FromObject(input)
        Assert.Equal("verbose", json.["id"].Value<string>())
        Assert.Equal("boolean?", json.["type"].Value<string>())
        Assert.Equal("Verbose", json.["label"].Value<string>())
        Assert.Equal("Enable verbose logging", json.["doc"].Value<string>())
        Assert.NotNull(json.["inputBinding"])
        Assert.Null(json.["Id"])
        Assert.Null(json.["primitiveType"])
