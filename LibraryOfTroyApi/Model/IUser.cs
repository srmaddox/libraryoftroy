namespace LibraryOfTroyApi.Model;

public interface User {
    public Guid GetUserId ( );
    public string GetUserName ( );
    public string GetDisplayName ( );
    public Role GetRoles ( );

    [Flags]
    public enum Role {
        None = 0,
        Customer = 1,
        Librarian = 2,
    }
}
