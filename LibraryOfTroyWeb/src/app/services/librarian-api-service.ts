import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Guid } from '../model/guid';
import { CheckOutRequest } from '../dtos/requests/check-out-request';
import { BookCreateRequest } from '../dtos/requests/book-create-request';
import { BookUpdateRequest } from '../dtos/requests/book-update-request';
import { BookBatchCreateResponse } from '../dtos/responses/book-batch-create-response';
import { CheckInRequest } from '../dtos/requests/check-in-request';

@Injectable({
  providedIn: 'root'
})
export class LibrarianApiService {
  private apiUrl = 'https://localhost:7023/api/Librarian';

  constructor(private http: HttpClient) {}

  createBook(book: BookCreateRequest): Observable<boolean> {
    return this.http.post(`${this.apiUrl}/Books`, book, { responseType: 'text' })
      .pipe(
        map(() => true),
        catchError(() => of(false))
      );
  }

  createMultipleBooks(books: BookCreateRequest[]): Observable<BookBatchCreateResponse | null> {
    return this.http.put<BookBatchCreateResponse>(`${this.apiUrl}/Books`, books)
      .pipe(
        catchError(() => of(null))
      );
  }

  updateBook(bookId: Guid, book: BookUpdateRequest): Observable<boolean> {
    return this.http.patch(`${this.apiUrl}/Books/${bookId}`, book, { responseType: 'text' })
      .pipe(
        map(() => true),
        catchError(() => of(false))
      );
  }

  deleteBook(bookId: Guid): Observable<boolean> {
    return this.http.delete(`${this.apiUrl}/Books/${bookId}`, { responseType: 'text' })
      .pipe(
        map(() => true),
        catchError(() => of(false))
      );
  }

  checkInBook(bookId: Guid, request?: Partial<CheckInRequest>): Observable<boolean> {
    return this.http.post(`${this.apiUrl}/Books/${bookId}/CheckIn`, request || {}, { responseType: 'text' })
      .pipe(
        map(() => true),
        catchError(() => of(false))
      );
  }

  checkOutBook(bookId: Guid, request: CheckOutRequest): Observable<boolean> {
    return this.http.post(`${this.apiUrl}/Books/${bookId}/CheckOut`, request, { responseType: 'text' })
      .pipe(
        map(() => true),
        catchError(() => of(false))
      );
  }

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
