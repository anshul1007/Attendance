import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AttendanceService } from '../../../core/services/attendance.service';
import { LeaveService } from '../../../core/services/leave.service';
import { AuthService } from '../../../core/auth/auth.service';
import { TodayAttendanceStatus } from '../../../shared/models/attendance.model';
import { LeaveBalance } from '../../../shared/models/leave.model';

@Component({
  selector: 'app-employee-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  template: `
    <div class="dashboard-container">
      <div class="header">
        <h1>Welcome, {{ currentUser()?.firstName }}!</h1>
        <p class="subtitle">{{ getCurrentDate() }}</p>
      </div>

      <div class="cards-grid">
        <!-- Attendance Card -->
        <mat-card class="attendance-card">
          <mat-card-header>
            <mat-icon mat-card-avatar>schedule</mat-icon>
            <mat-card-title>Today's Attendance</mat-card-title>
          </mat-card-header>
          
          <mat-card-content>
            @if (loadingAttendance()) {
              <div class="loading-container">
                <mat-spinner diameter="40"></mat-spinner>
              </div>
            } @else {
              @if (todayStatus()) {
                <div class="attendance-status">
                  @if (!todayStatus()!.hasLoggedIn) {
                    <p class="status-text">You haven't logged in yet today</p>
                    <button 
                      mat-raised-button 
                      color="primary" 
                      (click)="onLogin()"
                      [disabled]="processingAttendance()">
                      <mat-icon>login</mat-icon>
                      Login
                    </button>
                  } @else if (!todayStatus()!.hasLoggedOut) {
                    <p class="status-text logged-in">
                      Logged in at {{ formatTime(todayStatus()!.loginTime!) }}
                    </p>
                    <button 
                      mat-raised-button 
                      color="warn" 
                      (click)="onLogout()"
                      [disabled]="processingAttendance()">
                      <mat-icon>logout</mat-icon>
                      Logout
                    </button>
                  } @else {
                    <p class="status-text completed">
                      ✓ Attendance completed for today
                    </p>
                  }
                </div>
              }
            }
          </mat-card-content>
        </mat-card>

        <!-- Leave Balance Card -->
        <mat-card class="leave-balance-card">
          <mat-card-header>
            <mat-icon mat-card-avatar>event_available</mat-icon>
            <mat-card-title>Leave Balance</mat-card-title>
          </mat-card-header>
          
          <mat-card-content>
            @if (loadingLeaveBalance()) {
              <div class="loading-container">
                <mat-spinner diameter="40"></mat-spinner>
              </div>
            } @else if (leaveBalance()) {
              <div class="balance-grid">
                <div class="balance-item">
                  <span class="balance-label">Casual Leave</span>
                  <span class="balance-value">{{ leaveBalance()!.casualLeaveBalance }}</span>
                </div>
                <div class="balance-item">
                  <span class="balance-label">Earned Leave</span>
                  <span class="balance-value">{{ leaveBalance()!.earnedLeaveBalance }}</span>
                </div>
                <div class="balance-item">
                  <span class="balance-label">Comp Off</span>
                  <span class="balance-value">{{ leaveBalance()!.compensatoryOffBalance }}</span>
                </div>
              </div>
            }
          </mat-card-content>
        </mat-card>

        <!-- Quick Actions Card -->
        <mat-card class="quick-actions-card">
          <mat-card-header>
            <mat-icon mat-card-avatar>dashboard</mat-icon>
            <mat-card-title>Quick Actions</mat-card-title>
          </mat-card-header>
          
          <mat-card-content>
            <div class="actions-grid">
              <button mat-raised-button class="action-button">
                <mat-icon>event_note</mat-icon>
                Request Leave
              </button>
              <button mat-raised-button class="action-button">
                <mat-icon>history</mat-icon>
                Attendance History
              </button>
              <button mat-raised-button class="action-button">
                <mat-icon>list_alt</mat-icon>
                Leave Requests
              </button>
              <button mat-raised-button class="action-button">
                <mat-icon>person</mat-icon>
                My Profile
              </button>
            </div>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: 24px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .header {
      margin-bottom: 32px;
    }

    .header h1 {
      margin: 0;
      font-size: 32px;
      font-weight: 500;
    }

    .subtitle {
      margin: 8px 0 0 0;
      color: #666;
      font-size: 16px;
    }

    .cards-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
      gap: 24px;
    }

    mat-card {
      height: 100%;
    }

    mat-card-header {
      margin-bottom: 16px;
    }

    mat-icon[mat-card-avatar] {
      font-size: 40px;
      width: 40px;
      height: 40px;
    }

    .loading-container {
      display: flex;
      justify-content: center;
      padding: 40px 0;
    }

    .attendance-status {
      text-align: center;
      padding: 20px 0;
    }

    .status-text {
      font-size: 18px;
      margin-bottom: 20px;
      font-weight: 500;
    }

    .status-text.logged-in {
      color: #4caf50;
    }

    .status-text.completed {
      color: #2196f3;
    }

    .balance-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 16px;
    }

    .balance-item {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 16px;
      background: #f5f5f5;
      border-radius: 8px;
    }

    .balance-label {
      font-size: 14px;
      color: #666;
      margin-bottom: 8px;
    }

    .balance-value {
      font-size: 28px;
      font-weight: 600;
      color: #1976d2;
    }

    .actions-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 12px;
    }

    .action-button {
      height: 60px;
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .action-button mat-icon {
      margin: 0;
    }

    button[mat-raised-button] {
      width: 100%;
    }
  `]
})
export class EmployeeDashboardComponent implements OnInit {
  private attendanceService = inject(AttendanceService);
  private leaveService = inject(LeaveService);
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);

  currentUser = signal(this.authService.currentUser);
  todayStatus = signal<TodayAttendanceStatus | null>(null);
  leaveBalance = signal<LeaveBalance | null>(null);
  loadingAttendance = signal(false);
  loadingLeaveBalance = signal(false);
  processingAttendance = signal(false);

  ngOnInit(): void {
    this.loadTodayStatus();
    this.loadLeaveBalance();
  }

  loadTodayStatus(): void {
    this.loadingAttendance.set(true);
    this.attendanceService.getTodayStatus().subscribe({
      next: (status) => {
        this.todayStatus.set(status);
        this.loadingAttendance.set(false);
      },
      error: (error) => {
        console.error('Failed to load attendance status', error);
        this.loadingAttendance.set(false);
      }
    });
  }

  loadLeaveBalance(): void {
    this.loadingLeaveBalance.set(true);
    this.leaveService.getLeaveBalance().subscribe({
      next: (balance) => {
        this.leaveBalance.set(balance);
        this.loadingLeaveBalance.set(false);
      },
      error: (error) => {
        console.error('Failed to load leave balance', error);
        this.loadingLeaveBalance.set(false);
      }
    });
  }

  onLogin(): void {
    this.processingAttendance.set(true);
    this.attendanceService.login().subscribe({
      next: (response) => {
        this.processingAttendance.set(false);
        this.loadTodayStatus();
        
        let message = `Login recorded at ${this.formatTime(response.loginTime)}`;
        if (response.isWeekend || response.isPublicHoliday) {
          message += ' - Eligible for compensatory off';
        }
        
        this.snackBar.open(message, 'Close', { duration: 5000 });
      },
      error: (error) => {
        this.processingAttendance.set(false);
        this.snackBar.open(error.message || 'Failed to record login', 'Close', { duration: 3000 });
      }
    });
  }

  onLogout(): void {
    const attendanceId = this.todayStatus()?.attendanceId;
    if (!attendanceId) return;

    this.processingAttendance.set(true);
    this.attendanceService.logout({ attendanceId }).subscribe({
      next: (response) => {
        this.processingAttendance.set(false);
        this.loadTodayStatus();
        
        let message = `Logout recorded. Duration: ${response.duration}`;
        if (response.compensatoryOffEarned) {
          message += ' - Compensatory off earned!';
        }
        
        this.snackBar.open(message, 'Close', { duration: 5000 });
      },
      error: (error) => {
        this.processingAttendance.set(false);
        this.snackBar.open(error.message || 'Failed to record logout', 'Close', { duration: 3000 });
      }
    });
  }

  getCurrentDate(): string {
    return new Date().toLocaleDateString('en-US', { 
      weekday: 'long', 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric' 
    });
  }

  formatTime(date: Date | string): string {
    return new Date(date).toLocaleTimeString('en-US', { 
      hour: '2-digit', 
      minute: '2-digit' 
    });
  }
}
