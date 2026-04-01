import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ConfirmDialog, ConfirmDialogConfig } from '../confirm-dialog/confirm-dialog';
import { PostsService } from '../../../core/services/posts.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { SeoService } from '../../../core/services/seo.service';
import { PostSummary } from '../../../core/models/post.model';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faFilePen, faPen, faPlus, faRocket, faSpinner } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-drafts',
  imports: [CommonModule, RouterModule, ConfirmDialog, FaIconComponent],
  templateUrl: './drafts.html',
  styleUrl: './drafts.css',
})
export class Drafts implements OnInit {
  private postsService = inject(PostsService);
  private auth = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);
  private seo = inject(SeoService);

  drafts = signal<PostSummary[]>([]);
  loading = signal(true);
  publishing = signal<string | null>(null);
  deleteId = signal<string | null>(null);
  deleting = signal(false);

  readonly deleteConfig: ConfirmDialogConfig = {
    title: 'Удалить черновик?',
    message: 'Статья будет удалена без возможности восстановления.',
    confirmLabel: 'Удалить',
    cancelLabel: 'Отмена',
    danger: true,
  };

  ngOnInit(): void {
    this.seo.setPage({ title: 'Мои черновики' });
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.postsService.getDrafts().subscribe({
      next: (res) => {
        this.drafts.set(res.items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  publish(post: PostSummary): void {
    this.publishing.set(post.id);
    this.postsService.publish(post.id).subscribe({
      next: () => {
        this.toast.success(`«${post.title}» опубликована`);
        this.drafts.update((list) => list.filter((d) => d.id !== post.id));
        this.publishing.set(null);
      },
      error: (err) => {
        this.toast.error(err?.error?.message ?? 'Не удалось опубликовать');
        this.publishing.set(null);
      },
    });
  }

  confirmDelete(): void {
    const id = this.deleteId();
    if (!id) return;
    this.deleting.set(true);
    this.postsService.delete(id).subscribe({
      next: () => {
        this.toast.success('Черновик удалён');
        this.drafts.update((list) => list.filter((d) => d.id !== id));
        this.deleteId.set(null);
        this.deleting.set(false);
      },
      error: (err) => {
        this.toast.error(err?.error?.message ?? 'Ошибка при удалении');
        this.deleteId.set(null);
        this.deleting.set(false);
      },
    });
  }

  formatDate(dateStr: string): string {
    return new Intl.DateTimeFormat('ru-RU', {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(new Date(dateStr));
  }

  getImageUrl(path: string | null): string | null {
    if (!path) return null;
    if (path.startsWith('http')) return path;
    return this.auth.FILES_BASE + path;
  }

  protected readonly faPlus = faPlus;
  protected readonly faFilePen = faFilePen;
  protected readonly faSpinner = faSpinner;
  protected readonly faRocket = faRocket;
  protected readonly faPen = faPen;
}
