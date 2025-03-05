import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PublicApiService } from '../../services/public-api-service';
import { BookDetailResponse } from '../../dtos/responses/book-detail-response';

@Component({
  selector: 'app-featured-books',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './featured-books.component.html',
  styleUrls: ['./featured-books.component.scss']
})
export class FeaturedBooksComponent implements OnInit {
  @Input() count: number = 4; // Default to showing 4 books
  books: BookDetailResponse[] = [];

  constructor(private publicApiService: PublicApiService) {}

  ngOnInit(): void {
    this.publicApiService.getFeaturedBooks().subscribe((books) => {
      this.books = this.getRandomSelection(books, this.count);
    });
  }

  getRandomSelection<T>(array: T[], count: number): T[] {
    return array.sort(() => 0.5 - Math.random()).slice(0, count);
  }
}
