let [<Literal>]PACKAGE_METADATA = """(*
---
Name: insdc_curation
Description: Validates certain INSDC record curation metadata.
Summary: |
  Validates certain INSDC record curation metadata.
    - fastqc files
      - experiment accession annotation is present and correct
      - descends from a correctly named sample
MajorVersion: 0
MinorVersion: 0
PatchVersion: 1
Publish: true
Authors:
  - FullName: Kevin Schneider
    Affiliation: DataPLANT
Tags:
  - Name: INSDC
  - Name: Metadata curation
ReleaseNotes: |
  - fastqc process graph validation
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

    /// represents the relations provided by ENA portal API in TSV form
    type INSDC_Relations = {
        [<FieldAttribute("study_accession")>]
        study_accession: string
        [<FieldAttribute("sample_accession")>]
        sample_accession: string
        [<FieldAttribute("experiment_accession")>]
        experiment_accession: string
        [<FieldAttribute("run_accession")>]
        run_accession: string
        [<FieldAttribute("fastq_ftp")>]
        fastq_ftp: string
    }

    /// Some records map from sample to multiple fastq files in a single line (e.g., paired end reads). This function splits such records into multiple records, one for each fastq file.
    let splitByFastq (r: INSDC_Relations) =
        match r.fastq_ftp.Split(';') |> Array.filter (fun s -> s <> "") with
        | [||]  -> [ r ]                                        // no fastq → keep row as-is
        | files -> [ for f in files -> { r with fastq_ftp = f } ]

    /// lookup index for retrieving INSDC_Relations by any of the accession numbers or fastq file name. Last-wins in case of duplicates.
    let buildIndex (records: INSDC_Relations seq) =
        let d = Dictionary<string, INSDC_Relations>(StringComparer.OrdinalIgnoreCase)
        for r in records do
            for v in [ 
                r.study_accession
                r.sample_accession
                r.experiment_accession
                r.run_accession
                r.fastq_ftp 
            ] do
                if not (String.IsNullOrEmpty v) then d[v] <- r   // last-wins
        d

    module OntologyTerms = 
        let experiment_accession_term =
            OntologyAnnotation.fromTermAnnotation(
                "NCIT:C175892",
                name = "Experiment Accession Number"
            )

module ExpectedData =

    let reader = new Csv.CsvReader<Domain.INSDC_Relations>(SchemaMode=Csv.Fill)

    let relations = 
        http {
            GET $"https://www.ebi.ac.uk/ena/portal/api/filereport?accession={project_accession}&result=read_run&fields=study_accession,sample_accession,experiment_accession,run_accession,tax_id,scientific_name,fastq_ftp,submitted_ftp,bam_ftp&format=tsv&download=true&limit=0"
        }
        |> Request.send
        |> Response.toText
        |> fun response -> reader.ReadFromString(response, '\t', firstLineHasHeader = true)
        |> Seq.collect Domain.splitByFastq
        |> List.ofSeq

    let relations_index = Domain.buildIndex relations

    let tryFindRelation (s: string) =
        match relations_index.TryGetValue s with
        | true, r  -> Some r
        | false, _ -> None


let create_insdc_relation_validation_cases_for_fastq_file (node: QNode) =

    let samples =
        arc.ArcTables.SamplesOf(node)
        |> Seq.map (fun n -> n.Name)
        |> Array.ofSeq

    let experiment_accession_actual =
        try 
            Some (arc.PreviousParametersOf(node)).[Domain.OntologyTerms.experiment_accession_term]
        with
            | _ -> None

    let expected_relations = Expect.wantSome (ExpectedData.tryFindRelation node.Name) "Experiment accession annotation is missing in INSDC relations" 

    testList node.Name [

        test $"has correct experiment accession annotation (expected: {expected_relations.experiment_accession})" {
            let actual = Expect.wantSome experiment_accession_actual "Experiment accession annotation is missing"
            Expect.equal actual.ValueText expected_relations.experiment_accession "Experiment accession annotation does not match node name"
        }

        test $"descends from correct sample (expected: {expected_relations.sample_accession})" {
            Expect.contains samples expected_relations.sample_accession $"Sample accession {expected_relations.sample_accession} not found in ARC"
        }
    ]

let fastqc_cases =
    testList "fastqc" (
        arc.TryGetAssay("sequencing")
        |> Option.map (fun assay ->
            assay.LastData
            |> Seq.map (fun node -> create_insdc_relation_validation_cases_for_fastq_file node)
            |> Seq.toList
        ) 
        |> Option.defaultValue []
    )

Setup.ValidationPackage(
    metadata = Setup.Metadata(PACKAGE_METADATA, AVPRIndex.Frontmatter.FSharpFrontmatter),
    CriticalValidationCases = [
        test "has sequencing assay" {
            Expect.isSome (arc.TryGetAssay("sequencing")) "No sequencing assay found"
        }
        fastqc_cases
    ]
)
|> Execute.ValidationPipeline(
    basePath = arcDir
)