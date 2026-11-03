export interface SiteSettings {
  webName: string;
  registerUrl: string | null;
  kruUrl: string | null;
  pageFacebook: string | null;
  logoFileId: string | null;
  videoUrl: string | null;
  mapLocation: string | null;
  aboutTitle: string | null;
  aboutDescription: string | null;
  quickMenuEnabled: boolean;
  quickMenuBackgroundFileId: string | null;
  quickMenuItems: QuickMenuItemSetting[];
}

export interface QuickMenuItemSetting {
  key: string;
  enabled: boolean;
  title: string | null;
  imageFileId: string | null;
}
