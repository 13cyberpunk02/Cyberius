import { Component, inject, input, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../../core/services/auth.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faEye } from '@fortawesome/free-solid-svg-icons';

interface AuthorStats {
  totalViews: number;
  totalComments: number;
  totalPosts: number;
  topPosts: { id: string; title: string; slug: string; viewCount: number; commentCount: number }[];
  dailyViews: { date: string; count: number }[];
  reactions: { type: string; count: number }[];
}

@Component({
  selector: 'app-author-stats',
  imports: [CommonModule, RouterModule, FaIconComponent],
  templateUrl: './author-stats.html',
  styleUrl: './author-stats.css',
})
export class AuthorStatsComponent implements OnInit {
  authorId = input.required<string>();

  private http = inject(HttpClient);
  private auth = inject(AuthService);

  stats = signal<AuthorStats | null>(null);
  loading = signal(true);

  ngOnInit(): void {
    this.http.get<AuthorStats>(`${this.auth.API}/stats/author/${this.authorId()}`).subscribe({
      next: (s) => {
        this.stats.set(s);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  readonly reactionEmoji: Record<string, string> = {
    Like: '👍',
    Heart: '❤️',
    Fire: '🔥',
    Clap: '👏',
    Thinking: '🤔',
  };

  // Фиксированная высота контейнера графика в пикселях
  private readonly CHART_HEIGHT_PX = 96;

  barHeightPx(count: number): string {
    const max = Math.max(...(this.stats()?.dailyViews.map((d) => d.count) ?? [1]), 1);
    const px =
      count === 0
        ? 2 // минимум 2px — всегда видна засечка
        : Math.max(4, Math.round((count / max) * this.CHART_HEIGHT_PX));
    return `${px}px`;
  }

  barColor(count: number): string {
    const max = Math.max(...(this.stats()?.dailyViews.map((d) => d.count) ?? [1]), 1);
    const ratio = count / max;
    if (count === 0) return 'rgba(255,255,255,0.05)';
    if (ratio > 0.6) return 'rgb(14, 165, 233)'; // sky-500
    if (ratio > 0.3) return 'rgb(56, 189, 248)'; // sky-400
    return 'rgb(125, 211, 252)'; // sky-300
  }

  getAvatarUrl(path: string | null): string | null {
    if (!path) return null;
    if (path.startsWith('http')) return path;
    return this.auth.FILES_BASE + path;
  }

  formatDate(dateStr: string): string {
    const d = new Date(dateStr);
    return `${d.getDate()}.${String(d.getMonth() + 1).padStart(2, '0')}`;
  }

  // Показываем только каждый 5-й лейбл чтобы не было каши
  showLabel(index: number): boolean {
    return index % 5 === 0;
  }

  protected readonly faEye = faEye;
}
