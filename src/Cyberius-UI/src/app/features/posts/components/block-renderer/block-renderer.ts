import { Component, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../../core/services/auth.service';
import { ContentBlockDto } from '../../../../core/models/post.model';
import { HighlightDirective } from '../../../../shared/directives/highlight.directive';

@Component({
  selector: 'app-block-renderer',
  imports: [CommonModule, HighlightDirective],
  templateUrl: './block-renderer.html',
  styleUrl: './block-renderer.css',
})
export class BlockRenderer {
  blocks = input.required<ContentBlockDto[]>();
  private auth = inject(AuthService);

  copiedId = signal<string | null>(null);

  getImageUrl(path: string | null): string | null {
    if (!path) return null;
    if (path.startsWith('http')) return path;
    return this.auth.FILES_BASE + path;
  }

  get sortedBlocks(): ContentBlockDto[] {
    return [...this.blocks()].sort((a, b) => a.order - b.order);
  }

  copyCode(blockId: string, content: string): void {
    navigator.clipboard.writeText(content).then(() => {
      this.copiedId.set(blockId);
      setTimeout(() => this.copiedId.set(null), 2000);
    });
  }
}
