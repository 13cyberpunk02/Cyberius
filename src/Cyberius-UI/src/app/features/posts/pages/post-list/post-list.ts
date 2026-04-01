import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PostCard } from '../../components/post-card/post-card';
import { PostsService } from '../../../../core/services/posts.service';
import { CategoryService } from '../../../../core/services/category.service';
import { AuthService } from '../../../../core/services/auth.service';
import { PagedResponse, PostSummary } from '../../../../core/models/post.model';
import { CategoryResponse } from '../../../../core/models/category.model';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faAngleLeft,
  faAngleRight,
  faMagnifyingGlass,
  faPlusCircle,
  faXmark,
} from '@fortawesome/free-solid-svg-icons';
import { SeoService } from '../../../../core/services/seo.service';

@Component({
  selector: 'app-post-list',
  imports: [CommonModule, RouterModule, FormsModule, PostCard, FaIconComponent],
  templateUrl: './post-list.html',
  styleUrl: './post-list.css',
})
export class PostList implements OnInit {
  private postsService = inject(PostsService);
  private categoryService = inject(CategoryService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  readonly auth = inject(AuthService);
  private seo = inject(SeoService);

  protected readonly faPlusCircle = faPlusCircle;
  protected readonly faMagnifyingGlass = faMagnifyingGlass;
  protected readonly faAngleLeft = faAngleLeft;
  protected readonly faAngleRight = faAngleRight;
  protected readonly faXmark = faXmark;

  paged = signal<PagedResponse<PostSummary> | null>(null);
  categories = signal<CategoryResponse[]>([]);
  loading = signal(true);
  searchQuery = signal('');

  // Filters from query params
  activeCategoryId = signal<string | null>(null);
  activeTag = signal<string | null>(null);
  currentPage = signal(1);
  readonly pageSize = 9;

  get posts(): PostSummary[] {
    return this.paged()?.items ?? [];
  }
  get totalPages(): number {
    return this.paged()?.totalPages ?? 1;
  }
  get totalCount(): number {
    return this.paged()?.totalCount ?? 0;
  }
  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  ngOnInit(): void {
    this.seo.setPage({
      title: 'Все статьи',
      description: 'Статьи о C#, .NET 10, Angular и архитектуре программного обеспечения',
    });

    this.route.queryParams.subscribe((params) => {
      this.activeCategoryId.set(params['category'] ?? null);
      this.activeTag.set(params['tag'] ?? null);
      this.currentPage.set(Number(params['page'] ?? 1));
      this.searchQuery.set(params['q'] ?? '');
      this.load();
    });

    this.categoryService.getAll().subscribe({
      next: (cats) => this.categories.set(cats),
    });
  }

  private load(): void {
    this.loading.set(true);
    const params = { page: this.currentPage(), pageSize: this.pageSize };
    const q = this.searchQuery();
    const catId = this.activeCategoryId();
    const tag = this.activeTag();

    const req$ = q
      ? this.postsService.search({ q, ...params })
      : catId
        ? this.postsService.getByCategory(catId, params)
        : tag
          ? this.postsService.getByTag(tag, params)
          : this.postsService.getPublished(params);

    req$.subscribe({
      next: (res) => {
        this.paged.set(res);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  search(): void {
    this.navigate({ q: this.searchQuery() || null, page: 1, category: null, tag: null });
  }

  clearSearch(): void {
    this.searchQuery.set('');
    this.navigate({ q: null, page: 1 });
  }

  selectCategory(id: string | null): void {
    this.navigate({ category: id, page: 1, q: null, tag: null });
  }

  goToPage(page: number): void {
    this.navigate({ page });
  }

  private navigate(extras: Record<string, unknown>): void {
    const current = this.route.snapshot.queryParams;
    this.router.navigate([], {
      queryParams: { ...current, ...extras },
      queryParamsHandling: 'merge',
    });
  }
}
