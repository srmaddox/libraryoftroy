// Update the overlay-checkout-confirm.component.ts
import { Component, EventEmitter, Input, OnInit, Output, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { BookDetailResponse } from '../../dtos/responses/book-detail-response';
import { Guid } from '../../model/guid';
import { CustomerApiService } from '../../services/customer-api-service';
import { CheckOutRequest } from '../../dtos/requests/check-out-request';
import { AppEventManager } from '../../services/app-event-manager';
import { AuthService } from '../../services/auth-service';

@Component({
  selector: 'app-overlay-checkout-confirm',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './overlay-checkout-confirm.component.html',
  styleUrl: './overlay-checkout-confirm.component.scss'
})
export class OverlayCheckoutConfirmComponent implements OnInit, OnDestroy {
  @Input() book: BookDetailResponse | null = null;
  @Input() active: boolean = false;
  @Output() close = new EventEmitter<void>();
  @Output() checkoutComplete = new EventEmitter<boolean>();

  returnDate: Date = new Date();
  todayFormatted: string = '';
  returnDateFormatted: string = '';
  isSubmitting: boolean = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  isCheckoutSuccessful: boolean = false;
  needsLogin: boolean = false;

  private subscriptions = new Subscription();

  constructor(
    private customerApiService: CustomerApiService,
    private eventManager: AppEventManager,
    private cdr: ChangeDetectorRef,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.subscriptions.add(
      this.eventManager.bookTryCheckoutBus$.subscribe((book: BookDetailResponse) => {
        this.book = book;
        this.active = true;
        this.isCheckoutSuccessful = false;
        this.errorMessage = null;

        // Check if user is authenticated
        this.needsLogin = !this.authService.isAuthenticated();

        this.cdr.detectChanges();
      })
    );

    const today = new Date();
    this.returnDate = new Date(today);
    this.returnDate.setDate(today.getDate() + 14);

    this.todayFormatted = this.formatDate(today);
    this.returnDateFormatted = this.formatDate(this.returnDate);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  redirectToLogin(): void {
    this.cancelCheckout();
    this.router.navigate(['/login'], {
      queryParams: { returnUrl: this.router.url }
    });
  }

  confirmCheckout(): void {
    if (!this.book) {
      this.errorMessage = "No book selected for checkout";
      return;
    }

    if (!this.authService.isAuthenticated()) {
      this.needsLogin = true;
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = null;
    this.successMessage = null;

    // Get the current user's ID from AuthService
    const user = this.authService.currentUserValue;
    if (!user) {
      this.errorMessage = "User information not available. Please log in again.";
      this.isSubmitting = false;
      return;
    }

    const request: CheckOutRequest = {
      customerId: user.getUserId()
    };

    this.customerApiService.checkOutBook(this.book.id, request).subscribe({
      next: (success) => {
        this.isSubmitting = false;
        this.isCheckoutSuccessful = true;
        this.successMessage = "Checkout successful!";
        this.checkoutComplete.emit(success);
        this.cdr.detectChanges();
        // Refresh results when closing
        this.eventManager.emitSearchResultsInvalidated();
      },
      error: (error) => {
        this.isSubmitting = false;
        this.errorMessage = "Failed to check out the book. Please try again.";
        console.error('Checkout error:', error);
      }
    });
  }

  cancelCheckout(): void {
    // Reset state
    this.active = false;
    this.isCheckoutSuccessful = false;
    this.errorMessage = null;
    this.successMessage = null;
    this.needsLogin = false;
    // Refresh results when closing
    this.eventManager.emitSearchResultsInvalidated();

    this.close.emit();
    this.eventManager.hideOverlay("rightOverlayPane");
  }

  formatDate(date: Date): string {
    return date.toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}
