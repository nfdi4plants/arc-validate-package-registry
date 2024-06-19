module Domain

open System.Text.Json

let jsonSerializerOptions = JsonSerializerOptions(WriteIndented = true)

type AVPRClient.ValidationPackage with
    
    static member toJson (p: AVPRClient.ValidationPackage) = 
        JsonSerializer.Serialize(p, jsonSerializerOptions)

    static member printJson (p: AVPRClient.ValidationPackage) = 
        let json = AVPRClient.ValidationPackage.toJson p
        printfn ""
        printfn $"Package info:{System.Environment.NewLine}{json}"
        printfn ""