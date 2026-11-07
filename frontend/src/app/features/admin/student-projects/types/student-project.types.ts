export const SEMESTER_LABELS: Record<number, string> = {
  1: 'ภาคเรียนที่ 1',
  2: 'ภาคเรียนที่ 2',
  3: 'ภาคฤดูร้อน',
};

export interface AdminStudentProjectListItem {
  id: string;
  title: string;
  academicYear: number;
  semester: number;
  studentName: string;
  isPublished: boolean;
  createdAt: string;
}
