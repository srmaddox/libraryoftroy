using Newtonsoft.Json;

namespace LibraryOfTroyApi.DTOs;
public partial record BookDetailResponse {
    //==========================================================
    //= Required Properties
    //==========================================================
    [JsonRequired]
    [JsonConverter ( typeof ( TypeScriptGuidConverter ) )]
    public required Guid Id { get; init; }

    [JsonRequired]
    [JsonProperty ( "title" )]
    public required string Title { get; init; }

    [JsonRequired]
    [JsonProperty ( "author" )]
    public required string Author { get; init; }

    //==========================================================
    //= Optional Properties
    //==========================================================
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

    //==========================================================
    //= Foreign Fields
    //==========================================================
    [JsonProperty ( "available", NullValueHandling = NullValueHandling.Ignore )]
    public bool? Available { get; init; }

    [JsonProperty ( "averageCustomerReview", NullValueHandling = NullValueHandling.Ignore )]
    public float? AverageCustomerReview { get; init; }

    [JsonProperty ( "customerReviewCount", NullValueHandling = NullValueHandling.Ignore )]
    public int? CustomerReviewCount { get; init; }


    //==========================================================
    //= Ranking
    //==========================================================
    [JsonProperty ( "searchRank" )]
    public float? SearchRank { get; set; }
    public string RankingMetadata {  get; set; }
}

