import {
  Component,
  computed,
  HostListener,
  inject,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { BlockRenderer } from '../../components/block-renderer/block-renderer';
import {
  ConfirmDialog,
  ConfirmDialogConfig,
} from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { PostsService } from '../../../../core/services/posts.service';
import { AuthService } from '../../../../core/services/auth.service';
import { PostDetailModel, PostSummary, ReactionType } from '../../../../core/models/post.model';
import { Comments } from '../../components/comments/comments';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faAngleLeft,
  faAngleRight,
  faAnglesUp,
  faBookmark,
  faCommentDots,
  faCopy,
  faEye,
  faPenToSquare,
  faTrashCan,
} from '@fortawesome/free-solid-svg-icons';
import { SeoService } from '../../../../core/services/seo.service';
import { PostCard } from '../../components/post-card/post-card';
import { ToastService } from '../../../../core/services/toast.service';
import { BookmarkService } from '../../../../core/services/bookmarks.service';
import { faTelegram, faXTwitter } from '@fortawesome/free-brands-svg-icons';

const REACTION_EMOJI: Record<string, string> = {
  Like: '👍',
  Heart: '❤️',
  Fire: '🔥',
  Clap: '👏',
  Thinking: '🤔',
};

export interface TocItem {
  id: string;
  text: string;
  level: 2 | 3;
}

@Component({
  selector: 'app-post-detail',
  imports: [
    CommonModule,
    RouterModule,
    BlockRenderer,
    ConfirmDialog,
    Comments,
    PostCard,
    FaIconComponent,
  ],
  templateUrl: './post-detail.html',
  styleUrl: './post-detail.css',
})
export class PostDetail implements OnInit, OnDestroy {
  private postsService = inject(PostsService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private seo = inject(SeoService);
  private toast = inject(ToastService);
  readonly auth = inject(AuthService);
  readonly bookmarks = inject(BookmarkService);

  protected readonly faAngleLeft = faAngleLeft;
  protected readonly faPenToSquare = faPenToSquare;
  protected readonly faTrashCan = faTrashCan;
  protected readonly faEye = faEye;
  protected readonly faCommentDots = faCommentDots;

  post = signal<PostDetailModel | null>(null);
  loading = signal(true);
  notFound = signal(false);
  showDelete = signal(false);
  deleting = signal(false);
  related = signal<PostSummary[]>([]);
  toc = signal<TocItem[]>([]);
  readingProgress = signal(0);
  neighbors = signal<{
    prev: { id: string; title: string; slug: string } | null;
    next: { id: string; title: string; slug: string } | null;
  }>({ prev: null, next: null });

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

  @HostListener('window:scroll')
  onScroll(): void {
    const el = document.documentElement;
    const top = el.scrollTop || document.body.scrollTop;
    const height = el.scrollHeight - el.clientHeight;
    this.readingProgress.set(height > 0 ? Math.round((top / height) * 100) : 0);
  }

  ngOnInit(): void {
    // Подписываемся на изменения параметров — важно для навигации между статьями
    this.route.paramMap.subscribe((params) => {
      const slug = params.get('slug')!;

      // Сбрасываем состояние при смене статьи
      this.post.set(null);
      this.loading.set(true);
      this.notFound.set(false);
      this.related.set([]);
      this.toc.set([]);

      this.postsService.getBySlug(slug).subscribe({
        next: (post) => {
          this.post.set(post);
          this.loading.set(false);
          this.trackView(post.id);
          this.buildToc(post);
          this.loadRelated(post.id);

          this.seo.setPage({
            title: post.title,
            description: post.excerpt ?? undefined,
            image: post.coverImageUrl
              ? (this.postsService.getImageUrl(post.coverImageUrl) ?? undefined)
              : undefined,
            url: `http://localhost:4200/posts/${post.slug}`,
            type: 'article',
            publishedAt: post.publishedAt ?? undefined,
            author: post.author.fullName,
          });
        },
        error: (err) => {
          this.loading.set(false);
          if (err.status === 404) this.notFound.set(true);
        },
      });
    });
  }

  private trackView(postId: string): void {
    // Небольшая задержка — считаем просмотр только если пользователь реально открыл статью
    setTimeout(() => {
      this.postsService.trackView(postId).subscribe({ error: () => {} });
    }, 3000);
  }

  // ── Table of Contents ──────────────────────────────────────────
  private buildToc(post: PostDetailModel): void {
    const items: TocItem[] = post.blocks
      .filter((b) => {
        const t = String(b.type);
        return t === 'Heading2' || t === '2' || t === 'Heading3' || t === '3';
      })
      .sort((a, b) => a.order - b.order)
      .map((b, i) => {
        const t = String(b.type);
        const text = b.content ?? '';
        return {
          id: this.slugify(text) || `heading-${i}`,
          text,
          level: t === 'Heading2' || t === '2' ? (2 as const) : (3 as const),
        };
      })
      .filter((item) => item.text.length > 0); // пропускаем пустые заголовки
    this.toc.set(items);
  }

  scrollToHeading(id: string): void {
    const el = document.getElementById(id);
    if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  private slugify(text: string): string {
    return text
      .toLowerCase()
      .replace(/\s+/g, '-')
      .replace(/[^\w-]/g, '')
      .replace(/-+/g, '-');
  }

  // ── Copy link ──────────────────────────────────────────────────
  copyLink(): void {
    navigator.clipboard.writeText(window.location.href).then(() => {
      this.toast.success('Ссылка скопирована');
    });
  }

  // ── Related posts ──────────────────────────────────────────────
  private loadRelated(postId: string): void {
    this.postsService.getRelated(postId, 3).subscribe({
      next: (posts) => this.related.set(posts),
      error: () => {},
    });
  }

  private loadNeighbors(postId: string): void {
    this.postsService.getNeighbors(postId).subscribe({
      next: (data) => this.neighbors.set(data),
      error: () => {},
    });
  }

  toggleBookmark(): void {
    const p = this.post();
    if (!p) return;
    // Конвертируем PostDetail в PostSummary для хранения
    const summary = {
      id: p.id,
      title: p.title,
      slug: p.slug,
      excerpt: p.excerpt,
      coverImageUrl: p.coverImageUrl,
      readTimeMinutes: p.readTimeMinutes,
      status: p.status,
      publishedAt: p.publishedAt,
      createdAt: p.createdAt,
      author: p.author,
      category: p.category,
      tags: p.tags,
      viewCount: p.viewCount,
      commentCount: p.commentCount,
      reactions: p.reactions,
    } as any;
    this.bookmarks.toggle(summary);
    this.toast.success(
      this.bookmarks.isBookmarked(p.id) ? 'Добавлено в сохранённые' : 'Удалено из сохранённых',
    );
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

  ngOnDestroy(): void {
    // Сбрасываем прогресс при уходе со страницы
    this.readingProgress.set(0);
  }

  // ── Share ──────────────────────────────────────────────────────
  shareTo(platform: 'telegram' | 'X'): void {
    const url = encodeURIComponent(window.location.href);
    const title = encodeURIComponent(this.post()?.title ?? '');
    const links: Record<string, string> = {
      telegram: `https://t.me/share/url?url=${url}&text=${title}`,
      X: `https://x.com/intent/tweet?url=${url}&text=${title}`,
    };
    window.open(links[platform], '_blank', 'noopener,noreferrer,width=600,height=400');
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

  protected readonly faCopy = faCopy;
  protected readonly faBookmark = faBookmark;
  protected readonly faTelegram = faTelegram;
  protected readonly faXTwitter = faXTwitter;
  protected readonly faAngleRight = faAngleRight;
}
