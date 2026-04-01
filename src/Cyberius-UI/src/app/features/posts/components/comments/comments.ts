import { Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CommentService } from '../../../../core/services/comment.service';
import { AuthService } from '../../../../core/services/auth.service';
import { CommentResponse, PagedComments } from '../../../../core/models/comment.model';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faReply, faSpinner } from '@fortawesome/free-solid-svg-icons';

const REACTION_EMOJI: Record<string, string> = {
  Like: '👍',
  Heart: '❤️',
  Fire: '🔥',
  Clap: '👏',
  Thinking: '🤔',
};

@Component({
  selector: 'app-comments',
  imports: [FormsModule, CommonModule, FaIconComponent],
  templateUrl: './comments.html',
  styleUrl: './comments.css',
})
export class Comments implements OnInit {
  postId = input.required<string>();

  private commentService = inject(CommentService);
  readonly auth = inject(AuthService);

  paged = signal<PagedComments | null>(null);
  loading = signal(true);
  submitting = signal(false);

  // Новый комментарий
  newText = signal('');

  // Ответ на комментарий
  replyToId = signal<string | null>(null);
  replyToName = signal('');
  replyText = signal('');

  // Редактирование
  editingId = signal<string | null>(null);
  editText = signal('');

  readonly reactions = Object.entries(REACTION_EMOJI);

  readonly isAuthenticated = computed(() => this.auth.isAuthenticated());

  get comments(): CommentResponse[] {
    return this.paged()?.items ?? [];
  }

  get totalCount(): number {
    return this.paged()?.totalCount ?? 0;
  }

  ngOnInit(): void {
    this.load();
  }

  private load(page = 1): void {
    this.loading.set(true);
    this.commentService.getByPost(this.postId(), page).subscribe({
      next: (p) => {
        this.paged.set(p);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  // ── Submit new comment ─────────────────────────────────────────

  submitComment(): void {
    const content = this.newText().trim();
    if (!content || this.submitting()) return;

    this.submitting.set(true);
    this.commentService
      .create({ postId: this.postId(), content, parentCommentId: null })
      .subscribe({
        next: (comment) => {
          this.paged.update((p) =>
            p ? { ...p, items: [comment, ...p.items], totalCount: p.totalCount + 1 } : p,
          );
          this.newText.set('');
          this.submitting.set(false);
        },
        error: () => this.submitting.set(false),
      });
  }

  // ── Reply ──────────────────────────────────────────────────────

  openReply(comment: CommentResponse): void {
    this.replyToId.set(comment.id);
    this.replyToName.set(comment.author.fullName);
    this.replyText.set('');
    this.editingId.set(null);
  }

  cancelReply(): void {
    this.replyToId.set(null);
    this.replyText.set('');
  }

  submitReply(): void {
    const content = this.replyText().trim();
    const parentId = this.replyToId();
    if (!content || !parentId || this.submitting()) return;

    this.submitting.set(true);
    this.commentService
      .create({
        postId: this.postId(),
        content,
        parentCommentId: parentId,
      })
      .subscribe({
        next: (reply) => {
          // Добавляем ответ в нужный комментарий
          this.paged.update((p) => {
            if (!p) return p;
            return {
              ...p,
              items: p.items.map((c) =>
                c.id === parentId
                  ? { ...c, replies: [...(c.replies ?? []), reply], replyCount: c.replyCount + 1 }
                  : c,
              ),
            };
          });
          this.cancelReply();
          this.submitting.set(false);
        },
        error: () => this.submitting.set(false),
      });
  }

  // ── Edit ───────────────────────────────────────────────────────

  openEdit(comment: CommentResponse): void {
    this.editingId.set(comment.id);
    this.editText.set(comment.content);
    this.replyToId.set(null);
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.editText.set('');
  }

  submitEdit(commentId: string, parentId: string | null): void {
    const content = this.editText().trim();
    if (!content || this.submitting()) return;

    this.submitting.set(true);
    this.commentService.update(commentId, { content }).subscribe({
      next: (updated) => {
        this.updateInTree(commentId, parentId, updated);
        this.cancelEdit();
        this.submitting.set(false);
      },
      error: () => this.submitting.set(false),
    });
  }

  // ── Delete ─────────────────────────────────────────────────────

  deleteComment(commentId: string, parentId: string | null): void {
    this.commentService.delete(commentId).subscribe({
      next: () => {
        if (parentId) {
          // Удаляем из replies
          this.paged.update((p) =>
            p
              ? {
                  ...p,
                  items: p.items.map((c) =>
                    c.id === parentId
                      ? {
                          ...c,
                          replies: (c.replies ?? []).filter((r) => r.id !== commentId),
                          replyCount: c.replyCount - 1,
                        }
                      : c,
                  ),
                }
              : p,
          );
        } else {
          // Soft delete — контент скроется, ответы останутся
          this.paged.update((p) =>
            p
              ? {
                  ...p,
                  items: p.items.map((c) =>
                    c.id === commentId
                      ? { ...c, isDeleted: true, content: '[Комментарий удалён]' }
                      : c,
                  ),
                }
              : p,
          );
        }
      },
    });
  }

  // ── React ──────────────────────────────────────────────────────

  react(commentId: string, parentId: string | null, type: string): void {
    if (!this.isAuthenticated()) return;

    this.commentService.react(commentId, type).subscribe({
      next: () => {
        const update = (c: CommentResponse): CommentResponse => {
          if (c.id !== commentId) return c;
          const reactions = { ...c.reactions };
          const prev = c.myReaction;
          if (prev === type) {
            reactions[type] = Math.max(0, (reactions[type] ?? 1) - 1);
            return { ...c, reactions, myReaction: null };
          }
          if (prev) reactions[prev] = Math.max(0, (reactions[prev] ?? 1) - 1);
          reactions[type] = (reactions[type] ?? 0) + 1;
          return { ...c, reactions, myReaction: type };
        };

        this.paged.update((p) =>
          p
            ? {
                ...p,
                items: p.items.map((c) => ({
                  ...update(c),
                  replies: (c.replies ?? []).map(update),
                })),
              }
            : p,
        );
      },
    });
  }

  // ── Helpers ────────────────────────────────────────────────────

  canModify(comment: CommentResponse): boolean {
    const user = this.auth.user();
    if (!user) return false;
    const roles = this.auth.profile()?.roles ?? [];
    return comment.author.id === user.id || roles.includes('Admin') || roles.includes('Manager');
  }

  reactionTotal(comment: CommentResponse): number {
    return Object.values(comment.reactions).reduce((a, b) => a + b, 0);
  }

  formatDate(dateStr: string): string {
    const d = new Date(dateStr);
    const now = new Date();
    const diff = now.getTime() - d.getTime();
    const mins = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);

    if (mins < 1) return 'только что';
    if (mins < 60) return `${mins} мин. назад`;
    if (hours < 24) return `${hours} ч. назад`;
    if (days < 7) return `${days} дн. назад`;
    return new Intl.DateTimeFormat('ru-RU', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    }).format(d);
  }

  getAvatarUrl(url: string | null): string | null {
    if (!url) return null;
    if (url.startsWith('http')) return url;
    return this.auth.FILES_BASE + url;
  }

  private updateInTree(commentId: string, parentId: string | null, updated: CommentResponse): void {
    this.paged.update((p) => {
      if (!p) return p;
      if (parentId) {
        return {
          ...p,
          items: p.items.map((c) =>
            c.id === parentId
              ? { ...c, replies: (c.replies ?? []).map((r) => (r.id === commentId ? updated : r)) }
              : c,
          ),
        };
      }
      return { ...p, items: p.items.map((c) => (c.id === commentId ? updated : c)) };
    });
  }

  protected readonly faSpinner = faSpinner;
  protected readonly faReply = faReply;
}
