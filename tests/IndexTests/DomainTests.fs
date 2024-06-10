namespace DomainTests

open System
open System.IO
open Xunit
open AVPRIndex
open AVPRIndex.Domain
open ReferenceObjects

module Author =
    
    [<Fact>]
    let ``create function for mandatory fields``() =
        let actual = Author.create(fullName = "test")
        Assert.Equivalent(Author.mandatoryFields, actual)

    [<Fact>]
    let ``create function for all fields``() =
        let actual = Author.create(
            fullName = "test", 
            Email = "test@test.test",
            Affiliation = "testaffiliation",
            AffiliationLink = "test.com"
        )
        Assert.Equivalent(Author.allFields, actual)

module OntologyAnnotation =

    [<Fact>]
    let ``create function for mandatory fields``() =
        let actual = OntologyAnnotation.create(name = "test")
        Assert.Equivalent(OntologyAnnotation.mandatoryFields, actual)

    [<Fact>]
    let ``create function for all fields``() =
        let actual = OntologyAnnotation.create(
            name = "test",
            TermSourceREF = "REF",
            TermAccessionNumber = "TAN"
        )
        Assert.Equivalent(OntologyAnnotation.allFields, actual)

module ValidationPackageMetadata =
    
    [<Fact>]
    let ``create function for mandatory fields``() =
        let actual = ValidationPackageMetadata.create(
            name = "name",
            summary = "summary" ,
            description = "description" ,
            majorVersion = 1,
            minorVersion = 0,
            patchVersion = 0
        )
        Assert.Equivalent(ValidationPackageMetadata.mandatoryFields, actual)

    [<Fact>]
    let ``create function for all fields``() =
        let actual = ValidationPackageMetadata.create(
            name = "name",
            summary = "summary" ,
            description = "description" ,
            majorVersion = 1,
            minorVersion = 0,
            patchVersion = 0,
            Publish = true,
            Authors = [|
                Author.create(
                    fullName = "test", 
                    Email = "test@test.test",
                    Affiliation = "testaffiliation",
                    AffiliationLink = "test.com"
                )
            |],
            Tags = [|
            OntologyAnnotation.create(
                    name = "test",
                    TermSourceREF = "REF",
                    TermAccessionNumber = "TAN"
                )
            |],
            ReleaseNotes = "releasenotes",
            CQCHookEndpoint = "hookendpoint"
        )
        Assert.Equivalent(ValidationPackageMetadata.allFields, actual)