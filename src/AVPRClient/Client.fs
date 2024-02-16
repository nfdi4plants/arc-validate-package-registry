namespace AVPRClient

module AVPR_V1 =

    let getAllPackages() =
        Globals.Client_V1.GetAllPackages()
        |> Async.AwaitTask
        |> Async.RunSynchronously