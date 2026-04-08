import { Component, computed, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PostSummary } from '../../../../core/models/post.model';
import { AuthService } from '../../../../core/services/auth.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faCirclePause, faClock, faCommentDots, faEye } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-post-card',
  imports: [CommonModule, RouterModule, FaIconComponent],
  templateUrl: './post-card.html',
  styleUrl: './post-card.css',
})
export class PostCard {
  post = input.required<PostSummary>();
  private auth = inject(AuthService);

  readonly authorAvatarUrl = computed(() => {
    const path = this.post().author.avatarUrl;
    if (!path) return null;
    if (path.startsWith('http')) return path;
    return this.auth.FILES_BASE + path;
  });

  get coverUrl(): string | null {
    return this.post().coverImageUrl;
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

  protected readonly faClock = faClock;
  protected readonly faEye = faEye;
  protected readonly faCommentDots = faCommentDots;
  protected readonly faCirclePause = faCirclePause;
}
