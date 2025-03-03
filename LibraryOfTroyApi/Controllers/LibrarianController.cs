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
[Authorize ( Roles = "Librarian,Admin" )]
public class LibrarianController ( LibraryDbContext context, ILogger<LibrarianController> logger ) : ControllerBase {
    // In LibrarianController.cs
    [HttpGet ( "test" )]
    [Authorize ( Roles = "Librarian,Admin" )]
    public IActionResult TestLibrarian ( ) {
        return Ok ( new { message = "Librarian controller is only accessible to librarians and admins" } );
    }
    /// <summary>
    /// Creates a new book in the library system.
    /// </summary>
    /// <param name="request">A BookCreateRequest object containing the book details</param>
    /// <returns>An HTTP 200 OK response if successful, or an appropriate error response if not</returns>
    /// <response code="200">Returns when the book was successfully created</response>
    /// <response code="400">Returns when the request is null or invalid</response>
    /// <response code="409">Returns when a book with the same ID already exists</response>
    /// <response code="500">Returns when an unexpected error occurs during processing</response>
    [HttpPost ( "Books" )]
    [ProducesResponseType ( typeof ( BookDetailResponse ), StatusCodes.Status200OK )]
    [ProducesResponseType ( StatusCodes.Status400BadRequest )]
    [ProducesResponseType ( StatusCodes.Status404NotFound )]
    [ProducesResponseType ( StatusCodes.Status500InternalServerError )]
    public async Task<IActionResult> CreateBook ( [FromBody] BookCreateRequest request ) {
        Debug.WriteLine ( request );
        if ( request is null ) {
            logger.LogCritical ( "Caller tried to CreateBook with a null BookCreateRequest body." );
            return BadRequest ( "Must pass a book object in the request body." );
        }

        try {
            await context.AddAsync<Book> ( Book.Factory.FromCreateRequest ( request ) );
            await context.SaveChangesAsync ( );
            return Ok ( );
        } catch ( DbUpdateException ex ) {
            logger.LogTrace ( $"Handled Exception: \n{ex.Message}\nGuid Conflict" );
            return Conflict ( "A book with that ID already exists" );
        } catch ( Exception ex ) {
            logger.LogError ( $"An unknown exception occured:\n{ex.Message}\nRequest: {request}" );
            return StatusCode ( 500, "An unknown error occured." );
        }
    }

    /// <summary>
    /// Creates multiple books in the library system in a single batch operation.
    /// </summary>
    /// <param name="requests">A list of BookCreateRequest objects containing the details for each book</param>
    /// <returns>An HTTP 200 OK response with a summary if successful, or an appropriate error response if not</returns>
    /// <response code="200">Returns when all books were successfully created</response>
    /// <response code="400">Returns when the request is null or invalid</response>
    /// <response code="409">Returns when one or more books with the same ID already exist</response>
    /// <response code="500">Returns when an unexpected error occurs during processing</response>
    [HttpPut ( "Books" )]
    [ProducesResponseType ( typeof ( BatchBookCreateResponse ), StatusCodes.Status200OK )]
    [ProducesResponseType ( StatusCodes.Status400BadRequest )]
    [ProducesResponseType ( StatusCodes.Status409Conflict )]
    [ProducesResponseType ( StatusCodes.Status500InternalServerError )]
    public async Task<IActionResult> CreateMultipleBooks ( [FromBody] List<BookCreateRequest> requests ) {
        if ( requests == null || !requests.Any ( ) ) {
            logger.LogCritical ( "Caller tried to CreateMultipleBooks with a null or empty list." );
            return BadRequest ( "Must pass a non-empty list of book objects in the request body." );
        }

        var response = new BatchBookCreateResponse
    {
            TotalRequested = requests.Count,
            SuccessCount = 0,
            FailedCount = 0,
            Errors = new List<string>()
        };

        try {
            foreach ( var request in requests ) {
                try {
                    if ( request == null ) {
                        response.FailedCount++;
                        response.Errors.Add ( "One of the book entries was null" );
                        continue;
                    }

                    await context.AddAsync<Book> ( Book.Factory.FromCreateRequest ( request ) );
                    response.SuccessCount++;
                } catch ( Exception ex ) {
                    response.FailedCount++;
                    response.Errors.Add ( $"Failed to create book '{request?.Title}': {ex.Message}" );
                    logger.LogError ( $"Error creating book in batch operation: {ex.Message}" );
                }
            }

            // Save all successful entries at once
            await context.SaveChangesAsync ( );

            return Ok ( response );
        } catch ( DbUpdateException ex ) {
            logger.LogTrace ( $"Handled Exception in batch operation: \n{ex.Message}" );
            return Conflict ( "One or more books with these IDs already exist" );
        } catch ( Exception ex ) {
            logger.LogError ( $"An unknown exception occurred during batch book creation:\n{ex.Message}" );
            return StatusCode ( 500, "An unknown error occurred." );
        }
    }

    /// <summary>
    /// Updates an existing book with new information.
    /// </summary>
    /// <param name="bookId">The unique GUID identifier of the book to update</param>
    /// <param name="request">A BookUpdateRequest object containing the updated book details</param>
    /// <returns>An HTTP 200 OK response if successful, or an appropriate error response if not</returns>
    /// <response code="200">Returns when the book was successfully updated</response>
    /// <response code="400">Returns when the bookId is invalid or the request is null</response>
    /// <response code="404">Returns when no book with the specified ID exists</response>
    /// <response code="500">Returns when an unexpected error occurs during processing</response>
    [ProducesResponseType ( StatusCodes.Status200OK )]
    [ProducesResponseType ( StatusCodes.Status400BadRequest )]
    [ProducesResponseType ( StatusCodes.Status404NotFound )]
    [HttpPatch ( "Books/{bookId}" )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "Performance", "HAA0101:Array allocation for params parameter", Justification = "<Not yet optimize>" )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "Performance", "HAA0601:Value type to reference type conversion causing boxing allocation", Justification = "<Not yet optimize>" )]
    public async Task<IActionResult> UpdateBook ( string bookId, [FromBody] BookUpdateRequest request ) {
        if ( request is null ) {
            logger.LogTrace ( "Caller tried to UpdateBook with a null request." );
            return BadRequest ( "Must include a BookUpdateRequest in the request body." );
        }

        if ( !Guid.TryParse ( bookId, out Guid bookIdGuid ) ) {
            logger.LogTrace ( "Called tried to UpdateBook with an invalid Guid." );
            return BadRequest ( "Invalid Guid." );
        }

        try {
            Book? originalBook = await context.FindAsync<Book>(bookIdGuid);
            if ( originalBook is null ) {
                logger.LogTrace ( "Caller tried to UpdateBook on a non-existant Guid." );
                return NotFound ( $"A book with Guid\"{bookIdGuid.ToString ( )}\" was not found to update." );
            }

            context.Entry ( originalBook ).State = EntityState.Detached;
            Book newBook = Book.Factory.FromUpdateRequest(request, originalBook);

            context.Update<Book> ( newBook );
            await context.SaveChangesAsync ( );

            return Ok ( );
        } catch ( Exception ex ) {
            logger.LogError ( $"An unknown exception occurred:\n{ex}" );
            return StatusCode ( 500, "An unknown error occured." );
        }
    }

    /// <summary>
    /// Deletes a book from the library system.
    /// </summary>
    /// <param name="bookId">The unique GUID identifier of the book to delete</param>
    /// <returns>An HTTP 200 OK response if successful, or an appropriate error response if not</returns>
    /// <response code="200">Returns when the book was successfully deleted</response>
    /// <response code="400">Returns when the bookId is null or not a valid GUID</response>
    /// <response code="404">Returns when no book with the specified ID exists</response>
    /// <response code="500">Returns when an unexpected error occurs during processing</response>
    [HttpDelete ( "Books/{bookId}" )]
    [ProducesResponseType ( StatusCodes.Status200OK )]
    [ProducesResponseType ( StatusCodes.Status400BadRequest )]
    [ProducesResponseType ( StatusCodes.Status404NotFound )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "Performance", "HAA0101:Array allocation for params parameter", Justification = "<Not yet optimize>" )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "Performance", "HAA0601:Value type to reference type conversion causing boxing allocation", Justification = "<Not yet optimize>" )]
    public async Task<IActionResult> DeleteBook ( string bookId ) {
        if ( bookId is null ) {
            logger.LogTrace ( "Caller tried to DeleteBook with a null request." );
            return BadRequest ( "Must include a Guid in the request." );
        }

        if ( !Guid.TryParse ( bookId, out Guid bookIdGuid ) ) {
            logger.LogTrace ( "Called tried to UpdateBook with an invalid Guid." );
            return BadRequest ( "Invalid Guid." );
        }

        try {
            Book? originalBook = await context.FindAsync<Book>(bookIdGuid);
            if ( originalBook is null ) {

                logger.LogTrace ( "Caller tried to DeleteBook on a non-existant Guid." );
                return NotFound ( $"A book with Guid\"{bookIdGuid.ToString ( )}\" was not found to delete." );
            }

            context.Remove<Book> ( originalBook );

            await context.SaveChangesAsync ( );

            return Ok ( );
        } catch ( Exception ex ) {
            logger.LogError ( $"An unknown exception occurred:\n{ex}" );
            return StatusCode ( 500, "An unknown error occured." );
        }
    }

    /// <summary>
    /// Checks in a previously checked out book.
    /// </summary>
    /// <param name="bookId">The unique GUID identifier of the book to check in</param>
    /// <param name="request">A CheckInRequest object (optional)</param>
    /// <returns>Confirmation of successful check-in or appropriate error response</returns>
    /// <response code="200">Returns when the book was successfully checked in</response>
    /// <response code="400">Returns when the bookId is invalid</response>
    /// <response code="404">Returns when no book with the specified ID exists or no active checkout was found</response>
    /// <response code="500">Returns when an unexpected error occurs during processing</response>
    [HttpPost ( "Books/{bookId}/CheckIn" )]
    [ProducesResponseType ( StatusCodes.Status200OK )]
    [ProducesResponseType ( StatusCodes.Status400BadRequest )]
    [ProducesResponseType ( StatusCodes.Status404NotFound )]
    [ProducesResponseType ( StatusCodes.Status500InternalServerError )]
    public async Task<IActionResult> CheckInBook ( string bookId, [FromBody] CheckInRequest? request = null ) {
        if ( string.IsNullOrEmpty ( bookId ) ) {
            logger.LogTrace ( "Librarian tried to CheckInBook with a null Guid." );
            return BadRequest ( "Must include a valid book ID in the request URI." );
        }

        if ( !Guid.TryParse ( bookId, out Guid bookIdGuid ) ) {
            logger.LogTrace ( "Librarian tried to CheckInBook with an invalid Guid." );
            return BadRequest ( "Invalid book ID format." );
        }

        try {
            // Find the book including its checkout events
            var book = await context.Books
            .Include(b => b.CheckOutEvents)
            .FirstOrDefaultAsync(b => b.Id == bookIdGuid);

            if ( book is null ) {
                logger.LogTrace ( $"Librarian tried to check in non-existent book ID: {bookIdGuid}" );
                return NotFound ( $"Book with ID {bookIdGuid} not found." );
            }

            // Find the latest checkout event without a return date
            var latestCheckout = book.CheckOutEvents
            .Where(co => co.ReturnDateTime == null)
            .OrderByDescending(co => co.CheckoutDateTime)
            .FirstOrDefault();

            if ( latestCheckout is null ) {
                logger.LogTrace ( $"Librarian tried to check in book ID: {bookIdGuid} which is not checked out" );
                return NotFound ( $"No active checkout found for book with ID {bookIdGuid}." );
            }

            // Handle the immutable record update by removing the old record and adding an updated one
            var returnDateTime = request?.ReturnDateTime ?? DateTime.Now;

            // Create a new record with the return date set
            var updatedCheckout = CheckOutEvent.Factory.WithReturnDate(latestCheckout, returnDateTime);

            // Remove the old record and add the new one
            context.CheckOuts.Remove ( latestCheckout );
            await context.CheckOuts.AddAsync ( updatedCheckout );

            await context.SaveChangesAsync ( );

            return Ok ( $"Book successfully checked in on {returnDateTime}." );
        } catch ( Exception ex ) {
            logger.LogError ( $"An error occurred while checking in a book: {ex}" );
            return StatusCode ( 500, "An unexpected error occurred while processing the check-in." );
        }
    }

    /// <summary>
    /// Gets all books currently borrowed from the library with their status details
    /// </summary>
    /// <param name="includeReturned">Whether to include already returned books (default: false)</param>
    /// <param name="count">The maximum number of items to return (default: 50)</param>
    /// <param name="offset">The number of items to skip (default: 0)</param>
    /// <returns>A list of borrowed books with checkout details</returns>
    /// <response code="200">Returns the list of borrowed books</response>
    /// <response code="500">Returns when an unexpected error occurs during processing</response>
    [HttpGet ( "Books/Borrowed" )]
    [ProducesResponseType ( typeof ( List<BorrowedBookResponse> ), StatusCodes.Status200OK )]
    [ProducesResponseType ( StatusCodes.Status500InternalServerError )]
    public async Task<IActionResult> GetBorrowedBooks (
        [FromQuery] bool includeReturned = false,
        [FromQuery] int count = 50,
        [FromQuery] int offset = 0 ) {
        try {
            // Debug output
            var checkoutsDebug = await context.CheckOuts.ToListAsync();
            logger.LogWarning ( $"Total checkouts: {checkoutsDebug.Count}" );
            logger.LogWarning ( $"With Book: {checkoutsDebug.Count ( c => c.Book != null )}" );
            logger.LogWarning ( $"With Customer: {checkoutsDebug.Count ( c => c.Customer != null )}" );

            // Query for active checkouts (and optionally returned books)
            IQueryable<CheckOutEvent> query = context.CheckOuts
                .Include(co => co.Book)
                //.Include(co => co.Customer) // EF Core will automatically do a left join

                .AsQueryable();

            // Filter only for active checkouts if not including returned
            if ( !includeReturned ) {
                query = query.Where ( co => co.ReturnDateTime == null );
            }

            // Apply pagination
            var checkouts = await query
            .OrderByDescending(co => co.CheckoutDateTime)
            .Skip(offset)
            .Take(count)
            .ToListAsync();

            // Calculate current date for overdue status
            var today = DateTime.Now;

            // Map to response DTO
            var response = checkouts.Select(co =>
            {
                // Calculate due date (14 days after checkout by default)
                var dueDate = co.CheckoutDateTime.AddDays(14);
                
                // Check if overdue
                bool isOverdue = co.ReturnDateTime == null && today > dueDate;
                
                // Create response with anonymous customer handling
                return new BorrowedBookResponse
                {
                    CheckoutId = co.Id,
                    Book = BookDetailResponse.Factory.FromBook(co.Book),
                    CustomerName = co.Customer?.UserName ?? "Anonymous", // Handle null customer
                    CustomerId = co.CustomerId,
                    CheckoutDate = co.CheckoutDateTime,
                    ReturnDate = co.ReturnDateTime,
                    DueDate = dueDate,
                    IsOverdue = isOverdue,
                    DaysOverdue = isOverdue ? (int)(today - dueDate).TotalDays : 0
                };
            }).ToList();

            return Ok ( response );
        } catch ( Exception ex ) {
            logger.LogError ( $"An error occurred while getting borrowed books: {ex}" );
            return StatusCode ( 500, "An unexpected error occurred while processing the request." );
        }
    }

    /// <summary>
    /// Checks out a book to a customer by a librarian.
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
            logger.LogTrace ( "Librarian tried to CheckOutBook with a null request." );
            return BadRequest ( "Must include checkout details in the request body." );
        }

        if ( string.IsNullOrEmpty ( bookId ) ) {
            logger.LogTrace ( "Librarian tried to CheckOutBook with a null Guid." );
            return BadRequest ( "Must include a valid book ID in the request URI." );
        }

        if ( !Guid.TryParse ( bookId, out Guid bookIdGuid ) ) {
            logger.LogTrace ( "Librarian tried to CheckOutBook with an invalid Guid." );
            return BadRequest ( "Invalid book ID format." );
        }

        try {
            // Find the book including its checkout events
            var book = await context.Books
                .Include(b => b.CheckOutEvents)
                .FirstOrDefaultAsync(b => b.Id == bookIdGuid);

            if ( book is null ) {
                logger.LogTrace ( $"Librarian tried to check out non-existent book ID: {bookIdGuid}" );
                return NotFound ( $"Book with ID {bookIdGuid} not found." );
            }

            // Check if the book is available
            if ( !book.IsAvailable ) {
                logger.LogTrace ( $"Librarian tried to check out book ID: {bookIdGuid} which is not available" );
                return Conflict ( $"Book with ID {bookIdGuid} is not available for checkout." );
            }

            // Create and save the checkout event
            var checkOutEvent = CheckOutEvent.Factory.FromCheckOutRequest(bookIdGuid, request);
            await context.CheckOuts.AddAsync ( checkOutEvent );
            await context.SaveChangesAsync ( );

            return Ok ( "Book successfully checked out." );
        } catch ( Exception ex ) {
            logger.LogError ( $"An error occurred while checking out a book: {ex}" );
            return StatusCode ( 500, "An unexpected error occurred while processing the checkout." );
        }
    }
}
