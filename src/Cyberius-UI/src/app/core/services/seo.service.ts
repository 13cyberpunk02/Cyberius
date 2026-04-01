import { Injectable, inject } from '@angular/core';
import { Title, Meta } from '@angular/platform-browser';

export interface SeoMeta {
  title: string;
  description?: string;
  image?: string;
  url?: string;
  type?: 'website' | 'article';
  publishedAt?: string;
  author?: string;
}

const SITE_NAME = 'Cyberius';
const BASE_URL = 'http://localhost:4200';
const DEFAULT_IMAGE = `${BASE_URL}/icons/og-default.png`;

@Injectable({ providedIn: 'root' })
export class SeoService {
  private title = inject(Title);
  private meta = inject(Meta);

  setPage(data: SeoMeta): void {
    const fullTitle = data.title.includes(SITE_NAME) ? data.title : `${data.title} — ${SITE_NAME}`;

    const description = data.description?.slice(0, 160) ?? 'Блог о .NET, C# и Angular разработке';
    const image = data.image ?? DEFAULT_IMAGE;
    const url = data.url ?? BASE_URL;
    const type = data.type ?? 'website';

    // ── Base ──────────────────────────────────────────────────────
    this.title.setTitle(fullTitle);
    this.meta.updateTag({ name: 'description', content: description });

    // ── Open Graph (Facebook, Telegram, VK) ──────────────────────
    this.meta.updateTag({ property: 'og:title', content: fullTitle });
    this.meta.updateTag({ property: 'og:description', content: description });
    this.meta.updateTag({ property: 'og:image', content: image });
    this.meta.updateTag({ property: 'og:url', content: url });
    this.meta.updateTag({ property: 'og:type', content: type });
    this.meta.updateTag({ property: 'og:site_name', content: SITE_NAME });

    // ── Twitter Card ──────────────────────────────────────────────
    this.meta.updateTag({ name: 'twitter:card', content: 'summary_large_image' });
    this.meta.updateTag({ name: 'twitter:title', content: fullTitle });
    this.meta.updateTag({ name: 'twitter:description', content: description });
    this.meta.updateTag({ name: 'twitter:image', content: image });

    // ── Article specific ──────────────────────────────────────────
    if (type === 'article') {
      if (data.publishedAt)
        this.meta.updateTag({ property: 'article:published_time', content: data.publishedAt });
      if (data.author) this.meta.updateTag({ property: 'article:author', content: data.author });
    }
  }

  setDefault(): void {
    this.setPage({
      title: SITE_NAME,
      description:
        'Блог о разработке на .NET 10, C# 14 и Angular. Практические статьи, примеры кода и архитектурные решения.',
      type: 'website',
    });
  }
}
