// ── Enums (должны совпадать с бэкендом) ───────────────────────────────────

export type PostStatus = 'Draft' | 'Published' | 'Archived';

export type BlockType =
  | 'Heading1'
  | 'Heading2'
  | 'Heading3'
  | 'Paragraph'
  | 'Code'
  | 'Image'
  | 'Quote'
  | 'Callout'
  | 'Divider';

export type ReactionType = 'Like' | 'Heart' | 'Fire' | 'Clap' | 'Thinking';

// ── Nested DTOs ────────────────────────────────────────────────────────────

export interface AuthorDto {
  id: string;
  username: string;
  fullName: string;
  avatarUrl: string | null;
}

export interface CategoryDto {
  id: string;
  name: string;
  slug: string;
  color: string | null;
  iconUrl: string | null;
}

export interface ContentBlockDto {
  id: string;
  type: BlockType;
  order: number;
  content: string | null;
  language: string | null;
  imageUrl: string | null;
  imageCaption: string | null;
  calloutType: string | null;
}

// ── Response models ────────────────────────────────────────────────────────

export interface PostSummary {
  id: string;
  title: string;
  slug: string;
  excerpt: string | null;
  coverImageUrl: string | null;
  readTimeMinutes: number;
  status: PostStatus;
  publishedAt: string | null;
  createdAt: string;
  author: AuthorDto;
  category: CategoryDto;
  tags: string[];
  viewCount: number;
  commentCount: number;
  reactions: Record<string, number>;
}

export interface PostDetailModel extends PostSummary {
  blocks: ContentBlockDto[];
  myReaction: ReactionType | null;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ── Request models ─────────────────────────────────────────────────────────

export interface CreateContentBlockRequest {
  type: BlockType;
  order: number;
  content: string | null;
  language: string | null;
  imageUrl: string | null;
  imageCaption: string | null;
  calloutType: string | null;
}

export interface CreatePostRequest {
  title: string;
  excerpt: string | null;
  coverImageUrl: string | null;
  categoryId: string;
  tags: string[];
  blocks: CreateContentBlockRequest[];
}

export type UpdatePostRequest = CreatePostRequest;

// ── Query params ───────────────────────────────────────────────────────────

export interface GetPostsParams {
  page?: number;
  pageSize?: number;
}

export interface SearchPostsParams extends GetPostsParams {
  q: string;
}

// ── UI helpers (для совместимости с существующими компонентами) ────────────

export type PostCategory = 'csharp' | 'dotnet' | 'angular' | 'architecture' | 'devops';

export const CATEGORY_META: Record<
  string,
  { label: string; outlineColor: string; iconFile: string }
> = {
  csharp: { label: 'C#', outlineColor: '#0ca2e7', iconFile: 'csharp' },
  dotnet: { label: '.NET 10', outlineColor: '#818cf8', iconFile: 'dotnet' },
  angular: { label: 'Angular', outlineColor: '#f43f5e', iconFile: 'angular' },
  architecture: { label: 'Architecture', outlineColor: '#f59e0b', iconFile: 'architecture' },
  devops: { label: 'DevOps', outlineColor: '#10b981', iconFile: 'devops' },
};
