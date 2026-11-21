export interface MyProfile {
  id: string;
  fullName: string;
  email: string;
  bio: string | null;
  imageFileId: string | null;
  showEmail: boolean;
  showProfile: boolean;
  phone: string | null;
  showPhone: boolean;
  linkedInUrl: string | null;
  githubUrl: string | null;
  skills: string | null;
  student: { studentCode: string; entryYear: number; previousSchool: string | null; address: string | null } | null;
}

export interface SaveMyProfileRequest {
  fullName: string;
  bio: string | null;
  imageFileId: string | null;
  showEmail: boolean;
  showProfile: boolean;
  phone: string | null;
  showPhone: boolean;
  linkedInUrl: string | null;
  githubUrl: string | null;
  skills: string | null;
  previousSchool: string | null;
  address: string | null;
}

export interface MyJob {
  id: string;
  jobName: string;
  company: string | null;
  position: string | null;
  startDate: string;
  description: string | null;
  isPublished: boolean;
}

export interface SaveMyJobRequest {
  jobName: string;
  company: string | null;
  position: string | null;
  startDate: string;
  description: string | null;
  isPublished: boolean;
}
