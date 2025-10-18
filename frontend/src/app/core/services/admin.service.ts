import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/models/api-response.model';
import { User, CreateUserRequest, UpdateUserRequest } from '../../shared/models/admin.model';

export interface PublicHoliday {
  id?: string;
  name: string;
  date: string;
  description?: string;
}

export interface LeaveEntitlementRequest {
  userId: string;
  year: number;
  casualLeaveBalance: number;
  earnedLeaveBalance: number;
  compensatoryOffBalance: number;
}

export interface LeaveEntitlementResponse {
  userId: string;
  year: number;
  casualLeaveBalance: number;
  earnedLeaveBalance: number;
  compensatoryOffBalance: number;
}

export interface Department {
  id?: string;
  name: string;
  description?: string;
  weeklyOffDays: string;
  isActive?: boolean;
}

export interface TeamMember {
  id: string;
  employeeId: string;
  firstName: string;
  lastName: string;
  email: string;
  casualLeaveBalance?: number;
  earnedLeaveBalance?: number;
  compensatoryOffBalance?: number;
  upcomingLeaves?: UpcomingLeave[];
}

export interface UpcomingLeave {
  id: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  totalDays: number;
  status: string;
}

export interface AssignCompOffRequest {
  employeeId: string;
  days: number;
  reason: string;
}

export interface LogPastAttendanceRequest {
  employeeId: string;
  date: string;
  loginTime: string;
  logoutTime?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/admin`;

  // User Management
  getAllUsers(): Observable<User[]> {
    return this.http.get<ApiResponse<User[]>>(`${this.apiUrl}/users`)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.message || response.error || 'Failed to fetch users');
        })
      );
  }

  createUser(request: CreateUserRequest): Observable<User> {
    return this.http.post<ApiResponse<User>>(`${this.apiUrl}/users`, request)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.message || response.error || 'Failed to create user');
        })
      );
  }

  updateUser(userId: string, request: UpdateUserRequest): Observable<User> {
    return this.http.put<ApiResponse<User>>(`${this.apiUrl}/users/${userId}`, request)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.message || response.error || 'Failed to update user');
        })
      );
  }

  // Department Management
  getAllDepartments(): Observable<Department[]> {
    return this.http.get<ApiResponse<Department[]>>(`${this.apiUrl}/departments`)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.message || response.error || 'Failed to fetch departments');
        })
      );
  }

  createDepartment(request: Department): Observable<Department> {
    return this.http.post<ApiResponse<Department>>(`${this.apiUrl}/departments`, request)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.message || response.error || 'Failed to create department');
        })
      );
  }

  updateDepartment(departmentId: string, request: Department): Observable<Department> {
    return this.http.put<ApiResponse<Department>>(`${this.apiUrl}/departments/${departmentId}`, request)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.message || response.error || 'Failed to update department');
        })
      );
  }

  deleteDepartment(departmentId: string): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/departments/${departmentId}`)
      .pipe(
        map(response => {
          if (!response.success) {
            throw new Error(response.message || response.error || 'Failed to delete department');
          }
        })
      );
  }

  // Leave Entitlement Management
  allocateLeaveEntitlement(request: LeaveEntitlementRequest): Observable<LeaveEntitlementResponse> {
    return this.http.post<ApiResponse<LeaveEntitlementResponse>>(`${this.apiUrl}/leave-entitlement`, request)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.error || 'Failed to allocate leave entitlement');
        })
      );
  }

  getLeaveEntitlement(userId: string, year?: number): Observable<LeaveEntitlementResponse> {
    let params = new HttpParams();
    if (year) params = params.set('year', year.toString());
    
    return this.http.get<ApiResponse<LeaveEntitlementResponse>>(`${this.apiUrl}/leave-entitlement/${userId}`, { params })
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.error || 'Failed to fetch leave entitlement');
        })
      );
  }

  // Public Holiday Management
  createPublicHoliday(holiday: PublicHoliday): Observable<PublicHoliday> {
    return this.http.post<ApiResponse<PublicHoliday>>(`${this.apiUrl}/holidays`, holiday)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.error || 'Failed to create holiday');
        })
      );
  }

  getPublicHolidays(year?: number): Observable<PublicHoliday[]> {
    let params = new HttpParams();
    if (year) params = params.set('year', year.toString());
    
    return this.http.get<ApiResponse<PublicHoliday[]>>(`${this.apiUrl}/holidays`, { params })
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.error || 'Failed to fetch holidays');
        })
      );
  }

  deletePublicHoliday(holidayId: string): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/holidays/${holidayId}`)
      .pipe(
        map(response => {
          if (!response.success) {
            throw new Error(response.error || 'Failed to delete holiday');
          }
        })
      );
  }

  // Team Management
  getAllTeamMembers(): Observable<TeamMember[]> {
    return this.http.get<ApiResponse<TeamMember[]>>(`${this.apiUrl}/team-members`)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.message || response.error || 'Failed to fetch team members');
        })
      );
  }

  assignCompensatoryOff(request: AssignCompOffRequest): Observable<void> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/assign-comp-off`, request)
      .pipe(
        map(response => {
          if (!response.success) {
            throw new Error(response.message || response.error || 'Failed to assign compensatory off');
          }
        })
      );
  }

  logPastAttendance(request: LogPastAttendanceRequest): Observable<void> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/log-past-attendance`, request)
      .pipe(
        map(response => {
          if (!response.success) {
            throw new Error(response.message || response.error || 'Failed to log past attendance');
          }
        })
      );
  }

  getTeamAttendanceHistory(startDate: string, endDate: string): Observable<any[]> {
    const params = new HttpParams()
      .set('startDate', startDate)
      .set('endDate', endDate);

    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/attendance/history`, { params })
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.error || 'Failed to fetch team attendance history');
        })
      );
  }

  getTeamLeaveHistory(startDate: string, endDate: string): Observable<any[]> {
    const params = new HttpParams()
      .set('startDate', startDate)
      .set('endDate', endDate);

    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/leave/history`, { params })
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.error || 'Failed to fetch team leave history');
        })
      );
  }

  getPendingAttendance(date?: string): Observable<any[]> {
    let params = new HttpParams();
    if (date) {
      params = params.set('date', date);
    }

    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/attendance/pending`, { params })
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.error || 'Failed to fetch pending attendance');
        })
      );
  }

  approveAttendance(attendanceId: string): Observable<void> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/attendance/${attendanceId}/approve`, {})
      .pipe(
        map(response => {
          if (!response.success) {
            throw new Error(response.error || 'Failed to approve attendance');
          }
        })
      );
  }

  rejectAttendance(attendanceId: string, reason: string): Observable<void> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/attendance/${attendanceId}/reject`, {
      rejectionReason: reason
    })
      .pipe(
        map(response => {
          if (!response.success) {
            throw new Error(response.error || 'Failed to reject attendance');
          }
        })
      );
  }

  getPendingLeaveRequests(): Observable<any[]> {
    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/leave/pending`)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.error || 'Failed to fetch pending leave requests');
        })
      );
  }

  approveOrRejectLeave(leaveRequestId: string, approved: boolean, rejectionReason?: string): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/leave/approve`, {
      leaveRequestId,
      approved,
      rejectionReason
    })
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.error || 'Failed to process leave request');
        })
      );
  }
}
