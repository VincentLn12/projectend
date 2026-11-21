export const KIND_PAGE = 1;
export const KIND_EXTERNAL = 2;
export const KIND_GROUP = 3;

export const KIND_LABELS: Record<number, string> = {
  [KIND_PAGE]: 'หน้าในเว็บไซต์',
  [KIND_EXTERNAL]: 'ลิงก์ภายนอก',
  [KIND_GROUP]: 'กลุ่มเมนูย่อย',
};

export interface AdminMenuItem {
  id: string;
  label: string;
  kind: number;
  target: string | null;
  parentId: string | null;
  sortOrder: number;
  isVisible: boolean;
  openInNewTab: boolean;
}

export interface SaveMenuItemRequest {
  label: string;
  kind: number;
  target: string | null;
  parentId: string | null;
  isVisible: boolean;
  openInNewTab: boolean;
}

export interface MenuPageOption {
  path: string;
  label: string;
}

// A line of the admin list: the item, how deep it sits, and whether it can move up or down among its siblings.
export interface MenuRow {
  item: AdminMenuItem;
  depth: number;
  canMoveUp: boolean;
  canMoveDown: boolean;
}
