export const USER_TYPE_GENERAL = 1;
export const USER_TYPE_STUDENT = 2;
export const USER_TYPE_LECTURER = 3;
export const USER_TYPE_ADMIN = 4;

export const USER_TYPE_LABELS: Record<number, string> = {
  [USER_TYPE_GENERAL]: 'บุคคลทั่วไป',
  [USER_TYPE_STUDENT]: 'นักศึกษา',
  [USER_TYPE_LECTURER]: 'อาจารย์',
  [USER_TYPE_ADMIN]: 'ผู้ดูแลระบบ',
};

export interface AdminUserListItem {
  id: string;
  fullName: string;
  email: string;
  userType: number;
  studentCode: string | null;
  isLocked: boolean;
  createdAt: string;
}

export interface StudentProfile {
  studentCode: string;
  entryYear: number;
  previousSchool: string | null;
  address: string | null;
}

export interface AdminUser {
  id: string;
  fullName: string;
  email: string;
  userType: number;
  isLocked: boolean;
  showEmail: boolean;
  showProfile: boolean;
  student: StudentProfile | null;
}

export interface CreateUserRequest {
  email: string;
  fullName: string;
  password: string;
  userType: number;
  studentCode: string | null;
  entryYear: number | null;
  previousSchool: string | null;
  address: string | null;
}

export interface UpdateUserRequest {
  fullName: string;
  userType: number;
  studentCode: string | null;
  entryYear: number | null;
  previousSchool: string | null;
  address: string | null;
}
