export const SEMESTER_LABELS: Record<number, string> = {
  1: 'ภาคเรียนที่ 1',
  2: 'ภาคเรียนที่ 2',
  3: 'ภาคฤดูร้อน',
};

export interface StudentProjectListItem {
  id: string;
  title: string;
  academicYear: number;
  semester: number;
  courseName: string | null;
  technologies: string | null;
  imageFileId: string | null;
  studentId: string;
  studentName: string;
}

export interface StudentProjectDetail {
  id: string;
  title: string;
  description: string | null;
  academicYear: number;
  semester: number;
  courseName: string | null;
  technologies: string | null;
  teamMembers: string | null;
  lecturerName: string | null;
  imageFileId: string | null;
  pdfFileId: string | null;
  videoUrl: string | null;
  webUrl: string | null;
  githubUrl: string | null;
  studentId: string;
  studentName: string;
}
