import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Guid } from '../model/uuid';
import { Book } from '../model/Book';
import { BookDetailResponse } from '../dtos/responses/BookDetailResponse';
import { BookDetailAndReviewsResponse } from '../dtos/responses/BookDetailAndReviewsResponse';

@Injectable({
  providedIn: 'root'
})
export class PublicApiService {
  private apiUrl = 'https://localhost:7023/api/Public';

  constructor(private http: HttpClient) {}

  /**
   * Retrieves a specific book by its unique identifier
   */
  getBook(bookId: Guid): Observable<BookDetailResponse | null> {
    return this.http.get<BookDetailResponse>(`${this.apiUrl}/Books/${bookId}`)
      .pipe(
        catchError(() => of(null))
      );
  }

  /**
   * Gets reviews for a specific book
   */
  getBookReviews(bookId: Guid): Observable<BookDetailAndReviewsResponse | null> {
    return this.http.get<BookDetailAndReviewsResponse>(`${this.apiUrl}/Books/${bookId}/Reviews`)
      .pipe(
        catchError(() => of(null))
      );
  }

  /**
   * Lists books with optional filtering and pagination
   */
  listBooks(
    count: number = 15,
    offset: number = 0,
    searchQuery?: string
  ): Observable<BookDetailResponse[]> {
    let params = new HttpParams()
      .set('count', count.toString())
      .set('offset', offset.toString());

    if (searchQuery) {
      params = params.set('searchQuery', searchQuery);
    }

    return this.http.get<{books: BookDetailResponse[], responseTimestamp: string}>(
      `${this.apiUrl}/Books`,
      { params }
    ).pipe(
      map(response => {
        // Normalize the availability property on each book
        const processedBooks = response.books.map(book => {
          // If available is undefined but Available exists, copy the value
          if (book.available === undefined && book.Available !== undefined) {
            book.available = book.Available;
          }
          return book;
        });

        // Sort by search rank if this was a search query
        if (searchQuery) {
          return processedBooks.sort((a, b) => {
            const rankA = a.searchRank || 0;
            const rankB = b.searchRank || 0;
            return rankB - rankA;
          });
        }
        return processedBooks;
      }),
      catchError(error => {
        console.error('Error fetching books:', error);
        return of([]);
      })
    );
  }
}

