import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CategoryResponse, CreateCategoryRequest } from '../models/category.model';
import { AuthService } from './auth.service';

export interface UpdateCategoryRequest extends CreateCategoryRequest {}

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private http = inject(HttpClient);
  private auth = inject(AuthService);

  private get base() {
    return `${this.auth.API}/categories`;
  }

  getAll(): Observable<CategoryResponse[]> {
    return this.http.get<CategoryResponse[]>(this.base);
  }

  create(req: CreateCategoryRequest): Observable<CategoryResponse> {
    return this.http.post<CategoryResponse>(this.base, req);
  }

  update(id: string, req: UpdateCategoryRequest): Observable<CategoryResponse> {
    return this.http.put<CategoryResponse>(`${this.base}/${id}`, req);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
