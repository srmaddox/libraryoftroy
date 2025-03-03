namespace LibraryOfTroyApi.DTOs;

public class ReviewCreateRequest {
    public required string ReviewerDisplayName { get; init; }
    public required string Review { get; init; }
    public required int Rating { get; init; }
}
