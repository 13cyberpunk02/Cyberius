import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { BlockRenderer } from '../../components/block-renderer/block-renderer';
import {
  ConfirmDialog,
  ConfirmDialogConfig,
} from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { PostsService } from '../../../../core/services/posts.service';
import { AuthService } from '../../../../core/services/auth.service';
import { PostDetailModel, ReactionType } from '../../../../core/models/post.model';
import { Comments } from '../../components/comments/comments';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faAngleLeft,
  faCommentDots,
  faEye,
  faPenToSquare,
  faTrashCan,
} from '@fortawesome/free-solid-svg-icons';

const REACTION_EMOJI: Record<string, string> = {
  Like: '👍',
  Heart: '❤️',
  Fire: '🔥',
  Clap: '👏',
  Thinking: '🤔',
};

@Component({
  selector: 'app-post-detail',
  imports: [CommonModule, RouterModule, BlockRenderer, ConfirmDialog, Comments, FaIconComponent],
  templateUrl: './post-detail.html',
  styleUrl: './post-detail.css',
})
export class PostDetail implements OnInit {
  private postsService = inject(PostsService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  readonly auth = inject(AuthService);

  post = signal<PostDetailModel | null>(null);
  loading = signal(true);
  notFound = signal(false);
  showDelete = signal(false);
  deleting = signal(false);

  readonly reactions = Object.entries(REACTION_EMOJI) as [ReactionType, string][];

  // Может ли текущий пользователь редактировать/удалять
  readonly canEdit = computed(() => {
    const post = this.post();
    const user = this.auth.user();
    if (!post || !user) return false;

    const roles = this.auth.profile()?.roles ?? [];
    const isPrivileged = roles.some((r) => r === 'Admin' || r === 'Manager');
    return post.author.id === user.id || isPrivileged;
  });

  readonly deleteConfig: ConfirmDialogConfig = {
    title: 'Удалить статью?',
    message: 'Это действие необратимо. Статья и все комментарии будут удалены.',
    confirmLabel: 'Да, удалить',
    cancelLabel: 'Отмена',
    danger: true,
  };

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug')!;
    this.postsService.getBySlug(slug).subscribe({
      next: (post) => {
        this.post.set(post);
        this.loading.set(false);
        // Трекаем просмотр после загрузки статьи
        this.trackView(post.id);
      },
      error: (err) => {
        this.loading.set(false);
        if (err.status === 404) this.notFound.set(true);
      },
    });
  }

  private trackView(postId: string): void {
    // Небольшая задержка — считаем просмотр только если пользователь реально открыл статью
    setTimeout(() => {
      this.postsService.trackView(postId).subscribe({ error: () => {} });
    }, 3000);
  }

  react(type: ReactionType): void {
    if (!this.auth.isAuthenticated() || !this.post()) return;
    this.postsService.react(this.post()!.id, type).subscribe({
      next: () => {
        // Оптимистичный апдейт
        this.post.update((p) => {
          if (!p) return p;
          const reactions = { ...p.reactions };
          const myReaction = p.myReaction;

          if (myReaction === type) {
            // Убираем реакцию
            reactions[type] = Math.max(0, (reactions[type] ?? 1) - 1);
            return { ...p, reactions, myReaction: null };
          } else {
            // Убираем старую если была
            if (myReaction) reactions[myReaction] = Math.max(0, (reactions[myReaction] ?? 1) - 1);
            // Добавляем новую
            reactions[type] = (reactions[type] ?? 0) + 1;
            return { ...p, reactions, myReaction: type };
          }
        });
      },
    });
  }

  confirmDelete(): void {
    if (!this.post()) return;
    this.deleting.set(true);
    this.postsService.delete(this.post()!.id).subscribe({
      next: () => this.router.navigate(['/posts']),
      error: () => {
        this.deleting.set(false);
        this.showDelete.set(false);
      },
    });
  }

  getReactionCount(type: string): number {
    return this.post()?.reactions[type] ?? 0;
  }

  getImageUrl(path: string | null): string | null {
    return this.postsService.getImageUrl(path);
  }

  formatDate(dateStr: string | null): string {
    if (!dateStr) return '';
    return new Intl.DateTimeFormat('ru-RU', {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
    }).format(new Date(dateStr));
  }

  protected readonly faAngleLeft = faAngleLeft;
  protected readonly faPenToSquare = faPenToSquare;
  protected readonly faTrashCan = faTrashCan;
  protected readonly faEye = faEye;
  protected readonly faCommentDots = faCommentDots;
}
