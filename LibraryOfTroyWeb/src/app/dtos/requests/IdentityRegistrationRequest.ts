export interface IdentityRegistrationRequest {
  userName: string;
  password: string;
  confirmPassword: string;
  displayName?: string; // Custom property we might add
}
