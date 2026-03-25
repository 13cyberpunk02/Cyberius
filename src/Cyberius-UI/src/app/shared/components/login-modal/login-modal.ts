import { Component, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faCode } from '@fortawesome/free-solid-svg-icons';

type FormStatus = 'idle' | 'loading' | 'error';

@Component({
  selector: 'app-login-modal',
  imports: [CommonModule, FormsModule, FaIconComponent],
  templateUrl: './login-modal.html',
  styleUrl: './login-modal.css',
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
}
