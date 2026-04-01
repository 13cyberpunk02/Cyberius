import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Toast, ToastService } from '../../../core/services/toast.service';
import { FaIconComponent, IconDefinition } from '@fortawesome/angular-fontawesome';
import {
  faCheckCircle,
  faExclamationCircle,
  faInfoCircle,
  faXmark,
  faXmarkCircle,
} from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-toast-container',
  imports: [CommonModule, FaIconComponent],
  templateUrl: './toast-container.html',
  styleUrl: './toast-container.css',
})
export class ToastContainer {
  readonly toastService = inject(ToastService);

  toastClass(toast: Toast): string {
    const base = 'border ';
    switch (toast.type) {
      case 'success':
        return base + 'bg-emerald-500/15 border-emerald-500/40 text-emerald-300';
      case 'error':
        return base + 'bg-rose-500/15    border-rose-500/40    text-rose-300';
      case 'warning':
        return base + 'bg-amber-500/15   border-amber-500/40   text-amber-300';
      case 'info':
        return base + 'bg-sky-500/15     border-sky-500/40     text-sky-300';
    }
  }

  toastIcon(toast: Toast): IconDefinition {
    switch (toast.type) {
      case 'success':
        return faCheckCircle;
      case 'error':
        return faXmarkCircle;
      case 'warning':
        return faExclamationCircle;
      case 'info':
        return faInfoCircle;
    }
  }

  protected readonly faXmark = faXmark;
}
