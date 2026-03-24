export interface Post {
  id: number;
  title: string;
  excerpt: string;
  category: PostCategory;
  tags: string[];
  readTime: number;
  date: string;
  slug: string;
  featured?: boolean;
}

export type PostCategory = 'csharp' | 'dotnet' | 'angular' | 'architecture' | 'devops';

export const CATEGORY_META: Record<PostCategory, { label: string; color: string; icon: string }> = {
  csharp: { label: 'C#', color: 'from-sky-500 to-cyan-400', icon: '#' },
  dotnet: { label: '.NET 10', color: 'from-purple-500 to-indigo-400', icon: '⬡' },
  angular: { label: 'Angular', color: 'from-rose-500 to-pink-400', icon: '▲' },
  architecture: { label: 'Architecture', color: 'from-amber-500 to-orange-400', icon: '◈' },
  devops: { label: 'DevOps', color: 'from-emerald-500 to-teal-400', icon: '⚙' },
};
