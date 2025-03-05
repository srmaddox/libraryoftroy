// Updated nav-header.component.ts
import {Component, OnDestroy, OnInit} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth-service';
import { AppEventManager } from '../../services/app-event-manager';
import { ClickOutsideDirective } from '../../directives/click-outside.directive';
import {Subscription} from 'rxjs';

@Component({
  selector: 'app-nav-header',
  standalone: true,
  imports: [CommonModule, RouterModule, ClickOutsideDirective],
  templateUrl: './nav-header.component.html',
  styleUrl: './nav-header.component.scss'
})
export class NavHeaderComponent implements OnInit, OnDestroy {
  isUserMenuOpen: boolean = false;
  isAuthenticated: boolean = false;
  private subscriptions = new Subscription();

  constructor(
    public authService: AuthService,
    private eventManager: AppEventManager
  ) {}

  ngOnInit(): void {
    this.isAuthenticated = this.authService.isAuthenticated();

    this.subscriptions.add(
      this.eventManager.uiRefreshBus$.subscribe(() => {
        this.isAuthenticated = this.authService.isAuthenticated();
        this.closeUserMenu();
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  toggleUserMenu(): void {
    this.isUserMenuOpen = !this.isUserMenuOpen;
  }

  closeUserMenu(): void {
    this.isUserMenuOpen = false;
  }

  logout(): void {
    this.authService.logout();
    this.eventManager.refreshUI();
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
