// This represents the token response we get after successful login
export interface AuthResponse {
  token: string;
  expiration: Date;
  user: {
    id: string;
    userName: string;
    displayName?: string;
    email: string;
    roles: string[];
  };
}
