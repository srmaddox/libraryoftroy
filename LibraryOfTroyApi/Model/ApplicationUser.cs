
using Microsoft.AspNetCore.Identity;

namespace LibraryOfTroyApi.Model;
// Custom user class that extends IdentityUser to add library-specific fields
public class ApplicationUser : IdentityUser {
    public string DisplayName { get; set; } = string.Empty;

    // Navigation property for linking to the Customer model
    public Customer? Customer { get; set; }
    public Guid? CustomerId { get; set; }

    // Flag to track if user is a librarian
    public bool IsLibrarian { get; set; } = false;
}
