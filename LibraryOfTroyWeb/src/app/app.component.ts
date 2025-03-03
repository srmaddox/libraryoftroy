// Update app.component.ts
import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectorRef,
  ViewContainerRef,
  ViewChild,
  ComponentRef
} from '@angular/core';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subscription, filter } from 'rxjs';

// Component imports - these would be lazy loaded in a real app
import { SearchQueryComponent } from './shared/search-query/search-query.component';
import { FeaturedBooksComponent } from './shared/featured-books/featured-books.component';
import { NavFooterComponent } from './shared/nav-footer/nav-footer.component';
import { NavHeaderComponent } from './shared/nav-header/nav-header.component';
import { LibraryBagComponent } from './shared/library-bag/library-bag.component';
import { SearchResultsComponent } from './shared/search-results/search-results.component';
import { OverlayDetailsComponent } from './shared/overlay-details/overlay-details.component';
import { OverlayCheckoutConfirmComponent } from './shared/overlay-checkout-confirm/overlay-checkout-confirm.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';

// Services, models and DTOs
import { PublicApiService } from './services/PublicApiService';
import { BookDetailResponse } from './dtos/responses/BookDetailResponse';
import { LayoutRouterService, OverlayType } from './services/LayoutRouterService';

// Event management
import {
  AppEventManager,
  AppEvent
} from './services/AppEventManager';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    SearchQueryComponent,
    FeaturedBooksComponent,
    NavFooterComponent,
    NavHeaderComponent,
    LibraryBagComponent,
    SearchResultsComponent,
    OverlayDetailsComponent,
    OverlayCheckoutConfirmComponent,
    LoginComponent,
    RegisterComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit, OnDestroy {
  @ViewChild('overlayContainer', { read: ViewContainerRef }) overlayContainer!: ViewContainerRef;
  title = 'LibraryOfTroy';
  books: BookDetailResponse[] = [];
  isAuthPage: boolean = false;

  overlayVisibility: { [key: string]: boolean } = {
    header: false,
    rightOverlayPane: false,
    tool: false,
    result: false,
    footer: false
  };

  // Collection of subscriptions to manage
  private subscriptions = new Subscription();
  private activeComponentRef: ComponentRef<any> | null = null;

  constructor(
    private publicApiService: PublicApiService,
    private eventManager: AppEventManager,
    private layoutRouter: LayoutRouterService,
    private cdr: ChangeDetectorRef,
    private router: Router,
  ) {}

  ngOnInit(): void {
    // Monitor route changes to determine if we're on an auth page
    this.subscriptions.add(
      this.router.events.pipe(
        filter(event => event instanceof NavigationEnd)
      ).subscribe((event: any) => {
        const url = event.url;
        this.isAuthPage = url.includes('/login') || url.includes('/register');
        this.cdr.detectChanges();
      })
    );

    // Set up initial routes for the main content
    this.setupInitialRoutes();

    // Subscribe to all application events
    this.setupEventSubscriptions();
  }

  ngOnDestroy(): void {
    // Clean up all subscriptions
    this.subscriptions.unsubscribe();
  }

  private setupInitialRoutes(): void {
    // Check current URL
    const currentUrl = this.router.url;
    if (!currentUrl.includes('/login') && !currentUrl.includes('/register')) {
      // Only set up initial routes if not on auth page
      this.router.navigate([{
        outlets: {
          toolsPane: ['search-query'],
          headerPane: ['nav-header'],
          cornerPane: ['library-bag'],
          resultsPane: ['search-results'],
          featurePane: ['featured-books'],
          footerPane: ['nav-footer']
        }
      }]);
    }
  }

  /**
   * Set up subscriptions to all relevant application events
   */
  private setupEventSubscriptions(): void {
    this.subscriptions.add(
      this.eventManager.appBus$.subscribe((event: AppEvent) => {
        switch (event.eventName) {
          case "setPanelOutput":
            this.setPanelOutput(event.payload.outletName, event.payload.componentPath);
            break;
          case "showOverlay":
            console.log("Showing overlay:", event.payload);
            this.overlayVisibility[event.payload] = true;
            break;
          case "hideOverlay":
            console.log("Hiding overlay:", event.payload);
            this.overlayVisibility[event.payload] = false;
            break;
        }
      })
    );
  }

  /**
   * Uses the router to dynamically load a component into an outlet
   */
  private setPanelOutput(outletName: string, componentPath: string): void {
    this.router.navigate([{ outlets: { [outletName]: [componentPath] } }]).then(() => {
      this.cdr.detectChanges(); // Force change detection
    });
  }
}
