import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule],
  template: `
    <div class="dashboard-container">
      <h1>Administrator Dashboard</h1>
      <mat-card>
        <mat-card-content>
          <p>Admin features coming soon...</p>
          <ul>
            <li>User Management</li>
            <li>Holiday Calendar Management</li>
            <li>Leave Entitlement Allocation</li>
            <li>System Reports</li>
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
export class AdminDashboardComponent {}
