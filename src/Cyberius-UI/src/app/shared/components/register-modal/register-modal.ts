import { Component, inject, OnDestroy, OnInit, output, signal } from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { FormErrors, RegisterRequest } from '../../../core/models/auth.model';
import {
  faAt,
  faCake,
  faCode,
  faEnvelope,
  faExclamationTriangle,
  faEye,
  faEyeSlash,
  faLock,
  faSpinner,
  faUser,
  faUserPlus,
  faXmark,
} from '@fortawesome/free-solid-svg-icons';
import { isValid, validateRegister } from '../../../core/validators/auth.validator';

type FormStatus = 'idle' | 'loading' | 'error';

@Component({
  selector: 'app-register-modal',
  imports: [CommonModule, FormsModule, FontAwesomeModule],
  templateUrl: './register-modal.html',
  styleUrl: './register-modal.css',
})
export class RegisterModal implements OnInit, OnDestroy {
  private auth = inject(AuthService);

  closed = output<void>();
  success = output<void>();
  openLogin = output<void>();

  protected readonly faCode = faCode;
  protected readonly faEnvelope = faEnvelope;
  protected readonly faLock = faLock;
  protected readonly faEye = faEye;
  protected readonly faEyeSlash = faEyeSlash;
  protected readonly faUser = faUser;
  protected readonly faAt = faAt;
  protected readonly faCake = faCake;
  protected readonly faXmark = faXmark;
  protected readonly faExclamationTriangle = faExclamationTriangle;
  protected readonly faSpinner = faSpinner;
  protected readonly faUserPlus = faUserPlus;

  form: RegisterRequest = {
    email: '',
    userName: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    dateOfBirth: '',
  };

  showPassword = signal(false);
  showConfirmPassword = signal(false);
  status = signal<FormStatus>('idle');
  serverError = signal('');
  errors = signal<FormErrors<RegisterRequest>>({});
  touched = signal<Partial<Record<keyof RegisterRequest, boolean>>>({});

  get maxDate(): string {
    const d = new Date();
    d.setFullYear(d.getFullYear() - 13);
    return d.toISOString().split('T')[0];
  }

  ngOnInit(): void {
    document.body.style.overflow = 'hidden';
  }
  ngOnDestroy(): void {
    document.body.style.overflow = '';
  }

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }
  toggleConfirmPassword(): void {
    this.showConfirmPassword.update((v) => !v);
  }

  onBlur(field: keyof RegisterRequest): void {
    this.touched.update((t) => ({ ...t, [field]: true }));
    this.validate();
  }

  private validate(): boolean {
    const result = validateRegister(this.form);
    this.errors.set(result);
    return isValid(result);
  }

  submit(): void {
    const all: Partial<Record<keyof RegisterRequest, boolean>> = {};
    (Object.keys(this.form) as (keyof RegisterRequest)[]).forEach((k) => (all[k] = true));
    this.touched.set(all);
    if (!this.validate()) return;

    this.status.set('loading');
    this.serverError.set('');

    this.auth.register(this.form).subscribe({
      next: () => {
        this.status.set('idle');
        this.success.emit();
        this.closed.emit();
      },
      error: (err) => {
        this.status.set('error');
        this.serverError.set(
          err?.status === 409
            ? 'Пользователь с таким email уже существует'
            : err?.status === 0
              ? 'Нет соединения с сервером'
              : (err?.error?.message ?? 'Произошла ошибка, попробуйте ещё раз'),
        );
      },
    });
  }

  fieldError(field: keyof RegisterRequest): string {
    return this.touched()[field] ? (this.errors()[field] ?? '') : '';
  }

  onBackdropClick(e: MouseEvent): void {
    if ((e.target as HTMLElement).classList.contains('modal-backdrop')) this.closed.emit();
  }
}
