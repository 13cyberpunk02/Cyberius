import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PostsService } from '../../../../core/services/posts.service';
import { CATEGORY_META, PostSummary } from '../../../../core/models/post.model';
import { SafeHtml } from '@angular/platform-browser';
import { SvgIconService } from '../../../../core/services/svgIcon.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faArrowRight, faBarsStaggered, faClock } from '@fortawesome/free-solid-svg-icons';

type CategoryMeta = {
  label:        string;
  outlineColor: string;
  iconFile:     string;
  icon?:        SafeHtml;
};

type FilterValue = 'all' | string; // slug категории

@Component({
  selector: 'app-featured-posts',
  imports: [CommonModule, FaIconComponent],
  templateUrl: './featured-posts.html',
  styleUrl: './featured-posts.css',
})
export class FeaturedPosts implements OnInit {
  private postsService = inject(PostsService);
  private svgService = inject(SvgIconService);

  posts = signal<PostSummary[]>([]);
  loading = signal(true);
  activeFilter = signal<FilterValue>('all');

  categoryMeta = signal<Record<string, CategoryMeta>>(
    Object.fromEntries(Object.entries(CATEGORY_META).map(([k, v]) => [k, { ...v }])),
  );

  filters: { label: string; value: FilterValue }[] = [
    { label: 'Все', value: 'all' },
    { label: 'C#', value: 'csharp' },
    { label: '.NET', value: 'dotnet' },
    { label: 'Angular', value: 'angular' },
    { label: 'Архитектура', value: 'architecture' },
    { label: 'DevOps', value: 'devops' },
  ];

  ngOnInit(): void {
    // Загружаем SVG иконки категорий
    Object.entries(CATEGORY_META).forEach(([key, meta]) => {
      this.svgService.load(meta.iconFile).subscribe((svg) => {
        this.categoryMeta.update((m) => ({
          ...m,
          [key]: { ...m[key], icon: svg },
        }));
      });
    });

    // Загружаем посты с API
    this.loadPosts();
  }

  private loadPosts(): void {
    this.loading.set(true);
    this.postsService.getPublished({ page: 1, pageSize: 6 }).subscribe({
      next: (res) => {
        this.posts.set(res.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  get filteredPosts(): PostSummary[] {
    const f = this.activeFilter();
    if (f === 'all') return this.posts();
    return this.posts().filter((p) => p.category.slug === f);
  }

  setFilter(value: FilterValue): void {
    this.activeFilter.set(value);
  }

  getMeta(categorySlug: string): CategoryMeta {
    return (
      this.categoryMeta()[categorySlug] ?? {
        label: categorySlug,
        outlineColor: '#0ca2e7',
        iconFile: '',
      }
    );
  }

  formatDate(dateStr: string): string {
    return new Intl.DateTimeFormat('ru-RU', {
      day: 'numeric',
      month: 'long',
    }).format(new Date(dateStr));
  }

  getReadDate(post: PostSummary): string {
    return this.formatDate(post.publishedAt ?? post.createdAt);
  }

  protected readonly faClock = faClock;
  protected readonly faArrowRight = faArrowRight;
  protected readonly faBarsStaggered = faBarsStaggered;
}
