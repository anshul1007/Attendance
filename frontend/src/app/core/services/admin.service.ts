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
          throw new Error(response.error || 'Failed to fetch users');
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
          throw new Error(response.error || 'Failed to create user');
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
          throw new Error(response.error || 'Failed to update user');
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
}
