namespace TypeExtensionsTests

open System
open System.IO
open System.Text
open Xunit
open AVPRClient

open System.Security.Cryptography

module ValidationPackageIndex =

    let test_validation_package_all_fields = AVPRIndex.Domain.ValidationPackageIndex.create(
        repoPath = "fixtures/test_validation_package_all_fields.fsx",
        fileName = "test_validation_package_all_fields.fsx",
        lastUpdated = ReferenceObjects.date,
        contentHash = ReferenceObjects.Hash.expected_hash,
        metadata = ReferenceObjects.ValidationPackageMetadata.allFields
    )

    [<Fact>]
    let ``toValidationPackage with release date`` () =
        let actual = test_validation_package_all_fields.toValidationPackage(ReferenceObjects.date)
        Assert.Equivalent(actual, ReferenceObjects.ValidationPackage.allFields)



    [<Fact>]
    let ``toPackageContentHash without direct file hash`` () =
        let actual = test_validation_package_all_fields.toPackageContentHash()
        Assert.Equivalent(ReferenceObjects.Hash.allFields, actual)

    [<Fact>]
    let ``toPackageContentHash with direct file hash`` () =
        let actual = test_validation_package_all_fields.toPackageContentHash(HashFileDirectly = true)
        Assert.Equivalent(ReferenceObjects.Hash.allFields, actual)

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