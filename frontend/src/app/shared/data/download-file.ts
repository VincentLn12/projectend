export function saveBlobAsFile(blob: Blob, fileName: string): void {
  const objectUrl = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = objectUrl;
  anchor.download = fileName;
  document.body.appendChild(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(objectUrl);
}

export async function describeDownloadError(error: unknown): Promise<string> {
  const fallback = 'ดาวน์โหลดไม่สำเร็จ กรุณาลองใหม่อีกครั้ง';
  const body = (error as { error?: unknown } | null)?.error;
  if (!(body instanceof Blob)) {
    return fallback;
  }
  try {
    const problem = JSON.parse(await body.text()) as { detail?: string };
    return problem.detail ?? fallback;
  } catch {
    return fallback;
  }
}
