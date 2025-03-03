import { Component, EventEmitter, Input, Output, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { BookDetailResponse } from '../../dtos/responses/book-detail-response';
import { AppEventManager } from '../../services/app-event-manager';
import { Guid } from '../../model/guid';

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './search-results.component.html',
  styleUrl: './search-results.component.scss'
})
export class SearchResultsComponent implements OnInit, OnDestroy {
  @Input() books: BookDetailResponse[] = [];
  @Output() bookSelected = new EventEmitter<Guid>();

  private subscriptions = new Subscription();
  selectedBookId: Guid | null = null;

  constructor(private eventManager: AppEventManager) {}

  ngOnInit(): void {
    // Subscribe to search results updates
    this.subscriptions.add(
      this.eventManager.resultsBus$.subscribe((results: BookDetailResponse[]) => {
        this.books = results;
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  selectBook(book: BookDetailResponse): void {
    this.selectedBookId = book.id;
    this.eventManager.emitBookDetails(book);
  }

  isBookAvailable(book: BookDetailResponse): boolean {
    // Handle both possible property names
    return book.available === true || book.Available === true;
  }

  getStarRating(rating: number | null | undefined): string {
    if (rating === null || rating === undefined || rating < 0) {
      return '☆☆☆☆☆';
    }

    const fullStars = Math.floor(rating);
    const halfStar = rating % 1 >= 0.5;
    let stars = '';

    // Add full stars
    for (let i = 0; i < fullStars; i++) {
      stars += '★';
    }

    // Add half star if needed
    if (halfStar) {
      stars += '⯨';
    }

    // Add empty stars to make 5 stars total
    const emptyStars = 5 - fullStars - (halfStar ? 1 : 0);
    for (let i = 0; i < emptyStars; i++) {
      stars += '☆';
    }

    return stars;
  }

  getRatingClass(rating: number | null | undefined): string {
    if (rating === null || rating === undefined || rating < 0) {
      return 'no-rating';
    }

    if (rating >= 4) {
      return 'high-rating';
    } else if (rating >= 3) {
      return 'medium-rating';
    } else {
      return 'low-rating';
    }
  }
}
