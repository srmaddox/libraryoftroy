import { IUser } from './IUser';
import { Guid } from './uuid';
import { AuthResponse } from '../dtos/responses/AuthResponse';

export class Librarian implements IUser {
  private readonly id: Guid;
  private readonly userName: string;
  private readonly displayName: string;
  private readonly roles: string[];

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

  public hasLibrarianAccess(): boolean {
    return this.roles.includes('Librarian');
  }

  // Nested Factory class
  static Factory = class {
    static fromAuthResponse(response: AuthResponse): Librarian {
      // Ensure the user has the Librarian role
      if (!response.user.roles.includes('Librarian')) {
        throw new Error('User does not have Librarian role');
      }

      return new Librarian(
        response.user.id as unknown as Guid, // May need conversion depending on Guid implementation
        response.user.userName,
        response.user.displayName || response.user.userName,
        response.user.roles
      );
    }
  };
}
