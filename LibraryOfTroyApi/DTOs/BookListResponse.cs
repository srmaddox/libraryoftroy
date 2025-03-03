
using System.Collections.Immutable;

using LibraryOfTroyApi.Model;

using Newtonsoft.Json;

namespace LibraryOfTroyApi.DTOs;

public record BookListResponse {
    public required DateTime ResponseTimestamp { get; init; }
    public ImmutableList<BookDetailResponse> Books { get; init; } = [];

    public class Builder {
        private DateTime _ResponseTimestamp { get; set; } = DateTime.Now;
        private List<BookDetailResponse> _Books { get; set; } = [];

        public Builder AddBook ( Book book ) {
            _Books.Add ( BookDetailResponse.Factory.FromBook ( book, null ) );
            return this;
        }

        public Builder AddBooks ( IEnumerable<Book> books ) {
            foreach ( Book book in books ) {
                AddBook ( book );
            }

            return this;
        }

        public BookListResponse Build ( ) {
            return new BookListResponse ( ) {
                Books = _Books.ToImmutableList ( ),
                ResponseTimestamp = _ResponseTimestamp,
            };
        }
    }
}

public record BookListScoredResponse {
    public required DateTime ResponseTimestamp { get; init; }
    public ImmutableList<BookDetailResponse> _Books { get; init; }

    public class Builder {
        private DateTime _ResponseTimestamp { get; set; } = DateTime.Now;
        private List<BookDetailResponse> _Books { get; set; } = [];

        public Builder AddBook ( Book book, float score, string meta ) {
            _Books.Add ( BookDetailResponse.Factory.FromBook ( book, score, meta ) );
            return this;
        }

        public Builder AddBooks ( IEnumerable<(float score, Book book, string meta)> books ) {
            foreach ( (float score, Book book, string meta) in books ) {
                AddBook ( book, score, meta );
            }

            return this;
        }

        public BookListResponse Build ( ) {
            return new BookListResponse ( ) {
                Books = _Books.ToImmutableList ( ),
                ResponseTimestamp = _ResponseTimestamp,
            };
        }
    }
}
