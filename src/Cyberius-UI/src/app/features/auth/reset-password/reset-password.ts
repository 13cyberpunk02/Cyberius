import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faCheckCircle,
  faEye,
  faEyeSlash,
  faLock,
  faSpinner,
} from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-reset-password',
  imports: [CommonModule, RouterModule, FormsModule, FaIconComponent],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.css',
})
export class ResetPassword implements OnInit {
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  token = '';
  newPassword = '';
  confirm = '';
  loading = signal(false);
  success = signal(false);
  error = signal('');
  showPass = signal(false);

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    if (!this.token) {
      this.error.set('Недействительная ссылка. Запросите новую.');
    }
  }

  get passwordsMatch(): boolean {
    return this.newPassword === this.confirm;
  }

  submit(): void {
    this.error.set('');

    if (!this.newPassword || this.newPassword.length < 8) {
      this.error.set('Пароль должен быть не менее 8 символов');
      return;
    }
    if (!this.passwordsMatch) {
      this.error.set('Пароли не совпадают');
      return;
    }

    this.loading.set(true);
    this.http
      .post(`${this.auth.API}/auth/reset-password`, {
        token: this.token,
        newPassword: this.newPassword,
      })
      .subscribe({
        next: () => {
          this.success.set(true);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(err?.error?.detail ?? 'Ссылка недействительна или истекла');
          this.loading.set(false);
        },
      });
  }

  goToHome(): void {
    this.router.navigate(['/']);
  }

  protected readonly faLock = faLock;
  protected readonly faCheckCircle = faCheckCircle;
  protected readonly faEyeSlash = faEyeSlash;
  protected readonly faEye = faEye;
  protected readonly faSpinner = faSpinner;
}
