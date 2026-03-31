import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CommentResponse,
  PagedComments,
  CreateCommentRequest,
  UpdateCommentRequest,
} from '../models/comment.model';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class CommentService {
  private http = inject(HttpClient);
  private auth = inject(AuthService);

  private get base() {
    return `${this.auth.API}/comments`;
  }

  getByPost(postId: string, page = 1, pageSize = 20): Observable<PagedComments> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedComments>(`${this.base}/post/${postId}`, { params });
  }

  create(request: CreateCommentRequest): Observable<CommentResponse> {
    return this.http.post<CommentResponse>(this.base, request);
  }

  update(id: string, request: UpdateCommentRequest): Observable<CommentResponse> {
    return this.http.put<CommentResponse>(`${this.base}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  react(id: string, type: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/react/${type}`, {});
  }
}
