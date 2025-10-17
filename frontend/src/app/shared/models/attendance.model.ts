export enum ApprovalStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Rejected = 'Rejected'
}

export interface Attendance {
  id: string;
  userId: string;
  loginTime: Date;
  logoutTime?: Date;
  date: Date;
  isWeekend: boolean;
  isPublicHoliday: boolean;
  status: ApprovalStatus;
  approvedBy?: string;
  approvedAt?: Date;
  createdAt: Date;
  updatedAt: Date;
}

export interface AttendanceLoginRequest {
  timestamp?: Date;
}

export interface AttendanceLogoutRequest {
  attendanceId: string;
  timestamp?: Date;
}

export interface AttendanceResponse {
  attendanceId: string;
  loginTime: Date;
  logoutTime?: Date;
  date: Date;
  isWeekend: boolean;
  isPublicHoliday: boolean;
  duration?: string;
  compensatoryOffEarned?: boolean;
  message?: string;
}

export interface TodayAttendanceStatus {
  hasLoggedIn: boolean;
  hasLoggedOut: boolean;
  attendanceId?: string;
  loginTime?: Date;
}
