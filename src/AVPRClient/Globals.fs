namespace AVPRClient

open SwaggerProvider

module Globals =

    let [<Literal>] AVPR_SCHEMA_V1 = "https://avpr.nfdi4plants.org/swagger/v1/swagger.json"

    type AVPR = OpenApiClientProvider<AVPR_SCHEMA_V1>

    let Client_V1 = AVPR.Client()