import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { PostCard } from '../../../features/posts/components/post-card/post-card';
import { PostsService } from '../../../core/services/posts.service';
import { AuthService } from '../../../core/services/auth.service';
import { HttpClient } from '@angular/common/http';
import { SeoService } from '../../../core/services/seo.service';
import { PagedResponse, PostSummary } from '../../../core/models/post.model';
import { UserProfile } from '../../../core/models/auth.model';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faArrowLeft, faArrowRight, faCalendar } from '@fortawesome/free-solid-svg-icons';
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
  paged = signal<PagedResponse<PostSummary> | null>(null);
  loading = signal(true);
  notFound = signal(false);
  currentPage = signal(1);
  readonly pageSize = 9;

  get posts(): PostSummary[] {
    return this.paged()?.items ?? [];
  }
  get totalPages(): number {
    return this.paged()?.totalPages ?? 1;
  }
  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  readonly avatarUrl = computed(() => {
    const path = this.profile()?.avatarUrl;
    if (!path) return null;
    if (path.startsWith('http')) return path;
    return this.auth.FILES_BASE + path;
  });

  ngOnInit(): void {
    const userId = this.route.snapshot.paramMap.get('userId')!;
    this.loadProfile(userId);
  }

  private loadProfile(userId: string): void {
    this.http.get<UserProfile>(`${this.auth.API}/auth/${userId}`).subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.seo.setPage({
          title: `${profile.firstName} ${profile.lastName}`,
          description: `Статьи автора ${profile.firstName} ${profile.lastName} на Cyberius`,
        });
        this.loadPosts(userId);
      },
      error: () => {
        this.loading.set(false);
        this.notFound.set(true);
      },
    });
  }

  private loadPosts(userId: string): void {
    this.postsService
      .getByAuthor(userId, { page: this.currentPage(), pageSize: this.pageSize })
      .subscribe({
        next: (res) => {
          this.paged.set(res);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
  }

  goToPage(page: number): void {
    this.currentPage.set(page);
    const userId = this.route.snapshot.paramMap.get('userId')!;
    this.loadPosts(userId);
    window.scrollTo({ top: 0, behavior: 'smooth' });
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
}
