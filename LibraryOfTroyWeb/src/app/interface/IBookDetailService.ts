import { Guid } from '../model/uuid';
import { Book } from '../model/Book';
import { Observable } from 'rxjs';
import { BookCreateRequest } from '../dtos/requests/BookCreateRequest';
import { BookUpdateRequest } from '../dtos/requests/BookUpdateRequest';

export interface IBookDetailService {
  createBookDetail(book: BookCreateRequest): Observable<undefined | Error>;
  readBookDetails(fetchId: Guid): Observable<Book | undefined | Error>;
  updateBookDetails(updateId: Guid, book: BookUpdateRequest): Observable<undefined | Error>;
  deleteBookDetails(deleteId: Guid): Observable<undefined | Error>;
  listBooks(count: number, offset: number, ): Observable<Book[] | Error>;
}
