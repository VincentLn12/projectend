export interface AdminJobHistory {
  id: string;
  userId: string;
  jobName: string;
  company: string | null;
  position: string | null;
  startDate: string;
  description: string | null;
  personName: string;
  isPublished: boolean;
  createdAt: string;
}

export interface SaveJobHistoryRequest {
  userId: string;
  jobName: string;
  company: string | null;
  position: string | null;
  startDate: string;
  description: string | null;
  isPublished: boolean;
}

export interface PickerOption {
  id: string;
  label: string;
}
