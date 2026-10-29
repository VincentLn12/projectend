export interface AdminNewsListItem {
  id: string;
  title: string;
  publishedAt: string | null;
  isPublished: boolean;
  authorName: string;
  createdAt: string;
}

export interface AdminNewsFile {
  fileId: string;
  isMain: boolean;
  sortOrder: number;
}

export interface AdminNews {
  id: string;
  title: string;
  subTitle: string | null;
  body: string;
  publishedAt: string | null;
  isPublished: boolean;
  authorName: string;
  createdAt: string;
  updatedAt: string | null;
  files: AdminNewsFile[];
}

export interface SaveNewsRequest {
  title: string;
  subTitle: string | null;
  body: string;
  isPublished: boolean;
  fileIds: string[];
  mainFileId: string | null;
}
