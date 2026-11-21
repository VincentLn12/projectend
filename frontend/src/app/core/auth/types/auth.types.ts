export type RoleName = 'User' | 'Student' | 'Lecturer' | 'Admin';

export interface UserInfo {
  id: string;
  fullName: string;
  email: string;
  roles: RoleName[];
}

export interface RegisterStudentRequest {
  email: string;
  fullName: string;
  studentCode: string;
  password: string;
  acceptPrivacy: boolean;
}

export interface LoginRequest {
  identifier: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  refreshToken: string;
  user: UserInfo;
}
