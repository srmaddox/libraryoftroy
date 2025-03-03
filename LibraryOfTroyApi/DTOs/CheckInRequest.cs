using Newtonsoft.Json;

namespace LibraryOfTroyApi.DTOs;

public record CheckInRequest {
    // Optional property to specify check-in date/time (defaults to current time if not provided)
    [JsonProperty ( "returnDateTime", NullValueHandling = NullValueHandling.Ignore )]
    public DateTime? ReturnDateTime { get; init; }

    // Optional notes about the condition of the book upon return
    [JsonProperty ( "notes", NullValueHandling = NullValueHandling.Ignore )]
    public string? Notes { get; init; }
}