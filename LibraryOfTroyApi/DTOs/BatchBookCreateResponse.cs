using System.Collections.Generic;

using Newtonsoft.Json;

namespace LibraryOfTroyApi.DTOs;

public record BatchBookCreateResponse {
    [JsonProperty ( "totalRequested" )]
    public int TotalRequested { get; set; }

    [JsonProperty ( "successCount" )]
    public int SuccessCount { get; set; }

    [JsonProperty ( "failedCount" )]
    public int FailedCount { get; set; }

    [JsonProperty ( "errors", NullValueHandling = NullValueHandling.Ignore )]
    public List<string> Errors { get; set; }
}
