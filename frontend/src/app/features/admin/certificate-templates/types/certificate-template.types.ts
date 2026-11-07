export interface CertificateTemplate {
  id: string;
  name: string;
  signerName: string;
  signerPosition: string;
  secondSignerName: string | null;
  secondSignerPosition: string | null;
  backgroundFileId: string | null;
  signatureFileId: string | null;
  secondSignatureFileId: string | null;
}

export interface SaveCertificateTemplateRequest {
  name: string;
  signerName: string;
  signerPosition: string;
  secondSignerName: string | null;
  secondSignerPosition: string | null;
  backgroundFileId: string | null;
  signatureFileId: string | null;
  secondSignatureFileId: string | null;
}
