// One look for every text-like control, with a red variant when the field has an error.
export function inputClass(invalid: boolean): string {
  const base =
    'block w-full rounded-md border bg-white px-3 py-2 text-slate-900 placeholder:text-slate-400 focus:outline-none focus:ring-2 disabled:cursor-not-allowed disabled:bg-slate-100 disabled:text-slate-500';

  return invalid
    ? `${base} border-red-500 focus:ring-red-200`
    : `${base} border-slate-400 focus:border-brand-700 focus:ring-brand-700/25`;
}

// The rounded, taller look of the search box, for filters that sit beside it (shape="pill").
export function pillInputClass(invalid: boolean): string {
  return `${inputClass(invalid)} h-11 rounded-full! px-4 shadow-sm`;
}
