import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface Toast {
  id: string;
  type: ToastType;
  message: string;
  duration?: number;
}

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  readonly toasts = signal<Toast[]>([]);

  success(message: string, duration = 3000): void {
    this.add({ type: 'success', message, duration });
  }

  error(message: string, duration = 5000): void {
    this.add({ type: 'error', message, duration });
  }

  info(message: string, duration = 3000): void {
    this.add({ type: 'info', message, duration });
  }

  warning(message: string, duration = 4000): void {
    this.add({ type: 'warning', message, duration });
  }

  dismiss(id: string): void {
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }

  private add(toast: Omit<Toast, 'id'>): void {
    const id = crypto.randomUUID();
    this.toasts.update((list) => [...list, { ...toast, id }]);
    if (toast.duration) {
      setTimeout(() => this.dismiss(id), toast.duration);
    }
  }
}
