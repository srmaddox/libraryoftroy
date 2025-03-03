import { Guid} from '../../model/uuid';

export interface ReviewDetailResponse {
  id: Guid;
  reviewDateTime: string;
  reviewerDisplayName: string;
  review: string;
  rating: number;
}
