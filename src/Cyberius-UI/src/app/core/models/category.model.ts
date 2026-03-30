export interface CategoryResponse {
  id: string;
  name: string;
  slug: string;
  color: string | null;
  iconUrl: string | null;
  postCount: number;
}

export interface CreateCategoryRequest {
  name: string;
  slug: string;
  color: string | null;
  iconUrl: string | null;
}
