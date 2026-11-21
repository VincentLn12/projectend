export type CertificatePageSize = 'A4' | 'A5' | 'Letter';
export type CertificateElementType = 'text' | 'image';
export type CertificateAlign = 'left' | 'center' | 'right';

export interface CertificateElement {
  type: CertificateElementType;
  x: number;
  y: number;
  width: number;
  height: number;
  text: string | null;
  fontSize: number;
  bold: boolean;
  italic: boolean;
  underline: boolean;
  color: string;
  align: CertificateAlign;
  imageFileId: string | null;
}

export interface CertificateTemplate {
  id: string;
  name: string;
  pageSize: CertificatePageSize;
  landscape: boolean;
  backgroundFileId: string | null;
  elements: CertificateElement[];
}

export type SaveCertificateTemplateRequest = Omit<CertificateTemplate, 'id'>;

// Size of the portrait page in points (1 pt = 1/72 inch); the same numbers the PDF uses.
export const PAGE_SIZE_POINTS: Record<CertificatePageSize, { width: number; height: number }> = {
  A4: { width: 595, height: 842 },
  A5: { width: 420, height: 595 },
  Letter: { width: 612, height: 792 },
};

export const TEMPLATE_VARIABLES: { token: string; label: string; sample: string }[] = [
  { token: '{ชื่อ-นามสกุล}', label: 'ชื่อผู้เข้าอบรม', sample: 'นายสมชาย ใจดี' },
  { token: '{ชื่อหลักสูตร}', label: 'ชื่อหลักสูตรอบรม', sample: 'การเขียนโปรแกรมเบื้องต้น' },
  { token: '{วันที่อบรม}', label: 'วันที่อบรม', sample: '1 – 2 มกราคม 2570' },
  { token: '{จำนวนชั่วโมง}', label: 'จำนวนชั่วโมง (ว่างถ้าไม่ระบุ)', sample: 'จำนวน 12 ชั่วโมง' },
  { token: '{สถานที่}', label: 'สถานที่อบรม', sample: 'ห้องปฏิบัติการคอมพิวเตอร์ อาคาร 9' },
  { token: '{วิทยากร}', label: 'วิทยากร', sample: 'อาจารย์ตัวอย่าง' },
];
