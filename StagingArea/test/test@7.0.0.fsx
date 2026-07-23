let [<Literal>]PACKAGE_METADATA = """(*
---
Name: test
MajorVersion: 7
MinorVersion: 0
PatchVersion: 0
Publish: true
Summary: this package is here for testing purposes only.
Description: this package is here for testing purposes only - now with payload in json output.
CLIArguments:
  - Flags: 
      - "-t"
      - "--test"
    Description: use to do some testing idk
    Example: --test
  - Flags: 
      - "-e"
      - "--echo"
    Description: print the arg given
    Example: --echo "yo whadup"
Authors:
  - FullName: John Doe
    Email: j@d.com
    Affiliation: University of Nowhere
    AffiliationLink: https://nowhere.edu
  - FullName: Jane Doe
    Email: jj@d.com
    Affiliation: University of Somewhere
    AffiliationLink: https://somewhere.edu
Tags:
  - Name: validation
  - Name: my-package
  - Name: thing
ReleaseNotes: add cli args
CQCHookEndpoint: https://archigator-beta.nfdi4plants.org
---
*)"""

printfn "If you can read this in your console, you are executing test package v7.0.0!" 

#r "nuget: ARCExpect.Core, 7.0.0-alpha"

open ARCExpect
open Expecto

module CLIArgs =

    type Args = {
        // -i and -o will always get passed if the package is called via arc-validate
        InputPath: string
        OutputPath: string
        Test : bool
        Echo : string
    }

    let defaultArgs = {
        InputPath = ""
        OutputPath = ""
        Test = false
        Echo = ""
    }

    let rec parseArgs args state =
        match args with
        | "-t" :: next :: rest when next.StartsWith("-") ->
            parseArgs (next :: rest) { state with Test = true }
        | "--test" :: next :: rest when next.StartsWith("-") ->
            parseArgs (next :: rest) { state with Test = true }
        | "-e" :: value :: rest ->
            parseArgs rest { state with Echo = value }
        | "--echo" :: value :: rest ->
            parseArgs rest { state with Echo = value }
        | "-i" :: value :: rest ->
            parseArgs rest { state with InputPath = value }
        | "-o" :: value :: rest ->
            parseArgs rest { state with OutputPath = value }
        | unknown :: _ ->
            failwith $"Unknown argument: {unknown}. Valid arguments are: -i, --input, -o, --output, -t, --test, -e, --echo"
        | [] ->
            state

let args =
    fsi.CommandLineArgs
    |> Array.skip 1
    |> Array.toList
    |> fun args -> CLIArgs.parseArgs args CLIArgs.defaultArgs

if args.Test then
    printfn "Test mode enabled."

printfn $"Echo: {args.Echo}"

printfn $"InputPath: {args.InputPath}"

printfn $"OutputPath: {args.OutputPath}"

let test_package =
    Setup.ValidationPackage(
        metadata = Setup.Metadata(PACKAGE_METADATA, AVPRIndex.Frontmatter.FSharpFrontmatter),
        CriticalValidationCases = [
            test "yes" {Expect.equal 1 1 "yes"}
        ]
    )

open System.Collections.Generic

test_package
|> Execute.ValidationPipeline(
    basePath = System.Environment.CurrentDirectory,
    Payload = Dictionary<string,obj>([
        KeyValuePair("some", box "payload")
        KeyValuePair(
            "inner", 
            box (
                Dictionary<string,obj>([
                    KeyValuePair("inner?", box "yes")
                ])
            )
        )
        KeyValuePair("integer", box 42)
    ])
)

printfn "If you can read this in your console, you successfully executed test package v7.0.0!" 