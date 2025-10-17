export enum LeaveType {
  CasualLeave = 'CasualLeave',
  EarnedLeave = 'EarnedLeave',
  CompensatoryOff = 'CompensatoryOff'
}

export enum LeaveStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Rejected = 'Rejected'
}

export interface LeaveRequest {
  id?: string;
  userId?: string;
  leaveType: LeaveType;
  startDate: Date;
  endDate: Date;
  totalDays?: number;
  reason: string;
  status?: LeaveStatus;
  approvedBy?: string;
  approvedAt?: Date;
  rejectionReason?: string;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface LeaveBalance {
  year: number;
  casualLeaveBalance: number;
  earnedLeaveBalance: number;
  compensatoryOffBalance: number;
}

export interface LeaveRequestResponse {
  leaveRequestId: string;
  totalDays: number;
  status: LeaveStatus;
  remainingBalance: number;
}
