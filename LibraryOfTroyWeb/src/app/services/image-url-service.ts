import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ImageUrlService {
  private readonly baseUrl = 'books/';

  constructor() {}

  /**
   * Transforms a book cover URL to include the proper base path
   * @param url Original cover image URL from the API
   * @returns Properly formatted URL for displaying in the application
   */
  getBookCoverUrl(url: string | null | undefined): string {
    if (!url) {
      return ''; // Return empty string for null/undefined URLs
    }

    // If the URL is already absolute (starts with http:// or https://), return it as is
    if (url.startsWith('http://') || url.startsWith('https://')) {
      return url;
    }

    // If the URL already includes the base path, return it as is
    if (url.startsWith(this.baseUrl)) {
      return url;
    }

    // Otherwise, prepend the base path
    return this.baseUrl + url;
  }
}
