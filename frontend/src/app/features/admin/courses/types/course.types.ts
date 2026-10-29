export const REGISTRATION_LABELS: Record<number, string> = {
  1: 'รอยืนยัน',
  2: 'ยืนยันแล้ว',
  3: 'ยกเลิกแล้ว',
};
export const REGISTRATION_CANCELLED = 3;

export interface AdminCourseListItem {
  id: string;
  title: string;
  lecturerName: string | null;
  isPublished: boolean;
  generationCount: number;
  createdAt: string;
}

export interface CoursePhoto {
  fileId: string;
  isMain: boolean;
}

export interface Generation {
  id: string;
  courseId: string;
  subTitle: string | null;
  description: string | null;
  startDate: string;
  endDate: string;
  quantity: number;
  registeredCount: number;
  fee: number | null;
  photoFileId: string | null;
  isCancelled: boolean;
  isRegistrationOpen: boolean;
}

export interface AdminCourse {
  id: string;
  title: string;
  description: string | null;
  lecturerId: string | null;
  lecturerName: string | null;
  isPublished: boolean;
  photos: CoursePhoto[];
  generations: Generation[];
}

export interface SaveCourseRequest {
  title: string;
  description: string | null;
  lecturerId: string | null;
  isPublished: boolean;
  fileIds: string[];
  mainFileId: string | null;
}

export interface SaveGenerationRequest {
  subTitle: string | null;
  description: string | null;
  startDate: string;
  endDate: string;
  quantity: number;
  fee: number | null;
  photoFileId: string | null;
  isCancelled: boolean;
}

export interface RegistrationItem {
  userId: string;
  fullName: string;
  email: string;
  status: number;
  registeredAt: string;
  cancelledAt: string | null;
}

export interface PickerOption {
  id: string;
  label: string;
}
