export interface IdentityRegistrationRequest {
  userName: string;
  password: string;
  confirmPassword: string;
  displayName?: string;
}
