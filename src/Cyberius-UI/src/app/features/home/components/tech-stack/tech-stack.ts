import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface TechItem {
  name: string;
  version: string;
  description: string;
  icon: string;
  color: string;
  features: string[];
}

@Component({
  selector: 'app-tech-stack',
  imports: [CommonModule],
  templateUrl: './tech-stack.html',
  styleUrl: './tech-stack.css',
})
export class TechStack {
  techs: TechItem[] = [
    {
      name: 'C#',
      version: '13.0',
      description: 'Современный объектно-ориентированный язык с мощной системой типов',
      icon: '#',
      color: 'from-sky-600 to-cyan-500',
      features: ['Extension Members', 'Field Keyword', 'Params Collections', 'Lock Object'],
    },
    {
      name: '.NET',
      version: '10.0',
      description: 'Кроссплатформенный фреймворк нового поколения с Native AOT',
      icon: '⬡',
      color: 'from-indigo-600 to-purple-500',
      features: ['Native AOT', 'Minimal API v2', 'OTEL Built-in', 'Blazor SSR'],
    },
    {
      name: 'Angular',
      version: '21+',
      description: 'Полнофункциональный фреймворк для enterprise SPA приложений',
      icon: '▲',
      color: 'from-rose-600 to-pink-500',
      features: ['Signals API', 'Zoneless CD', 'Deferrable Views', 'Resource API'],
    },
    {
      name: 'EF Core',
      version: '10.0',
      description: 'ORM нового поколения с поддержкой сложных запросов',
      icon: '⬢',
      color: 'from-emerald-600 to-teal-500',
      features: ['Complex Types', 'JSON Columns', 'Raw SQL', 'Compiled Models'],
    },
  ];
}
