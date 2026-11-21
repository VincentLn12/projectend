export interface AdminStudent {
  id: string;
  fullName: string;
  email: string;
  imageFileId: string | null;
  studentCode: string;
  entryYear: number;
  previousSchool: string | null;
  address: string | null;
  jobHistoryCount: number;
  isLocked: boolean;
  createdAt: string;
}

export interface StudentJobHistory {
  id: string;
  jobName: string;
  company: string | null;
  position: string | null;
  startDate: string;
  description: string | null;
  isPublished: boolean;
}

export const SEMESTER_LABELS: Record<number, string> = {
  1: 'ภาคเรียนที่ 1',
  2: 'ภาคเรียนที่ 2',
  3: 'ภาคฤดูร้อน',
};

export interface StudentCourseProject {
  id: string;
  title: string;
  academicYear: number;
  semester: number;
  courseName: string | null;
  technologies: string | null;
  imageFileId: string | null;
  videoUrl: string | null;
  webUrl: string | null;
  githubUrl: string | null;
  isPublished: boolean;
}

export interface StudentProject {
  id: string;
  nameTH: string;
  academicYear: number;
  imageFileId: string | null;
  keyWords: string | null;
  videoUrl: string | null;
  webUrl: string | null;
  githubUrl: string | null;
  isPublished: boolean;
}
