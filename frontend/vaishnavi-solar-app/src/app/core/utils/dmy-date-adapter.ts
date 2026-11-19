import { Injectable, Provider } from '@angular/core';
import { DateAdapter, NativeDateAdapter } from '@angular/material/core';

// Shows and reads datepicker dates as dd/mm/yyyy instead of the browser's default month-first format.
@Injectable()
export class DmyDateAdapter extends NativeDateAdapter {
  override format(date: Date, displayFormat: object): string {
    // Only the typed-in date box (day, month and year all numeric) is reformatted; calendar headers keep their labels.
    const f = displayFormat as Intl.DateTimeFormatOptions;
    if (!(f?.day === 'numeric' && f?.month === 'numeric' && f?.year === 'numeric')) {
      return super.format(date, displayFormat);
    }
    const dd = String(date.getDate()).padStart(2, '0');
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    return `${dd}/${mm}/${date.getFullYear()}`;
  }

  override parse(value: unknown, parseFormat?: unknown): Date | null {
    if (typeof value === 'string') {
      const m = value.trim().match(/^(\d{1,2})[\/\-.](\d{1,2})[\/\-.](\d{4})$/);
      if (m) {
        const date = new Date(+m[3], +m[2] - 1, +m[1]);
        return date.getMonth() === +m[2] - 1 ? date : this.invalid();
      }
    }
    return super.parse(value, parseFormat);
  }
}

export const DMY_DATE_PROVIDERS: Provider[] = [{ provide: DateAdapter, useClass: DmyDateAdapter }];
