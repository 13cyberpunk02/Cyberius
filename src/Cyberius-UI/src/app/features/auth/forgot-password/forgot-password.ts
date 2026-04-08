import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faArrowLeft, faEnvelope, faKey, faSpinner } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-forgot-password',
  imports: [CommonModule, RouterModule, FormsModule, FaIconComponent],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css',
})
export class ForgotPassword {
  private http = inject(HttpClient);
  private auth = inject(AuthService);

  email = '';
  loading = signal(false);
  sent = signal(false);
  error = signal('');

  submit(): void {
    const emailTrimmed = this.email.trim();
    if (!emailTrimmed) {
      this.error.set('Введите email');
      return;
    }

    this.loading.set(true);
    this.error.set('');

    this.http.post(`${this.auth.API}/auth/forgot-password`, { email: emailTrimmed }).subscribe({
      next: () => {
        this.sent.set(true);
        this.loading.set(false);
      },
      error: () => {
        // Намеренно показываем успех даже при ошибке — не раскрываем существование email
        this.sent.set(true);
        this.loading.set(false);
      },
    });
  }

  protected readonly faKey = faKey;
  protected readonly faEnvelope = faEnvelope;
  protected readonly faSpinner = faSpinner;
  protected readonly faArrowLeft = faArrowLeft;
}
