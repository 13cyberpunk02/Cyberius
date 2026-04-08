import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { PostCard } from '../../../posts/components/post-card/post-card';
import { PostsService } from '../../../../core/services/posts.service';
import { AuthService } from '../../../../core/services/auth.service';
import { HttpClient } from '@angular/common/http';
import { SeoService } from '../../../../core/services/seo.service';
import { PagedResponse, PostSummary } from '../../../../core/models/post.model';
import { UserProfile } from '../../../../core/models/auth.model';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faArrowLeft,
  faArrowRight,
  faCalendar,
  faEnvelopesBulk,
  faSpinner,
} from '@fortawesome/free-solid-svg-icons';
import { faBlogger } from '@fortawesome/free-brands-svg-icons';

@Component({
  selector: 'app-user-profile',
  imports: [CommonModule, RouterModule, PostCard, FaIconComponent],
  templateUrl: './user-profile.html',
  styleUrl: './user-profile.css',
})
export class UserProfileComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private postsService = inject(PostsService);
  private auth = inject(AuthService);
  private http = inject(HttpClient);
  private seo = inject(SeoService);

  profile = signal<UserProfile | null>(null);
  posts = signal<PostSummary[]>([]);
  loading = signal(true);
  loadingMore = signal(false);
  notFound = signal(false);
  hasMore = signal(false);
  private page = 1;
  private userId = '';
  readonly pageSize = 9;

  readonly avatarUrl = computed(() => {
    const path = this.profile()?.avatarUrl;
    if (!path) return null;
    if (path.startsWith('http')) return path;
    return this.auth.FILES_BASE + path;
  });

  ngOnInit(): void {
    this.userId = this.route.snapshot.paramMap.get('userId')!;
    this.loadProfile();
  }

  private loadProfile(): void {
    this.http.get<UserProfile>(`${this.auth.API}/auth/${this.userId}`).subscribe({
      next: (profile) => {
        this.profile.set(profile);
        const fullName = `${profile.firstName} ${profile.lastName}`.trim();
        this.seo.setPage({
          title: fullName,
          description: `Статьи автора ${fullName} на DevBlog`,
        });
        this.loadPosts(false);
      },
      error: () => {
        this.loading.set(false);
        this.notFound.set(true);
      },
    });
  }

  private loadPosts(append: boolean): void {
    if (append) this.loadingMore.set(true);

    this.postsService
      .getByAuthor(this.userId, { page: this.page, pageSize: this.pageSize })
      .subscribe({
        next: (res) => {
          if (append) {
            this.posts.update((existing) => [...existing, ...res.items]);
          } else {
            this.posts.set(res.items);
          }
          this.hasMore.set(this.page < res.totalPages);
          this.loading.set(false);
          this.loadingMore.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.loadingMore.set(false);
        },
      });
  }

  loadMore(): void {
    if (this.loadingMore() || !this.hasMore()) return;
    this.page++;
    this.loadPosts(true);
  }

  formatDate(dateStr: string): string {
    return new Intl.DateTimeFormat('ru-RU', {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
    }).format(new Date(dateStr));
  }

  protected readonly faCalendar = faCalendar;
  protected readonly faBlogger = faBlogger;
  protected readonly faArrowLeft = faArrowLeft;
  protected readonly faArrowRight = faArrowRight;
  protected readonly faEnvelopesBulk = faEnvelopesBulk;
  protected readonly faSpinner = faSpinner;
}
