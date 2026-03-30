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
  items = [
    { value: '48+', label: 'Статей', icon: '📝', color: 'text-sky-400' },
    { value: '12K', label: 'Читателей', icon: '👥', color: 'text-cyan-400' },
    { value: '5', label: 'Тем', icon: '🗂', color: 'text-indigo-400' },
    { value: '8 лет', label: 'Опыта', icon: '🚀', color: 'text-emerald-400' },
  ];
}
