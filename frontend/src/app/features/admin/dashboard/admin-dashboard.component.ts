import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AdminService, PublicHoliday } from '../../../core/services/admin.service';
import { AuthService } from '../../../core/auth/auth.service';
import { User } from '../../../shared/models/admin.model';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss'
})
export class AdminDashboardComponent implements OnInit {
  private adminService = inject(AdminService);
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);

  currentUser = this.authService.currentUser;
  activeTab = signal<'users' | 'leaves' | 'holidays'>('users');
  users = signal<User[]>([]);
  holidays = signal<PublicHoliday[]>([]);
  loading = signal(false);
  message = signal('');
  error = signal(false);
  
  showUserForm = signal(false);
  showLeaveForm = signal(false);
  showHolidayForm = signal(false);
  
  userForm: FormGroup;
  leaveForm: FormGroup;
  holidayForm: FormGroup;

  constructor() {
    this.userForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      employeeId: ['', Validators.required],
      role: ['Employee', Validators.required],
      managerId: [''],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });

    this.leaveForm = this.fb.group({
      userId: ['', Validators.required],
      year: [new Date().getFullYear(), Validators.required],
      casualLeaveBalance: [12, [Validators.required, Validators.min(0)]],
      earnedLeaveBalance: [15, [Validators.required, Validators.min(0)]],
      compensatoryOffBalance: [0, [Validators.required, Validators.min(0)]]
    });

    this.holidayForm = this.fb.group({
      name: ['', Validators.required],
      date: ['', Validators.required],
      description: ['']
    });
  }

  ngOnInit() {
    this.loadUsers();
    this.loadHolidays();
  }

  setActiveTab(tab: 'users' | 'leaves' | 'holidays') {
    this.activeTab.set(tab);
  }

  loadUsers() {
    this.loading.set(true);
    this.adminService.getAllUsers().subscribe({
      next: (users) => {
        this.users.set(users);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading users:', err);
        this.loading.set(false);
      }
    });
  }

  loadHolidays() {
    this.adminService.getPublicHolidays(new Date().getFullYear()).subscribe({
      next: (holidays) => {
        this.holidays.set(holidays);
      },
      error: (err) => {
        console.error('Error loading holidays:', err);
      }
    });
  }

  toggleUserForm() {
    this.showUserForm.set(!this.showUserForm());
    if (!this.showUserForm()) {
      this.userForm.reset({ role: 'Employee' });
    }
  }

  submitUserForm() {
    if (this.userForm.invalid) return;

    this.loading.set(true);
    const formValue = this.userForm.value;
    
    const request = {
      ...formValue,
      managerId: formValue.managerId || undefined
    };

    this.adminService.createUser(request).subscribe({
      next: () => {
        this.message.set('User created successfully!');
        this.error.set(false);
        this.userForm.reset({ role: 'Employee' });
        this.showUserForm.set(false);
        this.loadUsers();
        this.loading.set(false);
      },
      error: (err) => {
        this.message.set(err.message || 'Failed to create user');
        this.error.set(true);
        this.loading.set(false);
      }
    });
  }

  toggleLeaveForm() {
    this.showLeaveForm.set(!this.showLeaveForm());
    if (!this.showLeaveForm()) {
      this.leaveForm.reset({
        year: new Date().getFullYear(),
        casualLeaveBalance: 12,
        earnedLeaveBalance: 15,
        compensatoryOffBalance: 0
      });
    }
  }

  submitLeaveForm() {
    if (this.leaveForm.invalid) return;

    this.loading.set(true);
    this.adminService.allocateLeaveEntitlement(this.leaveForm.value).subscribe({
      next: () => {
        this.message.set('Leave entitlement allocated successfully!');
        this.error.set(false);
        this.leaveForm.reset({
          year: new Date().getFullYear(),
          casualLeaveBalance: 12,
          earnedLeaveBalance: 15,
          compensatoryOffBalance: 0
        });
        this.showLeaveForm.set(false);
        this.loading.set(false);
      },
      error: (err) => {
        this.message.set(err.message || 'Failed to allocate leave entitlement');
        this.error.set(true);
        this.loading.set(false);
      }
    });
  }

  toggleHolidayForm() {
    this.showHolidayForm.set(!this.showHolidayForm());
    if (!this.showHolidayForm()) {
      this.holidayForm.reset();
    }
  }

  submitHolidayForm() {
    if (this.holidayForm.invalid) return;

    this.loading.set(true);
    this.adminService.createPublicHoliday(this.holidayForm.value).subscribe({
      next: () => {
        this.message.set('Holiday created successfully!');
        this.error.set(false);
        this.holidayForm.reset();
        this.showHolidayForm.set(false);
        this.loadHolidays();
        this.loading.set(false);
      },
      error: (err) => {
        this.message.set(err.message || 'Failed to create holiday');
        this.error.set(true);
        this.loading.set(false);
      }
    });
  }

  deleteHoliday(holidayId: string) {
    if (!confirm('Are you sure you want to delete this holiday?')) return;

    this.adminService.deletePublicHoliday(holidayId).subscribe({
      next: () => {
        this.message.set('Holiday deleted successfully');
        this.error.set(false);
        this.loadHolidays();
      },
      error: (err) => {
        this.message.set(err.message || 'Failed to delete holiday');
        this.error.set(true);
      }
    });
  }

  getRoleBadgeClass(role: string): string {
    switch(role) {
      case 'Administrator': return 'badge-danger';
      case 'Manager': return 'badge-warning';
      case 'Employee': return 'badge-secondary';
      default: return 'badge-secondary';
    }
  }

  getManagers(): User[] {
    return this.users().filter(u => u.role === 'Manager' || u.role === 'Administrator');
  }
}
