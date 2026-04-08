import {
  Component,
  inject,
  signal,
  output,
  ViewEncapsulation,
  OnInit,
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { LoginRequest, FormErrors } from '../../../core/models/auth.model';
import { validateLogin, isValid } from '../../../core/validators/auth.validator';
import {
  faCode,
  faEnvelope,
  faLock,
  faEye,
  faEyeSlash,
  faXmark,
  faExclamationTriangle,
  faArrowRightToBracket,
  faSpinner,
} from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { RouterLink } from '@angular/router';

type FormStatus = 'idle' | 'loading' | 'error';

@Component({
  selector: 'app-login-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, FontAwesomeModule, RouterLink],
  templateUrl: './login-modal.html',
  encapsulation: ViewEncapsulation.None,
})
export class LoginModal implements OnInit, OnDestroy {
  private auth = inject(AuthService);

  closed = output<void>();
  success = output<void>();
  openRegister = output<void>();

  protected readonly faCode = faCode;
  protected readonly faEnvelope = faEnvelope;
  protected readonly faEye = faEye;
  protected readonly faEyeSlash = faEyeSlash;
  protected readonly faXmark = faXmark;
  protected readonly faLock = faLock;
  protected readonly faExclamationTriangle = faExclamationTriangle;
  protected readonly faArrowRightToBracket = faArrowRightToBracket;
  protected readonly faSpinner = faSpinner;

  email = '';
  password = '';
  showPassword = signal(false);
  status = signal<FormStatus>('idle');
  serverError = signal('');
  errors = signal<FormErrors<LoginRequest>>({});
  touched = signal<Partial<Record<keyof LoginRequest, boolean>>>({});

  ngOnInit(): void {
    document.body.style.overflow = 'hidden';
  }
  ngOnDestroy(): void {
    document.body.style.overflow = '';
  }

  onBlur(field: keyof LoginRequest): void {
    this.touched.update((t) => ({ ...t, [field]: true }));
    this.validate();
  }

  private validate(): boolean {
    const result = validateLogin({ email: this.email, password: this.password });
    this.errors.set(result);
    return isValid(result);
  }

  submit(): void {
    this.touched.set({ email: true, password: true });
    if (!this.validate()) return;

    this.status.set('loading');
    this.serverError.set('');

    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: () => {
        this.status.set('idle');
        this.success.emit();
        this.closed.emit();
      },
      error: (err) => {
        this.status.set('error');
        this.serverError.set(
          err?.status === 401
            ? 'Неверный email или пароль'
            : err?.status === 0
              ? 'Нет соединения с сервером'
              : (err?.error?.message ?? 'Произошла ошибка, попробуйте ещё раз'),
        );
      },
    });
  }

  fieldError(field: keyof LoginRequest): string {
    return this.touched()[field] ? (this.errors()[field] ?? '') : '';
  }

  onBackdropClick(e: MouseEvent): void {
    if ((e.target as HTMLElement).classList.contains('modal-backdrop')) this.closed.emit();
  }

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }
}
