import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Guid } from '../model/uuid';
import { CheckOutRequest } from '../dtos/requests/CheckOutRequest';
import { BookCreateRequest } from '../dtos/requests/BookCreateRequest';
import { BookUpdateRequest } from '../dtos/requests/BookUpdateRequest';
import { BatchBookCreateResponse } from '../dtos/responses/BatchBookCreateResponse';
import { CheckInRequest } from '../dtos/requests/CheckInRequest';

@Injectable({
  providedIn: 'root'
})
export class LibrarianApiService {
  private apiUrl = 'https://localhost:7023/api/Librarian';

  constructor(private http: HttpClient) {}

  /**
   * Creates a new book in the library system
   */
  createBook(book: BookCreateRequest): Observable<boolean> {
    return this.http.post(`${this.apiUrl}/Books`, book, { responseType: 'text' })
      .pipe(
        map(() => true),
        catchError(() => of(false))
      );
  }

  /**
   * Creates multiple books in the library system in a single batch operation
   */
  createMultipleBooks(books: BookCreateRequest[]): Observable<BatchBookCreateResponse | null> {
    return this.http.put<BatchBookCreateResponse>(`${this.apiUrl}/Books`, books)
      .pipe(
        catchError(() => of(null))
      );
  }

  /**
   * Updates an existing book with new information
   */
  updateBook(bookId: Guid, book: BookUpdateRequest): Observable<boolean> {
    return this.http.patch(`${this.apiUrl}/Books/${bookId}`, book, { responseType: 'text' })
      .pipe(
        map(() => true),
        catchError(() => of(false))
      );
  }

  /**
   * Deletes a book from the library system
   */
  deleteBook(bookId: Guid): Observable<boolean> {
    return this.http.delete(`${this.apiUrl}/Books/${bookId}`, { responseType: 'text' })
      .pipe(
        map(() => true),
        catchError(() => of(false))
      );
  }

  /**
   * Checks in a previously checked out book
   */
  checkInBook(bookId: Guid, request?: Partial<CheckInRequest>): Observable<boolean> {
    return this.http.post(`${this.apiUrl}/Books/${bookId}/CheckIn`, request || {}, { responseType: 'text' })
      .pipe(
        map(() => true),
        catchError(() => of(false))
      );
  }

  /**
   * Checks out a book to a customer by a librarian
   */
  checkOutBook(bookId: Guid, request: CheckOutRequest): Observable<boolean> {
    return this.http.post(`${this.apiUrl}/Books/${bookId}/CheckOut`, request, { responseType: 'text' })
      .pipe(
        map(() => true),
        catchError(() => of(false))
      );
  }

  // Add this method to your LibrarianApiService class

  /**
   * Gets all books currently borrowed from the library with their status details
   * @param includeReturned Whether to include already returned books (default: false)
   * @param count The maximum number of items to return (default: 50)
   * @param offset The number of items to skip (default: 0)
   */
  getBorrowedBooks(
    includeReturned: boolean = false,
    count: number = 50,
    offset: number = 0
  ): Observable<any[]> {
    let params = new HttpParams()
      .set('includeReturned', includeReturned.toString())
      .set('count', count.toString())
      .set('offset', offset.toString());

    return this.http.get<any[]>(`${this.apiUrl}/Books/Borrowed`, { params })
      .pipe(
        catchError(error => {
          console.error('Error fetching borrowed books:', error);
          return of([]);
        })
      );
  }
}
