export interface AdminUser {
  id: string;
  email: string;
  userName: string;
  firstName: string;
  lastName: string;
  avatarUrl: string | null;
  isActive: boolean;
  joinedDate: string;
  roles: string[];
  postCount: number;
}

export interface PagedAdminUsers {
  items: AdminUser[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export const ALL_ROLES = ['Admin', 'Manager', 'User'] as const;
export type RoleName = (typeof ALL_ROLES)[number];
