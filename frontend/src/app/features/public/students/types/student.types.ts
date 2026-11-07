export const SEMESTER_LABELS: Record<number, string> = {
  1: 'ภาคเรียนที่ 1',
  2: 'ภาคเรียนที่ 2',
  3: 'ภาคฤดูร้อน',
};

export interface PortfolioStudentProject {
  id: string;
  title: string;
  academicYear: number;
  semester: number;
  courseName: string | null;
  technologies: string | null;
  imageFileId: string | null;
}

export interface PortfolioFinalProject {
  id: string;
  nameTH: string;
  academicYear: number;
  keyWords: string | null;
  imageFileId: string | null;
}

export interface PortfolioJob {
  id: string;
  jobName: string;
  company: string | null;
  position: string | null;
  startDate: string | null;
}

export interface StudentPortfolio {
  id: string;
  fullName: string;
  bio: string | null;
  imageFileId: string | null;
  entryYear: number | null;
  email: string | null;
  projects: PortfolioFinalProject[];
  studentProjects: PortfolioStudentProject[];
  jobs: PortfolioJob[];
}
