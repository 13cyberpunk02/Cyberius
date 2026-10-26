import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PostCard } from '../../components/post-card/post-card';
import { PostsService } from '../../../../core/services/posts.service';
import { CategoryService } from '../../../../core/services/category.service';
import { AuthService } from '../../../../core/services/auth.service';
import { PostSummary } from '../../../../core/models/post.model';
import { CategoryResponse } from '../../../../core/models/category.model';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faEnvelopesBulk,
  faMagnifyingGlass,
  faPenSquare,
  faPlusCircle, faSpinner,
  faXmark,
} from '@fortawesome/free-solid-svg-icons';
import { SeoService } from '../../../../core/services/seo.service';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-post-list',
  imports: [CommonModule, RouterModule, FormsModule, PostCard, FaIconComponent],
  templateUrl: './post-list.html',
  styleUrl: './post-list.css',
})

export class PostList implements OnInit, OnDestroy {
  private postsService = inject(PostsService);
  private categoryService = inject(CategoryService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  readonly auth = inject(AuthService);
  private seo = inject(SeoService);
  private toast = inject(ToastService);

  protected readonly faPlusCircle = faPlusCircle;
  protected readonly faMagnifyingGlass = faMagnifyingGlass;
  protected readonly faXmark = faXmark;
  protected readonly faPenSquare = faPenSquare;
  protected readonly faEnvelopesBulk = faEnvelopesBulk;

  posts = signal<PostSummary[]>([]);
  categories = signal<CategoryResponse[]>([]);
  loading = signal(true);
  loadingMore = signal(false);
  hasMore = signal(false);
  totalCount = signal(0);
  searchQuery = signal('');

  activeCategoryId = signal<string | null>(null);
  activeTag = signal<string | null>(null);
  private page = 1;
  readonly pageSize = 9;

  private observer: IntersectionObserver | null = null;

  ngOnInit(): void {
    this.seo.setPage({
      title: 'Все статьи',
      description: 'Статьи о C#, .NET 10, Angular и архитектуре программного обеспечения',
    });

    this.route.queryParams.subscribe((params) => {
      const categorySlug = params['categorySlug'] ?? null;

      if (categorySlug) {
        this.categoryService.getAll().subscribe((cats) => {
          const found = cats.find((c) => c.slug === categorySlug);
          this.activeCategoryId.set(found?.id ?? null);
          this.activeTag.set(null);
          this.searchQuery.set('');
          this.resetAndLoad();
        });
      } else {
        this.activeCategoryId.set(params['category'] ?? null);
        this.activeTag.set(params['tag'] ?? null);
        this.searchQuery.set(params['q'] ?? '');
        this.resetAndLoad();
      }
    });

    this.categoryService.getAll().subscribe({
      next: (cats) => this.categories.set(cats),
    });
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }

  private resetAndLoad(): void {
    this.page = 1;
    this.posts.set([]);
    this.hasMore.set(false);
    this.load(false);
  }

  private load(append: boolean): void {
    if (append) {
      this.loadingMore.set(true);
    } else {
      this.loading.set(true);
    }

    const params = { page: this.page, pageSize: this.pageSize };
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
        if (append) {
          this.posts.update((existing) => [...existing, ...res.items]);
        } else {
          this.posts.set(res.items);
          this.totalCount.set(res.totalCount);
        }
        this.hasMore.set(this.page < res.totalPages);
        this.loading.set(false);
        this.loadingMore.set(false);

        if (this.hasMore()) {
          setTimeout(() => this.setupObserver(), 100);
        } else {
          this.observer?.disconnect();
        }
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
    this.load(true);
  }

  // Настройка IntersectionObserver на sentinel элемент
  setupObserver(): void {
    this.observer?.disconnect();

    const sentinel = document.getElementById('scroll-sentinel');
    if (!sentinel) return;

    this.observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && this.hasMore() && !this.loadingMore()) {
          this.loadMore();
        }
      },
      { rootMargin: '200px' }, // начинаем загрузку за 200px до конца списка
    );

    this.observer.observe(sentinel);
  }

  search(): void {
    this.navigate({ q: this.searchQuery() || null, category: null, tag: null });
  }

  clearSearch(): void {
    this.searchQuery.set('');
    this.navigate({ q: null });
  }

  selectCategory(id: string | null): void {
    this.navigate({ category: id, q: null, tag: null });
  }

  private navigate(extras: Record<string, unknown>): void {
    const current = this.route.snapshot.queryParams;
    this.router.navigate([], {
      queryParams: { ...current, ...extras },
      queryParamsHandling: 'merge',
    });
  }

  protected readonly faSpinner = faSpinner;
}

