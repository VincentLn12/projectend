export const AVAILABILITY_LABELS: Record<number, string> = {
  1: 'ยังไม่เปิดรับสมัคร',
  2: 'เปิดรับสมัคร',
  3: 'เต็มแล้ว',
  4: 'ปิดรับสมัคร',
};

export const AVAILABILITY_OPEN = 2;

export const REGISTRATION_LABELS: Record<number, string> = {
  1: 'รอยืนยัน',
  2: 'สมัครแล้ว',
  3: 'ยกเลิกแล้ว',
};
export const REGISTRATION_CANCELLED = 3;

export interface AdminShortCourseListItem {
  id: string;
  title: string;
  startDate: string;
  endDate: string;
  maxParticipants: number;
  registeredCount: number;
  isPublished: boolean;
  availability: number;
}

export interface AdminShortCourse {
  id: string;
  title: string;
  description: string | null;
  audience: string | null;
  location: string | null;
  teacherName: string | null;
  startDate: string;
  endDate: string;
  hours: number | null;
  maxParticipants: number;
  registeredCount: number;
  registrationOpenAt: string | null;
  registrationCloseAt: string | null;
  isRegistrationEnabled: boolean;
  isPublished: boolean;
  coverFileId: string | null;
  certificateTemplateId: string | null;
}

export interface SaveShortCourseRequest {
  title: string;
  description: string | null;
  audience: string | null;
  location: string | null;
  teacherName: string | null;
  startDate: string;
  endDate: string;
  hours: number | null;
  maxParticipants: number;
  registrationOpenAt: string | null;
  registrationCloseAt: string | null;
  isRegistrationEnabled: boolean;
  isPublished: boolean;
  coverFileId: string | null;
  certificateTemplateId: string | null;
}

export interface ShortCourseRegistration {
  id: string;
  prefix: string | null;
  firstName: string;
  lastName: string;
  phone: string;
  status: number;
  registeredAt: string;
  cancelledAt: string | null;
}

export interface CertificateTemplateOption {
  id: string;
  name: string;
}
