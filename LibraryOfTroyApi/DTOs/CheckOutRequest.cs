
using Newtonsoft.Json;

namespace LibraryOfTroyApi.DTOs;

public record CheckOutRequest {
    [JsonRequired]
    [JsonProperty ( "customerId" )]
    public required Guid CustomerId { get; init; }
}
