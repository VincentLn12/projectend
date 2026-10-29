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

export interface SyllabusDetail extends SyllabusListItem {
  objectives: string[];
  occupations: string[];
  subjects: Subject[];
}

export interface SubjectGroup {
  category: number;
  label: string;
  subjects: Subject[];
  totalCredits: number;
}
