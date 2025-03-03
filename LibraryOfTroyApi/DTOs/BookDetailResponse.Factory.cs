using LibraryOfTroyApi.Model;

namespace LibraryOfTroyApi.DTOs;

public partial record BookDetailResponse {
    public static class Factory {
        public static BookDetailResponse FromBook ( Book book, float? searchRank = null, string? rankingMeta = "" ) {
            return new BookDetailResponse ( ) {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                Publisher = book.Publisher,
                PublicationDate = book.PublicationDate,
                Category = book.Category,
                ISBN = book.ISBN,
                PageCount = book.PageCount,
                CoverImageUrl = book.CoverImageUrl,
                CoverImageAltText = book.CoverImageAltText,
                AverageCustomerReview = book.CustomerReviews.Any ( )
                    ? (float) Math.Round ( book.CustomerReviews.Average ( review => review.Rating ), 1 )
                    : -1,
                Available = book.IsAvailable,
                CustomerReviewCount = ( book.CustomerReviews.Count ),
                SearchRank = searchRank,
                RankingMetadata = rankingMeta
            };

        }
    }
}
