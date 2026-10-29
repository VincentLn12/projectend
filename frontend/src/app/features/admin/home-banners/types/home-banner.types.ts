export interface AdminHomeBanner {
  id: string;
  title: string | null;
  fileId: string;
  linkUrl: string | null;
  sortOrder: number;

  isPublished: boolean;
  createdAt: string;
}

export interface SaveHomeBannerRequest {
  title: string | null;
  fileId: string;
  linkUrl: string | null;
  sortOrder: number;
  isPublished: boolean;
}
