namespace TypeExtensionsTests

open System
open System.IO
open System.Text
open Xunit
open AVPRClient

open System.Security.Cryptography
open Newtonsoft.Json.Linq

module ValidationPackage =
    
    [<Fact>]
    let ``IdentityEquals returns true for the same package (no suffixes)`` () =
        Assert.True(
            ReferenceObjects.ValidationPackage.allFields_cqcHookAddition.IdentityEquals(
                ReferenceObjects.ValidationPackage.allFields_cqcHookAddition
            )
        )
        
    [<Fact>]
    let ``IdentityEquals returns true for the same package with suffixes`` () =
        Assert.True(
            ReferenceObjects.ValidationPackage.allFields_semVerAddition.IdentityEquals(
                ReferenceObjects.ValidationPackage.allFields_semVerAddition
            )
        )

    [<Fact>]
    let ``IdentityEquals returns false for the different packages`` () =
        Assert.False(
            ReferenceObjects.ValidationPackage.allFields_cqcHookAddition.IdentityEquals(
                ReferenceObjects.ValidationPackage.allFields_semVerAddition
            )
        )

    [<Fact>]
    let ``IdentityEquals returns true for indexed package with same version (no suffixes)`` () =
        Assert.True(
            ReferenceObjects.ValidationPackage.allFields_cqcHookAddition.IdentityEquals(
                ReferenceObjects.ValidationPackageIndex.allFields_cqcHookAddition
            )
        )
        
    [<Fact>]
    let ``IdentityEquals returns true for indexed package with same version with suffixes`` () =
        Assert.True(
            ReferenceObjects.ValidationPackage.allFields_semVerAddition.IdentityEquals(
                ReferenceObjects.ValidationPackageIndex.allFields_semVerAddition
            )
        )

    [<Fact>]
    let ``IdentityEquals returns false for a different indexed package`` () =
        Assert.False(
            ReferenceObjects.ValidationPackage.allFields_cqcHookAddition.IdentityEquals(
                ReferenceObjects.ValidationPackageIndex.allFields_semVerAddition
            )
        )

    [<Fact>]
    let ``toValidationPackageMetadata maps nested inputs to the index model`` () =
        let actual =
            ReferenceObjects.ValidationPackage.allFields_inputsAddition.toValidationPackageMetadata()

        Assert.Equivalent(
            ReferenceObjects.ValidationPackageMetadata.allFields_inputsAddition.Inputs,
            actual.Inputs
        )

module ValidationPackageIndex =

    let allFields_cqcHookAddition = AVPRIndex.Domain.ValidationPackageIndex.create(
        repoPath = "fixtures/allFields_cqcHookAddition.fsx",
        fileName = "allFields_cqcHookAddition.fsx",
        lastUpdated = ReferenceObjects.date,
        contentHash = ReferenceObjects.Hash.expected_hash_cqcHookAddition,
        metadata = ReferenceObjects.ValidationPackageMetadata.allFields_cqcHookAddition
    )

    [<Fact>]
    let ``CQCHook Addition - toValidationPackage with release date`` () =
        let actual = allFields_cqcHookAddition.toValidationPackage(ReferenceObjects.date)
        Assert.Equivalent(actual, ReferenceObjects.ValidationPackage.allFields_cqcHookAddition)

    [<Fact>]
    let ``CQCHook Addition - toPackageContentHash without direct file hash`` () =
        let actual = allFields_cqcHookAddition.toPackageContentHash()
        Assert.Equivalent(ReferenceObjects.Hash.allFields_cqcHookAddition, actual)

    [<Fact>]
    let ``CQCHook Addition - toPackageContentHash with direct file hash`` () =
        let actual = allFields_cqcHookAddition.toPackageContentHash(HashFileDirectly = true)
        Assert.Equivalent(ReferenceObjects.Hash.allFields_cqcHookAddition, actual)

    let allFields_semVerAddition = AVPRIndex.Domain.ValidationPackageIndex.create(
        repoPath = "fixtures/allFields_semVerAddition.fsx",
        fileName = "allFields_semVerAddition.fsx",
        lastUpdated = ReferenceObjects.date,
        contentHash = ReferenceObjects.Hash.expected_hash_semVerAddition,
        metadata = ReferenceObjects.ValidationPackageMetadata.allFields_semVerAddition
    )

    [<Fact>]
    let ``SemVer Addition - toValidationPackage with release date`` () =
        let actual = allFields_semVerAddition.toValidationPackage(ReferenceObjects.date)
        Assert.Equivalent(actual, ReferenceObjects.ValidationPackage.allFields_semVerAddition)

    [<Fact>]
    let ``SemVer Addition - toPackageContentHash without direct file hash`` () =
        let actual = allFields_semVerAddition.toPackageContentHash()
        Assert.Equivalent(ReferenceObjects.Hash.allFields_semVerAddition, actual)

    [<Fact>]
    let ``SemVer Addition - toPackageContentHash with direct file hash`` () =
        let actual = allFields_semVerAddition.toPackageContentHash(HashFileDirectly = true)
        Assert.Equivalent(ReferenceObjects.Hash.allFields_semVerAddition, actual)


    let allFields_inputsAddition = AVPRIndex.Domain.ValidationPackageIndex.create(
        repoPath = "fixtures/allFields_inputsAddition.fsx",
        fileName = "allFields_inputsAddition.fsx",
        lastUpdated = ReferenceObjects.date,
        contentHash = ReferenceObjects.Hash.expected_hash_inputsAddition,
        metadata = ReferenceObjects.ValidationPackageMetadata.allFields_inputsAddition
    )

    [<Fact>]
    let ``Inputs Addition - toValidationPackage with release date`` () =
        let actual = allFields_inputsAddition.toValidationPackage(ReferenceObjects.date)
        Assert.Equivalent(ReferenceObjects.ValidationPackage.allFields_inputsAddition, actual)

    [<Fact>]
    let ``Inputs Addition - toPackageContentHash without direct file hash`` () =
        let actual = allFields_inputsAddition.toPackageContentHash()
        Assert.Equivalent(ReferenceObjects.Hash.allFields_inputsAddition, actual)

    [<Fact>]
    let ``Inputs Addition - toPackageContentHash with direct file hash`` () =
        let actual = allFields_inputsAddition.toPackageContentHash(HashFileDirectly = true)
        Assert.Equivalent(ReferenceObjects.Hash.allFields_inputsAddition, actual)

    [<Fact>]
    let ``IdentityEquals returns true for package with same version (no suffixes)`` () =
        Assert.True(
            ReferenceObjects.ValidationPackageIndex.allFields_cqcHookAddition.IdentityEquals(
                ReferenceObjects.ValidationPackage.allFields_cqcHookAddition
            )
        )
        
    [<Fact>]
    let ``IdentityEquals returns true for package with same version with suffixes`` () =
        Assert.True(
            ReferenceObjects.ValidationPackageIndex.allFields_semVerAddition.IdentityEquals(
                ReferenceObjects.ValidationPackage.allFields_semVerAddition
            )
        )

    [<Fact>]
    let ``IdentityEquals returns false for a different package`` () =
        Assert.False(
            ReferenceObjects.ValidationPackageIndex.allFields_cqcHookAddition.IdentityEquals(
                ReferenceObjects.ValidationPackage.allFields_semVerAddition
            )
        )
module Author =

    open System.Collections
    open System.Collections.Generic

    [<Fact>]
    let ``AsIndexType with mandatory fields`` () =
        let actual = ReferenceObjects.Author.mandatoryFieldsClient.AsIndexType()
        Assert.Equivalent(actual, ReferenceObjects.Author.mandatoryFieldsClient)

    [<Fact>]
    let ``AsIndexType with all fields`` () =
        let actual = ReferenceObjects.Author.allFieldsClient.AsIndexType()
        Assert.Equivalent(actual, ReferenceObjects.Author.allFieldsClient)

    [<Fact>]
    let ``AsIndexType with mandatory fields on collection`` () =
        let actual = [|ReferenceObjects.Author.mandatoryFieldsClient|].AsIndexType()
        Assert.Equivalent(actual, [|ReferenceObjects.Author.mandatoryFieldsClient|])

    [<Fact>]
    let ``AsIndexType with all fields on collection`` () =
        let actual = [|ReferenceObjects.Author.allFieldsClient|].AsIndexType()
        Assert.Equivalent(actual, [|ReferenceObjects.Author.allFieldsClient|])

module OntologyAnnotation =
    
    [<Fact>]
    let ``AsIndexType with mandatory fields`` () =
        let actual = ReferenceObjects.OntologyAnnotation.mandatoryFieldsClient.AsIndexType()
        Assert.Equivalent(actual, ReferenceObjects.OntologyAnnotation.mandatoryFieldsClient)

    [<Fact>]
    let ``AsIndexType with all fields`` () =
        let actual = ReferenceObjects.OntologyAnnotation.allFieldsClient.AsIndexType()
        Assert.Equivalent(actual, ReferenceObjects.OntologyAnnotation.allFieldsClient)

    [<Fact>]
    let ``AsIndexType with mandatory fields on collection`` () =
        let actual = [|ReferenceObjects.OntologyAnnotation.mandatoryFieldsClient|].AsIndexType()
        Assert.Equivalent(actual, [|ReferenceObjects.OntologyAnnotation.mandatoryFieldsClient|])

    [<Fact>]
    let ``AsIndexType with all fields on collection`` () =
        let actual = [|ReferenceObjects.OntologyAnnotation.allFieldsClient|].AsIndexType()
        Assert.Equivalent(actual, [|ReferenceObjects.OntologyAnnotation.allFieldsClient|])

module CommandInput =

    [<Fact>]
    let ``default binding fields map in both directions`` () =
        let clientBinding = ReferenceObjects.CommandInputBinding.defaultFieldsIndex.AsClientType()
        Assert.Equivalent(ReferenceObjects.CommandInputBinding.defaultFieldsClient, clientBinding)

        let indexBinding = ReferenceObjects.CommandInputBinding.defaultFieldsClient.AsIndexType()
        Assert.Equivalent(ReferenceObjects.CommandInputBinding.defaultFieldsIndex, indexBinding)

    [<Fact>]
    let ``parameter maps client to index with mandatory fields`` () =
        let actual = ReferenceObjects.CommandInputParameter.mandatoryFieldsClient.AsIndexType()
        Assert.Equivalent(ReferenceObjects.CommandInputParameter.mandatoryFieldsIndex, actual)

    [<Fact>]
    let ``parameter maps client to index with all fields`` () =
        let actual = ReferenceObjects.CommandInputParameter.allFieldsClient.AsIndexType()
        Assert.Equivalent(ReferenceObjects.CommandInputParameter.allFieldsIndex, actual)

    [<Fact>]
    let ``parameter maps index to client with all fields`` () =
        let actual = ReferenceObjects.CommandInputParameter.allFieldsIndex.AsClientType()
        Assert.Equivalent(ReferenceObjects.CommandInputParameter.allFieldsClient, actual)

    [<Fact>]
    let ``all supported types map in both directions`` () =
        let cases = [|
            AVPRClient.CommandInputType.Boolean, AVPRIndex.Domain.CwlPrimitive.Boolean, false
            AVPRClient.CommandInputType.Boolean_, AVPRIndex.Domain.CwlPrimitive.Boolean, true
            AVPRClient.CommandInputType.Int, AVPRIndex.Domain.CwlPrimitive.Int, false
            AVPRClient.CommandInputType.Int_, AVPRIndex.Domain.CwlPrimitive.Int, true
            AVPRClient.CommandInputType.Long, AVPRIndex.Domain.CwlPrimitive.Long, false
            AVPRClient.CommandInputType.Long_, AVPRIndex.Domain.CwlPrimitive.Long, true
            AVPRClient.CommandInputType.Float, AVPRIndex.Domain.CwlPrimitive.Float, false
            AVPRClient.CommandInputType.Float_, AVPRIndex.Domain.CwlPrimitive.Float, true
            AVPRClient.CommandInputType.Double, AVPRIndex.Domain.CwlPrimitive.Double, false
            AVPRClient.CommandInputType.Double_, AVPRIndex.Domain.CwlPrimitive.Double, true
            AVPRClient.CommandInputType.String, AVPRIndex.Domain.CwlPrimitive.String, false
            AVPRClient.CommandInputType.String_, AVPRIndex.Domain.CwlPrimitive.String, true
        |]

        for clientType, primitiveType, isNullable in cases do
            let indexType = clientType.AsIndexType()
            Assert.Equal(primitiveType, indexType.PrimitiveType)
            Assert.Equal(isNullable, indexType.IsNullable)
            Assert.Equal(clientType, indexType.AsClientType())

    [<Fact>]
    let ``unsupported generated type is rejected`` () =
        let invalidType = enum<AVPRClient.CommandInputType> 999
        Assert.Throws<ArgumentOutOfRangeException>(fun () -> invalidType.AsIndexType() |> ignore)
        |> ignore

    [<Fact>]
    let ``null binding maps to CWL defaults`` () =
        let input =
            AVPRClient.CommandInputParameter(
                Id = "value",
                Type = AVPRClient.CommandInputType.String,
                InputBinding = null
            )

        let actual = input.AsIndexType()
        Assert.Equal(0, actual.InputBinding.Position)
        Assert.Equal("", actual.InputBinding.Prefix)
        Assert.True(actual.InputBinding.Separate)

    [<Fact>]
    let ``null and empty input collections map to an empty array`` () =
        let nullInputs: System.Collections.Generic.ICollection<AVPRClient.CommandInputParameter> = null
        let emptyInputs = ResizeArray<AVPRClient.CommandInputParameter>()
        Assert.Empty(nullInputs.AsIndexType())
        Assert.Empty(emptyInputs.AsIndexType())

    [<Fact>]
    let ``generated parameter JSON uses CWL scalar type and lower camel property names`` () =
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
