import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';

export enum OverlayType {
  NONE = 'none',
  DETAILS = 'details',
  CHECKOUT = 'checkout',
  LOGIN = 'login'
}

// Type that defines all available outlet names in the application
export type OutletName =
  'headerPane' |
  'cornerPane' |
  'toolsPane' |
  'resultsPane' |
  'featurePane' |
  'footerPane' |
  'headerOverlayPane' |
  'rightOverlayPane' |
  'toolsOverlayPane' |
  'resultOverlayPane' |
  'footerOverlayPane';

@Injectable({
  providedIn: 'root'
})
export class LayoutRouterService {
  // Track active overlay
  private activeOverlaySubject = new BehaviorSubject<OverlayType>(OverlayType.NONE);
  activeOverlay$ = this.activeOverlaySubject.asObservable();

  constructor(private router: Router) {}

  /**
   * Routes a component to a specified outlet
   * @param outletName The name of the outlet to route to
   * @param path The path to route to
   * @param params Optional parameters to pass to the route
   */
  routeToOutlet(outletName: OutletName, path: string, params: any = {}): void {
    // Build the commands array for router.navigate
    // Format: ['path', { outlets: { outletName: ['component-path', params] }}]
    const commands = ['', { outlets: { [outletName]: [path, ...Object.values(params)] } }];

    this.router.navigate(commands);
  }

  /**
   * Clear a specific outlet
   * @param outletName The name of the outlet to clear
   */
  clearOutlet(outletName: OutletName): void {
    const commands = ['', { outlets: { [outletName]: null } }];
    this.router.navigate(commands);
  }

  /**
   * Shows a specific overlay
   * @param type The type of overlay to show
   * @param params Optional parameters to pass
   */
  showOverlay(type: OverlayType, params: any = {}): void {
    this.activeOverlaySubject.next(type);

    switch (type) {
      case OverlayType.DETAILS:
        // Example routing book details to right overlay pane
        this.routeToOutlet('rightOverlayPane', 'book-details', params);
        break;

      case OverlayType.CHECKOUT:
        // Example routing checkout confirmation to right overlay pane
        this.routeToOutlet('rightOverlayPane', 'checkout-confirm', params);
        break;

      case OverlayType.LOGIN:
        // Example routing login to right overlay pane
        this.routeToOutlet('rightOverlayPane', 'login', params);
        break;

      default:
        this.hideOverlay();
        break;
    }
  }

  /**
   * Hides the active overlay
   */
  hideOverlay(): void {
    this.activeOverlaySubject.next(OverlayType.NONE);
    this.clearOutlet('rightOverlayPane');
    this.clearOutlet('headerOverlayPane');
    this.clearOutlet('toolsOverlayPane');
    this.clearOutlet('resultOverlayPane');
    this.clearOutlet('footerOverlayPane');
  }
}
