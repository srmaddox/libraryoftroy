using LibraryOfTroyApi.DTOs;
using LibraryOfTroyApi.Model;
using System.Diagnostics;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LibraryOfTroyApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LibraryOfTroyApi.Controllers;
[Route ( "api/[controller]" )]
[ApiController]
[Authorize ( Roles = "Customer,Librarian,Admin" )]
public class CustomerController ( LibraryDbContext context, ILogger<CustomerController> logger ) : ControllerBase {
    [HttpGet ( "test" )]
    [Authorize ( Roles = "Customer,Librarian,Admin" )]
    public IActionResult TestCustomer ( ) {
        return Ok ( new { message = "Customer controller is accessible to authenticated customers, librarians, and admins" } );
    }
    /// <summary>
    /// Adds a new customer review to a book.
    /// </summary>
    /// <param name="bookId">The unique GUID identifier of the book to review</param>
    /// <param name="request">A ReviewCreateRequest object containing the review details</param>
    /// <returns>The newly created review if successful, or an appropriate error response</returns>
    /// <response code="200">Returns the created review</response>
    /// <response code="400">Returns when the bookId is invalid or the request is null</response>
    /// <response code="404">Returns when no book with the specified ID exists</response>
    /// <response code="500">Returns when an unexpected error occurs during processing</response>
    [HttpPost ( "Books/{bookId}/Reviews" )]
    [ProducesResponseType ( typeof ( ReviewDetailResponse ), StatusCodes.Status200OK )]
    [ProducesResponseType ( StatusCodes.Status400BadRequest )]
    [ProducesResponseType ( StatusCodes.Status404NotFound )]
    [ProducesResponseType ( StatusCodes.Status500InternalServerError )]
    public async Task<IActionResult> AddBookReview ( string bookId, [FromBody] ReviewCreateRequest request ) {
        if ( request is null ) {
            logger.LogTrace ( "Caller tried to AddBookReview with a null request." );
            return BadRequest ( "Must include a review in the request body." );
        }

        if ( string.IsNullOrEmpty ( bookId ) ) {
            logger.LogTrace ( "Caller tried to AddBookReview with a null Guid." );
            return BadRequest ( "Must include a valid book ID in the request URI." );
        }

        if ( !Guid.TryParse ( bookId, out Guid bookIdGuid ) ) {
            logger.LogTrace ( "Caller tried to AddBookReview with an invalid Guid." );
            return BadRequest ( "Invalid book ID format." );
        }

        try {
            // Verify the book exists
            if ( await context.Books.FindAsync ( bookIdGuid ) is null ) {
                logger.LogTrace ( $"Caller tried to add a review to non-existent book ID: {bookIdGuid}" );
                return NotFound ( $"Book with ID {bookIdGuid} not found." );
            }

            // Create and save the review
            CustomerReview newReview = CustomerReview.Factory.FromReviewCreateRequest(bookIdGuid, request);
            await context.CustomerReviews.AddAsync ( newReview );
            await context.SaveChangesAsync ( );

            // Return the created review details
            ReviewDetailResponse response = ReviewDetailResponse.Factory.FromCustomerReview(newReview);
            return Ok ( response );
        } catch ( Exception ex ) {
            logger.LogError ( $"An error occurred while adding a review: {ex}" );
            return StatusCode ( 500, "An unexpected error occurred while adding the review." );
        }
    }

    /// <summary>
    /// Gets all books currently checked out by a customer
    /// </summary>
    /// <param name="username">The username of the customer</param>
    /// <returns>A list of books checked out by the customer</returns>
    /// <response code="200">Returns the list of checked out books</response>
    /// <response code="400">Returns when the username is not provided</response>
    /// <response code="404">Returns when the customer is not found</response>
    /// <response code="500">Returns when an unexpected error occurs during processing</response>
    [HttpGet ( "Checkouts" )]
    [ProducesResponseType ( typeof ( List<BookDetailResponse> ), StatusCodes.Status200OK )]
    [ProducesResponseType ( StatusCodes.Status400BadRequest )]
    [ProducesResponseType ( StatusCodes.Status404NotFound )]
    [ProducesResponseType ( StatusCodes.Status500InternalServerError )]
    public async Task<IActionResult> GetCustomerCheckouts ( [FromQuery] string username ) {
        if ( string.IsNullOrEmpty ( username ) ) {
            logger.LogTrace ( "Caller tried to GetCustomerCheckouts with a null or empty username." );
            return BadRequest ( "Username parameter is required." );
        }

        try {
            // Find the customer by username
            var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.UserName == username);

            if ( customer == null ) {
                logger.LogTrace ( $"Customer with username '{username}' not found." );
                return NotFound ( $"Customer with username '{username}' not found." );
            }

            // Get active checkouts for this customer
            var checkedOutBooks = await context.CheckOuts
            .Where(co => co.CustomerId == customer.Id && co.ReturnDateTime == null)
            .Include(co => co.Book)
            .ThenInclude(b => b.CustomerReviews)
            .Select(co => co.Book)
            .ToListAsync();

            // Map to DTOs
            var response = checkedOutBooks
            .Select(book => BookDetailResponse.Factory.FromBook(book))
            .ToList();

            return Ok ( response );
        } catch ( Exception ex ) {
            logger.LogError ( $"An error occurred while getting customer checkouts: {ex}" );
            return StatusCode ( 500, "An unexpected error occurred while processing the request." );
        }
    }

    /// <summary>
    /// Checks out a book to a customer.
    /// </summary>
    /// <param name="bookId">The unique GUID identifier of the book to check out</param>
    /// <param name="request">A CheckOutRequest object containing checkout details</param>
    /// <returns>Confirmation of successful checkout or appropriate error response</returns>
    /// <response code="200">Returns when the book was successfully checked out</response>
    /// <response code="400">Returns when the bookId is invalid or the request is null</response>
    /// <response code="404">Returns when no book with the specified ID exists</response>
    /// <response code="409">Returns when the book is not available for checkout</response>
    /// <response code="500">Returns when an unexpected error occurs during processing</response>
    [HttpPost ( "Books/{bookId}/CheckOut" )]
    [ProducesResponseType ( StatusCodes.Status200OK )]
    [ProducesResponseType ( StatusCodes.Status400BadRequest )]
    [ProducesResponseType ( StatusCodes.Status404NotFound )]
    [ProducesResponseType ( StatusCodes.Status409Conflict )]
    [ProducesResponseType ( StatusCodes.Status500InternalServerError )]
    public async Task<IActionResult> CheckOutBook ( string bookId, [FromBody] CheckOutRequest request ) {
        if ( request is null ) {
            logger.LogTrace ( "Caller tried to CheckOutBook with a null request." );
            return BadRequest ( "Must include checkout details in the request body." );
        }

        if ( string.IsNullOrEmpty ( bookId ) ) {
            logger.LogTrace ( "Caller tried to CheckOutBook with a null Guid." );
            return BadRequest ( "Must include a valid book ID in the request URI." );
        }

        if ( !Guid.TryParse ( bookId, out Guid bookIdGuid ) ) {
            logger.LogTrace ( "Caller tried to CheckOutBook with an invalid Guid." );
            return BadRequest ( "Invalid book ID format." );
        }

        try {
            // Find the book including its checkout events
            var book = await context.Books
            .Include(b => b.CheckOutEvents)
            .FirstOrDefaultAsync(b => b.Id == bookIdGuid);

            if ( book is null ) {
                logger.LogTrace ( $"Caller tried to check out non-existent book ID: {bookIdGuid}" );
                return NotFound ( $"Book with ID {bookIdGuid} not found." );
            }

            // Check if the book is available
            if ( !book.IsAvailable ) {
                logger.LogTrace ( $"Caller tried to check out book ID: {bookIdGuid} which is not available" );
                return Conflict ( $"Book with ID {bookIdGuid} is not available for checkout." );
            }

            // Create and save the checkout event
            var checkOutEvent = new CheckOutEvent
        {
                Id = Guid.NewGuid(),
                BookId = bookIdGuid,
                CustomerId = request.CustomerId, // This assumes the request contains a CustomerId
                CheckoutDateTime = DateTime.Now,
                ReturnDateTime = null // No return date since we're checking out
            };

            await context.CheckOuts.AddAsync ( checkOutEvent );
            await context.SaveChangesAsync ( );

            return Ok ( "Book successfully checked out." );
        } catch ( Exception ex ) {
            logger.LogError ( $"An error occurred while checking out a book: {ex}" );
            return StatusCode ( 500, "An unexpected error occurred while processing the checkout." );
        }
    }
}