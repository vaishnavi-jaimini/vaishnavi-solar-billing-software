export interface GstLine {
  gstPercent: number;
  cgstAmount: number;
  sgstAmount: number;
  igstAmount: number;
}

export interface GstRateRow {
  label: string;
  amount: number;
}

// One CGST + SGST row (or one IGST row) per GST rate, so a bill with 5% and 18% items
// shows "CGST @ 2.5%", "SGST @ 2.5%", "CGST @ 9%", "SGST @ 9%" instead of a single lump sum.
export function gstRateRows(lines: GstLine[], isInterState: boolean): GstRateRow[] {
  const byRate = new Map<number, GstLine>();
  for (const l of lines) {
    const rate = Number(l.gstPercent) || 0;
    if (rate <= 0) continue;
    const t = byRate.get(rate) ?? { gstPercent: rate, cgstAmount: 0, sgstAmount: 0, igstAmount: 0 };
    t.cgstAmount += l.cgstAmount || 0;
    t.sgstAmount += l.sgstAmount || 0;
    t.igstAmount += l.igstAmount || 0;
    byRate.set(rate, t);
  }
  const rates = [...byRate.values()].sort((a, b) => a.gstPercent - b.gstPercent);

  if (isInterState) {
    return rates.map((r) => ({ label: `IGST @ ${r.gstPercent}%`, amount: r.igstAmount }));
  }
  return rates.flatMap((r) => [
    { label: `CGST @ ${r.gstPercent / 2}%`, amount: r.cgstAmount },
    { label: `SGST @ ${r.gstPercent / 2}%`, amount: r.sgstAmount }
  ]);
}
