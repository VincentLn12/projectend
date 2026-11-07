export const SEMESTER_LABELS: Record<number, string> = {
  1: 'ภาคเรียนที่ 1',
  2: 'ภาคเรียนที่ 2',
  3: 'ภาคฤดูร้อน',
};

export interface MyStudentProject {
  id: string;
  title: string;
  description: string | null;
  academicYear: number;
  semester: number;
  courseName: string | null;
  technologies: string | null;
  lecturerId: string | null;
  imageFileId: string | null;
  pdfFileId: string | null;
  videoUrl: string | null;
  webUrl: string | null;
  githubUrl: string | null;
  isPublished: boolean;
  createdAt: string;
}

export interface SaveStudentProjectRequest {
  title: string;
  description: string | null;
  academicYear: number;
  semester: number;
  courseName: string | null;
  technologies: string | null;
  lecturerId: string | null;
  imageFileId: string | null;
  pdfFileId: string | null;
  videoUrl: string | null;
  webUrl: string | null;
  githubUrl: string | null;
  isPublished: boolean;
}

export interface MyProfile {
  id: string;
  fullName: string;
  bio: string | null;
  imageFileId: string | null;
  showEmail: boolean;
  showProfile: boolean;
  phone: string | null;
  showPhone: boolean;
  linkedInUrl: string | null;
  githubUrl: string | null;
  skills: string | null;
  student: { studentCode: string; entryYear: number; previousSchool: string | null; address: string | null } | null;
}

export interface LecturerOption {
  id: string;
  label: string;
}
