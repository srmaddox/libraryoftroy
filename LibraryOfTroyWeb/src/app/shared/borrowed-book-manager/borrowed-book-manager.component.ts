import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { LibrarianApiService } from '../../services/librarian-api-service';
import { AppEventManager } from '../../services/app-event-manager';
import {Guid} from '../../model/guid';

// Define the BorrowedBookResponse interface
interface BorrowedBookResponse {
  checkoutId: string;
  book: {
    id: Guid;
    title: string;
    author: string;
    description?: string;
    coverImageUrl?: string;
  };
  customerId: string;
  customerName: string;
  checkoutDate: string;
  dueDate: string;
  returnDate?: string;
  isOverdue: boolean;
  daysOverdue: number;
}

@Component({
  selector: 'app-borrowed-book-manager',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './borrowed-book-manager.component.html',
  styleUrl: './borrowed-book-manager.component.scss'
})
export class BorrowedBookManagerComponent implements OnInit, OnDestroy {
  borrowedBooks: BorrowedBookResponse[] = [];
  filteredBooks: BorrowedBookResponse[] = [];
  selectedBooks: BorrowedBookResponse[] = [];

  isLoading: boolean = true;
  showReturned: boolean = false;
  showOverdueOnly: boolean = false;
  searchTerm: string = '';
  currentlyReturningBooks: boolean = false;

  private subscriptions = new Subscription();

  constructor(
    private librarianService: LibrarianApiService,
    private eventManager: AppEventManager
  ) {}

  ngOnInit(): void {
    this.loadBorrowedBooks();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadBorrowedBooks(): void {
    this.isLoading = true;
    this.selectedBooks = [];

    this.librarianService.getBorrowedBooks(this.showReturned).subscribe({
      next: (books) => {
        this.borrowedBooks = books;
        this.applyFilters();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading borrowed books:', error);
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    let filtered = [...this.borrowedBooks];

    // Apply overdue filter if enabled
    if (this.showOverdueOnly) {
      filtered = filtered.filter(book => book.isOverdue);
    }

    // Apply search term if provided
    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(book =>
        book.book.title.toLowerCase().includes(term) ||
        book.book.author.toLowerCase().includes(term) ||
        book.customerName.toLowerCase().includes(term)
      );
    }

    this.filteredBooks = filtered;

    // Clear any selected books that are no longer in the filtered list
    this.selectedBooks = this.selectedBooks.filter(selected =>
      this.filteredBooks.some(book => book.checkoutId === selected.checkoutId)
    );
  }

  getStatusText(book: BorrowedBookResponse): string {
    if (book.returnDate) {
      return 'Returned';
    }
    return book.isOverdue ? 'Overdue' : 'Active';
  }

  isBookSelected(book: BorrowedBookResponse): boolean {
    return this.selectedBooks.some(selected => selected.checkoutId === book.checkoutId);
  }

  toggleBookSelection(book: BorrowedBookResponse): void {
    if (this.isBookSelected(book)) {
      this.selectedBooks = this.selectedBooks.filter(
        selected => selected.checkoutId !== book.checkoutId
      );
    } else {
      this.selectedBooks.push(book);
    }
  }

  areAllBooksSelected(): boolean {
    const selectableBooks = this.filteredBooks.filter(book => !book.returnDate);
    return selectableBooks.length > 0 &&
      selectableBooks.every(book => this.isBookSelected(book));
  }

  someButNotAllBooksSelected(): boolean {
    const selectableBooks = this.filteredBooks.filter(book => !book.returnDate);
    return this.selectedBooks.length > 0 &&
      this.selectedBooks.length < selectableBooks.length;
  }

  toggleSelectAll(): void {
    if (this.areAllBooksSelected()) {
      this.selectedBooks = [];
    } else {
      this.selectedBooks = this.filteredBooks.filter(book => !book.returnDate);
    }
  }

  returnBook(book: BorrowedBookResponse): void {
    this.currentlyReturningBooks = true;

    this.librarianService.checkInBook(book.book.id).subscribe({
      next: () => {
        // Update the book in our arrays
        const updatedBook = {
          ...book,
          returnDate: new Date().toISOString(),
          isOverdue: false,
          daysOverdue: 0
        };

        this.updateBookInArrays(book.checkoutId, updatedBook);
        this.selectedBooks = this.selectedBooks.filter(
          selected => selected.checkoutId !== book.checkoutId
        );

        this.currentlyReturningBooks = false;

        // Refresh search results to show updated availability
        this.eventManager.emitSearchResultsInvalidated();
      },
      error: (error) => {
        console.error('Error returning book:', error);
        this.currentlyReturningBooks = false;
      }
    });
  }

  returnSelectedBooks(): void {
    if (this.selectedBooks.length === 0) return;

    this.currentlyReturningBooks = true;
    let completedCount = 0;
    let errorCount = 0;

    for (const book of this.selectedBooks) {
      this.librarianService.checkInBook(book.book.id).subscribe({
        next: () => {
          // Update the book in our arrays
          const updatedBook = {
            ...book,
            returnDate: new Date().toISOString(),
            isOverdue: false,
            daysOverdue: 0
          };

          this.updateBookInArrays(book.checkoutId, updatedBook);

          completedCount++;
          if (completedCount + errorCount === this.selectedBooks.length) {
            this.selectedBooks = [];
            this.currentlyReturningBooks = false;

            // Refresh search results to show updated availability
            this.eventManager.emitSearchResultsInvalidated();
          }
        },
        error: (error) => {
          console.error(`Error returning book ${book.book.title}:`, error);
          errorCount++;
          if (completedCount + errorCount === this.selectedBooks.length) {
            this.currentlyReturningBooks = false;
          }
        }
      });
    }
  }

  private updateBookInArrays(checkoutId: string, updatedBook: BorrowedBookResponse): void {
    // Update in borrowedBooks array
    const bookIndex = this.borrowedBooks.findIndex(b => b.checkoutId === checkoutId);
    if (bookIndex !== -1) {
      this.borrowedBooks[bookIndex] = updatedBook;
    }

    // Update in filteredBooks array
    const filteredIndex = this.filteredBooks.findIndex(b => b.checkoutId === checkoutId);
    if (filteredIndex !== -1) {
      this.filteredBooks[filteredIndex] = updatedBook;
    }
  }

  closePanel(): void {
    this.eventManager.hideOverlay("rightOverlayPane");
  }
}
