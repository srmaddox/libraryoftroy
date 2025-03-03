// Updated nav-header.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/AuthService';
import { AppEventManager } from '../../services/AppEventManager';
import { ClickOutsideDirective } from '../../directives/click-outside.directive';

@Component({
  selector: 'app-nav-header',
  standalone: true,
  imports: [CommonModule, RouterModule, ClickOutsideDirective],
  templateUrl: './nav-header.component.html',
  styleUrl: './nav-header.component.scss'
})
export class NavHeaderComponent implements OnInit {
  isUserMenuOpen: boolean = false;
  isAuthenticated: boolean = false;

  constructor(
    public authService: AuthService,
    private eventManager: AppEventManager
  ) {}

  ngOnInit(): void {
    // Initialize authentication state
    this.isAuthenticated = this.authService.isAuthenticated();

    // Subscribe to auth state changes if needed
  }

  toggleUserMenu(): void {
    this.isUserMenuOpen = !this.isUserMenuOpen;
  }

  closeUserMenu(): void {
    this.isUserMenuOpen = false;
  }

  logout(): void {
    this.authService.logout();
    this.closeUserMenu();
  }

  viewMyBorrows(): void {
    // Implement view borrows functionality
    this.closeUserMenu();
    // Could navigate to a personal borrows view
  }

  openBookInventory(): void {
    this.eventManager.showOverlay("rightOverlayPane");
    this.eventManager.setPanelOutput('rightOverlayPane', 'book-inventory-manager');
    this.closeUserMenu();
  }

  openBorrowedManager(): void {
    this.eventManager.navigateToBorrowedBookManager();
    this.closeUserMenu();
  }
}
