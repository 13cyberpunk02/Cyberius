import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PostsService } from '../../../../core/services/posts.service';
import { AuthService } from '../../../../core/services/auth.service';
import { PostSummary } from '../../../../core/models/post.model';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faArrowRight,
  faClock,
  faCommentDots,
  faEye,
  faFire,
  faHeart,
  faHeartbeat,
  faNewspaper,
} from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-top-post',
  imports: [CommonModule, RouterModule, FaIconComponent],
  templateUrl: './top-post.html',
  styleUrl: './top-post.css',
})
export class TopPost implements OnInit {
  private postsService = inject(PostsService);
  private auth = inject(AuthService);

  post = signal<PostSummary | null>(null);
  loading = signal(true);

  readonly coverUrl = computed(() => {
    const p = this.post();
    if (!p?.coverImageUrl) return null;
    if (p.coverImageUrl.startsWith('http')) return p.coverImageUrl;
    return this.auth.FILES_BASE + p.coverImageUrl;
  });

  readonly authorAvatarUrl = computed(() => {
    const path = this.post()?.author.avatarUrl;
    if (!path) return null;
    if (path.startsWith('http')) return path;
    return this.auth.FILES_BASE + path;
  });

  readonly totalReactions = computed(() =>
    Object.values(this.post()?.reactions ?? {}).reduce((a, b) => a + b, 0),
  );

  ngOnInit(): void {
    // Берём первую страницу и выбираем пост с наибольшим числом просмотров
    this.postsService.getPublished({ page: 1, pageSize: 20 }).subscribe({
      next: (res) => {
        const top = res.items.reduce(
          (best, cur) => (cur.viewCount > (best?.viewCount ?? -1) ? cur : best),
          null as PostSummary | null,
        );
        this.post.set(top);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  formatDate(dateStr: string | null): string {
    if (!dateStr) return '';
    return new Intl.DateTimeFormat('ru-RU', {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
    }).format(new Date(dateStr));
  }

  get categoryColor(): string {
    return this.post()?.category.color ?? '#0ca2e7';
  }

  protected readonly faClock = faClock;
  protected readonly faEye = faEye;
  protected readonly faCommentDots = faCommentDots;
  protected readonly faHeartbeat = faHeartbeat;
  protected readonly faHeart = faHeart;
  protected readonly faArrowRight = faArrowRight;
  protected readonly faFire = faFire;
  protected readonly faNewspaper = faNewspaper;
}
