import { Component, inject, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../../core/services/auth.service';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faCheckCircle, faEnvelope, faSpinner, faXmark } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-unsubscribe-component',
  imports: [CommonModule, RouterModule, FaIconComponent],
  templateUrl: './unsubscribe-component.html',
  styleUrl: './unsubscribe-component.css',
})
export class UnsubscribeComponent implements OnInit {
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  private route = inject(ActivatedRoute);

  status = signal<'loading' | 'success' | 'error'>('loading');

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (!token) {
      this.status.set('error');
      return;
    }

    this.http.get(`${this.auth.API}/newsletter/unsubscribe?token=${token}`).subscribe({
      next: () => this.status.set('success'),
      error: () => this.status.set('error'),
    });
  }

  protected readonly faEnvelope = faEnvelope;
  protected readonly faSpinner = faSpinner;
  protected readonly faCheckCircle = faCheckCircle;
  protected readonly faXmark = faXmark;
}
