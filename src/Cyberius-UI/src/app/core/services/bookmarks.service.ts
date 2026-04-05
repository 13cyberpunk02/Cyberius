import { Injectable, signal, computed } from '@angular/core';
import { PostSummary } from '../models/post.model';

const STORAGE_KEY = 'blog_bookmarks';

@Injectable({ providedIn: 'root' })
export class BookmarkService {
  private ids = signal<Set<string>>(this.loadIds());

  readonly count = computed(() => this.ids().size);

  isBookmarked(postId: string): boolean {
    return this.ids().has(postId);
  }

  toggle(post: PostSummary): void {
    const current = new Set(this.ids());
    if (current.has(post.id)) {
      current.delete(post.id);
      this.removeSaved(post.id);
    } else {
      current.add(post.id);
      this.addSaved(post);
    }
    this.ids.set(current);
    localStorage.setItem(STORAGE_KEY + '_ids', JSON.stringify([...current]));
  }

  getSaved(): PostSummary[] {
    try {
      const raw = localStorage.getItem(STORAGE_KEY + '_posts');
      return raw ? JSON.parse(raw) : [];
    } catch {
      return [];
    }
  }

  private addSaved(post: PostSummary): void {
    const saved = this.getSaved();
    if (!saved.find((p) => p.id === post.id)) {
      saved.unshift(post);
      localStorage.setItem(STORAGE_KEY + '_posts', JSON.stringify(saved));
    }
  }

  private removeSaved(postId: string): void {
    const saved = this.getSaved().filter((p) => p.id !== postId);
    localStorage.setItem(STORAGE_KEY + '_posts', JSON.stringify(saved));
  }

  private loadIds(): Set<string> {
    try {
      const raw = localStorage.getItem(STORAGE_KEY + '_ids');
      return raw ? new Set(JSON.parse(raw)) : new Set();
    } catch {
      return new Set();
    }
  }
}
