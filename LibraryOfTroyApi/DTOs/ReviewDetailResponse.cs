using LibraryOfTroyApi.Model;

using Newtonsoft.Json;

namespace LibraryOfTroyApi.DTOs;

public record ReviewDetailResponse {
    [JsonRequired]
    [JsonConverter ( typeof ( TypeScriptGuidConverter ) )]
    public required Guid Id { get; init; }

    [JsonRequired]
    [JsonConverter ( typeof ( DateOnlyJsonConverter ) )]
    [JsonProperty ( "reviewDateTime" )]
    public required DateTime ReviewDateTime { get; init; }

    [JsonRequired]
    [JsonProperty ( "reviewerDisplayName" )]
    public required string ReviewerDisplayName { get; init; }

    [JsonRequired]
    [JsonProperty ( "review" )]
    public required string Review { get; init; }

    [JsonRequired]
    [JsonProperty ( "rating" )]
    public required int Rating { get; init; }

    public static class Factory {
        public static ReviewDetailResponse FromCustomerReview ( CustomerReview review ) {
            return new ReviewDetailResponse ( ) {
                Id = review.Id,
                ReviewDateTime = review.ReviewDateTime,
                Review = review.Review,
                ReviewerDisplayName = review.ReviewerDisplayName,
                Rating = review.Rating,
            };
        }
    }
}
