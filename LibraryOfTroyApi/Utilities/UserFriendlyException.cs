namespace LibraryOfTroyApi.Utilities;

public class UserFriendlyException : Exception {
    public int StatusCode;
    public string UserMessage;

    public UserFriendlyException ( string message, int statusCode, string userMessage, Exception innerException ) : base ( message, innerException ) {
        StatusCode = statusCode;
        UserMessage = userMessage;
    }
}
