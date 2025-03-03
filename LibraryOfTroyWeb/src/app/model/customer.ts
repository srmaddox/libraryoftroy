import {User} from './user';
import {Guid} from './guid';
import {AuthResponse} from '../dtos/responses/auth-response';

export class Customer implements User{
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

  static Factory = class {
    static fromAuthResponse(response: AuthResponse): Customer {
      return new Customer(
        response.user.id as unknown as Guid,
        response.user.userName,
        response.user.displayName || response.user.userName,
        response.user.roles
      );
    }
  };

}
