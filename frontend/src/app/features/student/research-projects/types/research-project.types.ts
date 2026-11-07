export const ADVISOR_MAIN = 1;
export const ADVISOR_CO = 2;

export interface MyResearchProjectListItem {
  id: string;
  nameTH: string;
  academicYear: number;
  imageFileId: string | null;
  keyWords: string | null;
  isPublished: boolean;
  createdAt: string;
}

export interface MyResearchProject {
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
  isPublished: boolean;
  advisors: { lecturerId: string; lecturerName: string; role: number }[];
}

export interface SaveMyResearchProjectRequest {
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
  isPublished: boolean;
  advisors: { lecturerId: string; role: number }[];
}

export interface LecturerOption {
  id: string;
  label: string;
}
