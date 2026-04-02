import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SafeHtml } from '@angular/platform-browser';
import { SvgIconService } from '../../../../core/services/svgIcon.service';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faCheck } from '@fortawesome/free-solid-svg-icons';

interface TechItem {
  name: string;
  version: string;
  description: string;
  iconFile: string;
  icon?: SafeHtml;
  outlineColor: string; // CSS цвет обводки иконки и карточки
  features: string[];
}

@Component({
  selector: 'app-tech-stack',
  standalone: true,
  imports: [CommonModule, FaIconComponent],
  templateUrl: './tech-stack.html',
})
export class TechStack implements OnInit {
  private svgService = inject(SvgIconService);

  techs = signal<TechItem[]>([
    {
      name: 'C#',
      version: '13.0',
      description: 'Современный объектно-ориентированный язык с мощной системой типов',
      iconFile: 'csharp',
      outlineColor: '#0ca2e7',
      features: ['Extension Members', 'Field Keyword', 'Params Collections', 'Lock Object'],
    },
    {
      name: '.NET',
      version: '10.0',
      description: 'Кроссплатформенный фреймворк нового поколения с Native AOT',
      iconFile: 'dotnet',
      outlineColor: '#818cf8',
      features: ['Native AOT', 'Minimal API v2', 'OTEL Built-in', 'Blazor SSR'],
    },
    {
      name: 'Angular',
      version: '21+',
      description: 'Полнофункциональный фреймворк для enterprise SPA приложений',
      iconFile: 'angular',
      outlineColor: '#f43f5e',
      features: ['Signals API', 'Zoneless CD', 'Deferrable Views', 'Resource API'],
    },
    {
      name: 'EF Core',
      version: '10.0',
      description: 'ORM нового поколения с поддержкой сложных запросов',
      iconFile: 'ef-core',
      outlineColor: '#10b981',
      features: ['Complex Types', 'JSON Columns', 'Raw SQL', 'Compiled Models'],
    },
  ]);

  ngOnInit(): void {
    this.techs().forEach((tech, index) => {
      this.svgService.load(tech.iconFile).subscribe((svg) => {
        this.techs.update((items) =>
          items.map((item, i) => (i === index ? { ...item, icon: svg } : item)),
        );
      });
    });
  }

  protected readonly faCheck = faCheck;
}
