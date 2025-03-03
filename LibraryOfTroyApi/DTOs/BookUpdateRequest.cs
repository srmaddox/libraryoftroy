using Newtonsoft.Json;

namespace LibraryOfTroyApi.DTOs;

public record BookUpdateRequest {
    //==========================================================
    //= Optional Properties
    //==========================================================
    [JsonProperty ( "title", NullValueHandling = NullValueHandling.Ignore )]
    public required string Title { get; init; }

    [JsonProperty ( "author", NullValueHandling = NullValueHandling.Ignore )]
    public required string Author { get; init; }

    [JsonProperty ( "description", NullValueHandling = NullValueHandling.Ignore )]
    public string? Description { get; init; }

    [JsonProperty ( "publisher", NullValueHandling = NullValueHandling.Ignore )]
    public string? Publisher { get; init; }

    [JsonProperty ( "publicationDate", NullValueHandling = NullValueHandling.Ignore )]
    [JsonConverter ( typeof ( DateOnlyJsonConverter ) )]
    public DateOnly? PublicationDate { get; init; }

    [JsonProperty ( "category", NullValueHandling = NullValueHandling.Ignore )]
    public string? Category { get; init; }

    [JsonProperty ( "isbn", NullValueHandling = NullValueHandling.Ignore )]
    public string? ISBN { get; init; }

    [JsonProperty ( "pageCount", NullValueHandling = NullValueHandling.Ignore )]
    public int? PageCount { get; init; }

    //==========================================================
    //= Cover Image
    //==========================================================
    [JsonProperty ( "coverImageUrl", NullValueHandling = NullValueHandling.Ignore )]
    public string? CoverImageUrl { get; init; }

    [JsonProperty ( "coverImageAltText", NullValueHandling = NullValueHandling.Ignore )]
    public string? CoverImageAltText { get; init; }
}
