export interface HomeBanner {
  id: string;
  title: string | null;
  fileId: string;
  linkUrl: string | null;
}

export interface HomeNews {
  id: string;
  title: string;
  subTitle: string | null;
  publishedAt: string | null;
  mainImageFileId: string | null;
}

export interface HomeProject {
  id: string;
  nameTH: string;
  academicYear: number;
  imageFileId: string | null;
  studentName: string;
}
