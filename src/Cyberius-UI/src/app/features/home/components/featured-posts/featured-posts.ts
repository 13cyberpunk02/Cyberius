import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PostsService } from '../../../../core/services/posts.service';
import { CATEGORY_META, Post, PostCategory } from '../../../../core/models/post.model';

@Component({
  selector: 'app-featured-posts',
  imports: [CommonModule],
  templateUrl: './featured-posts.html',
  styleUrl: './featured-posts.css',
})
export class FeaturedPosts {
  private postsService = inject(PostsService);

  allPosts = this.postsService.getFeaturedPosts();
  activeFilter = signal<PostCategory | 'all'>('all');

  filters: { label: string; value: PostCategory | 'all' }[] = [
    { label: 'Все', value: 'all' },
    { label: 'C#', value: 'csharp' },
    { label: '.NET', value: 'dotnet' },
    { label: 'Angular', value: 'angular' },
    { label: 'Архитектура', value: 'architecture' },
    { label: 'DevOps', value: 'devops' },
  ];

  get filteredPosts(): Post[] {
    const f = this.activeFilter();
    return f === 'all' ? this.allPosts : this.allPosts.filter((p) => p.category === f);
  }

  setFilter(value: PostCategory | 'all') {
    this.activeFilter.set(value);
  }

  getCategoryMeta(category: PostCategory) {
    return CATEGORY_META[category];
  }

  formatDate(dateStr: string): string {
    return new Intl.DateTimeFormat('ru-RU', { day: 'numeric', month: 'long' }).format(
      new Date(dateStr),
    );
  }
}
