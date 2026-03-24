import { Injectable, signal, effect } from '@angular/core';

export type Theme = 'dark' | 'light';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly THEME_KEY = 'blog-theme';

  theme = signal<Theme>(this.getInitialTheme());

  constructor() {
    effect(() => {
      const current = this.theme();
      localStorage.setItem(this.THEME_KEY, current);

      const html = document.documentElement;
      const body = document.body;

      if (current === 'dark') {
        html.classList.add('dark');
        html.classList.remove('light');
        body.classList.add('dark');
        body.classList.remove('light');
      } else {
        html.classList.add('light');
        html.classList.remove('dark');
        body.classList.add('light');
        body.classList.remove('dark');
      }
    });
  }

  toggle(): void {
    this.theme.update((t) => (t === 'dark' ? 'light' : 'dark'));
  }

  private getInitialTheme(): Theme {
    const saved = localStorage.getItem(this.THEME_KEY) as Theme | null;
    if (saved) return saved;
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }
}
