import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-manager-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule],
  template: `
    <div class="dashboard-container">
      <h1>Manager Dashboard</h1>
      <mat-card>
        <mat-card-content>
          <p>Manager features coming soon...</p>
          <ul>
            <li>Approve/Reject Leave Requests</li>
            <li>Approve/Reject Attendance</li>
            <li>View Team Reports</li>
          </ul>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: 24px;
    }
  `]
})
export class ManagerDashboardComponent {}
