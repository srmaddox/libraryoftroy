
using LibraryOfTroyApi.Model;

using Newtonsoft.Json;

namespace LibraryOfTroyApi.DTOs;
/// <summary>
/// Response DTO containing details about a borrowed book
/// </summary>
public record BorrowedBookResponse {
    /// <summary>
    /// Unique identifier for the checkout event
    /// </summary>
    [JsonRequired]
    [JsonConverter ( typeof ( TypeScriptGuidConverter ) )]
    public Guid CheckoutId { get; set; }

    /// <summary>
    /// Details of the borrowed book
    /// </summary>
    [JsonRequired]
    [JsonProperty ( "book" )]
    public BookDetailResponse Book { get; set; }

    /// <summary>
    /// Unique identifier of the customer who borrowed the book
    /// </summary>
    [JsonRequired]
    [JsonConverter ( typeof ( TypeScriptGuidConverter ) )]
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Username or display name of the customer who borrowed the book
    /// </summary>
    [JsonRequired]
    [JsonProperty ( "customerName" )]
    public string CustomerName { get; set; }

    /// <summary>
    /// Date and time when the book was checked out
    /// </summary>
    [JsonRequired]
    [JsonProperty ( "checkoutDate" )]
    public DateTime CheckoutDate { get; set; }

    /// <summary>
    /// Date and time when the book is due to be returned
    /// </summary>
    [JsonRequired]
    [JsonProperty ( "dueDate" )]
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Date and time when the book was returned (null if not yet returned)
    /// </summary>
    [JsonProperty ( "returnDate", NullValueHandling = NullValueHandling.Ignore )]
    public DateTime? ReturnDate { get; set; }

    /// <summary>
    /// Indicates whether the book is overdue
    /// </summary>
    [JsonProperty ( "isOverdue" )]
    public bool IsOverdue { get; set; }

    /// <summary>
    /// Number of days the book is overdue (0 if not overdue)
    /// </summary>
    [JsonProperty ( "daysOverdue" )]
    public int DaysOverdue { get; set; }
}
