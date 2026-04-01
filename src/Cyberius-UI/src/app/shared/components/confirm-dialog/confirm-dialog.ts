import { Component, input, OnDestroy, OnInit, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faCircleQuestion,
  faExclamationTriangle,
  faSpinner,
} from '@fortawesome/free-solid-svg-icons';

export interface ConfirmDialogConfig {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  danger?: boolean;
}

@Component({
  selector: 'app-confirm-dialog',
  imports: [CommonModule, FaIconComponent],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.css',
})
export class ConfirmDialog implements OnInit, OnDestroy {
  config = input.required<ConfirmDialogConfig>();
  loading = input<boolean>(false);

  confirmed = output<void>();
  cancelled = output<void>();

  ngOnInit(): void {
    document.body.style.overflow = 'hidden';
  }
  ngOnDestroy(): void {
    document.body.style.overflow = '';
  }

  get confirmLabel(): string {
    return this.config().confirmLabel ?? 'Да';
  }
  get cancelLabel(): string {
    return this.config().cancelLabel ?? 'Нет';
  }
  get danger(): boolean {
    return this.config().danger ?? false;
  }

  onBackdropClick(e: MouseEvent): void {
    if ((e.target as HTMLElement).classList.contains('modal-backdrop')) {
      this.cancelled.emit();
    }
  }

  protected readonly faExclamationTriangle = faExclamationTriangle;
  protected readonly faCircleQuestion = faCircleQuestion;
  protected readonly faSpinner = faSpinner;
}
