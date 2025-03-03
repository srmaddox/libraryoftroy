import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Guid } from '../model/uuid';
import { ReviewCreateRequest } from '../dtos/requests/ReviewCreateRequest';
import { ReviewDetailResponse } from '../dtos/responses/ReviewDetailResponse';
import { CheckOutRequest } from '../dtos/requests/CheckOutRequest';

@Injectable({
  providedIn: 'root'
})
export class CustomerApiService {
  private apiUrl = 'https://localhost:7023/api/Customer';

  constructor(private http: HttpClient) {}

  /**
   * Adds a new customer review to a book
   */
  addBookReview(bookId: Guid, review: ReviewCreateRequest): Observable<ReviewDetailResponse | null> {
    return this.http.post<ReviewDetailResponse>(`${this.apiUrl}/Books/${bookId}/Reviews`, review)
      .pipe(
        catchError(() => of(null))
      );
  }

  /**
   * Checks out a book to a customer
   */
  checkOutBook(bookId: Guid, request: CheckOutRequest): Observable<boolean> {
    return this.http.post(`${this.apiUrl}/Books/${bookId}/CheckOut`, request, { responseType: 'text' })
      .pipe(
        map(() => true),
        catchError(() => of(false))
      );
  }
}
