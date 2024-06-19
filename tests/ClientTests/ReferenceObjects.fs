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

module Hash =

    let expected_hash = "C5BD4262301D27CF667106D9024BD721"

    let allFields = AVPRClient.PackageContentHash(
        PackageName = "name",
        PackageMajorVersion = 1,
        PackageMinorVersion = 0,
        PackagePatchVersion = 0,
        Hash = expected_hash
    )

module BinaryContent =

    open System.IO

    let expected_content = "(*
---
Name: name
Summary: summary
Description = description
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: true
Authors:
  - FullName: test
    Email: test@test.test
    Affiliation: testaffiliation
    AffiliationLink: test.com
Tags:
  - Name: test
    TermSourceREF: REF
    TermAccessionNumber: TAN
ReleaseNotes: releasenotes
CQCHookEndpoint: hookendpoint
---
*)

printfn \"yes\""                  .ReplaceLineEndings("\n")

    let expected_binary_content = expected_content |> System.Text.Encoding.UTF8.GetBytes


module ValidationPackage =

    open System.IO

    let allFields = AVPRClient.ValidationPackage(
        Name = "name",
        Summary = "summary" ,
        Description = "description" ,
        MajorVersion = 1,
        MinorVersion = 0,
        PatchVersion = 0,
        PackageContent = BinaryContent.expected_binary_content,
        ReleaseDate = date,
        Authors = [|Author.allFieldsClient|],
        Tags = [|OntologyAnnotation.allFieldsClient|],
        ReleaseNotes = "releasenotes",
        CQCHookEndpoint = "hookendpoint"
    )