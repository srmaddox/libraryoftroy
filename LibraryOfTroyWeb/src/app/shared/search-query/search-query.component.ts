import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';
import { PublicApiService } from '../../services/public-api-service';
import { BookDetailResponse } from '../../dtos/responses/book-detail-response';
import { AppEventManager } from '../../services/app-event-manager';

@Component({
  selector: 'app-search-query',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './search-query.component.html',
  styleUrl: './search-query.component.scss'
})
export class SearchQueryComponent implements OnInit {
  @ViewChild('searchInput') searchInput!: ElementRef<HTMLInputElement>;

  searchQuery: string = '';
  isSearching: boolean = false;
  private searchTerms = new Subject<string>();
  private focused: boolean = false;

  constructor(
    private publicApiService: PublicApiService,
    private eventManager: AppEventManager
  ) {}

  ngOnInit(): void {
    // Set up the search debounce to avoid too many API calls
    this.searchTerms.pipe(
      debounceTime(250), // wait 250ms after each keystroke
      distinctUntilChanged() // ignore if search term hasn't changed
    ).subscribe(term => {
      if (term.trim() === '') {
        this.fetchDefaultBooks();
      } else {
        this.performSearch(term);
      }
    });

    // Initial fetch when component loads
    this.fetchDefaultBooks();
  }

  // Remember focus state
  onFocus(): void {
    this.focused = true;
  }

  onBlur(): void {
    this.focused = false;
  }

  // This method is called when the user types in the search box
  onSearchInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchTerms.next(input.value);
  }

  // Manually trigger search (e.g., when clicking a search button)
  search(query: string): void {
    this.searchTerms.next(query);
  }

  private performSearch(query: string): void {
    this.isSearching = true;
    this.searchQuery = query;

    this.publicApiService.listBooks(15, 0, query).subscribe({
      next: (books) => {
        this.eventManager.emitSearchResults(books, query); // Send results via AppEventManager
        this.isSearching = false;
      },
      error: (error) => {
        console.error('Error searching for books:', error);
        this.eventManager.emitSearchResults([]); // Emit empty results on error
        this.isSearching = false;
      }
    });
  }

  // Fetch default books when no search input is provided
  private fetchDefaultBooks(): void {
    this.publicApiService.listBooks(15, 0, '').subscribe({
      next: (books) => {
        this.eventManager.emitSearchResults(books);
      },
      error: (error) => {
        console.error('Error fetching default books:', error);
      }
    });
  }

/*
  // Perform the actual search request
  private performSearch(query: string): void {
    this.isSearching = true;
    this.searchQuery = query;

    // Store cursor position before API call
    const cursorPosition = this.searchInput?.nativeElement?.selectionStart || 0;

    // If query is empty, we'll just get the default list of books
    this.publicApiService.listBooks(15, 0, query).subscribe({
      next: (books) => {
        // Emit search results through the global event manager
        this.eventManager.emit({
          type: AppEventType.SEARCH_RESULTS,
          payload: books
        });

        this.isSearching = false;

        // Restore focus if it was focused before
        if (this.focused) {
          setTimeout(() => {
            this.searchInput.nativeElement.focus();
            // Restore cursor position
            this.searchInput.nativeElement.setSelectionRange(cursorPosition, cursorPosition);
          });
        }
      },
      error: (error) => {
        console.error('Error searching for books:', error);

        // Emit empty results in case of error
        this.eventManager.emit({
          type: AppEventType.SEARCH_RESULTS,
          payload: []
        });

        this.isSearching = false;

        // Restore focus if it was focused before
        if (this.focused) {
          setTimeout(() => {
            this.searchInput.nativeElement.focus();
            // Restore cursor position
            this.searchInput.nativeElement.setSelectionRange(cursorPosition, cursorPosition);
          });
        }
      }
    });
  }
 */
}
