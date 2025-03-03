import {IUser} from './IUser';
import {Guid} from './uuid';
import {AuthResponse} from '../dtos/responses/AuthResponse';

export class Customer implements IUser{
  private id: Guid;
  private userName: string;
  private displayName: string;
  private roles: string[];

  private constructor(id: Guid, userName: string, displayName: string, roles: string[]) {
    this.id = id;
    this.userName = userName;
    this.displayName = displayName;
    this.roles = roles;
  }

  public getUserId(): Guid {
    return this.id;
  }

  public getUserName(): string {
    return this.userName;
  }

  public getDisplayName(): string {
    return this.displayName;
  }

  public getRoles(): string[] {
    return this.roles;
  }

  // Nested Factory class
  static Factory = class {
    static fromAuthResponse(response: AuthResponse): Customer {
      return new Customer(
        response.user.id as unknown as Guid, // May need conversion depending on Guid implementation
        response.user.userName,
        response.user.displayName || response.user.userName,
        response.user.roles
      );
    }
  };

}
