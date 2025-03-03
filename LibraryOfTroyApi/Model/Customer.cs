using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LibraryOfTroyApi.Model;

[Table ( "Customers" )]
public record Customer : User {
    [Key]
    public Guid Id { get; init; }

    [EmailAddress]
    public required string UserName { get; init; }

    public string GetDisplayName ( ) {
        throw new NotImplementedException ( );
    }

    public User.Role GetRoles ( ) {
        throw new NotImplementedException ( );
    }

    public Guid GetUserId ( ) {
        throw new NotImplementedException ( );
    }

    public string GetUserName ( ) {
        throw new NotImplementedException ( );
    }
}
