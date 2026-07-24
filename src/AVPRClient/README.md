# AVPR Client

.NET client library for https://avpr.nfdi4plants.org.
 
Generated with NSwag based on the OpenAPI specification.

## Usage

### fsharp

```fsharp
open AVPRClient

let client = 
    let httpClient = new System.Net.Http.HttpClient()
    httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey)
    new AVPRClient.Client(httpClient)
```

### csharp

```csharp
using AVPRClient;

var httpClient = new System.Net.Http.HttpClient()
httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey)

var client = new Client(httpClient);
```

### (Re)generate client

NSwag is pinned as a repository-local .NET tool. Restore it once after cloning
or whenever `.config/dotnet-tools.json` changes:

```shell
dotnet tool restore
```

Start the registry service locally with its HTTP launch profile so that its
OpenAPI document is available at
`http://localhost:5099/swagger/v1/swagger.json`:

```shell
dotnet run --project src/PackageRegistryService/PackageRegistryService.csproj --launch-profile http
```

Then, from the repository root in another terminal, regenerate the client with:

```shell
dotnet tool run nswag -- run src/AVPRClient/nswag.json
```

The checked-in `nswag.json` fixes the input URL, namespace, client name,
generation mode, and output path. To use another running service without
editing the configuration, override `OpenApiUrl`:

```shell
dotnet tool run nswag -- run src/AVPRClient/nswag.json /variables:OpenApiUrl=http://localhost:5001/swagger/v1/swagger.json
```

Review the generated diff and build/test the client before committing it. Do
not regenerate against the production API when implementing an API schema
change, because production will still expose the previous schema.
