export interface NewsListItem {
  id: string;
  title: string;
  subTitle: string | null;
  publishedAt: string | null;
  mainImageFileId: string | null;
}

export interface NewsDetail {
  id: string;
  title: string;
  subTitle: string | null;
  body: string;
  publishedAt: string | null;
  authorName: string;
  imageFileIds: string[];
}
