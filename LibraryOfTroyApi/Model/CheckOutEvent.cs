using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryOfTroyApi.Model;

[Table ( "CheckOutEvents" )]
public partial record CheckOutEvent {
    [Key]
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public Guid BookId { get; init; }
    public DateTime CheckoutDateTime { get; init; }
    public DateTime? ReturnDateTime { get; init; }

    //= Navigation Properties
    //========================================================================================================
    public virtual Book? Book { get; init; }
    public virtual Customer? Customer { get; init; }
}