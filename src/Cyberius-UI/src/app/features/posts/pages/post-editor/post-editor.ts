import { Component, computed, inject, OnDestroy, OnInit, signal } from '@angular/core';
import {
  BlockType,
  CreateContentBlockRequest,
  CreatePostRequest,
  PostDetailModel,
  UpdatePostRequest,
} from '../../../../core/models/post.model';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PostsService } from '../../../../core/services/posts.service';
import { CategoryService } from '../../../../core/services/category.service';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../../core/services/auth.service';
import { CategoryResponse } from '../../../../core/models/category.model';
import { SafeUrlPipe } from '../../../../core/pipes/safe-url-pipe';
import { FaIconComponent, IconDefinition } from '@fortawesome/angular-fontawesome';
import {
  faArrowDown,
  faArrowLeft,
  faArrowUp,
  faClipboardList,
  faCode,
  faDivide,
  faExclamationTriangle,
  faFeatherPointed,
  faFireBurner,
  faFloppyDisk,
  faHeading,
  faImage,
  faInfoCircle,
  faLightbulb,
  faMinus,
  faPaperclip,
  faParagraph,
  faPlay,
  faPlus,
  faQuoteLeft,
  faRocket,
  faSpinner,
  faTableList,
  faXmark,
} from '@fortawesome/free-solid-svg-icons';
import { ToastService } from '../../../../core/services/toast.service';

interface EditorBlock extends CreateContentBlockRequest {
  id: string; // локальный id для track-by
}

type SaveStatus = 'idle' | 'saving' | 'publishing' | 'error';

const BLOCK_TYPES: { type: BlockType; label: string; icon: IconDefinition }[] = [
  { type: 'Paragraph', label: 'Параграф', icon: faParagraph },
  { type: 'Heading2', label: 'Заголовок', icon: faHeading },
  { type: 'Heading3', label: 'Подзаголовок', icon: faClipboardList },
  { type: 'Code', label: 'Код', icon: faCode },
  { type: 'Image', label: 'Изображение', icon: faImage },
  { type: 'VideoEmbed', label: 'Видео', icon: faPlay },
  { type: 'Table', label: 'Таблица', icon: faTableList },
  { type: 'Quote', label: 'Цитата', icon: faQuoteLeft },
  { type: 'Callout', label: 'Callout', icon: faFeatherPointed },
  { type: 'Divider', label: 'Разделитель', icon: faDivide },
];

@Component({
  selector: 'app-post-editor',
  imports: [CommonModule, RouterModule, FormsModule, SafeUrlPipe, FaIconComponent],
  templateUrl: './post-editor.html',
  styleUrl: './post-editor.css',
})
export class PostEditor implements OnInit, OnDestroy {
  private postsService = inject(PostsService);
  private categoryService = inject(CategoryService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);
  readonly auth = inject(AuthService);
  private toast = inject(ToastService);

  protected readonly faInfoCircle = faInfoCircle;
  protected readonly faSpinner = faSpinner;
  protected readonly faPaperclip = faPaperclip;
  protected readonly faFloppyDisk = faFloppyDisk;
  protected readonly faRocket = faRocket;
  protected readonly faLightbulb = faLightbulb;
  protected readonly faExclamationTriangle = faExclamationTriangle;
  protected readonly faFireBurner = faFireBurner;
  protected readonly faXmark = faXmark;
  protected readonly faArrowDown = faArrowDown;
  protected readonly faArrowUp = faArrowUp;

  // Mode
  isEdit = signal(false);
  editSlug = signal<string | null>(null);
  editPostId = signal<string | null>(null);
  loading = signal(false);
  saveStatus = signal<SaveStatus>('idle');
  errorMsg = signal('');

  // Form
  title = signal('');
  excerpt = signal('');
  categoryId = signal('');
  tags = signal(''); // comma-separated
  coverImageUrl = signal('');
  blocks = signal<EditorBlock[]>([]);
  categories = signal<CategoryResponse[]>([]);

  readonly blockTypes = BLOCK_TYPES;
  readonly liveReadTime = computed(() => {
    const words = this.blocks()
      .filter(
        (b) =>
          b.content &&
          ['Paragraph', 'Heading1', 'Heading2', 'Heading3', 'Quote', 'Callout', 'Code'].includes(
            b.type,
          ),
      )
      .reduce((sum, b) => sum + b.content!.split(/\s+/).filter(Boolean).length, 0);
    return Math.max(1, Math.ceil(words / 200));
  });

  uploadingCover = signal(false);

  ngOnInit(): void {
    this.categoryService.getAll().subscribe({ next: (c) => this.categories.set(c) });

    const slug = this.route.snapshot.paramMap.get('slug');
    if (slug) {
      this.isEdit.set(true);
      this.editSlug.set(slug);
      this.loading.set(true);
      this.postsService.getBySlug(slug).subscribe({
        next: (post) => {
          this.fillFromPost(post);
          this.loading.set(false);
          this.startAutosave();
        },
        error: () => {
          this.router.navigate(['/posts']);
        },
      });
    } else {
      this.addBlock('Paragraph');
      this.autosaveKey = 'draft_new';
      this.loadDraft();
      this.startAutosave();
    }
  }

  private fillFromPost(post: PostDetailModel): void {
    this.editPostId.set(post.id);
    this.title.set(post.title);
    this.excerpt.set(post.excerpt ?? '');
    this.categoryId.set(post.category.id);
    this.coverImageUrl.set(post.coverImageUrl ?? '');
    this.tags.set(post.tags.join(', '));
    this.blocks.set(
      post.blocks
        .sort((a, b) => a.order - b.order)
        .map((b) => ({
          id: crypto.randomUUID(),
          type: b.type,
          order: b.order,
          content: b.content,
          language: b.language,
          imageUrl: b.imageUrl,
          imageCaption: b.imageCaption,
          calloutType: b.calloutType,
        })),
    );
  }

  // ── Blocks ─────────────────────────────────────────────────────

  addBlock(type: BlockType, afterIndex?: number): void {
    const block: EditorBlock = {
      id: crypto.randomUUID(),
      type,
      order: 0,
      content:
        type === 'Divider'
          ? null
          : type === 'Table'
            ? JSON.stringify({ headers: ['Столбец 1', 'Столбец 2'], rows: [['', '']] })
            : '',
      language: type === 'Code' ? 'csharp' : null,
      imageUrl: null,
      imageCaption: null,
      calloutType: type === 'Callout' ? 'info' : null,
    };

    this.blocks.update((list) => {
      const idx = afterIndex !== undefined ? afterIndex + 1 : list.length;
      const newList = [...list];
      newList.splice(idx, 0, block);
      return newList.map((b, i) => ({ ...b, order: i }));
    });
  }

  removeBlock(index: number): void {
    this.blocks.update((list) =>
      list.filter((_, i) => i !== index).map((b, i) => ({ ...b, order: i })),
    );
  }

  moveBlock(index: number, dir: -1 | 1): void {
    const target = index + dir;
    if (target < 0 || target >= this.blocks().length) return;
    this.blocks.update((list) => {
      const arr = [...list];
      [arr[index], arr[target]] = [arr[target], arr[index]];
      return arr.map((b, i) => ({ ...b, order: i }));
    });
  }

  updateBlock(index: number, patch: Partial<EditorBlock>): void {
    this.blocks.update((list) => list.map((b, i) => (i === index ? { ...b, ...patch } : b)));
  }

  // ── Autosave ───────────────────────────────────────────────────
  private autosaveKey = '';
  private autosaveTimer: ReturnType<typeof setInterval> | null = null;

  private startAutosave(): void {
    this.autosaveKey = `draft_${this.editPostId() ?? 'new'}`;
    this.autosaveTimer = setInterval(() => this.saveDraft(), 30_000);
  }

  private saveDraft(): void {
    if (!this.title().trim() && this.blocks().length <= 1) return;
    const draft = {
      title: this.title(),
      excerpt: this.excerpt(),
      categoryId: this.categoryId(),
      tags: this.tags(),
      coverImageUrl: this.coverImageUrl(),
      blocks: this.blocks(),
      savedAt: new Date().toISOString(),
    };
    localStorage.setItem(this.autosaveKey, JSON.stringify(draft));
  }

  private loadDraft(): void {
    const raw = localStorage.getItem(this.autosaveKey);
    if (!raw) return;
    try {
      const draft = JSON.parse(raw);
      const savedAt = new Date(draft.savedAt);
      const mins = Math.floor((Date.now() - savedAt.getTime()) / 60000);
      // Предлагаем восстановить если черновик свежее 24 часов
      if (mins < 1440) {
        this.toast.info(
          `Найден несохранённый черновик (${mins} мин. назад). Восстановлен автоматически.`,
          5000,
        );
        this.title.set(draft.title ?? '');
        this.excerpt.set(draft.excerpt ?? '');
        this.categoryId.set(draft.categoryId ?? '');
        this.tags.set(draft.tags ?? '');
        this.coverImageUrl.set(draft.coverImageUrl ?? '');
        if (draft.blocks?.length) this.blocks.set(draft.blocks);
      }
    } catch {
      /* ignore corrupt data */
    }
  }

  clearDraft(): void {
    if (this.autosaveKey) localStorage.removeItem(this.autosaveKey);
  }

  ngOnDestroy(): void {
    if (this.autosaveTimer) clearInterval(this.autosaveTimer);
  }

  // ── Cover image upload ─────────────────────────────────────────

  onCoverFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.uploadingCover.set(true);
    const fd = new FormData();
    fd.append('file', file);

    this.http
      .post<{ objectName: string; url: string }>(`${this.auth.FILES_BASE}covers`, fd)
      .subscribe({
        next: (res) => {
          this.coverImageUrl.set(res.url);
          this.uploadingCover.set(false);
        },
        error: () => this.uploadingCover.set(false),
      });
  }

  // ── Block image upload ─────────────────────────────────────────

  onBlockImageSelected(event: Event, index: number): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const fd = new FormData();
    fd.append('file', file);

    this.http
      .post<{ objectName: string; url: string }>(`${this.auth.FILES_BASE}blocks`, fd)
      .subscribe({
        next: (res) => this.updateBlock(index, { imageUrl: res.url }),
      });
  }

  // Извлекает embed URL из ссылки YouTube / Vimeo / Rutube
  static extractEmbedUrl(url: string): string | null {
    try {
      // YouTube: youtu.be/ID или youtube.com/watch?v=ID или youtube.com/shorts/ID
      const ytShort = url.match(/youtu\.be\/([a-zA-Z0-9_-]{11})/);
      const ytWatch = url.match(/youtube\.com\/watch\?v=([a-zA-Z0-9_-]{11})/);
      const ytShorts = url.match(/youtube\.com\/shorts\/([a-zA-Z0-9_-]{11})/);
      const ytId = (ytShort ?? ytWatch ?? ytShorts)?.[1];
      if (ytId) return `https://www.youtube.com/embed/${ytId}`;

      // Vimeo: vimeo.com/ID
      const vimeo = url.match(/vimeo\.com\/(\d+)/);
      if (vimeo) return `https://player.vimeo.com/video/${vimeo[1]}`;

      // Rutube: rutube.ru/video/ID
      const rutube = url.match(/rutube\.ru\/video\/([a-zA-Z0-9]+)/);
      if (rutube) return `https://rutube.ru/play/embed/${rutube[1]}`;

      // Уже embed URL — возвращаем как есть
      if (url.includes('/embed/') || url.includes('player.')) return url;
    } catch {
      /* ignore */
    }
    return null;
  }

  onVideoUrlChange(index: number, url: string): void {
    const embedUrl = PostEditor.extractEmbedUrl(url) ?? url;
    this.updateBlock(index, { content: url, imageUrl: embedUrl });
  }

  // ── Table helpers ──────────────────────────────────────────────

  parseTableContent(content: string | null): { headers: string[]; rows: string[][] } {
    if (!content) return { headers: ['Столбец 1', 'Столбец 2'], rows: [['', '']] };
    try {
      return JSON.parse(content);
    } catch {
      return { headers: ['Столбец 1', 'Столбец 2'], rows: [['', '']] };
    }
  }

  saveTable(index: number, data: { headers: string[]; rows: string[][] }): void {
    this.updateBlock(index, { content: JSON.stringify(data) });
  }

  tableAddColumn(index: number): void {
    const t = this.parseTableContent(this.blocks()[index].content);
    t.headers.push(`Столбец ${t.headers.length + 1}`);
    t.rows = t.rows.map((r) => [...r, '']);
    this.saveTable(index, t);
  }

  tableRemoveColumn(index: number): void {
    const t = this.parseTableContent(this.blocks()[index].content);
    if (t.headers.length <= 1) return;
    t.headers.pop();
    t.rows = t.rows.map((r) => r.slice(0, -1));
    this.saveTable(index, t);
  }

  tableAddRow(index: number): void {
    const t = this.parseTableContent(this.blocks()[index].content);
    t.rows.push(new Array(t.headers.length).fill(''));
    this.saveTable(index, t);
  }

  tableRemoveRow(blockIndex: number, rowIndex: number): void {
    const t = this.parseTableContent(this.blocks()[blockIndex].content);
    if (t.rows.length <= 1) return;
    t.rows.splice(rowIndex, 1);
    this.saveTable(blockIndex, t);
  }

  tableUpdateHeader(blockIndex: number, colIndex: number, value: string): void {
    const t = this.parseTableContent(this.blocks()[blockIndex].content);
    t.headers[colIndex] = value;
    this.saveTable(blockIndex, t);
  }

  tableUpdateCell(blockIndex: number, rowIndex: number, colIndex: number, value: string): void {
    const t = this.parseTableContent(this.blocks()[blockIndex].content);
    t.rows[rowIndex][colIndex] = value;
    this.saveTable(blockIndex, t);
  }

  // ── Save / Publish ─────────────────────────────────────────────

  private buildRequest(): CreatePostRequest {
    return {
      title: this.title().trim(),
      excerpt: this.excerpt().trim() || null,
      coverImageUrl: this.coverImageUrl().trim() || null,
      categoryId: this.categoryId(),
      tags: this.tags()
        .split(',')
        .map((t) => t.trim())
        .filter(Boolean),
      blocks: this.blocks().map(({ id, ...b }) => b),
    };
  }

  save(andPublish = false): void {
    if (!this.title().trim()) {
      this.errorMsg.set('Введите заголовок');
      return;
    }
    if (!this.categoryId()) {
      this.errorMsg.set('Выберите категорию');
      return;
    }

    this.saveStatus.set(andPublish ? 'publishing' : 'saving');
    this.errorMsg.set('');

    const req = this.buildRequest();

    const op$ =
      this.isEdit() && this.editPostId()
        ? this.postsService.update(this.editPostId()!, req as UpdatePostRequest)
        : this.postsService.create(req);

    op$.subscribe({
      next: (post) => {
        if (andPublish) {
          this.postsService.publish(post.id).subscribe({
            next: () => this.router.navigate(['/posts', post.slug]),
            error: () => this.router.navigate(['/posts', post.slug]),
          });
        } else {
          this.saveStatus.set('idle');
          if (!this.isEdit()) {
            this.router.navigate(['/posts', post.slug, 'edit']);
          }
        }
      },
      error: (err) => {
        this.saveStatus.set('error');
        this.errorMsg.set(err?.error?.message ?? 'Не удалось сохранить статью');
      },
    });
  }

  protected readonly faArrowLeft = faArrowLeft;
  protected readonly faPlus = faPlus;
  protected readonly faMinus = faMinus;
}
