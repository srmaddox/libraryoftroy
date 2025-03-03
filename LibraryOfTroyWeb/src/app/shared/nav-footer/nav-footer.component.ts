// Updated src/app/shared/nav-footer/nav-footer.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AppEventManager } from '../../services/app-event-manager';
import { AuthService } from '../../services/auth-service';

@Component({
  selector: 'app-nav-footer',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './nav-footer.component.html',
  styleUrl: './nav-footer.component.scss'
})
export class NavFooterComponent implements OnInit {
  isAuthenticated = false;

  constructor(
    private eventManager: AppEventManager,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    // Set initial authentication state
    this.isAuthenticated = this.authService.isAuthenticated();
  }

  navigateToBorrowedBookManager(): void {
    this.eventManager.navigateToBorrowedBookManager();
  }

  navigateToBookInventoryManager(): void {
    this.eventManager.showOverlay("rightOverlayPane");
    this.eventManager.setPanelOutput('rightOverlayPane', 'book-inventory-manager');
  }

  viewMyBorrows(): void {
    // Navigate to personal borrows view
    // For now, this could just open a notification or alert
    alert('My Borrows feature coming soon!');
  }
}
