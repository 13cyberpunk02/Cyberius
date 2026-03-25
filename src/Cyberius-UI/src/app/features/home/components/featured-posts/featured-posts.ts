import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PostsService } from '../../../../core/services/posts.service';
import { CATEGORY_META, Post, PostCategory } from '../../../../core/models/post.model';
import { SafeHtml } from '@angular/platform-browser';
import { SvgIconService } from '../../../../core/services/svgIcon.service';

type CategoryMeta = {
  label: string;
  color: string;
  iconFile: string;
  outlineColor: string;
  icon?: SafeHtml;
};

@Component({
  selector: 'app-featured-posts',
  imports: [CommonModule],
  templateUrl: './featured-posts.html',
  styleUrl: './featured-posts.css',
})
export class FeaturedPosts implements OnInit {
  private postsService = inject(PostsService);
  private svgService = inject(SvgIconService);

  allPosts = this.postsService.getFeaturedPosts();
  activeFilter = signal<PostCategory | 'all'>('all');

  // Мета с загруженными SVG
  categoryMeta = signal<Record<PostCategory, CategoryMeta>>(
    Object.fromEntries(Object.entries(CATEGORY_META).map(([k, v]) => [k, { ...v }])) as Record<
      PostCategory,
      CategoryMeta
    >,
  );

  filters: { label: string; value: PostCategory | 'all' }[] = [
    { label: 'Все', value: 'all' },
    { label: 'C#', value: 'csharp' },
    { label: '.NET', value: 'dotnet' },
    { label: 'Angular', value: 'angular' },
    { label: 'Архитектура', value: 'architecture' },
    { label: 'DevOps', value: 'devops' },
  ];

  ngOnInit(): void {
    // Загружаем SVG для каждой категории через SvgIconService (с кэшем)
    (Object.keys(CATEGORY_META) as PostCategory[]).forEach((category) => {
      const { iconFile } = CATEGORY_META[category];
      this.svgService.load(iconFile).subscribe((svg) => {
        this.categoryMeta.update((meta) => ({
          ...meta,
          [category]: { ...meta[category], icon: svg },
        }));
      });
    });
  }

  get filteredPosts(): Post[] {
    const f = this.activeFilter();
    return f === 'all' ? this.allPosts : this.allPosts.filter((p) => p.category === f);
  }

  setFilter(value: PostCategory | 'all') {
    this.activeFilter.set(value);
  }

  getMeta(category: PostCategory): CategoryMeta {
    return this.categoryMeta()[category];
  }

  formatDate(dateStr: string): string {
    return new Intl.DateTimeFormat('ru-RU', { day: 'numeric', month: 'long' }).format(
      new Date(dateStr),
    );
  }
}
