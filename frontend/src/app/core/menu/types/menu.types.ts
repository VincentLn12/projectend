export const MENU_KIND_PAGE = 1;
export const MENU_KIND_EXTERNAL = 2;
export const MENU_KIND_GROUP = 3;

export interface MenuEntry {
  id: string;
  label: string;
  kind: number;
  target: string | null;
  openInNewTab: boolean;
  children: MenuEntry[];
}
