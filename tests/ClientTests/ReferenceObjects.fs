module ReferenceObjects

open Utils
open AVPRIndex
open AVPRClient

let date = System.DateTime.ParseExact("2021-01-01", "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)

let testDate = System.DateTimeOffset.Parse("01/01/2024")

module Author = 
    
    let mandatoryFieldsClient = AVPRClient.Author(FullName = "test")

    let allFieldsClient =
        AVPRClient.Author(
            FullName = "test",
            Email = "test@test.test",
            Affiliation = "testaffiliation",
            AffiliationLink = "test.com"
        )

    let mandatoryFieldsIndex = AVPRIndex.Domain.Author(FullName = "test")

    let allFieldsIndex =
        AVPRIndex.Domain.Author(
            FullName = "test",
            Email = "test@test.test",
            Affiliation = "testaffiliation",
            AffiliationLink = "test.com"
        )

module OntologyAnnotation = 
    
    let mandatoryFieldsClient = AVPRClient.OntologyAnnotation(Name = "test")

    let allFieldsClient = AVPRClient.OntologyAnnotation(
        Name = "test",
        TermSourceREF = "REF",
        TermAccessionNumber = "TAN"
    )

    let mandatoryFieldsIndex = AVPRIndex.Domain.OntologyAnnotation(Name = "test")

    let allFieldsIndex = AVPRIndex.Domain.OntologyAnnotation(
        Name = "test",
        TermSourceREF = "REF",
        TermAccessionNumber = "TAN"
    )

module ValidationPackageMetadata = 
    
    let mandatoryFields = ValidationPackageMetadata(
        Name = "name",
        Summary = "summary" ,
        Description = "description" ,
        MajorVersion = 1,
        MinorVersion = 0,
        PatchVersion = 0
    )

    let allFields = ValidationPackageMetadata(
        Name = "name",
        Summary = "summary" ,
        Description = "description" ,
        MajorVersion = 1,
        MinorVersion = 0,
        PatchVersion = 0,
        Publish = true,
        Authors = [|Author.allFieldsIndex|],
        Tags = [|OntologyAnnotation.allFieldsIndex|],
        ReleaseNotes = "releasenotes",
        CQCHookEndpoint = "hookendpoint"
    )

module PackageContentHash =
    open System
    open System.IO
    open System.Text
    open System.Security.Cryptography

    let expected_hash = 
        let md5 = MD5.Create()
        "fixtures/test_validation_package_all_fields.fsx"
        |> File.ReadAllText
        |> fun s -> s.ReplaceLineEndings("\n")
        |> Encoding.UTF8.GetBytes
        |> md5.ComputeHash
        |> Convert.ToHexString

    let allFields = AVPRClient.PackageContentHash(
        PackageName = "name",
        PackageMajorVersion = 1,
        PackageMinorVersion = 0,
        PackagePatchVersion = 0,
        Hash = expected_hash
    )
  

module ValidationPackage =

    open System.IO

    let expected_content = 

        "fixtures/test_validation_package_all_fields.fsx"
        |> File.ReadAllBytes


    let allFields = AVPRClient.ValidationPackage(
        Name = "name",
        Summary = "summary" ,
        Description = "description" ,
        MajorVersion = 1,
        MinorVersion = 0,
        PatchVersion = 0,
        PackageContent = expected_content,
        ReleaseDate = date,
        Authors = [|Author.allFieldsClient|],
        Tags = [|OntologyAnnotation.allFieldsClient|],
        ReleaseNotes = "releasenotes",
        CQCHookEndpoint = "hookendpoint"
    )