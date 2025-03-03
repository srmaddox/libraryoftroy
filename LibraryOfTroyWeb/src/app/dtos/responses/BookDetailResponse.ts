import { Guid } from '../../model/uuid';

export interface BookDetailResponse {
  id: Guid;
  title: string;
  author: string;
  description: string;
  publisher: string;
  publicationDate: string;
  category: string;
  isbn: string;
  pageCount: number;
  coverImageUrl: string;
  coverImageAltText: string;

  // Allow for both property names - one will be undefined
  available?: boolean;
  Available?: boolean;

  averageCustomerReview: number;
  customerReviewCount: number;
  searchRank: number;
  rankingMetaData: string;

}
