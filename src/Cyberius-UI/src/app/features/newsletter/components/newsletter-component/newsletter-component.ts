import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../../core/services/auth.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faCheckCircle, faEnvelope, faSpinner } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-newsletter-component',
  imports: [CommonModule, FormsModule, FaIconComponent],
  templateUrl: './newsletter-component.html',
  styleUrl: './newsletter-component.css',
})
export class NewsletterComponent {
  private http = inject(HttpClient);
  private auth = inject(AuthService);

  email = '';
  loading = signal(false);
  success = signal(false);
  error = signal('');

  submit(): void {
    const emailTrimmed = this.email.trim();
    if (!emailTrimmed) {
      this.error.set('Введите эл. почту');
      return;
    }
    if (!this.isValidEmail(emailTrimmed)) {
      this.error.set('Некорректная эл. почта');
      return;
    }

    this.loading.set(true);
    this.error.set('');

    this.http.post(`${this.auth.API}/newsletter/subscribe`, { email: emailTrimmed }).subscribe({
      next: () => {
        this.success.set(true);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err?.error?.detail ?? 'Не удалось подписаться. Попробуйте позже.');
        this.loading.set(false);
      },
    });
  }

  private isValidEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  protected readonly faCheckCircle = faCheckCircle;
  protected readonly faEnvelope = faEnvelope;
  protected readonly faSpinner = faSpinner;
}
