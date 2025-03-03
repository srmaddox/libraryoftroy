import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { BookDetailResponse } from '../dtos/responses/book-detail-response';
import { Router } from '@angular/router';
import { PublicApiService } from './public-api-service';

export interface AppEvent {
  eventName: string;
  payload?: any;
}
/**
 * Central service for managing application-wide events
 */
@Injectable({
  providedIn: 'root'
})
export class AppEventManager {
  private appComponentBus = new Subject<AppEvent>();
  appBus$ = this.appComponentBus.asObservable();

  private resultsBus = new Subject<any>(); // New results event bus
  resultsBus$ = this.resultsBus.asObservable();

  private bookSelectedBus = new Subject<BookDetailResponse>();
  bookSelectedBus$ = this.bookSelectedBus.asObservable();

  private bookTryCheckoutBus = new Subject<BookDetailResponse>();
  bookTryCheckoutBus$ = this.bookTryCheckoutBus.asObservable();

  private refreshSearchBus = new Subject<void>();
  refreshSearchBus$ = this.refreshSearchBus.asObservable();

  selectedBook: BookDetailResponse | undefined;
  lastSearchQuery: string = '';

  constructor(private router: Router, private publicApiService: PublicApiService) {}

  setPanelOutput(outletName: string, componentPath: string): void {
    this.appComponentBus.next({
      eventName: "setPanelOutput",
      payload: { outletName, componentPath }
    });

    this.router.navigate([{ outlets: { [outletName]: [componentPath] } }]);
  }

  showOverlay(overlayName: string): void {
    this.appComponentBus.next({
      eventName: "showOverlay",
      payload: overlayName
    });
  }

  hideOverlay(overlayName: string): void {
    this.appComponentBus.next({
      eventName: "hideOverlay",
      payload: overlayName
    });
  }

  emitSearchResults(results: any, query: string =''): void {
    this.lastSearchQuery = query;
    this.resultsBus.next(results);
  }

  emitBookDetails(book: BookDetailResponse): void {
    this.selectedBook = book;
    this.bookSelectedBus.next(book);
    setTimeout(() => {
      this.bookSelectedBus.next(book);
    })
    this.showOverlay("rightOverlayPane");
    this.setPanelOutput('rightOverlayPane', 'book-details');
  }

  emitTryCheckoutBook(book: BookDetailResponse): void {
    this.selectedBook = book;

    this.showOverlay("rightOverlayPane");
    this.setPanelOutput('rightOverlayPane', 'checkout-confirm');

    // hate this hack
    // TODO - Update approach here
    this.bookTryCheckoutBus.next(book);
    setTimeout(() => {
      this.bookTryCheckoutBus.next(book);
    }, 50);
  }

  emitSearchResultsInvalidated(): void {
    this.refreshSearchBus.next();

    this.publicApiService.listBooks(15, 0, this.lastSearchQuery).subscribe({
      next: (books) => {
        this.resultsBus.next(books);
      },
      error: (error) => {
        console.error('Error refreshing search results:', error);
      }
    });
  }

  navigateToBorrowedBookManager(): void {
    this.showOverlay("rightOverlayPane");
    this.setPanelOutput('rightOverlayPane', 'borrowed-book-manager');
  }



}
