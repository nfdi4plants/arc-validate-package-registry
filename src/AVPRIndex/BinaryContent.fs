namespace AVPRIndex

open System
open System.IO
open System.Text
open System.Security.Cryptography

type BinaryContent = 
    
    /// reads the content of the file at the given path and ensures that line endings are unified to `\n`
    static member fromString (str: string) =
        str.ReplaceLineEndings("\n")
        |> Encoding.UTF8.GetBytes


    /// reads the content of the file at the given path and ensures that line endings are unified to `\n`
    static member fromFile (path: string) =
        path 
        |> File.ReadAllText
        |> BinaryContent.fromString