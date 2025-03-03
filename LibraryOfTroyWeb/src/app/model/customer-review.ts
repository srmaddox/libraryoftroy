import { Guid } from './guid';
import {Book} from './book';

export interface CustomerReview {
  id: Guid;
  reviewDateTime: string;
  bookId: Guid;
  book: Book;
  reviewerDisplayName: string;
  review: string;
  rating: number;
}
