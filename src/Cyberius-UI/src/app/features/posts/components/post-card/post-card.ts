import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PostSummary } from '../../../../core/models/post.model';

@Component({
  selector: 'app-post-card',
  imports: [CommonModule, RouterModule],
  templateUrl: './post-card.html',
  styleUrl: './post-card.css',
})
export class PostCard {
  post = input.required<PostSummary>();

  get coverUrl(): string {
    return this.post().coverImageUrl!;
  }

  get categoryColor(): string {
    return this.post().category.color ?? '#0ca2e7';
  }

  formatDate(dateStr: string | null): string {
    if (!dateStr) return '';
    return new Intl.DateTimeFormat('ru-RU', {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
    }).format(new Date(dateStr));
  }

  totalReactions(): number {
    return Object.values(this.post().reactions).reduce((a, b) => a + b, 0);
  }
}
