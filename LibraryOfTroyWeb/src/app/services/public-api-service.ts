import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Guid } from '../model/guid';
import { BookDetailResponse } from '../dtos/responses/book-detail-response';
import { BookDetailAndReviewsResponse } from '../dtos/responses/book-detail-and-reviews-response';

@Injectable({
  providedIn: 'root'
})
export class PublicApiService {
  private apiUrl = 'https://localhost:7023/api/Public';
  private coverImageBaseUrl = 'books/';

  constructor(private http: HttpClient) {}

  getBook(bookId: Guid): Observable<BookDetailResponse | null> {
    return this.http.get<BookDetailResponse>(`${this.apiUrl}/Books/${bookId}`)
      .pipe(
        map(book => this.transformBookCoverUrl(book)),
        catchError(() => of(null))
      );
  }

  getBookReviews(bookId: Guid): Observable<BookDetailAndReviewsResponse | null> {
    return this.http.get<BookDetailAndReviewsResponse>(`${this.apiUrl}/Books/${bookId}/Reviews`)
      .pipe(
        map(response => {
          if (response.bookDetailResponse) {
            response.bookDetailResponse = this.transformBookCoverUrl(response.bookDetailResponse);
          }
          return response;
        }),
        catchError(() => of(null))
      );
  }

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
        const processedBooks = response.books.map(book => {
          // Transform cover image URL
          book = this.transformBookCoverUrl(book);

          // Handle property name discrepancy
          if (book.available === undefined && book.Available !== undefined) {
            book.available = book.Available;
          }
          return book;
        });

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
  getFeaturedBooks(): Observable<BookDetailResponse[]> {
    return this.http.get<BookDetailResponse[]>(`${this.apiUrl}/featured`)
      .pipe(
        map(response => {
          const processedBooks = response.map(book => {
            // Transform cover image URL
            book = this.transformBookCoverUrl(book);

            // Handle property name discrepancy
            if (book.available === undefined && book.Available !== undefined) {
              book.available = book.Available;
            }
            return book;
          });

          return processedBooks;
        }),
        catchError(() => of([]))
      );
  }
  /**
   * Transform the cover image URL for a book by prepending the base URL if needed
   */
  private transformBookCoverUrl(book: BookDetailResponse): BookDetailResponse {
    if (book.coverImageUrl) {
      // If URL is not absolute and doesn't already have the base path
      if (!book.coverImageUrl.startsWith('http://') &&
        !book.coverImageUrl.startsWith('https://') &&
        !book.coverImageUrl.startsWith(this.coverImageBaseUrl)) {
        book.coverImageUrl = this.coverImageBaseUrl + book.coverImageUrl;
      }
    }
    return book;
  }
}
