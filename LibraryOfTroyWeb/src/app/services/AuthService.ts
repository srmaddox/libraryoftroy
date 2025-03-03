// src/app/services/AuthService.ts - Improved with better error handling

import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, throwError, of } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { Customer } from '../model/Customer';
import { IdentityLoginRequest } from '../dtos/requests/IdentityLoginRequest';
import { IdentityRegistrationRequest } from '../dtos/requests/IdentityRegistrationRequest';
import { AuthResponse } from '../dtos/responses/AuthResponse';
import { IdentityResult } from '../dtos/responses/IdentityResult';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiBaseUrl = 'https://localhost:7023/api/auth';
  private currentUserSubject = new BehaviorSubject<Customer | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  private authTokenKey = 'auth_token';
  private authExpKey = 'auth_expiration';

  constructor(private http: HttpClient, private router: Router) {
    this.loadUserFromStoredToken();
  }

  public get currentUserValue(): Customer | null {
    return this.currentUserSubject.value;
  }

  public isAuthenticated(): boolean {
    try {
      const token = localStorage.getItem(this.authTokenKey);
      const expiration = localStorage.getItem(this.authExpKey);

      if (!token || !expiration) {
        return false;
      }

      // Check if token is expired
      const expirationDate = new Date(expiration);
      if (new Date() > expirationDate) {
        this.logout();
        return false;
      }

      return true;
    } catch (error) {
      console.error('Error checking authentication status:', error);
      return false;
    }
  }

  public hasRole(role: string): boolean {
    try {
      const user = this.currentUserValue;
      if (!user) {
        return false;
      }

      return user.getRoles().includes(role);
    } catch (error) {
      console.error('Error checking user role:', error);
      return false;
    }
  }

  public register(request: IdentityRegistrationRequest): Observable<IdentityResult> {
    return this.http.post<IdentityResult>(`${this.apiBaseUrl}/register`, request)
      .pipe(
        catchError(error => {
          console.error('Registration error:', error);
          return throwError(() => new Error(error.error?.message || 'Registration failed'));
        })
      );
  }

  public login(request: IdentityLoginRequest): Observable<Customer> {
    return this.http.post<AuthResponse>(`${this.apiBaseUrl}/login`, request)
      .pipe(
        tap(response => {
          // Store auth data
          localStorage.setItem(this.authTokenKey, response.token);
          localStorage.setItem(this.authExpKey, response.expiration.toString());

          // Create and store user
          const customer = Customer.Factory.fromAuthResponse(response);
          this.currentUserSubject.next(customer);
        }),
        map(response => Customer.Factory.fromAuthResponse(response)),
        catchError(error => {
          console.error('Login error:', error);
          return throwError(() => new Error(error.error?.message || 'Login failed'));
        })
      );
  }

  public logout(): void {
    localStorage.removeItem(this.authTokenKey);
    localStorage.removeItem(this.authExpKey);
    this.currentUserSubject.next(null);
    this.navigateToHome();
  }
  private navigateToHome(): void {
    // Navigate to the home route with all necessary outlets
    this.router.navigate([{
      outlets: {
        headerPane: ['nav-header'],
        cornerPane: ['library-bag'],
        toolsPane: ['search-query'],
        resultsPane: ['search-results'],
        featurePane: ['featured-books'],
        footerPane: ['nav-footer']
      }
    }]);
  }

  public getUserInfo(): Observable<any> {
    return this.http.get<any>(`${this.apiBaseUrl}/userinfo`)
      .pipe(
        catchError(error => {
          console.error('Error getting user info:', error);
          return throwError(() => new Error('Failed to get user information'));
        })
      );
  }

  // Get the JWT token for API requests
  public getAuthToken(): string | null {
    try {
      return localStorage.getItem(this.authTokenKey);
    } catch (error) {
      console.error('Error getting auth token:', error);
      return null;
    }
  }

  // Add auth header to HTTP requests
  public getAuthHeaders(): HttpHeaders {
    const token = this.getAuthToken();
    if (token) {
      return new HttpHeaders({
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      });
    }
    return new HttpHeaders({
      'Content-Type': 'application/json'
    });
  }

  // Load user from stored token on app start
  private loadUserFromStoredToken(): void {
    try {
      const token = localStorage.getItem(this.authTokenKey);
      const expiration = localStorage.getItem(this.authExpKey);

      if (token && expiration) {
        // Check if token is expired
        const expirationDate = new Date(expiration);
        if (new Date() > expirationDate) {
          this.logout();
          return;
        }

        // Decode token to get user info
        const tokenPayload = this.parseJwt(token);

        if (tokenPayload) {
          // Convert token payload to AuthResponse format
          const authResponse: AuthResponse = {
            token: token,
            expiration: expirationDate,
            user: {
              id: tokenPayload.nameid || tokenPayload.sub,
              userName: tokenPayload.unique_name || tokenPayload.email,
              displayName: tokenPayload.DisplayName || tokenPayload.unique_name,
              email: tokenPayload.email || tokenPayload.unique_name,
              roles: this.extractRoles(tokenPayload)
            }
          };

          // Create user from auth response
          const customer = Customer.Factory.fromAuthResponse(authResponse);
          this.currentUserSubject.next(customer);
        }
      }
    } catch (e) {
      console.error('Error loading user from token:', e);
      this.logout();
    }
  }

  // Helper function to parse JWT
  private parseJwt(token: string): any {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch (e) {
      console.error('Error parsing JWT token:', e);
      return null;
    }
  }

  // Extract roles from token payload
  private extractRoles(tokenPayload: any): string[] {
    // Roles can be in different formats based on the token structure
    const roleKey = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

    try {
      if (tokenPayload.role) {
        return Array.isArray(tokenPayload.role) ? tokenPayload.role : [tokenPayload.role];
      } else if (tokenPayload[roleKey]) {
        return Array.isArray(tokenPayload[roleKey]) ? tokenPayload[roleKey] : [tokenPayload[roleKey]];
      }
    } catch (error) {
      console.error('Error extracting roles:', error);
    }

    return [];
  }
}
