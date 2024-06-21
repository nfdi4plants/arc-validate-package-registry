namespace AVPRIndex

open System
open System.IO
open System.Text
open System.Security.Cryptography

type Hash = 

    // This is the function used as the first point of entry, as it is used when parsing packages that do not exist in the prioduction DB
    // unifying line endings is crucial to ensure that the hash is the same on all platforms

    /// calculates a md5 hash of the given byte array and returns it as a hex string
    static member hashContent (content: byte array) =
        let md5 = MD5.Create()
        content
        |> md5.ComputeHash
        |> Convert.ToHexString

    /// calculates a md5 hash of the given string with line endings unified to `\n` and returns it as a hex string
    static member hashString (content: string) =
        content
        |> fun s -> s.ReplaceLineEndings("\n")
        |> Encoding.UTF8.GetBytes
        |> Hash.hashContent

    /// calculates a md5 hash of the file at the given path with line endings unified to `\n` and returns it as a hex string
    static member hashFile (path: string) =
        path
        |> File.ReadAllText
        |> Hash.hashString
