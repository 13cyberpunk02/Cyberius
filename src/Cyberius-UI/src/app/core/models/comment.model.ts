export type ReactionType = 'Like' | 'Heart' | 'Fire' | 'Clap' | 'Thinking';

export interface CommentAuthorDto {
  id: string;
  username: string;
  fullName: string;
  avatarUrl: string | null;
}

export interface CommentResponse {
  id: string;
  content: string;
  isEdited: boolean;
  isDeleted: boolean;
  createdAt: string;
  updatedAt: string;
  author: CommentAuthorDto;
  reactions: Record<string, number>;
  myReaction: string | null;
  replyCount: number;
  replies: CommentResponse[] | null;
}

export interface PagedComments {
  items: CommentResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateCommentRequest {
  postId: string;
  content: string;
  parentCommentId: string | null;
}

export interface UpdateCommentRequest {
  content: string;
}
