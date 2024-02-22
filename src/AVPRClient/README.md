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