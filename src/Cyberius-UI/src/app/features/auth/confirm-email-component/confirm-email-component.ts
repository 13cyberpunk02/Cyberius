import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faCheckCircle, faEnvelope, faSpinner, faXmark } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-confirm-email-component',
  imports: [CommonModule, RouterModule, FaIconComponent],
  templateUrl: './confirm-email-component.html',
  styleUrl: './confirm-email-component.css',
})
export class ConfirmEmailComponent implements OnInit {
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  status = signal<'loading' | 'success' | 'error'>('loading');
  error = signal('');

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!token) {
      this.status.set('error');
      this.error.set('Недействительная ссылка');
      return;
    }

    this.http.get(`${this.auth.API}/auth/confirm-email?token=${token}`).subscribe({
      next: () => this.status.set('success'),
      error: (err) => {
        this.status.set('error');
        this.error.set(err?.error?.detail ?? 'Ссылка недействительна или истекла');
      },
    });
  }

  protected readonly faEnvelope = faEnvelope;
  protected readonly faSpinner = faSpinner;
  protected readonly faCheckCircle = faCheckCircle;
  protected readonly faXmark = faXmark;
}
