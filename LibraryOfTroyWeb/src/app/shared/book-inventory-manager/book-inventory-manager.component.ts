import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { LibrarianApiService } from '../../services/LibrarianApiService';
import { PublicApiService } from '../../services/PublicApiService';
import { AppEventManager } from '../../services/AppEventManager';
import { BookDetailResponse } from '../../dtos/responses/BookDetailResponse';
import { BookCreateRequest } from '../../dtos/requests/BookCreateRequest';
import { BookUpdateRequest } from '../../dtos/requests/BookUpdateRequest';
import { Guid } from '../../model/uuid';
import { Router } from '@angular/router';

enum InventoryMode {
  LIST = 'list',
  CREATE = 'create',
  EDIT = 'edit',
  DELETE = 'delete'
}

@Component({
  selector: 'app-book-inventory-manager',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './book-inventory-manager.component.html',
  styleUrl: './book-inventory-manager.component.scss'
})
export class BookInventoryManagerComponent implements OnInit, OnDestroy {
  // Mode tracking
  currentMode: InventoryMode = InventoryMode.LIST;
  InventoryMode = InventoryMode; // Expose enum to template

  // Book data
  books: BookDetailResponse[] = [];
  selectedBook: BookDetailResponse | null = null;

  // Form data for create/edit
  newBook: BookCreateRequest = {
    title: '',
    author: '',
    description: '',
    publisher: '',
    publicationDate: '',
    category: '',
    ISBN: '',
    pageCount: undefined,
    coverImageUrl: '',
    coverImageAltText: ''
  };

  // UI state
  isLoading: boolean = false;
  searchQuery: string = '';
  errorMessage: string | null = null;
  successMessage: string | null = null;

  // Pagination
  currentPage: number = 1;
  itemsPerPage: number = 10;
  totalItems: number = 0;

  private subscriptions = new Subscription();

  constructor(
    private librarianApiService: LibrarianApiService,
    private publicApiService: PublicApiService,
    private eventManager: AppEventManager,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadBooks();

    // Subscribe to search results
    this.subscriptions.add(
      this.eventManager.resultsBus$.subscribe((results: BookDetailResponse[]) => {
        this.books = results;
        this.totalItems = results.length; // This would need refinement with server-side pagination
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadBooks(page: number = 1): void {
    this.isLoading = true;
    this.currentPage = page;

    // Calculate offset for pagination
    const offset = (page - 1) * this.itemsPerPage;

    this.publicApiService.listBooks(this.itemsPerPage, offset, this.searchQuery).subscribe({
      next: (books) => {
        this.books = books;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading books:', error);
        this.errorMessage = 'Failed to load books. Please try again.';
        this.isLoading = false;
      }
    });
  }

  searchBooks(): void {
    this.loadBooks(1); // Reset to first page when searching
  }

  selectBook(book: BookDetailResponse): void {
    this.selectedBook = book;
  }

  // Mode switching methods
  switchToCreateMode(): void {
    this.currentMode = InventoryMode.CREATE;
    // Reset form
    this.newBook = {
      title: '',
      author: '',
      description: '',
      publisher: '',
      publicationDate: '',
      category: '',
      ISBN: '',
      pageCount: undefined,
      coverImageUrl: '',
      coverImageAltText: ''
    };
    this.errorMessage = null;
    this.successMessage = null;
  }

  switchToEditMode(book: BookDetailResponse): void {
    this.currentMode = InventoryMode.EDIT;
    this.selectedBook = book;
    // Initialize form with book data
    this.newBook = {
      title: book.title,
      author: book.author,
      description: book.description || '',
      publisher: book.publisher || '',
      publicationDate: book.publicationDate || '',
      category: book.category || '',
      ISBN: book.isbn || '',
      pageCount: book.pageCount,
      coverImageUrl: book.coverImageUrl || '',
      coverImageAltText: book.coverImageAltText || ''
    };
    this.errorMessage = null;
    this.successMessage = null;
  }

  switchToDeleteMode(book: BookDetailResponse): void {
    this.currentMode = InventoryMode.DELETE;
    this.selectedBook = book;
    this.errorMessage = null;
    this.successMessage = null;
  }

  switchToListMode(): void {
    this.currentMode = InventoryMode.LIST;
    this.selectedBook = null;
    this.errorMessage = null;
    this.successMessage = null;
  }

  // CRUD operations
  createBook(): void {
    if (!this.validateBookForm()) {
      return;
    }

    this.isLoading = true;
    this.librarianApiService.createBook(this.newBook).subscribe({
      next: (success) => {
        if (success) {
          this.successMessage = 'Book created successfully!';
          this.switchToListMode();
          this.loadBooks(); // Refresh the list
        } else {
          this.errorMessage = 'Failed to create book. Please try again.';
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error creating book:', error);
        this.errorMessage = 'Failed to create book. Please try again.';
        this.isLoading = false;
      }
    });
  }

  updateBook(): void {
    if (!this.selectedBook || !this.validateBookForm()) {
      return;
    }

    const updateRequest: BookUpdateRequest = { ...this.newBook };
    this.isLoading = true;

    this.librarianApiService.updateBook(this.selectedBook.id, updateRequest).subscribe({
      next: (success) => {
        if (success) {
          this.successMessage = 'Book updated successfully!';
          this.switchToListMode();
          this.loadBooks(); // Refresh the list
        } else {
          this.errorMessage = 'Failed to update book. Please try again.';
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error updating book:', error);
        this.errorMessage = 'Failed to update book. Please try again.';
        this.isLoading = false;
      }
    });
  }

  deleteBook(): void {
    if (!this.selectedBook) {
      return;
    }

    this.isLoading = true;
    this.librarianApiService.deleteBook(this.selectedBook.id).subscribe({
      next: (success) => {
        if (success) {
          this.successMessage = 'Book deleted successfully!';
          this.switchToListMode();
          this.loadBooks(); // Refresh the list
        } else {
          this.errorMessage = 'Failed to delete book. Please try again.';
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error deleting book:', error);
        this.errorMessage = 'Failed to delete book. Please try again.';
        this.isLoading = false;
      }
    });
  }

  validateBookForm(): boolean {
    if (!this.newBook.title || !this.newBook.author) {
      this.errorMessage = 'Title and author are required fields.';
      return false;
    }
    return true;
  }

  closeManager(): void {
    this.eventManager.hideOverlay("rightOverlayPane");

    // Navigate back to main view and refresh the book list
    this.router.navigate([{
      outlets: {
        toolsPane: ['search-query'],
        resultsPane: ['search-results']
      }
    }]);

    // Refresh the search results
    this.eventManager.emitSearchResultsInvalidated();
  }

  // Pagination methods
  previousPage(): void {
    if (this.currentPage > 1) {
      this.loadBooks(this.currentPage - 1);
    }
  }

  nextPage(): void {
    if (this.currentPage * this.itemsPerPage < this.totalItems) {
      this.loadBooks(this.currentPage + 1);
    }
  }

  // Date formatter helper
  formatDate(dateString: string | null | undefined): string {
    if (!dateString) return '';

    try {
      const date = new Date(dateString);
      return date.toLocaleDateString();
    } catch (e) {
      return dateString;
    }
  }
}
