export interface TechSubjectListItem {
  id: string;
  icon: string | null;
  subjectName: string;
  subTitle: string | null;
  mainImageFileId: string | null;
}

export interface TechSubjectDetail {
  id: string;
  icon: string | null;
  subjectName: string;
  subTitle: string | null;
  description: string | null;
  imageFileIds: string[];
}
