import { Guid} from '../../model/guid';

export interface ReviewDetailResponse {
  id: Guid;
  reviewDateTime: string;
  reviewerDisplayName: string;
  review: string;
  rating: number;
}
