export interface AdminTechSubject {
  id: string;
  icon: string | null;
  subjectName: string;
  subTitle: string | null;
  description: string | null;
  sortOrder: number;
  imageFileIds: string[];

  isPublished: boolean;
  createdAt: string;
}

export interface SaveTechSubjectRequest {
  icon: string | null;
  subjectName: string;
  subTitle: string | null;
  description: string | null;
  sortOrder: number;
  fileIds: string[];
  isPublished: boolean;
}
