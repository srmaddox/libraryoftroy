import { BookDetailResponse } from './book-detail-response';


export interface BookDetailAndReviewsResponse {
  bookDetailResponse: BookDetailResponse;
  topReviews: BookDetailResponse[];
  worstReviews: BookDetailResponse[];
}
