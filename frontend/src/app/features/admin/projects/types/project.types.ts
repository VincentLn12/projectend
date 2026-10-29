export const ADVISOR_ROLE_MAIN = 1;
export const ADVISOR_ROLE_CO = 2;

export interface AdminProjectListItem {
  id: string;
  nameTH: string;
  academicYear: number;
  studentName: string;
  isPublished: boolean;
  createdAt: string;
}

export interface ProjectAdvisor {
  lecturerId: string;
  lecturerName: string;
  role: number;
}

export interface AdminProject {
  id: string;
  nameTH: string;
  nameEN: string | null;
  description: string | null;
  academicYear: number;
  keyWords: string | null;
  videoUrl: string | null;
  webUrl: string | null;
  githubUrl: string | null;
  imageFileId: string | null;
  pdfFileId: string | null;
  studentId: string;
  studentName: string;
  isPublished: boolean;
  advisors: ProjectAdvisor[];
}

export interface SaveProjectRequest {
  nameTH: string;
  nameEN: string | null;
  description: string | null;
  academicYear: number;
  keyWords: string | null;
  videoUrl: string | null;
  webUrl: string | null;
  githubUrl: string | null;
  imageFileId: string | null;
  pdfFileId: string | null;
  studentId: string;
  isPublished: boolean;
  advisors: { lecturerId: string; role: number }[];
}

// A person or lecturer that can be picked in the form.
export interface PickerOption {
  id: string;
  label: string;
}
