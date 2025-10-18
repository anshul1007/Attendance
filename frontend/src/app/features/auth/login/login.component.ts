import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../core/auth/auth.service';
import { LoginRequest } from '../../../shared/models/user.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule
  ],
  template: `
    <div class="login-container">
      <mat-card class="login-card">
        <mat-card-header>
          <mat-card-title>VermillionIndia</mat-card-title>
          <mat-card-subtitle>Attendance Management System</mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          <form #loginForm="ngForm" (ngSubmit)="onSubmit()">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Email</mat-label>
              <input 
                matInput 
                type="email" 
                [(ngModel)]="credentials.email" 
                name="email" 
                required 
                email
                placeholder="your.email@company.com">
              <mat-icon matPrefix>email</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Password</mat-label>
              <input 
                matInput 
                [type]="hidePassword() ? 'password' : 'text'" 
                [(ngModel)]="credentials.password" 
                name="password" 
                required
                placeholder="Enter your password">
              <mat-icon matPrefix>lock</mat-icon>
              <button 
                mat-icon-button 
                matSuffix 
                type="button"
                (click)="hidePassword.set(!hidePassword())">
                <mat-icon>{{hidePassword() ? 'visibility_off' : 'visibility'}}</mat-icon>
              </button>
            </mat-form-field>

            @if (errorMessage()) {
              <div class="error-message">
                <mat-icon>error</mat-icon>
                {{ errorMessage() }}
              </div>
            }

            <button 
              mat-raised-button 
              color="primary" 
              type="submit" 
              class="full-width login-button"
              [disabled]="!loginForm.valid || loading()">
              @if (loading()) {
                <mat-spinner diameter="20"></mat-spinner>
              } @else {
                Login
              }
            </button>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .login-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #8B3A3A 0%, #6B2C2C 100%);
      padding: 20px;
    }

    .login-card {
      width: 100%;
      max-width: 400px;
      padding: 20px;
    }

    mat-card-header {
      display: flex;
      flex-direction: column;
      align-items: center;
      margin-bottom: 20px;
    }

    mat-card-title {
      font-size: 32px;
      font-weight: 700;
      text-align: center;
      margin-bottom: 8px;
      background: linear-gradient(135deg, #8B3A3A 0%, #6B2C2C 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
      letter-spacing: 1px;
    }

    mat-card-subtitle {
      text-align: center;
    }

    .full-width {
      width: 100%;
    }

    mat-form-field {
      margin-bottom: 16px;
    }

    .login-button {
      height: 48px;
      font-size: 16px;
      margin-top: 16px;
    }

    .error-message {
      display: flex;
      align-items: center;
      gap: 8px;
      color: #f44336;
      background: #ffebee;
      padding: 12px;
      border-radius: 4px;
      margin-bottom: 16px;
      font-size: 14px;
    }

    mat-spinner {
      margin: 0 auto;
    }
  `]
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  credentials: LoginRequest = {
    email: '',
    password: ''
  };

  loading = signal(false);
  errorMessage = signal('');
  hidePassword = signal(true);

  onSubmit(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.authService.login(this.credentials).subscribe({
      next: (response) => {
        this.loading.set(false);
        // Navigate based on user role
        const role = response.user.role;
        if (role === 'Administrator') {
          this.router.navigate(['/admin']);
        } else if (role === 'Manager') {
          this.router.navigate(['/manager']);
        } else {
          this.router.navigate(['/employee']);
        }
      },
      error: (error) => {
        this.loading.set(false);
        this.errorMessage.set(error.message || 'Login failed. Please try again.');
      }
    });
  }
}
