// <input type="datetime-local"> gives "2030-01-01T09:00" without a time zone. The API compares times as UTC,
// so convert before sending, and convert what the API returns back before showing it in the input.

export function localDateTimeToIso(value: string | null): string | null {
  if (!value) {
    return null;
  }
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? null : date.toISOString();
}

export function isoToLocalDateTime(iso: string | null): string | null {
  if (!iso) {
    return null;
  }
  // The API sends UTC times without a trailing Z in some places; treat those as UTC too.
  const utc = /[zZ]|[+-]\d\d:\d\d$/.test(iso) ? iso : `${iso}Z`;
  const date = new Date(utc);
  if (Number.isNaN(date.getTime())) {
    return null;
  }

  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}
