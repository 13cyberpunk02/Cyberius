import {
  Component,
  inject,
  OnDestroy,
  OnInit,
  output,
  signal,
  ViewEncapsulation,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faArrowRightToBracket,
  faCode,
  faEnvelope,
  faExclamationTriangle,
  faEye,
  faEyeSlash,
  faLock,
  faSpinner,
  faXmark,
} from '@fortawesome/free-solid-svg-icons';
import { FormErrors, LoginRequest } from '../../../core/models/auth.model';
import { isValid, validateLogin } from '../../../core/validators/auth.validator';
import { ToastService } from '../../../core/services/toast.service';

type FormStatus = 'idle' | 'loading' | 'error';

@Component({
  selector: 'app-login-modal',
  imports: [CommonModule, FormsModule, FontAwesomeModule],
  templateUrl: './login-modal.html',
  styleUrl: './login-modal.css',
})
export class LoginModal implements OnInit, OnDestroy {
  private auth = inject(AuthService);
  private toast = inject(ToastService);

  closed = output<void>();
  success = output<void>();
  openRegister = output<void>();

  protected readonly faCode = faCode;
  protected readonly faEnvelope = faEnvelope;
  protected readonly faLock = faLock;
  protected readonly faEye = faEye;
  protected readonly faEyeSlash = faEyeSlash;
  protected readonly faXmark = faXmark;
  protected readonly faArrowRightToBracket = faArrowRightToBracket;
  protected readonly faSpinner = faSpinner;
  protected readonly faExclamationTriangle = faExclamationTriangle;

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

  // Валидация при потере фокуса
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
    // Помечаем все поля как тронутые
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
            ? 'Неверно ввели эл. почту или пароль'
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
