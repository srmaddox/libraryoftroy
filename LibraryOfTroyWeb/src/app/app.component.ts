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

import { BookDetailResponse } from './dtos/responses/book-detail-response';

import {
  AppEventManager,
  AppEvent
} from './services/app-event-manager';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
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

  private subscriptions = new Subscription();
  private activeComponentRef: ComponentRef<any> | null = null;

  constructor(
    private eventManager: AppEventManager,
    private cdr: ChangeDetectorRef,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.subscriptions.add(
      this.router.events.pipe(
        filter(event => event instanceof NavigationEnd)
      ).subscribe((event: any) => {
        const url = event.url;
        this.isAuthPage = url.includes('/login') || url.includes('/register');
        this.cdr.detectChanges();
      })
    );

    this.setupInitialRoutes();

    this.setupEventSubscriptions();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  private setupInitialRoutes(): void {
    const currentUrl = this.router.url;
    if (!currentUrl.includes('/login') && !currentUrl.includes('/register')) {
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

  private setPanelOutput(outletName: string, componentPath: string): void {
    this.router.navigate([{ outlets: { [outletName]: [componentPath] } }]).then(() => {
      this.cdr.detectChanges(); // Force change detection
    });
  }
}
