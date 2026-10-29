export const REGISTRATION_PENDING = 1;
export const REGISTRATION_CONFIRMED = 2;
export const REGISTRATION_CANCELLED = 3;

export interface CourseListItem {
  id: string;
  title: string;
  description: string | null;
  lecturerName: string | null;
  mainImageFileId: string | null;
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

export interface CourseDetail {
  id: string;
  title: string;
  description: string | null;
  lecturerName: string | null;
  imageFileIds: string[];
  generations: Generation[];
}

export interface MyRegistration {
  generationId: string;
  courseId: string;
  courseTitle: string;
  generationName: string | null;
  startDate: string;
  endDate: string;
  status: number;
  registeredAt: string;
}
