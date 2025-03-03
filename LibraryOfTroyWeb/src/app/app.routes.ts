import { Routes } from '@angular/router';
import {NavHeaderComponent} from './shared/nav-header/nav-header.component';
import {LibraryBagComponent} from './shared/library-bag/library-bag.component';
import {SearchQueryComponent} from './shared/search-query/search-query.component';
import {SearchResultsComponent} from './shared/search-results/search-results.component';
import {FeaturedBooksComponent} from './shared/featured-books/featured-books.component';
import {NavFooterComponent} from './shared/nav-footer/nav-footer.component';
import {OverlayDetailsComponent} from './shared/overlay-details/overlay-details.component';
import {OverlayCheckoutConfirmComponent} from './shared/overlay-checkout-confirm/overlay-checkout-confirm.component';
import {BorrowedBookManagerComponent} from './shared/borrowed-book-manager/borrowed-book-manager.component';
import {BookInventoryManagerComponent} from './shared/book-inventory-manager/book-inventory-manager.component';
import {LoginComponent} from './login/login.component';
import {RegisterComponent} from './register/register.component';
import {RoleGuard} from './role.guard';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'register',
    component: RegisterComponent
  },
  {
    path: 'home',
    redirectTo: '/(headerPane:nav-header//cornerPane:library-bag//toolsPane:search-query//resultsPane:search-results//featurePane:featured-books//footerPane:nav-footer)',
    pathMatch: 'full'
  },
  // Main content routes
  {
    path: 'nav-header',
    component: NavHeaderComponent,
    outlet: 'headerPane'
  },
  {
    path: 'library-bag',
    component: LibraryBagComponent,
    outlet: 'cornerPane'
  },
  {
    path: 'search-query',
    component: SearchQueryComponent,
    outlet: 'toolsPane'
  },
  {
    path: 'search-results',
    component: SearchResultsComponent,
    outlet: 'resultsPane'
  },
  {
    path: 'featured-books',
    component: FeaturedBooksComponent,
    outlet: 'featurePane'
  },
  {
    path: 'nav-footer',
    component: NavFooterComponent,
    outlet: 'footerPane'
  },

  // Overlay content routes
  {
    path: 'book-details',
    component: OverlayDetailsComponent,
    outlet: 'rightOverlayPane'
  },
  {
    path: 'checkout-confirm',
    component: OverlayCheckoutConfirmComponent,
    outlet: 'rightOverlayPane'
  },
  // Add this to your routes array in app.routes.ts

  {
    path: 'borrowed-book-manager',
    component: BorrowedBookManagerComponent,
    outlet: 'rightOverlayPane',
    canActivate: [RoleGuard],
    data: { roles: ['Librarian', 'Admin'] }
  },
  {
    path: 'book-inventory-manager',
    component: BookInventoryManagerComponent,
    outlet: 'rightOverlayPane',
    canActivate: [RoleGuard],
    data: { roles: ['Librarian', 'Admin'] }
  },
  // Default route
  { path: '', redirectTo: '/(headerPane:nav-header//cornerPane:library-bag//toolsPane:search-query//resultsPane:search-results//featurePane:featured-books//footerPane:nav-footer)', pathMatch: 'full' }
];

