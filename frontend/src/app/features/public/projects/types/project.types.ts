export interface ProjectListItem {
  id: string;
  nameTH: string;
  nameEN: string | null;
  academicYear: number;
  keyWords: string | null;
  imageFileId: string | null;
  studentName: string;
}

export interface ProjectAdvisor {
  lecturerId: string;
  lecturerName: string;
  role: number;
}

export interface ProjectDetail {
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
  studentName: string;
  advisors: ProjectAdvisor[];
}
