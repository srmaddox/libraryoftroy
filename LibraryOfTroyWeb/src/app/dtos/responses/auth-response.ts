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
