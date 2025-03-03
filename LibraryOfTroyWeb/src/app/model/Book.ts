import { Guid } from './uuid'

export interface Book {
  id: Guid;

  title: string;
  author: string;
  description: string | null | undefined;
  publisher: string | null | undefined;
  publicationDate: string | null | undefined;
  category: string | null | undefined;
  ISBN: string | null | undefined;
  pageCount: number | null | undefined;

  coverImageUrl: string | null | undefined;
  coverImageAltText: string | null | undefined;

  available: boolean | null | undefined;
  averageCustomerReview: number | null | undefined;
  customerReviewCount: number | null | undefined;


}
