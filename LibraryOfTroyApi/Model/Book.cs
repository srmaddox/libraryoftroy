using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryOfTroyApi.Model;

[Table ( "Books" )]
public partial record Book {
    [Key]
    public required Guid Id { get; init; }

    [Required]
    [MaxLength ( 255 )]
    public required string Title { get; init; }

    [Required]
    [MaxLength ( 255 )]
    public required string Author { get; init; }

    [MaxLength ( 4096 )]
    public required string Description { get; init; }

    [MaxLength ( 255 )]
    public required string Publisher { get; init; }
    public required DateOnly PublicationDate { get; init; }
    [MaxLength ( 255 )]
    public required string Category { get; init; }
    [MaxLength ( 17 )]      // Include dashes.
    [RegularExpression ( @"^(?:ISBN (?:-1[ 03 ])?:? )?(?=[-0-9 ]{17}$|[-0-9X]{13}$| [ 0 - 9X ]{ 10}$)(?: 97 [ 89 ] [ -] )? [ 0 - 9 ]{ 1,5}[ -] [ 0 - 9 ] + [ -] [ 0 - 9 ] + [ -] [ 0 - 9X ]$", ErrorMessage = "Invalid ISBN Format", MatchTimeoutInMilliseconds = 5000 )]
    public required string ISBN { get; init; }

    public int PageCount { get; init; }

    [MaxLength ( 255 )]
    public string? CoverImageUrl { get; init; }

    [MaxLength ( 255 )]
    public string? CoverImageAltText { get; init; }

    // New fields for phonetic matching
    [MaxLength ( 255 )]
    public string TitleSoundex { get; init; }

    [MaxLength ( 255 )]
    public string TitleMetaphone { get; init; }

    //= Navigation Properties
    //========================================================================================================
    public virtual ICollection<CustomerReview> CustomerReviews { get; init; } = new List<CustomerReview> ( );
    public virtual ICollection<CheckOutEvent> CheckOutEvents { get; init; } = new List<CheckOutEvent> ( );

    [NotMapped]
    public bool IsAvailable {
        get {
            // If there are no checkouts, the book is available
            if ( !CheckOutEvents.Any ( ) )
                return true;

            // Get the latest checkout event for this book
            var latestCheckOut = CheckOutEvents
                .OrderByDescending(co => co.CheckoutDateTime)
                .FirstOrDefault();

            // Book is available if the latest checkout has a return date
            return latestCheckOut?.ReturnDateTime != null;
        }
    }

}

