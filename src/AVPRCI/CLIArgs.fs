module CLIArgs

open Argu

type PublishArgs =
    | [<AltCommandLine("-d")>] Dry_Run
    | [<ExactlyOnce; AltCommandLine("-k")>] API_Key of string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Dry_Run          -> """Optional | Default = false | Dry run mode enabled. This will only print a preview of the changes that would be pushed to the package database."""
            | API_Key _        -> """Required | API key for the package database."""


[<HelpFlags([|"--help"; "-h"|])>]
type EntryCommand=
    // Parameters
    | [<Unique>] Verbose
    | [<Unique>] Repo_Root_Path of string

    //Commands
    | [<Unique; CliPrefix(CliPrefix.None); AltCommandLine("p")>] Publish of ParseResults<PublishArgs>


    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Verbose           -> "Optional | Default = false | Use verbose error messages (with full error stack)."
            | Repo_Root_Path _  -> "Optional | Default = '.' | Path to the root of the repository."
            | Publish _         -> "Subcommand for publishing the staged packages to the package database."

    static member createParser() =

        let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some System.ConsoleColor.Red)

        ArgumentParser.Create<EntryCommand>(programName = "avpr-ci", errorHandler = errorHandler)
