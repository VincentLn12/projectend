export interface AdminLecturer {
  id: string;
  prefix: string | null;
  fullName: string;
  position: string | null;
  expertise: string | null;
  educationLevel: string | null;
  program: string | null;
  contact: string | null;
  imageFileId: string | null;
  sortOrder: number;
  isPublished: boolean;
  createdAt: string;
}

export interface SaveLecturerRequest {
  prefix: string | null;
  fullName: string;
  position: string | null;
  expertise: string | null;
  educationLevel: string | null;
  program: string | null;
  contact: string | null;
  imageFileId: string | null;
  sortOrder: number;
  isPublished: boolean;
}
