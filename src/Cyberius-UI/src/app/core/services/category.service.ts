import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CategoryResponse } from '../models/category.model';
import { AuthService } from './auth.service';

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
}
