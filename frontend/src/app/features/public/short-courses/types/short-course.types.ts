export const AVAILABILITY_NOT_OPEN_YET = 1;
export const AVAILABILITY_OPEN = 2;
export const AVAILABILITY_FULL = 3;
export const AVAILABILITY_CLOSED = 4;

export const AVAILABILITY_LABELS: Record<number, string> = {
  [AVAILABILITY_NOT_OPEN_YET]: 'เร็วๆ นี้',
  [AVAILABILITY_OPEN]: 'เปิดรับสมัคร',
  [AVAILABILITY_FULL]: 'เต็มแล้ว',
  [AVAILABILITY_CLOSED]: 'ปิดรับสมัคร',
};

export const AVAILABILITY_BADGE_CLASSES: Record<number, string> = {
  [AVAILABILITY_NOT_OPEN_YET]: 'bg-[#e7eeff] text-[#0f2854]',
  [AVAILABILITY_OPEN]: 'bg-[#f7be33] text-[#261a00]',
  [AVAILABILITY_FULL]: 'bg-slate-600 text-white',
  [AVAILABILITY_CLOSED]: 'bg-slate-500 text-white',
};

export const REGISTRATION_CANCELLED = 3;

export interface ShortCourseListItem {
  id: string;
  title: string;
  coverFileId: string | null;
  startDate: string;
  endDate: string;
  hours: number | null;
  location: string | null;
  maxParticipants: number;
  registeredCount: number;
  availability: number;
}

export interface ShortCourseDetail {
  id: string;
  title: string;
  description: string | null;
  audience: string | null;
  location: string | null;
  teacherName: string | null;
  coverFileId: string | null;
  startDate: string;
  endDate: string;
  hours: number | null;
  maxParticipants: number;
  registeredCount: number;
  registrationOpenAt: string | null;
  registrationCloseAt: string | null;
  availability: number;
}

export interface RegisterRequest {
  prefix: string | null;
  firstName: string;
  lastName: string;
  phone: string;
}

export interface LookupRequest {
  phone: string;
}

export interface CertificateRequest {
  registrationId: string;
  phone: string;
}

export interface UpdateRegistrationRequest {
  currentPhone: string;
  prefix: string | null;
  firstName: string;
  lastName: string;
}

export interface MyShortCourseRegistration {
  id: string;
  shortCourseId: string;
  courseTitle: string;
  startDate: string;
  endDate: string;
  prefix: string | null;
  firstName: string;
  lastName: string;
  phone: string;
  status: number;
  registeredAt: string;
  canModify: boolean;
}
