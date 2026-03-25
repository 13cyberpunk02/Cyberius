import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { Observable, map, shareReplay } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SvgIconService {
  private http = inject(HttpClient);
  private sanitizer = inject(DomSanitizer);

  // Кэш — каждый SVG загружается только один раз
  private cache = new Map<string, Observable<SafeHtml>>();

  load(iconName: string): Observable<SafeHtml> {
    const path = `icons/${iconName}.svg`;

    if (!this.cache.has(iconName)) {
      const req$ = this.http.get(path, { responseType: 'text' }).pipe(
        map((svg) => this.sanitizer.bypassSecurityTrustHtml(svg)),
        shareReplay(1),
      );
      this.cache.set(iconName, req$);
    }

    return this.cache.get(iconName)!;
  }
}
