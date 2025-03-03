import {Guid} from './uuid';

export interface IUser {
  getUserId(): Guid;
  getUserName(): string;
  getDisplayName(): string;
  getRoles(): string[];
}
