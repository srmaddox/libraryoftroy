using LibraryOfTroyApi.Data;
using LibraryOfTroyApi.DTOs;
using LibraryOfTroyApi.Model;

using OneOf;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using LinqKit;
using System.Diagnostics;
using Srm.Soundex;
using LibraryOfTroyApi.Utilities;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace LibraryOfTroyApi.Controllers;
/// <summary>
/// The books controller, providing access to library books and book management functionality.
/// </summary>
/// <remarks>
/// This controller handles all CRUD operations for books in the library system, including
/// listing, searching, retrieval, creation, updates, and deletion.
/// </remarks>

[Route ( "api/[controller]" )]
[ApiController]
public class PublicController ( LibraryDbContext _context, ILogger<PublicController> _logger, IMemoryCache _cache ) : ControllerBase {
    private static readonly string CacheKey = "FeaturedBooks";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(12);

    [HttpGet ( "test" )]
    public IActionResult TestPublic ( ) {
        return Ok ( new { message = "Public controller is accessible to anyone" } );
    }

    [HttpGet ( "featured" )]
    public async Task<IActionResult> GetFeaturedBooks ( ) {
        if ( !_cache.TryGetValue ( CacheKey, out List<Book> featuredBooks ) ) {
            _logger.LogInformation ( "Cache miss for featured books. Fetching new set." );
            featuredBooks = await _context.Books
                .OrderBy ( b => Guid.NewGuid ( ) )
                .Take ( 4 )
                .ToListAsync ( );

            _cache.Set ( CacheKey, featuredBooks, CacheDuration );
        } else {
            _logger.LogInformation ( "Cache hit for featured books." );
        }

        return Ok ( featuredBooks );
    }

    /// <summary>
    /// Retrieves a specific book by its unique identifier.
    /// </summary>
    /// <param name="bookId">The book's unique GUID identifier</param>
    /// <returns>A BookDetailResponse containing the book's details</returns>
    /// <response code="200">Returns the requested book details</response>
    /// <response code="400">Returns when the provided bookId is null or not a valid GUID</response>
    /// <response code="404">Returns when no book with the specified ID exists</response>
    /// <response code="500">Returns when an unexpected error occurs during processing</response>
    [HttpGet ( "Books/{bookId}" )]
    [ProducesResponseType ( typeof ( List<BookDetailResponse> ), StatusCodes.Status200OK )]
    [ProducesResponseType ( StatusCodes.Status400BadRequest )]
    [ProducesResponseType ( StatusCodes.Status404NotFound )]
    [ProducesResponseType ( StatusCodes.Status500InternalServerError )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "Performance", "HAA0101:Array allocation for params parameter", Justification = "<Not yet optimize>" )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "Performance", "HAA0601:Value type to reference type conversion causing boxing allocation", Justification = "<Not yet optimize>" )]
    public async Task<IActionResult> GetBook ( string bookId ) {
        if ( string.IsNullOrEmpty ( bookId ) ) {
            _logger.LogTrace ( "Caller tried to GetBook with a null Guid." );
            return BadRequest ( "Must include a Guid in request URI" );
        }

        if ( !Guid.TryParse ( bookId, null, out Guid bookIdGuid ) ) {
            _logger.LogTrace ( "Caller tried to GetBook with an invalid Guid." );
            return BadRequest ( "Invalid Guid." );
        }

        try {

            if ( await _context.FindAsync<Book> ( bookIdGuid ) is not Book book ) {
                return NotFound ( );
            }

            BookDetailResponse bookDetail = BookDetailResponse.Factory.FromBook( book );

            return Ok ( bookDetail );
        } catch ( Exception ex ) {
            _logger.LogError ( $"Unhandled exception occurred:\n{ex}" );
            return StatusCode ( 500, "An unexpected error occurred." );
        }
    }

    [HttpGet ( "Books/{bookId}/Reviews" )]
    [ProducesResponseType ( typeof ( List<BookDetailResponse> ), StatusCodes.Status200OK )]
    [ProducesResponseType ( StatusCodes.Status400BadRequest )]
    [ProducesResponseType ( StatusCodes.Status404NotFound )]
    [ProducesResponseType ( StatusCodes.Status500InternalServerError )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "Performance", "HAA0101:Array allocation for params parameter", Justification = "<Not yet optimize>" )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "Performance", "HAA0601:Value type to reference type conversion causing boxing allocation", Justification = "<Not yet optimize>" )]
    public async Task<IActionResult> GetBookReviews ( string bookId ) {
        if ( string.IsNullOrEmpty ( bookId ) ) {
            _logger.LogTrace ( "Caller tried to GetBook with a null Guid." );
            return BadRequest ( "Must include a Guid in request URI" );
        }

        if ( !Guid.TryParse ( bookId, null, out Guid bookIdGuid ) ) {
            _logger.LogTrace ( "Caller tried to GetBook with an invalid Guid." );
            return BadRequest ( "Invalid Guid." );
        }

        try {
            // Use Include to explicitly load the CustomerReviews navigation property
            Book? book = await _context.Books
            .Include(b => b.CustomerReviews)
            .FirstOrDefaultAsync(b => b.Id == bookIdGuid);

            if ( book is null ) {
                return NotFound ( );
            }

            BookDetailAndReviewsResponse bookAndReviewDetail = BookDetailAndReviewsResponse.Factory.FromBook(book);
            return Ok ( bookAndReviewDetail );
        } catch ( Exception ex ) {
            _logger.LogError ( $"Unhandled exception occurred:\n{ex}" );
            return StatusCode ( 500, "An unexpected error occurred." );
        }
    }



    /// <summary>
    /// Lists books in the library system with optional filtering and pagination.
    /// </summary>
    /// <param name="count">The maximum number of books to return (defaults to 15 if not specified)</param>
    /// <param name="offset">The number of books to skip before starting to return results (defaults to 0)</param>
    /// <param name="searchQuery">Optional search query to filter books by title or author</param>
    /// <returns>A list of books matching the criteria</returns>
    /// <remarks>
    /// The search query supports filtering by title and author. To search by author, use the format:
    /// "Title Author:AuthorName". Multiple authors can be specified with multiple "Author:" prefixes.
    /// </remarks>
    /// <response code="200">Returns the list of books matching the criteria</response>
    /// <response code="204">Returns when no books match the specified criteria</response>
    /// <response code="500">Returns when an unexpected error occurs during processing</response>
    [HttpGet ( "Books" )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "Performance", "HAA0603:Delegate allocation from a method group", Justification = "<Not yet optimized>" )]
    public async Task<IActionResult> ListBooks ( [FromQuery] int? count, [FromQuery] int? offset, [FromQuery] string? searchQuery ) {
        if ( count is null || count == 0 ) count = 15;
        if ( offset is null ) offset = 0;

        //= Initial query
        //===================================================

        try {
            if ( string.IsNullOrEmpty ( searchQuery ) ) {
                OneOf<List<Book>, Exception> results;
                results = await _ListBooks ( count.Value, offset.Value );

                if ( !results.TryPickT0 ( out List<Book> books, out Exception ex ) ) {
                    throw new UserFriendlyException ( $"TryPickT0 Failed in call: {nameof ( ListBooks )}\n{ex.Message}; searchQuery is null", 451, "An error 451 occured. The issue has been logged.", ex );
                }

                if ( books.Count <= 0 ) {
                    return StatusCode ( 450, "Found zero books" );
                }

                var builder = new BookListResponse.Builder();
                builder.AddBooks ( books );

                return Ok ( builder.Build ( ) );
            } else {
                OneOf<List<(float score, Book book, string meta)>, Exception> results;
                results = await _ListBooksWithQuery ( count.Value, offset.Value, searchQuery );

                if ( !results.TryPickT0 ( out List<(float score, Book book, string meta)> books, out Exception ex ) ) {
                    throw new UserFriendlyException ( $"TryPickT0 Failed in call: {nameof ( ListBooks )}\n{ex.Message}; searchQuery is not null;", 452, "An error 452 occured. The issue has been logged.", ex );
                }

                if ( books.Count <= 0 ) {
                    return StatusCode ( 450, "Found zero books" );
                }

                var builder = new BookListScoredResponse.Builder();
                builder.AddBooks ( books );

                return Ok ( builder.Build ( ) );
            }
        } catch ( Exception ex ) {
            _logger.LogError ( $"An unknown error occured during ListBooks call:\n{ex}" );
            return StatusCode ( 500, "An unknown error occurred." );
        }
    }

    private async Task<OneOf<List<Book>, Exception>> _ListBooks ( int count, int offset ) {
        try {
            // First apply Include
            Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Book, ICollection<CheckOutEvent>> query =
                _context.Books.Include(b => b.CheckOutEvents);

            // Then apply sorting to the IQueryable result (not to the IIncludableQueryable)
            IQueryable<Book> sortedQuery = ApplyStandardSorting(query);

            return await sortedQuery
                .Skip ( offset )
                .Take ( count )
                .ToListAsync ( );
        } catch ( Exception ex ) {
            return ex;
        }
    }

    private IQueryable<Book> ApplyStandardSorting ( IQueryable<Book> query ) {
        return query
            .OrderBy ( b => EF.Functions.Collate ( b.Title, "SQL_Latin1_General_CP1_CI_AS" ) )
            .ThenBy ( b => b.CheckOutEvents
                .Where ( co => co.ReturnDateTime == null )
                .Any ( ) ) // This will put books with active checkouts last
            .ThenByDescending ( b => b.PublicationDate );
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "Performance", "HAA0301:Closure Allocation Source", Justification = "<Not yet optimize>" )]
    private async Task<OneOf<List<(float score, Book book, string meta)>, Exception>> _ListBooksWithQuery ( int count, int offset, string searchQuery ) {
        try {
            // Extract author specifications
            var authorSpecs = searchQuery.Split("Author:", StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .Select(part => part.Trim())
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .ToList();

            // Extract title part
            string queryTitlePart = searchQuery;
            int firstAuthorIndex = searchQuery.IndexOf("Author:", StringComparison.InvariantCultureIgnoreCase);
            if ( firstAuthorIndex >= 0 ) {
                queryTitlePart = searchQuery.Substring ( 0, firstAuthorIndex ).Trim ( );
            }

            // Build the base query
            var query = _context.Books.AsQueryable();

            // Apply author filtering if specified
            if ( authorSpecs.Count > 0 ) {
                ExpressionStarter<Book> authorPredicate = PredicateBuilder.New<Book>();
                foreach ( string author in authorSpecs ) {
                    string authorPattern = $"%{author}%";
                    authorPredicate = authorPredicate.Or ( book => EF.Functions.Like ( book.Author, authorPattern ) );
                }
                query = query.Where ( authorPredicate );
            }

            // Apply title filtering with ONLY Soundex matching at the SQL level
            if ( !string.IsNullOrWhiteSpace ( queryTitlePart ) ) {
                var titleWords = queryTitlePart.Split(new[] { ' ', ',', '-', '.', ';', ':' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(word => word.Length > 1)
                    .ToList();

                if ( titleWords.Any ( ) ) {
                    // Create a predicate for all search conditions
                    ExpressionStarter<Book> titlePredicate = PredicateBuilder.New<Book>();

                    foreach ( var word in titleWords ) {
                        // Exact match (use this as a baseline)
                        string searchPattern = $"%{word}%";
                        titlePredicate = titlePredicate.Or ( book => EF.Functions.Like ( book.Title, searchPattern ) );

                        // Soundex match (ONLY SOUNDEX in the SQL query)
                        List<string> similarSoundexCodes = PhoneticMatcher.getSimilarPhoneticCodes(
                            word,
                            0.85f,
                            PhoneticMatcher.PhoneticAlgorithm.VowelSoundex
                        ).ToList();

                        foreach ( string soundexCode in similarSoundexCodes ) {
                            titlePredicate = titlePredicate.Or ( book =>
                                EF.Functions.Like ( book.TitleSoundex, $"%{soundexCode}%" ) );
                        }
                    }

                    query = query.Where ( titlePredicate );
                }
            }

            // Include related data
            query = query.Include ( b => b.CheckOutEvents );

            // Get candidate books (get more than needed for scoring)
            int fetchCount = Math.Min(count * 5, 100); // Get more candidates for ranking, but cap at reasonable number
            List<Book> candidateBooks = await query.Take(fetchCount).ToListAsync();

            // Calculate scores and rank books
            var scoredBooks = new List<(float Score, Book Book, string meta)>();

            foreach ( Book book in candidateBooks ) {
                float score = 0;
                StringBuilder sb = new();

                // Base score - everything starts at 1.0
                //score = 1.0f;

                // Title match scoring
                if ( !string.IsNullOrWhiteSpace ( queryTitlePart ) ) {
                    var titleQueryWords = queryTitlePart.Split(new[] { ' ', ',', '-', '.', ';', ':' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(word => word.Length > 1)
                        .ToList();

                    // We'll award a full score for a perfect title match
                    if ( queryTitlePart.Equals ( book.Title, StringComparison.OrdinalIgnoreCase ) ) {
                        score = 100;
                        sb.AppendLine ( $"Got full match: '{book.Title}' == '{queryTitlePart}'" );
                    } else if ( book.Title.StartsWith ( queryTitlePart, StringComparison.OrdinalIgnoreCase ) ) {
                        if ( score == 0 ) score = 1;
                        score *= 1.05f;
                        sb.AppendLine ( $"Boosting score for BeginsWith" );
                    } else {

                        sb.AppendLine ( $"No full match: '{book.Title}' != '{queryTitlePart}'" );
                        // We'll give total points of 80 for matching every word in the query
                        // but the last 20/20 points will be based on how close the number of words matched
                        var titleBookWords = book.Title.Split ( new char [] { ' ', ',', '-', '.', ';', ':', '&' }).ToList();
                        int titleBookLength = titleBookWords.Count;
                        int titleQueryLength = titleQueryWords.Count;
                        int titleLengthDifference = Math.Abs(titleQueryLength - titleBookLength);
                        float scoreFactorPerWord = (0.70f/titleQueryLength) + 0.20f*(titleLengthDifference/(Math.Max(titleBookLength, titleQueryLength)));
                        sb.AppendLine ( $"Using scoreFactorPerWord: '{scoreFactorPerWord}'" );
                        //float scoreFactorPerWord = 0.80f * 1/titleQueryWords.Count; // 80 points for all words matching

                        List<string> weakWords = ["the", "a", "by", "how", "what", "who", "when"];

                        foreach ( var word in titleQueryWords ) {
                            // Exact title match (case-insensitive) - highest score
                            if ( book.Title.Contains ( word, StringComparison.InvariantCultureIgnoreCase ) ) {
                                if ( weakWords.Contains ( word ) )
                                    score += scoreFactorPerWord * 50;
                                else
                                    score += scoreFactorPerWord * 100;
                                sb.AppendLine ( $"Exact Match, Adding '{scoreFactorPerWord * 100}' points for a total score of '{score}'" );
                            } else {
                                // Check for Soundex matches if we didn't get exact match
                                foreach ( string titleBookWord in titleBookWords ) {
                                    if ( weakWords.Contains ( word ) ) {
                                        score += (float) PhoneticMatcher.getPhoneticSimilarity ( titleBookWord, word, PhoneticMatcher.PhoneticAlgorithm.VowelSoundex ) * 45 * scoreFactorPerWord;
                                        score += (float) PhoneticMatcher.getPhoneticSimilarity ( titleBookWord, word, PhoneticMatcher.PhoneticAlgorithm.Metaphone ) * 45 * scoreFactorPerWord;
                                    } else {

                                        score += (float) PhoneticMatcher.getPhoneticSimilarity ( titleBookWord, word, PhoneticMatcher.PhoneticAlgorithm.VowelSoundex ) * 15 * scoreFactorPerWord;
                                        score += (float) PhoneticMatcher.getPhoneticSimilarity ( titleBookWord, word, PhoneticMatcher.PhoneticAlgorithm.Metaphone ) * 15 * scoreFactorPerWord;
                                    }
                                    sb.AppendLine ( $"Score updated for phonetic match and now is '{score}'" );
                                }
                            }
                        }
                    }
                }

                // Author match scoring

                if ( authorSpecs.Count > 0 ) {
                    foreach ( string author in authorSpecs ) {
                        if ( book.Author.Contains ( author, StringComparison.InvariantCultureIgnoreCase ) ) {
                            score += 8.0f; // High score for author match
                        }
                    }
                }


                if ( book.IsAvailable ) {
                    score *= 1.05f;
                    sb.AppendLine ( $"Score Availability boost, score is now '{score}'" );
                }

                score = Math.Clamp ( score, 0, 100 );
                sb.AppendLine ( $"Final Score: {score}" );
                var rankMeta = sb.ToString();

                // Recency boost (newer books ranked higher)
                /*
                var currentYear = DateOnly.FromDateTime(DateTime.Now).Year;
                var bookYear = book.PublicationDate.Year;
                var yearDiff = currentYear - bookYear;

                if ( yearDiff <= 1 ) {
                    score *= 1.3f; // 30% boost for very recent books (≤ 1 year)
                } else if ( yearDiff <= 3 ) {
                    score *= 1.2f; // 20% boost for recent books (≤ 3 years)
                } else if ( yearDiff <= 5 ) {
                    score *= 1.1f; // 10% boost for somewhat recent books (≤ 5 years)
                }
                */

                scoredBooks.Add ( (score, book, rankMeta) );
            }

            // Sort by score and apply pagination
            var rankedResults = scoredBooks
                .OrderByDescending(sb => sb.Score)
                .Skip(offset)
                .Take(count)
                .ToList();

            return rankedResults;
        } catch ( Exception ex ) {
            return ex;
        }
    }
}
