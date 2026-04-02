import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  PostSummary,
  PostDetailModel,
  PagedResponse,
  CreatePostRequest,
  UpdatePostRequest,
  GetPostsParams,
  SearchPostsParams,
  ReactionType,
} from '../models/post.model';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class PostsService {
  private http = inject(HttpClient);
  private auth = inject(AuthService);

  private get base(): string {
    return `${this.auth.API}/posts`;
  }

  // ── Queries ────────────────────────────────────────────────────

  getPublished(params: GetPostsParams = {}): Observable<PagedResponse<PostSummary>> {
    const httpParams = this.buildPageParams(params);
    return this.http.get<PagedResponse<PostSummary>>(this.base, { params: httpParams });
  }

  getById(id: string): Observable<PostDetailModel> {
    return this.http.get<PostDetailModel>(`${this.base}/${id}`);
  }

  getBySlug(slug: string): Observable<PostDetailModel> {
    return this.http.get<PostDetailModel>(`${this.base}/slug/${slug}`);
  }

  search(params: SearchPostsParams): Observable<PagedResponse<PostSummary>> {
    const httpParams = this.buildPageParams(params).set('q', params.q);
    return this.http.get<PagedResponse<PostSummary>>(`${this.base}/search`, { params: httpParams });
  }

  getByAuthor(
    authorId: string,
    params: GetPostsParams = {},
  ): Observable<PagedResponse<PostSummary>> {
    const httpParams = this.buildPageParams(params);
    return this.http.get<PagedResponse<PostSummary>>(`${this.base}/author/${authorId}`, {
      params: httpParams,
    });
  }

  getByCategory(
    categoryId: string,
    params: GetPostsParams = {},
  ): Observable<PagedResponse<PostSummary>> {
    const httpParams = this.buildPageParams(params);
    return this.http.get<PagedResponse<PostSummary>>(`${this.base}/category/${categoryId}`, {
      params: httpParams,
    });
  }

  getByTag(tagSlug: string, params: GetPostsParams = {}): Observable<PagedResponse<PostSummary>> {
    const httpParams = this.buildPageParams(params);
    return this.http.get<PagedResponse<PostSummary>>(`${this.base}/tag/${tagSlug}`, {
      params: httpParams,
    });
  }

  getDrafts(): Observable<PagedResponse<PostSummary>> {
    return this.http.get<PagedResponse<PostSummary>>(`${this.base}/drafts`);
  }

  // ── Commands ───────────────────────────────────────────────────

  create(request: CreatePostRequest): Observable<PostDetailModel> {
    return this.http.post<PostDetailModel>(this.base, request);
  }

  update(id: string, request: UpdatePostRequest): Observable<PostDetailModel> {
    return this.http.put<PostDetailModel>(`${this.base}/${id}`, request);
  }

  publish(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/publish`, {});
  }

  unpublish(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/unpublish`, {});
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  react(id: string, type: ReactionType): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/react/${type}`, {});
  }

  // Трекинг просмотра — POST /api/posts/{id}/view
  trackView(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/view`, {});
  }

  // Похожие статьи
  getRelated(id: string, count = 3): Observable<PostSummary[]> {
    return this.http.get<PostSummary[]>(`${this.base}/${id}/related?count=${count}`);
  }

  // ── Helpers ────────────────────────────────────────────────────

  private buildPageParams(params: GetPostsParams): HttpParams {
    let p = new HttpParams();
    if (params.page) p = p.set('page', params.page);
    if (params.pageSize) p = p.set('pageSize', params.pageSize);
    return p;
  }

  // Строим полный URL аватара/изображения через FILES_BASE
  getImageUrl(path: string | null): string | null {
    if (!path) return null;
    if (path.startsWith('http')) return path;
    return this.auth.FILES_BASE + path;
  }
}
