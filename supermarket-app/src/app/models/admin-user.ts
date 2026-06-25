export interface AdminUser {
  id: number;
  username: string;
  email: string;
  role: string;
  isActive: boolean;
  createdAt: Date;
  lastLoginAt: Date | null;
}

export interface UpdateUserStatus {
  isActive: boolean;
}

export interface UpdateUserRole {
  role: string;
}