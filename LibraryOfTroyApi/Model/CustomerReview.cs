using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using LibraryOfTroyApi.DTOs;

namespace LibraryOfTroyApi.Model;

[Table ( "Reviews" )]
public partial record CustomerReview {
    [Key]
    public required Guid Id { get; init; }

    [Required]
    public DateTime ReviewDateTime { get; init; }

    [Required]
    public required Guid BookId { get; init; }

    public Book? Book { get; init; }

    [Required]
    [MaxLength ( 64 )]
    public required string ReviewerDisplayName { get; init; }

    [Required]
    [MaxLength ( 4096 )]
    public required string Review { get; init; }

    [Required]
    [Range ( 0, 5, ErrorMessage = "Star rating must be between 0 and 5" )]
    public int Rating { get; init; }


    public static class Factory {
        public static CustomerReview FromReviewCreateRequest ( Guid bookId, ReviewCreateRequest request ) {
            return new CustomerReview ( ) {
                Id = Guid.NewGuid ( ),
                BookId = bookId,
                ReviewDateTime = DateTime.Now,
                ReviewerDisplayName = request.ReviewerDisplayName,
                Review = request.Review,
                Rating = request.Rating,
            };
        }
    }
}
