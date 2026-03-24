import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PostsService } from '../../../../core/services/posts.service';

@Component({
  selector: 'app-stats',
  imports: [CommonModule],
  templateUrl: './stats.html',
  styleUrl: './stats.css',
})
export class Stats {
  private postsService = inject(PostsService);
  stats = this.postsService.getStats();

  items = [
    { value: this.stats.posts + '+', label: 'Статей', icon: '📝', color: 'text-sky-400' },
    { value: this.stats.readers, label: 'Читателей', icon: '👥', color: 'text-cyan-400' },
    { value: this.stats.topics.toString(), label: 'Тем', icon: '🗂', color: 'text-indigo-400' },
    {
      value: this.stats.experience + ' лет',
      label: 'Опыта',
      icon: '🚀',
      color: 'text-emerald-400',
    },
  ];
}
