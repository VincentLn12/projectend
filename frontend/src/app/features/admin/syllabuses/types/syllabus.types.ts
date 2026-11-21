export const SUBJECT_CATEGORY_OPTIONS = [
  { value: 0, label: 'หมวดวิชาศึกษาทั่วไป' },
  { value: 1, label: 'หมวดวิชาเฉพาะ' },
  { value: 2, label: 'หมวดวิชาเลือกเสรี' },
];

export interface AdminSyllabusListItem {
  id: string;
  nameTH: string;
  year: number;
  isCurrent: boolean;
  isPublished: boolean;
  createdAt: string;
}

export interface AdminSubject {
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

export interface AdminSyllabus {
  id: string;
  nameTH: string;
  nameEN: string | null;
  year: number;
  degreeTH: string | null;
  degreeEN: string | null;
  totalCredits: number | null;
  pdfFileId: string | null;
  isCurrent: boolean;
  isPublished: boolean;
  objectives: string[];
  occupations: string[];
  subjects: AdminSubject[];
  categoryFiles: CategoryFile[];
}

export interface SubjectInput {
  code: string | null;
  name: string;
  credit: number;
  subjectCategory: number;
}

export interface SaveSyllabusRequest {
  nameTH: string;
  nameEN: string | null;
  year: number;
  degreeTH: string | null;
  degreeEN: string | null;
  totalCredits: number | null;
  pdfFileId: string | null;
  isCurrent: boolean;
  isPublished: boolean;
  objectives: string[];
  occupations: string[];
  subjects: SubjectInput[];
  categoryFiles: CategoryFile[];
}
