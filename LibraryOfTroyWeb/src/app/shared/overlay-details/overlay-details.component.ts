// Update overlay-details.component.ts to handle authentication

import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { BookDetailResponse } from '../../dtos/responses/book-detail-response';
import { AppEventManager } from '../../services/app-event-manager';
import { AuthService } from '../../services/auth-service';

@Component({
  selector: 'app-overlay-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './overlay-details.component.html',
  styleUrl: './overlay-details.component.scss'
})
export class OverlayDetailsComponent implements OnInit, OnDestroy {
  book?: BookDetailResponse | null;
  loading: boolean = false;
  error: Error | undefined;
  isAuthenticated: boolean = false;
  private subscriptions = new Subscription();

  constructor(
    private eventManager: AppEventManager,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Check authentication state
    this.isAuthenticated = this.authService.isAuthenticated();

    this.subscriptions.add(
      this.eventManager.bookSelectedBus$.subscribe((book) => {
        this.book = book;
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  closeOverlay(): void {
    this.eventManager.hideOverlay("rightOverlayPane");
  }

  checkoutBook(): void {
    if (!this.isAuthenticated) {
      // Redirect to login if not authenticated
      this.closeOverlay();
      this.router.navigate(['/login'], {
        queryParams: { returnUrl: this.router.url }
      });
      return;
    }

    if (this.book) {
      this.eventManager.emitTryCheckoutBook(this.book);
    }
  }
}
