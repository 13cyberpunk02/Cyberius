import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faSpinner } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-newsletter',
  imports: [CommonModule, FormsModule, FaIconComponent],
  templateUrl: './newsletter.html',
  styleUrl: './newsletter.css',
})
export class Newsletter {
  email = '';
  status = signal<'idle' | 'loading' | 'success' | 'error'>('idle');

  async subscribe() {
    if (!this.email || !this.email.includes('@')) return;
    this.status.set('loading');
    // Simulate API call
    await new Promise((r) => setTimeout(r, 1200));
    this.status.set('success');
    this.email = '';
  }

  protected readonly faSpinner = faSpinner;
}
