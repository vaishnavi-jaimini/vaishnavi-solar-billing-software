import { Injectable, computed, signal } from '@angular/core';

// Counts API requests in flight so the app can show one loader for all of them.
@Injectable({ providedIn: 'root' })
export class LoadingService {
  private pending = signal(0);
  readonly loading = computed(() => this.pending() > 0);

  start(): void {
    this.pending.update((n) => n + 1);
  }

  stop(): void {
    this.pending.update((n) => Math.max(0, n - 1));
  }
}
