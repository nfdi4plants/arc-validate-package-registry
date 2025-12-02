module CommandHandling

open CLIArgs
open API
open Argu

let handleEntryCommand (verbose:bool) (repo_root: string) command = 
    match command with

    | EntryCommand.Publish subcommand -> 
        if verbose then
            printfn ""
            printfn "Command: publish"
            printfn ""
        API.PublishAPI.publishPendingPackages verbose (repo_root) (subcommand)

    | _ -> failwith $"unrecognized command '{command}"