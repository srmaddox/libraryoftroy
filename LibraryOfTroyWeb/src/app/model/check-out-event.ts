import { Guid } from './guid'

export interface CheckOutEvent {
  id: Guid;
  customerId: Guid;
  bookId: Guid;
  CheckoutDateTime: string;
  CheckinDateTime: string | undefined | null;
}
