import { Directive, ElementRef, Input, OnChanges, OnInit, inject } from '@angular/core';
import { ThemeService } from '../../core/services/theme.service';
import { effect } from '@angular/core';

declare const hljs: {
  highlight: (code: string, opts: { language: string }) => { value: string };
  highlightAuto: (code: string) => { value: string };
  highlightElement: (el: HTMLElement) => void;
};

@Directive({
  selector: '[appHighlight]',
  standalone: true,
})
export class HighlightDirective implements OnChanges, OnInit {
  @Input('appHighlight') language: string | null = null;
  @Input() code: string | null = null;

  private el = inject(ElementRef<HTMLElement>);
  private theme = inject(ThemeService);

  private codeEl!: HTMLElement;

  constructor() {
    // При смене темы — переключаем стиль highlight.js
    effect(() => {
      const t = this.theme.theme();
      this.applyTheme(t);
    });
  }

  ngOnInit(): void {
    this.codeEl = this.el.nativeElement;
    this.highlight();
    this.applyTheme(this.theme.theme());
  }

  ngOnChanges(): void {
    if (this.codeEl) this.highlight();
  }

  private highlight(): void {
    if (!this.code || typeof hljs === 'undefined') return;

    try {
      const lang = this.language ?? 'plaintext';
      const result = hljs.highlight(this.code, { language: lang });
      this.codeEl.innerHTML = result.value;
    } catch {
      // Если язык не поддерживается — fallback на auto
      try {
        const result = hljs.highlightAuto(this.code ?? '');
        this.codeEl.innerHTML = result.value;
      } catch {
        this.codeEl.textContent = this.code;
      }
    }
  }

  private applyTheme(theme: 'dark' | 'light'): void {
    const dark = document.getElementById('hljs-dark') as HTMLLinkElement | null;
    const light = document.getElementById('hljs-light') as HTMLLinkElement | null;
    if (dark && light) {
      dark.disabled = theme !== 'dark';
      light.disabled = theme !== 'light';
    }
  }
}
