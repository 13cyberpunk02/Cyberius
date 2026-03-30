import { Component, inject, OnInit, signal } from '@angular/core';
import {
  ConfirmDialog,
  ConfirmDialogConfig,
} from '../../../shared/components/confirm-dialog/confirm-dialog';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CategoryService } from '../../../core/services/category.service';
import { CategoryResponse, CreateCategoryRequest } from '../../../core/models/category.model';

interface CategoryForm {
  name: string;
  slug: string;
  color: string;
  iconUrl: string;
}

const EMPTY_FORM: CategoryForm = { name: '', slug: '', color: '#0ca2e7', iconUrl: '' };

// Палитра популярных цветов для быстрого выбора
const COLOR_PALETTE = [
  '#0ca2e7',
  '#818cf8',
  '#f43f5e',
  '#f59e0b',
  '#10b981',
  '#06b6d4',
  '#8b5cf6',
  '#ec4899',
  '#f97316',
  '#14b8a6',
];


@Component({
  selector: 'app-category',
  imports: [CommonModule, FormsModule, RouterModule, ConfirmDialog],
  templateUrl: './category.html',
  styleUrl: './category.css',
})
export class Category implements OnInit {
  private categoryService = inject(CategoryService);

  categories = signal<CategoryResponse[]>([]);
  loading = signal(true);
  saving = signal(false);
  errorMsg = signal('');

  // Form state
  showForm = signal(false);
  editingId = signal<string | null>(null);
  form = signal<CategoryForm>({ ...EMPTY_FORM });

  // Delete state
  showDelete = signal(false);
  deletingId = signal<string | null>(null);
  deleting = signal(false);

  readonly palette = COLOR_PALETTE;

  readonly deleteConfig: ConfirmDialogConfig = {
    title: 'Удалить категорию?',
    message: 'Категорию нельзя удалить если в ней есть статьи.',
    confirmLabel: 'Удалить',
    cancelLabel: 'Отмена',
    danger: true,
  };

  ngOnInit(): void {
    this.loadCategories();
  }

  private loadCategories(): void {
    this.loading.set(true);
    this.categoryService.getAll().subscribe({
      next: (cats) => {
        this.categories.set(cats);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.form.set({ ...EMPTY_FORM });
    this.errorMsg.set('');
    this.showForm.set(true);
  }

  openEdit(cat: CategoryResponse): void {
    this.editingId.set(cat.id);
    this.form.set({
      name: cat.name,
      slug: cat.slug,
      color: cat.color ?? '#0ca2e7',
      iconUrl: cat.iconUrl ?? '',
    });
    this.errorMsg.set('');
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingId.set(null);
  }

  // Автогенерация slug из name
  onNameChange(name: string): void {
    this.form.update((f) => ({
      ...f,
      name,
      // Генерируем slug только если не редактируем
      ...(!this.editingId() ? { slug: this.toSlug(name) } : {}),
    }));
  }

  private toSlug(name: string): string {
    const map: Record<string, string> = {
      а: 'a',
      б: 'b',
      в: 'v',
      г: 'g',
      д: 'd',
      е: 'e',
      ё: 'yo',
      ж: 'zh',
      з: 'z',
      и: 'i',
      й: 'y',
      к: 'k',
      л: 'l',
      м: 'm',
      н: 'n',
      о: 'o',
      п: 'p',
      р: 'r',
      с: 's',
      т: 't',
      у: 'u',
      ф: 'f',
      х: 'kh',
      ц: 'ts',
      ч: 'ch',
      ш: 'sh',
      щ: 'shch',
      ъ: '',
      ы: 'y',
      ь: '',
      э: 'e',
      ю: 'yu',
      я: 'ya',
    };
    return name
      .toLowerCase()
      .split('')
      .map((c) => map[c] ?? (/[a-z0-9]/.test(c) ? c : '-'))
      .join('')
      .replace(/-+/g, '-')
      .replace(/^-|-$/g, '');
  }

  save(): void {
    const f = this.form();
    if (!f.name.trim()) {
      this.errorMsg.set('Введите название');
      return;
    }
    if (!f.slug.trim()) {
      this.errorMsg.set('Введите slug');
      return;
    }
    if (!/^#[0-9a-fA-F]{6}$/.test(f.color)) {
      this.errorMsg.set('Некорректный цвет (формат #RRGGBB)');
      return;
    }

    const req: CreateCategoryRequest = {
      name: f.name.trim(),
      slug: f.slug.trim().toLowerCase(),
      color: f.color,
      iconUrl: f.iconUrl.trim() || null,
    };

    this.saving.set(true);
    this.errorMsg.set('');

    const id = this.editingId();
    const op$ = id ? this.categoryService.update(id, req) : this.categoryService.create(req);

    op$.subscribe({
      next: () => {
        this.saving.set(false);
        this.closeForm();
        this.loadCategories();
      },
      error: (err) => {
        this.saving.set(false);
        this.errorMsg.set(err?.error?.message ?? 'Ошибка при сохранении');
      },
    });
  }

  askDelete(id: string): void {
    this.deletingId.set(id);
    this.showDelete.set(true);
  }

  confirmDelete(): void {
    const id = this.deletingId();
    if (!id) return;
    this.deleting.set(true);

    this.categoryService.delete(id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.showDelete.set(false);
        this.deletingId.set(null);
        this.loadCategories();
      },
      error: (err) => {
        this.deleting.set(false);
        this.showDelete.set(false);
        this.errorMsg.set(err?.error?.message ?? 'Нельзя удалить — в категории есть статьи');
      },
    });
  }
}
