module AVPRCI.Tests.ClientMappingsTests

open System
open System.Net
open System.Net.Http
open System.Threading
open System.Threading.Tasks
open AVPR.Staging
open Xunit

type private RecordingHandler() =
    inherit HttpMessageHandler()

    let mutable requestedPath = ""

    member _.RequestedPath = requestedPath

    override _.SendAsync(request: HttpRequestMessage, _: CancellationToken) =
        requestedPath <- request.RequestUri.AbsolutePath
        let response =
            new HttpResponseMessage(
                HttpStatusCode.OK,
                Content = new StringContent("""[{"Name":"published","Version":"1.2.3"}]""")
            )
        Task.FromResult(response)

let private releaseDate =
    DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero)

let private stagedPackage () =
    StagedValidationPackage.fromFile
        "fixtures/Frontmatter/Comment/valid@2.0.0.fsx"
        releaseDate

[<Fact>]
let ``publication mapping preserves metadata content and nested CWL values`` () =
    let staged = stagedPackage ()
    let actual =
        ClientMappings.toValidationPackage releaseDate staged

    Assert.Equal(staged.Metadata.Name, actual.Name)
    Assert.Equal(staged.Metadata.PreReleaseVersionSuffix, actual.PreReleaseVersionSuffix)
    Assert.Equal(staged.Metadata.BuildMetadataVersionSuffix, actual.BuildMetadataVersionSuffix)
    Assert.Equal<byte array>(NormalizedContent.fromFile staged.RepoPath, actual.PackageContent)
    Assert.Equal(releaseDate, actual.ReleaseDate)
    Assert.Equal(2, actual.Authors.Count)
    Assert.Equal(2, actual.Tags.Count)
    Assert.Equal(staged.Metadata.Inputs.Length, actual.Inputs.Count)

    let firstInput = actual.Inputs |> Seq.head
    Assert.Equal("input", firstInput.Id)
    Assert.Equal(AVPRClient.CommandInputType.String_, firstInput.Type)
    Assert.Equal("--input", firstInput.InputBinding.Prefix)

[<Fact>]
let ``published identity comparison uses canonical full SemVer`` () =
    let staged = stagedPackage ()
    let published =
        AVPRClient.ValidationPackageIdentity(
            Name = staged.Metadata.Name,
            Version = StagedValidationPackage.getSemanticVersionString staged
        )

    Assert.True(ClientMappings.identityEquals published staged)

    published.Version <- "2.0.0+different"
    Assert.False(ClientMappings.identityEquals published staged)

[<Fact>]
let ``content hash mapping chooses cached or direct hash deliberately`` () =
    let staged =
        { stagedPackage () with ContentHash = "CACHED" }

    let cached =
        ClientMappings.toPackageContentHash false staged

    let direct =
        ClientMappings.toPackageContentHash true staged

    Assert.Equal("CACHED", cached.Hash)
    Assert.Equal(ContentHash.ofFile staged.RepoPath, direct.Hash)

[<Fact>]
let ``publication discovery calls the lightweight package index`` () =
    use handler = new RecordingHandler()
    use httpClient = new HttpClient(handler)
    let client = AVPRClient.Client(httpClient)
    client.BaseUrl <- "https://example.org/"

    let identities = API.PublishedPackageDiscovery.get client

    Assert.Equal("/api/v1/package-index", handler.RequestedPath)
    let identity = Assert.Single(identities)
    Assert.Equal("published", identity.Name)
    Assert.Equal("1.2.3", identity.Version)
