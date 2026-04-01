import {
  Component,
  inject,
  OnDestroy,
  OnInit,
  output,
  signal,
  ViewEncapsulation,
} from '@angular/core';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import {
  faCheck,
  faExclamationTriangle,
  faEye,
  faEyeSlash,
  faLock,
  faShieldAlt,
  faSpinner,
  faXmark,
} from '@fortawesome/free-solid-svg-icons';
import { ToastService } from '../../../core/services/toast.service';

interface ChangePasswordForm {
  oldPassword: string;
  newPassword: string;
  confirmPassword: string;
}

interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
  confirmPassword: string;
}

type FormStatus = 'idle' | 'saving' | 'success' | 'error';

interface FieldErrors {
  oldPassword?: string;
  newPassword?: string;
  confirmPassword?: string;
}

@Component({
  selector: 'app-change-password-modal',
  imports: [CommonModule, FormsModule, FontAwesomeModule],
  templateUrl: './change-password-modal.html',
  styleUrl: './change-password-modal.css',
  encapsulation: ViewEncapsulation.None,
})
export class ChangePasswordModal implements OnInit, OnDestroy {
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  private toast = inject(ToastService);

  closed = output<void>();

  // Icons
  faLock = faLock;
  faEye = faEye;
  faEyeSlash = faEyeSlash;
  faShieldAlt = faShieldAlt;
  faSpinner = faSpinner;
  faXmark = faXmark;
  faCheck = faCheck;

  form: ChangePasswordForm = {
    oldPassword: '',
    newPassword: '',
    confirmPassword: '',
  };

  showCurrent = signal(false);
  showNew = signal(false);
  showConfirm = signal(false);

  status = signal<FormStatus>('idle');
  serverError = signal('');
  errors = signal<FieldErrors>({});
  touched = signal<Partial<Record<keyof ChangePasswordForm, boolean>>>({});

  // Strength indicator для нового пароля
  passwordStrength = signal<0 | 1 | 2 | 3>(0);

  ngOnInit(): void {
    document.body.style.overflow = 'hidden';
  }
  ngOnDestroy(): void {
    document.body.style.overflow = '';
  }

  onBlur(field: keyof ChangePasswordForm): void {
    this.touched.update((t) => ({ ...t, [field]: true }));
    this.validate();
  }

  onNewPasswordInput(): void {
    this.calcStrength(this.form.newPassword);
    if (this.touched()['newPassword']) this.validate();
  }

  private calcStrength(pw: string): void {
    let score = 0;
    if (pw.length >= 8) score++;
    if (/[A-Z]/.test(pw) && /[a-z]/.test(pw)) score++;
    if (/\d/.test(pw)) score++;
    if (/[^A-Za-z0-9]/.test(pw)) score++;
    this.passwordStrength.set(Math.min(score, 3) as 0 | 1 | 2 | 3);
  }

  private validate(): boolean {
    const e: FieldErrors = {};
    const { oldPassword, newPassword, confirmPassword } = this.form;

    if (!oldPassword) e.oldPassword = 'Введите текущий пароль';

    if (!newPassword) e.newPassword = 'Введите новый пароль';
    else if (newPassword.length < 8) e.newPassword = 'Минимум 8 символов';
    else if (!/(?=.*[A-Za-z])(?=.*\d)/.test(newPassword))
      e.newPassword = 'Должен содержать букву и цифру';
    else if (newPassword === oldPassword) e.newPassword = 'Новый пароль совпадает с текущим';

    if (!confirmPassword) e.confirmPassword = 'Подтвердите новый пароль';
    else if (newPassword !== confirmPassword) e.confirmPassword = 'Пароли не совпадают';

    this.errors.set(e);
    return Object.keys(e).length === 0;
  }

  fieldError(field: keyof ChangePasswordForm): string {
    return this.touched()[field] ? (this.errors()[field] ?? '') : '';
  }

  get strengthLabel(): string {
    return ['', 'Слабый', 'Средний', 'Надёжный'][this.passwordStrength()];
  }

  get strengthColor(): string {
    return ['', 'text-rose-400', 'text-amber-400', 'text-emerald-400'][this.passwordStrength()];
  }

  get strengthBars(): ('rose' | 'amber' | 'emerald' | 'empty')[] {
    const s = this.passwordStrength();
    const colors: ('rose' | 'amber' | 'emerald' | 'empty')[] = ['empty', 'empty', 'empty'];
    if (s >= 1) colors[0] = 'rose';
    if (s >= 2) colors[1] = 'amber';
    if (s >= 3) colors[2] = 'emerald';
    return colors;
  }

  submit(): void {
    this.touched.set({ oldPassword: true, newPassword: true, confirmPassword: true });
    if (!this.validate()) return;

    const userId = this.auth.user()?.id;
    if (!userId) return;

    this.status.set('saving');
    this.serverError.set('');

    const body: ChangePasswordRequest = {
      oldPassword: this.form.oldPassword,
      newPassword: this.form.newPassword,
      confirmPassword: this.form.confirmPassword,
    };

    this.http.put(`${this.auth.API}/users/${userId}/change-password`, body).subscribe({
      next: () => {
        this.status.set('success');
        this.toast.success('Пароль успешно изменён');
        setTimeout(() => this.closed.emit(), 2000);
      },
      error: (err) => {
        this.status.set('error');
        const msg =
          err?.status === 400
            ? 'Неверный текущий пароль'
            : (err?.error?.message ?? err?.error?.title ?? 'Произошла ошибка, попробуйте ещё раз');
        this.serverError.set(msg);
        this.toast.error(msg);
      },
    });
  }

  onBackdropClick(e: MouseEvent): void {
    if ((e.target as HTMLElement).classList.contains('modal-backdrop')) this.closed.emit();
  }

  toggleCurrent(): void {
    this.showCurrent.update((v) => !v);
  }
  toggleNew(): void {
    this.showNew.update((v) => !v);
  }
  toggleConfirm(): void {
    this.showConfirm.update((v) => !v);
  }

  protected readonly faExclamationTriangle = faExclamationTriangle;
}
