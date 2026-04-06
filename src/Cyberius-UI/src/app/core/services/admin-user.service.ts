import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { AdminUser, PagedAdminUsers, RoleName } from '../models/admin-user.model';

@Injectable({ providedIn: 'root' })
export class AdminUsersService {
  private http = inject(HttpClient);
  private auth = inject(AuthService);

  private get base(): string {
    return `${this.auth.API}/admin/users`;
  }

  getAll(page = 1, pageSize = 20, search?: string): Observable<PagedAdminUsers> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search?.trim()) params = params.set('search', search.trim());
    return this.http.get<PagedAdminUsers>(this.base, { params });
  }

  changeRole(userId: string, roleName: RoleName): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.base}/${userId}/role`, { roleName });
  }

  toggleBlock(userId: string): Observable<{ isActive: boolean }> {
    return this.http.put<{ isActive: boolean }>(`${this.base}/${userId}/toggle-block`, {});
  }

  deleteUser(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${userId}`);
  }

  getAvatarUrl(path: string | null): string | null {
    if (!path) return null;
    if (path.startsWith('http')) return path;
    return this.auth.FILES_BASE + path;
  }
}
