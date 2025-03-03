import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { AuthService } from '../services/auth-service';
import { IdentityLoginRequest } from '../dtos/requests/identity-login-request';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: '../styles/auth-forms.scss'
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  loading = false;
  error = '';
  isSubmitted = false;
  returnUrl: string = '/';

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.loginForm = this.formBuilder.group({
      userName: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      rememberMe: [false]
    });
  }

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.navigateToHome();
      return;
    }

    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  get f() { return this.loginForm.controls; }

  onSubmit() {
    this.isSubmitted = true;

    if (this.loginForm.invalid) {
      return;
    }

    this.loading = true;
    this.error = '';

    const loginRequest: IdentityLoginRequest = {
      userName: this.f['userName'].value,
      password: this.f['password'].value,
      rememberMe: this.f['rememberMe'].value
    };

    this.authService.login(loginRequest)
      .subscribe({
        next: () => {
          this.navigateToHome();
        },
        error: error => {
          this.error = error.message || 'Invalid credentials';
          this.loading = false;
        }
      });
  }

  private navigateToHome(): void {
    this.router.navigate([{
      outlets: {
        headerPane: ['nav-header'],
        cornerPane: ['library-bag'],
        toolsPane: ['search-query'],
        resultsPane: ['search-results'],
        featurePane: ['featured-books'],
        footerPane: ['nav-footer']
      }
    }]);
  }
}
