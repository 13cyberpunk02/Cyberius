import { Component, inject, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../../core/services/auth.service';
import { ContentBlockDto } from '../../../../core/models/post.model';

@Component({
  selector: 'app-block-renderer',
  imports: [CommonModule],
  templateUrl: './block-renderer.html',
  styleUrl: './block-renderer.css',
})
export class BlockRenderer {
  blocks = input.required<ContentBlockDto[]>();
  private auth = inject(AuthService);

  getImageUrl(path: string | null): string | null {
    if (!path) return null;
    if (path.startsWith('http')) return path;
    return this.auth.FILES_BASE + path;
  }

  get sortedBlocks(): ContentBlockDto[] {
    return [...this.blocks()].sort((a, b) => a.order - b.order);
  }
}
