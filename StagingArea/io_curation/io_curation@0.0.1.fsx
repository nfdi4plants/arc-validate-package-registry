let [<Literal>]PACKAGE_METADATA = """(*
---
Name: io_curation
Description: Validates IO type of metadata table in studies.
Summary: |
  Validates IO type of metadata table in studies.
    - IO
      - Input type should always be Source Name
      - Output type should always be Source Name except for the last table, there it should be Sample Name
      - descends from a correctly named sample
MajorVersion: 0
MinorVersion: 0
PatchVersion: 1
Publish: false
Authors:
  - FullName: Emre Filiz
    Affiliation: DataPLANT
Tags:
  - Name: Metadata curation
ReleaseNotes: |
  - IO process graph validation
---
*)"""

#r "nuget: ARCtrl"
#r "nuget: ARCtrl.QueryModel, 3.0.0-alpha.4"
#r "nuget: ARCExpect.Core, 7.0.0-alpha"
#r "nuget: FsHttp"
#r "nuget: FSharpAux"
#r "nuget: FSharpAux.IO"

open ARCtrl
open ARCtrl.QueryModel
open ARCExpect
open FsHttp
open FSharpAux.IO.SchemaReader
open Expecto
open System.IO
open System

let arcDir = Directory.GetCurrentDirectory()

let arc =
    try ARC.load arcDir with
    | _ -> ARC(identifier = "unable to load arc from this dir")

let project_accession = arc.Identifier

module Domain =

    open FSharpAux.IO.SchemaReader.Attribute
    open System.Collections.Generic

    let checkFirstTableIO (table: ArcTable) =
        let hasSourceInput =
            table.Headers
            |> Seq.exists (function
                | CompositeHeader.Input IOType.Source -> true
                | _ -> false
            )

        let hasOutput =
            table.Headers
            |> Seq.exists (function 
                | CompositeHeader.Output _ -> true
                | _ -> false
            )
        hasSourceInput && (not hasOutput)

    let checkTableIO (table: ArcTable) =
        let hasSourceInput =
            table.Headers
            |> Seq.exists (function
                | CompositeHeader.Input IOType.Source -> true
                | _ -> false
            )

        let hasSourceOutput =
            table.Headers
            |> Seq.exists (function
                | CompositeHeader.Output IOType.Source -> true
                | _ -> false
            )

        hasSourceInput && hasSourceOutput

    let checkLastTableIO (table: ArcTable) =
        let hasSourceInput =
            table.Headers
            |> Seq.exists (function
                | CompositeHeader.Input IOType.Source -> true
                | _ -> false
            )

        let hasSampleOutput = 
            table.Headers
            |> Seq.exists (function
                | CompositeHeader.Output IOType.Sample -> true
                | _ -> false
            )

        hasSourceInput && hasSampleOutput

let io_cases =
    arc.Studies
    |> Seq.map (fun study -> 
        let tables = study.Tables |> Seq.toList

        let tableTests =
            if tables.Length = 0 then
                [
                    test "Has atleast 2 tables" {
                        Expect.isTrue false $"Study {study.Identifier} has no tables for validation."
                    }
                ]
            elif tables.Length = 1 then
                [
                    test "Has atleast 2 tables" {
                        Expect.isTrue false $"Study {study.Identifier} has only 1 table for validation."
                    }
                ]
            else
                let firstTable = tables.Head
                let lastTable = tables |> List.last
                let middleTables = tables |> List.skip 1 |> List.take (tables.Length - 2)

                let firstTest = 
                    test $"First table '{firstTable.Name}'" {
                        Expect.isTrue (Domain.checkFirstTableIO firstTable) "First table must not have an output, but a Source-Input."
                    }

                let lastTest = 
                    test $"Last table '{firstTable.Name}'" {
                        Expect.isTrue (Domain.checkLastTableIO lastTable) "Last table must have a Source-Input and a Sample-Output."
                    }

                let midTests =
                    middleTables |> List.map (fun t -> 
                        test $"Mittlere Tabelle '{t.Name}'" {
                            Expect.isTrue (Domain.checkTableIO t) "Middle tables must have Source-Input and Source-Output."
                        }
                    )

                [firstTest] @ midTests @ [lastTest]
        testList $"Study: {study.Identifier}" tableTests
    )
    |> Seq.toList
    |> testList "IO Validation for Studies"

Setup.ValidationPackage(
    metadata = Setup.Metadata(PACKAGE_METADATA, AVPRIndex.Frontmatter.FSharpFrontmatter),
    CriticalValidationCases = [
        io_cases
    ]
)
|> Execute.ValidationPipeline(
    basePath = arcDir
)
