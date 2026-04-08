import { Component, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../../core/services/auth.service';
import { ContentBlockDto } from '../../../../core/models/post.model';
import { HighlightDirective } from '../../../../shared/directives/highlight.directive';
import { SafeUrlPipe } from '../../../../core/pipes/safe-url-pipe';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faCheck,
  faCopy,
  faExclamationTriangle,
  faFireBurner,
  faInfoCircle,
  faLightbulb,
  faMagnifyingGlassPlus,
  faXmark,
} from '@fortawesome/free-solid-svg-icons';

interface TableData {
  headers: string[];
  rows: string[][];
}

@Component({
  selector: 'app-block-renderer',
  imports: [CommonModule, HighlightDirective, SafeUrlPipe, FaIconComponent],
  templateUrl: './block-renderer.html',
  styleUrl: './block-renderer.css',
})
export class BlockRenderer {
  blocks = input.required<ContentBlockDto[]>();
  private auth = inject(AuthService);

  copiedId = signal<string | null>(null);
  lightboxUrl = signal<string | null>(null);
  lightboxCaption = signal<string | null>(null);

  openLightbox(url: string | null, caption: string | null): void {
    if (!url) return;
    this.lightboxUrl.set(url);
    this.lightboxCaption.set(caption);
    document.body.style.overflow = 'hidden';
  }

  closeLightbox(): void {
    this.lightboxUrl.set(null);
    this.lightboxCaption.set(null);
    document.body.style.overflow = '';
  }

  getImageUrl(path: string | null): string | null {
    if (!path) return null;
    if (path.startsWith('http')) return path;
    return this.auth.FILES_BASE + path;
  }

  get sortedBlocks(): ContentBlockDto[] {
    return [...this.blocks()].sort((a, b) => a.order - b.order);
  }

  // Параграф: переводим \n → <br> для отображения пустых строк
  formatParagraph(content: string | null): string {
    if (!content) return '';
    return content
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/\n/g, '<br>');
  }

  // Таблица хранится как JSON: { headers: [...], rows: [[...], [...]] }
  parseTable(content: string | null): TableData | null {
    if (!content) return null;
    try {
      return JSON.parse(content) as TableData;
    } catch {
      return null;
    }
  }

  slugify(text: string | null): string {
    if (!text) return '';
    return text
      .toLowerCase()
      .replace(/\s+/g, '-')
      .replace(/[^\w-]/g, '')
      .replace(/-+/g, '-');
  }

  copyCode(blockId: string, content: string): void {
    navigator.clipboard.writeText(content).then(() => {
      this.copiedId.set(blockId);
      setTimeout(() => this.copiedId.set(null), 2000);
    });
  }

  protected readonly faCheck = faCheck;
  protected readonly faCopy = faCopy;
  protected readonly faExclamationTriangle = faExclamationTriangle;
  protected readonly faFireBurner = faFireBurner;
  protected readonly faLightbulb = faLightbulb;
  protected readonly faInfoCircle = faInfoCircle;
  protected readonly faMagnifyingGlassPlus = faMagnifyingGlassPlus;
  protected readonly faXmark = faXmark;
}
