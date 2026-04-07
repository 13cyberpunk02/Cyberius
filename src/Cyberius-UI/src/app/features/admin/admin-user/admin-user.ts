import { Component, HostListener, inject, OnInit, signal } from '@angular/core';
import {
  ConfirmDialog,
  ConfirmDialogConfig,
} from '../../../shared/components/confirm-dialog/confirm-dialog';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AdminUsersService } from '../../../core/services/admin-user.service';
import { ToastService } from '../../../core/services/toast.service';
import { SeoService } from '../../../core/services/seo.service';
import { AuthService } from '../../../core/services/auth.service';
import { AdminUser, ALL_ROLES, RoleName } from '../../../core/models/admin-user.model';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faAngleDown,
  faAngleLeft,
  faAngleRight,
  faBan,
  faCheck,
  faCheckCircle,
  faEye,
  faSpinner,
  faTrashCan,
} from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-admin-user',
  imports: [CommonModule, RouterModule, FormsModule, ConfirmDialog, FaIconComponent],
  templateUrl: './admin-user.html',
  styleUrl: './admin-user.css',
})
export class AdminUserComponent implements OnInit {
  private adminService = inject(AdminUsersService);
  private toast = inject(ToastService);
  private seo = inject(SeoService);
  readonly auth = inject(AuthService);

  users = signal<AdminUser[]>([]);
  loading = signal(true);
  totalCount = signal(0);
  page = signal(1);
  totalPages = signal(1);
  search = signal('');
  searchInput = '';

  // Блокировка
  togglingId = signal<string | null>(null);

  // Удаление
  deleteTarget = signal<AdminUser | null>(null);
  deleting = signal(false);

  // Смена роли
  roleMenuId = signal<string | null>(null);
  roleMenuPos = signal<{ top: number; left: number }>({ top: 0, left: 0 });
  changingRole = signal<string | null>(null);

  openRoleMenu(event: MouseEvent, userId: string): void {
    if (this.roleMenuId() === userId) {
      this.roleMenuId.set(null);
      return;
    }
    const rect = (event.currentTarget as HTMLElement).getBoundingClientRect();
    this.roleMenuPos.set({
      top: rect.bottom + window.scrollY + 4,
      left: rect.left + window.scrollX,
    });
    this.roleMenuId.set(userId);
  }

  readonly allRoles = ALL_ROLES;
  readonly pageSize = 20;

  readonly deleteConfig: ConfirmDialogConfig = {
    title: 'Удалить пользователя?',
    message: 'Все статьи и комментарии этого пользователя будут удалены.',
    confirmLabel: 'Удалить',
    cancelLabel: 'Отмена',
    danger: true,
  };

  ngOnInit(): void {
    this.seo.setPage({ title: 'Управление пользователями' });
    this.load();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(e: MouseEvent): void {
    if (!(e.target as HTMLElement).closest('.role-menu-wrapper')) {
      this.roleMenuId.set(null);
    }
  }

  private load(): void {
    this.loading.set(true);
    this.adminService.getAll(this.page(), this.pageSize, this.search()).subscribe({
      next: (res) => {
        this.users.set(res.items);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onSearch(): void {
    this.search.set(this.searchInput);
    this.page.set(1);
    this.load();
  }

  clearSearch(): void {
    this.searchInput = '';
    this.search.set('');
    this.page.set(1);
    this.load();
  }

  goToPage(p: number): void {
    this.page.set(p);
    this.load();
  }

  toggleBlock(user: AdminUser): void {
    this.togglingId.set(user.id);
    this.adminService.toggleBlock(user.id).subscribe({
      next: (res) => {
        this.users.update((list) =>
          list.map((u) => (u.id === user.id ? { ...u, isActive: res.isActive } : u)),
        );
        this.toast.success(
          res.isActive ? `${user.userName} разблокирован` : `${user.userName} заблокирован`,
        );
        this.togglingId.set(null);
      },
      error: () => {
        this.toast.error('Ошибка');
        this.togglingId.set(null);
      },
    });
  }

  changeRole(user: AdminUser, role: RoleName): void {
    this.changingRole.set(user.id);
    this.roleMenuId.set(null);
    this.adminService.changeRole(user.id, role).subscribe({
      next: () => {
        this.users.update((list) =>
          list.map((u) => (u.id === user.id ? { ...u, roles: [role] } : u)),
        );
        this.toast.success(`Роль изменена на ${role}`);
        this.changingRole.set(null);
      },
      error: (err) => {
        this.toast.error(err?.error?.detail ?? 'Ошибка');
        this.changingRole.set(null);
      },
    });
  }

  confirmDelete(): void {
    const user = this.deleteTarget();
    if (!user) return;
    this.deleting.set(true);
    this.adminService.deleteUser(user.id).subscribe({
      next: () => {
        this.users.update((list) => list.filter((u) => u.id !== user.id));
        this.totalCount.update((c) => c - 1);
        this.toast.success(`Пользователь ${user.userName} удалён`);
        this.deleteTarget.set(null);
        this.deleting.set(false);
      },
      error: (err) => {
        this.toast.error(err?.error?.detail ?? 'Ошибка удаления');
        this.deleteTarget.set(null);
        this.deleting.set(false);
      },
    });
  }

  getRoleClass(role: string): string {
    switch (role) {
      case 'Admin':
        return 'bg-rose-500/15 text-rose-400 border-rose-500/30';
      case 'Manager':
        return 'bg-amber-500/15 text-amber-400 border-amber-500/30';
      default:
        return 'bg-sky-500/15 text-sky-400 border-sky-500/30';
    }
  }

  getAvatarUrl(path: string | null): string | null {
    return this.adminService.getAvatarUrl(path);
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages() }, (_, i) => i + 1);
  }

  isCurrentUser(userId: string): boolean {
    return this.auth.user()?.id === userId;
  }

  protected readonly faSpinner = faSpinner;
  protected readonly faAngleDown = faAngleDown;
  protected readonly faEye = faEye;
  protected readonly faBan = faBan;
  protected readonly faCheckCircle = faCheckCircle;
  protected readonly faTrashCan = faTrashCan;
  protected readonly faAngleLeft = faAngleLeft;
  protected readonly faAngleRight = faAngleRight;
  protected readonly faCheck = faCheck;
}
