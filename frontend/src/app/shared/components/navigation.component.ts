import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-navigation',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <nav class="navbar">
      <div class="nav-container">
        <div class="nav-brand">
          <h1>Attendance System</h1>
        </div>
        
        @if (authService.currentUser) {
          <div class="nav-links">
            @if (hasRole('Employee')) {
              <a routerLink="/employee" routerLinkActive="active" class="nav-link">
                Dashboard
              </a>
            }
            @if (hasRole('Manager')) {
              <a routerLink="/manager" routerLinkActive="active" class="nav-link">
                Team Management
              </a>
            }
            @if (hasRole('Administrator')) {
              <a routerLink="/admin" routerLinkActive="active" class="nav-link">
                Admin Panel
              </a>
            }
          </div>
          
          <div class="nav-user">
            <span class="user-name">{{ authService.currentUser.firstName }} {{ authService.currentUser.lastName }}</span>
            <span class="user-role">({{ authService.currentUser.role }})</span>
            <button class="btn-logout" (click)="logout()">Logout</button>
          </div>
        }
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      position: sticky;
      top: 0;
      z-index: 1000;
    }

    .nav-container {
      max-width: 1400px;
      margin: 0 auto;
      padding: 0 20px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      min-height: 64px;
    }

    .nav-brand h1 {
      color: white;
      font-size: 24px;
      font-weight: 700;
      margin: 0;
    }

    .nav-links {
      display: flex;
      gap: 8px;
    }

    .nav-link {
      color: rgba(255, 255, 255, 0.9);
      text-decoration: none;
      padding: 8px 16px;
      border-radius: 4px;
      font-weight: 500;
      transition: all 0.3s;
      
      &:hover {
        background: rgba(255, 255, 255, 0.1);
      }
      
      &.active {
        background: rgba(255, 255, 255, 0.2);
        color: white;
      }
    }

    .nav-user {
      display: flex;
      align-items: center;
      gap: 12px;
      color: white;
    }

    .user-name {
      font-weight: 600;
    }

    .user-role {
      opacity: 0.8;
      font-size: 14px;
    }

    .btn-logout {
      background: rgba(255, 255, 255, 0.2);
      color: white;
      border: 1px solid rgba(255, 255, 255, 0.3);
      padding: 8px 16px;
      border-radius: 4px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s;
      
      &:hover {
        background: rgba(255, 255, 255, 0.3);
      }
    }

    @media (max-width: 768px) {
      .nav-container {
        flex-wrap: wrap;
        gap: 12px;
      }
      
      .nav-links {
        order: 3;
        width: 100%;
        justify-content: center;
      }
      
      .user-role {
        display: none;
      }
    }
  `]
})
export class NavigationComponent {
  authService = inject(AuthService);
  private router = inject(Router);

  hasRole(role: string): boolean {
    const userRole = this.authService.currentUser?.role;
    if (role === 'Employee') {
      return userRole === 'Employee' || userRole === 'Manager' || userRole === 'Administrator';
    }
    if (role === 'Manager') {
      return userRole === 'Manager' || userRole === 'Administrator';
    }
    if (role === 'Administrator') {
      return userRole === 'Administrator';
    }
    return false;
  }

  logout() {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: (error) => {
        console.error('Logout error:', error);
        // Still navigate to login even if logout API call fails
        this.router.navigate(['/login']);
      }
    });
  }
}
