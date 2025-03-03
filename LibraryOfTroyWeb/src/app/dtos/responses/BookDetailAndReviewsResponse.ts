import { BookDetailResponse } from './BookDetailResponse';


export interface BookDetailAndReviewsResponse {
  bookDetailResponse: BookDetailResponse;
  topReviews: BookDetailResponse[];
  worstReviews: BookDetailResponse[];
}
