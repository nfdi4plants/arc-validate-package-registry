let [<Literal>]PACKAGE_METADATA = """(*
---
Name: workflowhub
Summary: Validates if the workflows and runs of an ARC carry the metadata needed to be packaged as a Workflow RO-Crate and deposited on WorkflowHub.
Description: |
  Validates if the workflows and runs of an ARC carry the metadata needed to be packaged as a
  Workflow RO-Crate and deposited on WorkflowHub (https://workflowhub.eu).

  Every workflow in `workflows/` and every run in `runs/` is checked separately and reported
  under its own identifier, so an ARC may contain a mix of depositable and not-yet-depositable
  items. The ARC as a whole passes when it has a license and at least one workflow or run that
  satisfies every requirement.

  Registration on WorkflowHub additionally requires choosing a WorkflowHub team. The team is not
  part of the ARC and cannot be validated here.

  ARC-wide (critical):
  - The ARC can be loaded
  - The ARC has a LICENSE file
  - At least one workflow or run is ready for WorkflowHub

  Per workflow (reported individually):
  - Title and Description are set
  - The title is at most 255 characters (WorkflowHub's limit)
  - At least one contact, each with a first name
  - A CWL description exists
  - URI and Version are set
  - Every component has a component type with a name
  - ARCtrl can convert it into a Workflow RO-Crate

  Per run (reported individually):
  - Title and Description are set
  - The title is at most 255 characters (WorkflowHub's limit)
  - At least one performer, each with a first name
  - A CWL description exists
  - Every CWL input parameter has an assigned value whose type matches the parameter
  - The first process table has input and output columns with a value in every row
  - ARCtrl can convert it into a Workflow Run RO-Crate
MajorVersion: 0
MinorVersion: 0
PatchVersion: 1
Publish: false
Authors:
  - FullName: Caroline Ott
    Email: caroline.ott@rptu.de
    Affiliation: DataPLANT
Tags:
  - Name: ARC
  - Name: WorkflowHub
  - Name: RO-Crate
  - Name: workflow
  - Name: CWL
  - Name: data publication
ReleaseNotes: |
  - first release, covering the ARCtrl workflow/run RO-Crate conversion requirements, the
    WorkflowHub Workflow RO-Crate profile 1.0 license requirement and WorkflowHub's title limit
---
*)"""

// TODO: ARCtrl 3.2.0 validates only the first workflow invocation of a run, so convertibility
// depends on process table order and a run without process tables never converts. The per-run
// table check requires a fully described first process, which is stricter than 3.2.0 needs.
// Revisit when ARCtrl changes this.

#r "nuget: ARCtrl, 3.2.0"
#r "nuget: ARCExpect.Core, 7.0.0-alpha"

open ARCtrl
open ARCtrl.CWL
open ARCExpect
open Expecto
open System.IO
open System.Runtime.ExceptionServices


// Input:
let arcDir = Directory.GetCurrentDirectory()


// Values:

/// Loading may fail on a malformed ARC. Keep the error so it can be surfaced as a test failure
/// instead of aborting the whole script.
let loadedArc =
    try Ok (ARC.load arcDir)
    with e -> Error e.Message

let workflows =
    match loadedArc with
    | Ok arc -> arc.Workflows |> List.ofSeq
    | Error _ -> []

let runs =
    match loadedArc with
    | Ok arc -> arc.Runs |> List.ofSeq
    | Error _ -> []

/// The license ARCtrl would hand to the RO-Crate writer. When the ARC has no LICENSE file,
/// ARCtrl silently substitutes "ALL RIGHTS RESERVED BY THE AUTHORS", which is why the presence
/// of a real license file is checked separately.
let license =
    match loadedArc with
    | Ok arc -> arc.License |> Option.defaultWith License.GetDefaultLicense
    | Error _ -> License.GetDefaultLicense()


// Helper types:

/// The outcome of a single named requirement. It is evaluated once, when the requirement list of a
/// workflow or run is built, and only the outcome is kept: the ARC-wide readiness case and the
/// reported test case both read it, so no requirement is ever checked twice.
type Requirement = {
    Name    : string
    Failure : exn option
}

/// Evaluates `run` immediately. It raises with an explanatory message when the requirement is not
/// met, and that exception is what gets recorded.
let requirement name (run : unit -> unit) =
    try run (); { Name = name; Failure = None }
    with e -> { Name = name; Failure = Some e }

let holds (r : Requirement) = r.Failure.IsNone

/// Replays the recorded outcome. Rethrowing the original exception with its original stack trace
/// keeps Expecto's distinction between a failed requirement and an unexpected error.
let toTestCase (r : Requirement) =
    testCase r.Name (fun () ->
        match r.Failure with
        | Some e -> ExceptionDispatchInfo.Capture(e).Throw()
        | None -> ()
    )

/// Reports a validation failure without an F# stack trace, so the JUnit report and the console
/// show the explanation instead of script internals.
let fail (message : string) : unit = Expecto.Tests.failtestNoStackf "%s" message

let expect condition message =
    if not condition then fail message

/// True when a cell contributes no name to the process input or output it becomes. The first
/// content entry is the naming one for every cell kind, and ARCtrl happily composes a node from an
/// empty one, which lands in the crate as an unnamed input or output.
let cellIsBlank (cell : CompositeCell) =
    match cell.GetContent() with
    | [||] -> true
    | content -> content.[0].Trim() = ""

/// Positions (1-based) of the persons ARCtrl cannot compose. It builds `creator` from the first
/// name and fails with "Person must have a given name" when it is missing.
let personsWithoutFirstName (persons : Person seq) =
    persons
    |> Seq.mapi (fun i p -> i, p)
    |> Seq.filter (fun (_, p) -> p.FirstName |> Option.forall (fun n -> n.Trim() = ""))
    |> Seq.map (fun (i, _) -> string (i + 1))
    |> List.ofSeq


// Requirements per workflow.
//
// Sources:
//   ARCtrl 3.2.0 src/ARCtrl/ROCrateIO.fs `ROCrate.writeWorkflowAsCrate`, which rejects an
//   ARC Workflow dataset without identifier/name/description/mainEntity/hasPart/creator and a
//   workflow protocol without creator/name/input/output/programmingLanguage/url/version/dateCreated.
//   Of those, only the ones checked below can actually be missing: identifier, mainEntity, hasPart,
//   programmingLanguage and dateCreated are always populated during composition, and `input`/
//   `output` are always set even when the CWL declares none, so an empty input or output list is
//   accepted (verified by running ARCtrl 3.2.0).
//   ARCtrl 3.2.0 src/ARCtrl/Conversion/Workflow.fs `WorkflowConversion.composeWorkflow`, which
//   fails on a missing CWL description, untyped inputs/outputs and untyped components. Untyped
//   inputs/outputs are not checked separately: the CWL decoder already rejects a parameter without
//   a valid type, so such an ARC fails to load and is reported by the critical ARC case instead.
//   WorkflowHub Workflow RO-Crate profile 1.0 (https://w3id.org/workflowhub/workflow-ro-crate/1.0)
//   and the WorkflowHub metadata list (https://about.workflowhub.eu/docs/metadata-list/), which
//   make the crate `name` the mandatory workflow title.
let workflowRequirements (workflow : ArcWorkflow) =
    let id = workflow.Identifier
    [
        requirement "Title is set" (fun () ->
            expect
                (workflow.Title |> Option.exists (fun t -> t.Trim() <> ""))
                $"Workflow '{id}' has no title. ARCtrl writes it as `name` of the ARC Workflow dataset and of the crate root, and WorkflowHub uses `name` as the workflow title, which is mandatory on registration."
        )

        requirement "Title is at most 255 characters" (fun () ->
            match workflow.Title with
            | Some t when t.Length > 255 ->
                fail $"The title of workflow '{id}' is {t.Length} characters long. WorkflowHub rejects titles longer than 255 characters."
            | _ -> ()
        )

        requirement "Description is set" (fun () ->
            expect
                (workflow.Description |> Option.exists (fun d -> d.Trim() <> ""))
                $"Workflow '{id}' has no description. ARCtrl requires `description` on the ARC Workflow dataset before writing the crate, and WorkflowHub shows it as the workflow description."
        )

        requirement "At least one contact" (fun () ->
            expect
                (workflow.Contacts.Count > 0)
                $"Workflow '{id}' has no contacts. ARCtrl requires `creator` both on the ARC Workflow dataset and on the workflow protocol, and WorkflowHub reads it as the workflow creators."
        )

        requirement "Every contact has a first name" (fun () ->
            personsWithoutFirstName workflow.Contacts
            |> function
                | [] -> ()
                | positions ->
                    fail $"""Contact(s) at position {String.concat ", " positions} of workflow '{id}' have no first name. ARCtrl composes `creator` from it and fails with "Person must have a given name"."""
        )

        requirement "CWL description exists" (fun () ->
            expect
                workflow.CWLDescription.IsSome
                $"Workflow '{id}' has no CWL description. ARCtrl cannot compose the workflow protocol without one, and it becomes the crate's `mainEntity`, which the Workflow RO-Crate profile requires. Add a `workflows/{id}/workflow.cwl`."
        )

        requirement "URI is set" (fun () ->
            expect
                (workflow.URI |> Option.exists (fun u -> u.Trim() <> ""))
                $"Workflow '{id}' has no URI. ARCtrl only writes `url` on the workflow protocol when the URI is set and rejects the crate without it."
        )

        requirement "Version is set" (fun () ->
            expect
                (workflow.Version |> Option.exists (fun v -> v.Trim() <> ""))
                $"Workflow '{id}' has no version. ARCtrl only writes `version` on the workflow protocol when the version is set and rejects the crate without it."
        )

        requirement "All components have a component type with a name" (fun () ->
            workflow.Components
            |> Seq.mapi (fun i c -> i, c)
            |> Seq.filter (fun (_, c) ->
                match c.ComponentType with
                | Some ct -> ct.Name.IsNone
                | None -> true
            )
            |> Seq.map (fun (i, _) -> string i)
            |> List.ofSeq
            |> function
                | [] -> ()
                | positions ->
                    fail $"""Workflow '{id}' has components without a named component type at position(s) {String.concat ", " positions}. ARCtrl needs the name to compose the computational tool PropertyValue and fails otherwise."""
        )

        requirement "ARCtrl can write the Workflow RO-Crate" (fun () ->
            match loadedArc with
            | Error _ -> ()
            | Ok arc ->
                try
                    ARCtrl.Json.ARC.ROCrate.writeWorkflowAsCrate(workflow, arc.FileSystem, license)
                    |> ignore
                with e ->
                    fail $"ARCtrl could not write workflow '{id}' as a Workflow RO-Crate: {e.Message}"
        )
    ]


// Requirements per run.
//
// Sources:
//   ARCtrl 3.2.0 src/ARCtrl/ROCrateIO.fs `ROCrate.writeRunAsCrate`, which rejects an ARC Run
//   dataset without identifier/name/description/about/mentions/creator/hasPart and a workflow
//   invocation without name/instrument/result/object.
//   ARCtrl 3.2.0 src/ARCtrl/Conversion/Run.fs `RunConversion.composeRun`, which fails on a
//   missing CWL description, an untyped input parameter, an input parameter without an assigned
//   value and a value whose type does not match the parameter. Untyped parameters are not checked
//   separately, for the same reason as for workflows: the CWL decoder rejects them at load time.
let runRequirements (run : ArcRun) =
    let id = run.Identifier
    [
        requirement "Title is set" (fun () ->
            expect
                (run.Title |> Option.exists (fun t -> t.Trim() <> ""))
                $"Run '{id}' has no title. ARCtrl writes it as `name` of the ARC Run dataset and of the crate root, and WorkflowHub uses `name` as the title, which is mandatory on registration."
        )

        requirement "Title is at most 255 characters" (fun () ->
            match run.Title with
            | Some t when t.Length > 255 ->
                fail $"The title of run '{id}' is {t.Length} characters long. WorkflowHub rejects titles longer than 255 characters."
            | _ -> ()
        )

        requirement "Description is set" (fun () ->
            expect
                (run.Description |> Option.exists (fun d -> d.Trim() <> ""))
                $"Run '{id}' has no description. ARCtrl requires `description` on the ARC Run dataset before writing the crate, and WorkflowHub shows it as the description."
        )

        requirement "At least one performer" (fun () ->
            expect
                (run.Performers.Count > 0)
                $"Run '{id}' has no performers. ARCtrl requires `creator` on the ARC Run dataset, and WorkflowHub reads it as the creators."
        )

        requirement "Every performer has a first name" (fun () ->
            personsWithoutFirstName run.Performers
            |> function
                | [] -> ()
                | positions ->
                    fail $"""Performer(s) at position {String.concat ", " positions} of run '{id}' have no first name. ARCtrl composes `creator` from it and fails with "Person must have a given name"."""
        )

        requirement "CWL description exists" (fun () ->
            expect
                run.CWLDescription.IsSome
                $"Run '{id}' has no CWL description. ARCtrl cannot compose the workflow protocol without one, and it becomes the crate's `mainEntity`, which the Workflow RO-Crate profile requires. Add a `runs/{id}/run.cwl`."
        )

        requirement "Every CWL input parameter has an assigned value" (fun () ->
            match run.CWLDescription with
            | None -> ()
            | Some pu ->
                let assigned = run.CWLInput |> Seq.map (fun i -> i.Key) |> Set.ofSeq
                CWLProcessingUnit.getInputs pu
                |> Seq.map (fun i -> i.Name)
                |> Seq.filter (fun n -> not (assigned.Contains n))
                |> List.ofSeq
                |> function
                    | [] -> ()
                    | unassigned ->
                        fail $"""Run '{id}' assigns no value to the CWL input parameter(s) {String.concat ", " unassigned}. ARCtrl builds the workflow invocation from these values and fails when a parameter has none. Add them to `runs/{id}/run.yml`."""
        )

        requirement "Assigned values match the parameter types" (fun () ->
            match run.CWLDescription with
            | None -> ()
            | Some pu ->
                let byName =
                    CWLProcessingUnit.getInputs pu
                    |> Seq.choose (fun i -> i.Type_ |> Option.map (fun t -> i.Name, t))
                    |> Map.ofSeq
                run.CWLInput
                |> Seq.choose (fun inputValue ->
                    match inputValue.Type, byName.TryFind inputValue.Key with
                    | Some valueType, Some paramType when not (CWLType.typesEqual valueType paramType) ->
                        Some $"{inputValue.Key} ({Encode.formatCWLType valueType} vs. {Encode.formatCWLType paramType})"
                    | _ -> None
                )
                |> List.ofSeq
                |> function
                    | [] -> ()
                    | mismatches ->
                        fail $"""Run '{id}' assigns values whose type does not match the CWL input parameter: {String.concat ", " mismatches}. ARCtrl rejects the conversion on a type mismatch."""
        )

        // Requiring an input and an output column is deliberately stricter
        // than ARCtrl 3.2.0; see the TODO at the top of this script.
        requirement "First process table has filled input and output columns" (fun () ->
            match run.Tables |> Seq.tryHead with
            | None ->
                fail $"Run '{id}' has no process table. ARCtrl requires `result` on the workflow invocation, and it is only filled from the outputs of a process table; the invocation composed from the CWL inputs alone carries none."
            | Some table ->
                // A missing column leaves the invocation without `object`/`result`; a present but
                // empty cell is worse, because ARCtrl composes an unnamed node from it and writes
                // the crate as if the process were described.
                let column (label : string) (isColumn : CompositeHeader -> bool) =
                    match table.Headers |> Seq.tryFindIndex isColumn with
                    | None -> Some $"has no {label} column"
                    | Some col ->
                        [ 0 .. table.RowCount - 1 ]
                        |> List.filter (fun row ->
                            match table.TryGetCellAt(col, row) with
                            | Some cell -> cellIsBlank cell
                            | None -> true
                        )
                        |> function
                            | [] -> None
                            | blank ->
                                let shown = blank |> List.truncate 5 |> List.map (fun r -> string (r + 1))
                                let more = if blank.Length > 5 then ", ..." else ""
                                Some $"""has an empty {label} cell in row(s) {String.concat ", " shown}{more}"""
                [
                    (if table.RowCount = 0 then Some "has no rows" else None)
                    column "input" (fun h -> h.isInput)
                    column "output" (fun h -> h.isOutput)
                ]
                |> List.choose (fun problem -> problem)
                |> function
                    | [] -> ()
                    | problems ->
                        fail $"""The first process table '{table.Name}' of run '{id}' {String.concat " and " problems}. ARCtrl composes the run's workflow invocation from this table's rows and validates only the first one, so the table has to name the process inputs (`object`) and outputs (`result`); later tables are not considered."""
        )

        requirement "ARCtrl can write the Workflow Run RO-Crate" (fun () ->
            match loadedArc with
            | Error _ -> ()
            | Ok arc ->
                try
                    ARCtrl.Json.ARC.ROCrate.writeRunAsCrate(run, arc.FileSystem, license)
                    |> ignore
                with e ->
                    fail $"ARCtrl could not write run '{id}' as a Workflow Run RO-Crate: {e.Message}"
        )
    ]


// Building the lists is what evaluates the requirements, once. The badge, the ARC-wide readiness
// case and the reported test cases all read these same outcomes.
let workflowResults = workflows |> List.map (fun w -> w, workflowRequirements w)
let runResults = runs |> List.map (fun r -> r, runRequirements r)

let readyWorkflows = workflowResults |> List.filter (fun (_, rs) -> rs |> List.forall holds)
let readyRuns = runResults |> List.filter (fun (_, rs) -> rs |> List.forall holds)

let readyCount = readyWorkflows.Length + readyRuns.Length
let totalCount = workflowResults.Length + runResults.Length


// Validation Cases:

let arcCases =
    testList "ARC" [
        testCase "ARC can be loaded" (fun () ->
            match loadedArc with
            | Ok _ -> ()
            | Error message -> fail $"The ARC at '{arcDir}' could not be loaded: {message}"
        )

        // Workflow RO-Crate profile 1.0: "The Crate MUST specify a license."
        // ARCtrl does not enforce this: without a LICENSE file it substitutes the placeholder
        // "ALL RIGHTS RESERVED BY THE AUTHORS", which is not one of the licenses WorkflowHub accepts.
        testCase "ARC has a LICENSE file" (fun () ->
            match loadedArc with
            | Error _ -> ()
            | Ok arc ->
                match arc.License with
                | None ->
                    fail """The ARC has no LICENSE file. The Workflow RO-Crate profile 1.0 requires the crate to specify a license, and without one ARCtrl writes the placeholder "ALL RIGHTS RESERVED BY THE AUTHORS" instead. Add a LICENSE, LICENSE.txt, LICENSE.md or LICENSE.rst at the ARC root."""
                | Some lic when lic.Content.Trim() = "" ->
                    fail $"The LICENSE file '{lic.Path}' of this ARC is empty. Its text is what ARCtrl writes into the crate as the license, so it has to state the license the workflow is published under."
                | Some _ -> ()
        )

        testCase "At least one workflow or run is ready for WorkflowHub" (fun () ->
            match loadedArc with
            | Error _ -> ()
            | Ok _ ->
                if totalCount = 0 then
                    fail "The ARC contains neither a workflow in `workflows/` nor a run in `runs/`, so there is nothing to deposit on WorkflowHub."
                else
                    expect
                        (readyCount > 0)
                        $"None of the {totalCount} workflow(s)/run(s) of this ARC meets all requirements for a WorkflowHub deposition. See the individual results below for what is missing."
        )
    ]

let workflowCases =
    workflowResults
    |> List.map (fun (workflow, requirements) ->
        testList $"workflows/{workflow.Identifier}" (requirements |> List.map toTestCase)
    )

let runCases =
    runResults
    |> List.map (fun (run, requirements) ->
        testList $"runs/{run.Identifier}" (requirements |> List.map toTestCase)
    )


// Execution:

Setup.ValidationPackage(
    metadata = Setup.Metadata(PACKAGE_METADATA, AVPRIndex.Frontmatter.FSharpFrontmatter),
    CriticalValidationCases = [arcCases],
    NonCriticalValidationCases = (workflowCases @ runCases)
)
|> Execute.ValidationPipeline(
    basePath = arcDir,
    BadgeLabelText = $"{readyCount}/{totalCount} ready for WorkflowHub"
)
