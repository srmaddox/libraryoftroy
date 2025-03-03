
using global::LibraryOfTroyApi.DTOs;

using LibraryOfTroyApi.DTOs;

namespace LibraryOfTroyApi.Model;

public partial record CheckOutEvent {
    public static class Factory {
        public static CheckOutEvent FromCheckOutRequest ( Guid bookId, CheckOutRequest request ) {
            return new CheckOutEvent {
                Id = Guid.NewGuid ( ),
                BookId = bookId,
                CustomerId = request.CustomerId,
                CheckoutDateTime = DateTime.Now,
                ReturnDateTime = null
            };
        }

        public static CheckOutEvent WithReturnDate ( CheckOutEvent original, DateTime? returnDateTime = null ) {
            // Create a new instance with the return date set
            return new CheckOutEvent {
                Id = original.Id,
                BookId = original.BookId,
                CustomerId = original.CustomerId,
                CheckoutDateTime = original.CheckoutDateTime,
                ReturnDateTime = returnDateTime ?? DateTime.Now,
                Book = original.Book
            };
        }
    }
}
