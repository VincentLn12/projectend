import { vi } from 'vitest';

// A plain in-memory Storage. Tests install it so they do not depend on which localStorage the
// runtime provides (Node's own experimental one can shadow the browser-like one from jsdom).
export function installMemoryLocalStorage(): void {
  const data = new Map<string, string>();

  vi.stubGlobal('localStorage', {
    getItem: (key: string) => data.get(key) ?? null,
    setItem: (key: string, value: string) => void data.set(key, String(value)),
    removeItem: (key: string) => void data.delete(key),
    clear: () => data.clear(),
  });
}
