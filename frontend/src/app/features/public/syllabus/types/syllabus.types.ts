// Order on the page: specialised subjects first, then general education, then free electives.
export const SUBJECT_CATEGORY_ORDER = [1, 0, 2];

export const SUBJECT_CATEGORY_LABELS: Record<number, string> = {
  0: 'หมวดวิชาศึกษาทั่วไป',
  1: 'หมวดวิชาเฉพาะ',
  2: 'หมวดวิชาเลือกเสรี',
};

export interface SyllabusListItem {
  id: string;
  nameTH: string;
  nameEN: string | null;
  year: number;
  degreeTH: string | null;
  degreeEN: string | null;
  totalCredits: number | null;
  pdfFileId: string | null;
  isCurrent: boolean;
}

export interface Subject {
  id: string;
  code: string | null;
  name: string;
  credit: number;
  subjectCategory: number;
}

export interface CategoryFile {
  subjectCategory: number;
  fileId: string;
}

export interface SyllabusDetail extends SyllabusListItem {
  objectives: string[];
  occupations: string[];
  subjects: Subject[];
  categoryFiles: CategoryFile[];
}

export interface SubjectGroup {
  category: number;
  label: string;
  subjects: Subject[];
  totalCredits: number;
  pdfUrl: string | null;
}
