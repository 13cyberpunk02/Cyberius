import { Component, inject, output, signal, ViewEncapsulation } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faArrowRightToBracket,
  faCode,
  faEnvelope,
  faExclamation,
  faExclamationTriangle,
  faEye,
  faEyeSlash,
  faLock,
  faSpinner,
  faXmark,
} from '@fortawesome/free-solid-svg-icons';

type FormStatus = 'idle' | 'loading' | 'error';

@Component({
  selector: 'app-login-modal',
  imports: [CommonModule, FormsModule, FaIconComponent],
  templateUrl: './login-modal.html',
  styleUrl: './login-modal.css',
  encapsulation: ViewEncapsulation.None,
})
export class LoginModal {
  private auth = inject(AuthService);

  closed = output<void>();
  success = output<void>();

  email = '';
  password = '';
  showPassword = signal(false);
  status = signal<FormStatus>('idle');
  errorMsg = signal('');

  submit(): void {
    if (!this.email || !this.password) return;
    this.status.set('loading');
    this.errorMsg.set('');

    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: () => {
        this.status.set('idle');
        this.success.emit();
        this.closed.emit();
      },
      error: (err) => {
        this.status.set('error');
        const msg = err?.error?.message ?? err?.message ?? '';
        this.errorMsg.set(
          err?.status === 401
            ? 'Неверный email или пароль'
            : err?.status === 0
              ? 'Нет соединения с сервером'
              : msg || 'Произошла ошибка, попробуйте ещё раз',
        );
      },
    });
  }

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal-backdrop')) {
      this.closed.emit();
    }
  }

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  protected readonly faCode = faCode;
  protected readonly faEnvelope = faEnvelope;
  protected readonly faLock = faLock;
  protected readonly faEye = faEye;
  protected readonly faEyeSlash = faEyeSlash;
  protected readonly faXmark = faXmark;
  protected readonly faArrowRightToBracket = faArrowRightToBracket;
  protected readonly faSpinner = faSpinner;
  protected readonly faExclamation = faExclamation;
  protected readonly faExclamationTriangle = faExclamationTriangle;
}
