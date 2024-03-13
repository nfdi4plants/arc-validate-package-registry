#r "nuget: AVPRIndex, 0.0.7"

open AVPRIndex
open System.IO
open System.Text.Json

JsonSerializer.Serialize(
    value = AVPRRepo.getStagedPackages(),
    options = JsonSerializerOptions(WriteIndented = true)
)
|> fun json -> File.WriteAllText("avpr-preview-index.json", json)
