import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { 
  LeaveRequest, 
  LeaveBalance, 
  LeaveRequestResponse,
  LeaveStatus 
} from '../../shared/models/leave.model';
import { ApiResponse } from '../../shared/models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class LeaveService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/leave`;

  createLeaveRequest(request: LeaveRequest): Observable<LeaveRequestResponse> {
    return this.http.post<ApiResponse<LeaveRequestResponse>>(`${this.apiUrl}/request`, request)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.error || 'Failed to create leave request');
        })
      );
  }

  getMyLeaveRequests(status?: LeaveStatus, year?: number): Observable<LeaveRequest[]> {
    let params = new HttpParams();
    if (status) {
      params = params.set('status', status);
    }
    if (year) {
      params = params.set('year', year.toString());
    }

    return this.http.get<ApiResponse<LeaveRequest[]>>(`${this.apiUrl}/my-requests`, { params })
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.error || 'Failed to fetch leave requests');
        })
      );
  }

  getLeaveBalance(): Observable<LeaveBalance> {
    return this.http.get<ApiResponse<LeaveBalance>>(`${this.apiUrl}/balance`)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.error || 'Failed to fetch leave balance');
        })
      );
  }

  getLeaveRequestById(id: string): Observable<LeaveRequest> {
    return this.http.get<ApiResponse<LeaveRequest>>(`${this.apiUrl}/${id}`)
      .pipe(
        map(response => {
          if (response.success && response.data) {
            return response.data;
          }
          throw new Error(response.error || 'Failed to fetch leave request');
        })
      );
  }

  cancelLeaveRequest(id: string): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
      .pipe(
        map(response => {
          if (!response.success) {
            throw new Error(response.error || 'Failed to cancel leave request');
          }
        })
      );
  }
}
