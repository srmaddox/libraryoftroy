import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, throwError, of } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { Customer } from '../model/customer';
import { IdentityLoginRequest } from '../dtos/requests/identity-login-request';
import { IdentityRegistrationRequest } from '../dtos/requests/identity-registration-request';
import { AuthResponse } from '../dtos/responses/auth-response';
import { IdentityResult } from '../dtos/responses/identity-result';
import {AppEventManager} from './app-event-manager';

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
          localStorage.setItem(this.authTokenKey, response.token);
          localStorage.setItem(this.authExpKey, response.expiration.toString());

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

  public getAuthToken(): string | null {
    try {
      return localStorage.getItem(this.authTokenKey);
    } catch (error) {
      console.error('Error getting auth token:', error);
      return null;
    }
  }

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

        const tokenPayload = this.parseJwt(token);

        if (tokenPayload) {
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

          const customer = Customer.Factory.fromAuthResponse(authResponse);
          this.currentUserSubject.next(customer);
        }
      }
    } catch (e) {
      console.error('Error loading user from token:', e);
      this.logout();
    }
  }

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

  private extractRoles(tokenPayload: any): string[] {
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
