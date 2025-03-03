import {Guid} from './guid';

export interface User {
  getUserId(): Guid;
  getUserName(): string;
  getDisplayName(): string;
  getRoles(): string[];
}
