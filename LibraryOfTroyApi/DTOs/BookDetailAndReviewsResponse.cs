using LibraryOfTroyApi.Model;
using Newtonsoft.Json;

namespace LibraryOfTroyApi.DTOs;

public class BookDetailAndReviewsResponse {
    [JsonRequired]
    [JsonProperty("bookDetails")]
    public required BookDetailResponse BookDetails { get; init; }

    [JsonRequired]
    [JsonProperty("topReviews")]
    public required List<ReviewDetailResponse> TopReviews { get; init; }

    [JsonRequired]
    [JsonProperty("worstReviews")]
    public required List<ReviewDetailResponse> WorstReviews { get; init; }

    public static class Factory {
        public static BookDetailAndReviewsResponse FromBook ( Book book, int topReviewCount = 3 ) {
            List<ReviewDetailResponse> reviewsResults = [];

            foreach ( CustomerReview review in book.CustomerReviews ) {
                reviewsResults.Add ( ReviewDetailResponse.Factory.FromCustomerReview ( review ) );
            }

            // Sort reviews by rating (descending)
            var sortedReviews = reviewsResults.OrderByDescending(r => r.Rating).ToList();

            // Handle the case where we have very few reviews
            if ( sortedReviews.Count <= 1 ) {
                // If only one review, put it in TopReviews
                return new BookDetailAndReviewsResponse ( ) {
                    BookDetails = BookDetailResponse.Factory.FromBook ( book ),
                    TopReviews = sortedReviews,
                    WorstReviews = []
                };
            }

            // If we have more than one review but fewer than 2*topReviewCount
            if ( sortedReviews.Count < 2 * topReviewCount ) {
                // Split reviews in half (rounded up for TopReviews if odd count)
                int halfCount = (int)Math.Ceiling(sortedReviews.Count / 2.0);

                return new BookDetailAndReviewsResponse ( ) {
                    BookDetails = BookDetailResponse.Factory.FromBook ( book ),
                    TopReviews = sortedReviews.Take ( halfCount ).ToList ( ),
                    WorstReviews = sortedReviews.Skip ( halfCount ).ToList ( )
                };
            }

            // Normal case - enough reviews to take topReviewCount for both top and worst
            return new BookDetailAndReviewsResponse ( ) {
                BookDetails = BookDetailResponse.Factory.FromBook ( book ),
                TopReviews = sortedReviews.Take ( topReviewCount ).ToList ( ),
                WorstReviews = sortedReviews.Skip ( Math.Max ( 0, sortedReviews.Count - topReviewCount ) ).ToList ( )
            };
        }
    }
}
