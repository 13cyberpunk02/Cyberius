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

export const CATEGORY_META: Record<
  PostCategory,
  { label: string; color: string; iconFile: string; outlineColor: string }
> = {
  csharp: {
    label: 'C#',
    color: 'from-sky-500 to-cyan-400',
    iconFile: 'csharp',
    outlineColor: '#0ca2e7',
  },
  dotnet: {
    label: '.NET 10',
    color: 'from-purple-500 to-indigo-400',
    iconFile: 'dotnet',
    outlineColor: '#818cf8',
  },
  angular: {
    label: 'Angular',
    color: 'from-rose-500 to-pink-400',
    iconFile: 'angular',
    outlineColor: '#f43f5e',
  },
  architecture: {
    label: 'Architecture',
    color: 'from-amber-500 to-orange-400',
    iconFile: 'architecture',
    outlineColor: '#f59e0b',
  },
  devops: {
    label: 'DevOps',
    color: 'from-emerald-500 to-teal-400',
    iconFile: 'devops',
    outlineColor: '#10b981',
  },
};
