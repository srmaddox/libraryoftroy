import { Injectable } from '@angular/core';
import {
  CanActivate,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  Router,
  UrlTree
} from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from './services/AuthService';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {

  constructor(private authService: AuthService, private router: Router) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

    // First check if the user is authenticated
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: state.url }
      });
      return false;
    }

    // Check if route has data.roles defined
    const roles = route.data['roles'] as Array<string>;
    if (!roles || roles.length === 0) {
      return true; // No specific roles required
    }

    // Check if user has at least one of the required roles
    const hasRequiredRole = roles.some(role => this.authService.hasRole(role));

    if (hasRequiredRole) {
      return true;
    }

    // If user doesn't have required roles, redirect to home page
    // You could also redirect to an "unauthorized" page
    this.router.navigate(['/']);
    return false;
  }
}
