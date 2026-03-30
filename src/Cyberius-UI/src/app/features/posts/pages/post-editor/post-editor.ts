import { Component, inject, OnInit, signal } from '@angular/core';
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

interface EditorBlock extends CreateContentBlockRequest {
  id: string; // локальный id для track-by
}

type SaveStatus = 'idle' | 'saving' | 'publishing' | 'error';

const BLOCK_TYPES: { type: BlockType; label: string; icon: string }[] = [
  { type: 'Paragraph', label: 'Параграф', icon: '¶' },
  { type: 'Heading2', label: 'Заголовок', icon: 'H2' },
  { type: 'Heading3', label: 'Подзаголовок', icon: 'H3' },
  { type: 'Code', label: 'Код', icon: '</>' },
  { type: 'Image', label: 'Изображение', icon: '🖼' },
  { type: 'Quote', label: 'Цитата', icon: '"' },
  { type: 'Callout', label: 'Callout', icon: 'ℹ' },
  { type: 'Divider', label: 'Разделитель', icon: '—' },
];

@Component({
  selector: 'app-post-editor',
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './post-editor.html',
  styleUrl: './post-editor.css',
})
export class PostEditor implements OnInit {
  private postsService = inject(PostsService);
  private categoryService = inject(CategoryService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);
  readonly auth = inject(AuthService);

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
        },
        error: () => {
          this.router.navigate(['/posts']);
        },
      });
    } else {
      // Start with one empty paragraph
      this.addBlock('Paragraph');
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
      content: type === 'Divider' ? null : '',
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

  // ── Cover image upload ─────────────────────────────────────────

  onCoverFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.uploadingCover.set(true);
    const fd = new FormData();
    fd.append('file', file);

    this.http.post<{ url: string }>(`${this.auth.API}/files/covers`, fd).subscribe({
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

    this.http.post<{ url: string }>(`${this.auth.API}/files/blocks`, fd).subscribe({
      next: (res) => this.updateBlock(index, { imageUrl: res.url }),
    });
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
}
