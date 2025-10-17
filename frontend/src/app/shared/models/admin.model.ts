export interface PublicHoliday {
  id: string;
  date: Date;
  name: string;
  description?: string;
  year: number;
  isActive: boolean;
  createdAt: Date;
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  employeeId: string;
  role: string;
  managerId?: string;
  isActive: boolean;
}

export interface CreateUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  employeeId: string;
  role: string;
  managerId?: string;
  password: string;
}

export interface UpdateUserRequest {
  email?: string;
  firstName?: string;
  lastName?: string;
  role?: string;
  managerId?: string;
  isActive?: boolean;
}

export interface LeaveEntitlement {
  id: string;
  userId: string;
  year: number;
  casualLeaveBalance: number;
  earnedLeaveBalance: number;
  compensatoryOffBalance: number;
}

export interface LeaveEntitlementRequest {
  userId: string;
  year: number;
  casualLeave: number;
  earnedLeave: number;
}
