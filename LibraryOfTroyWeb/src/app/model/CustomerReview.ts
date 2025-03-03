import { Guid } from './uuid';
import {Book} from './Book';

export interface CustomerReview {
  id: Guid;
  reviewDateTime: string;
  bookId: Guid;
  book: Book;
  reviewerDisplayName: string;
  review: string;
  rating: number;
}
