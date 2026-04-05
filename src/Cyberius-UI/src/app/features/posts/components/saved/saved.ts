import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PostCard } from '../post-card/post-card';
import { SeoService } from '../../../../core/services/seo.service';
import { PostSummary } from '../../../../core/models/post.model';
import { BookmarkService } from '../../../../core/services/bookmarks.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faBookmark } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-saved',
  imports: [CommonModule, RouterModule, PostCard, FaIconComponent],
  templateUrl: './saved.html',
  styleUrl: './saved.css',
})
export class Saved implements OnInit {
  private bookmarks = inject(BookmarkService);
  private seo = inject(SeoService);

  posts = signal<PostSummary[]>([]);

  ngOnInit(): void {
    this.seo.setPage({ title: 'Сохранённые статьи' });
    this.posts.set(this.bookmarks.getSaved());
  }

  clear(): void {
    this.posts().forEach((p) => this.bookmarks.toggle(p));
    this.posts.set([]);
  }

  protected readonly faBookmark = faBookmark;
}
