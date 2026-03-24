import { Injectable } from '@angular/core';
import { Post } from '../models/post.model';

@Injectable({ providedIn: 'root' })
export class PostsService {
  getFeaturedPosts(): Post[] {
    return [
      {
        id: 1,
        title: 'Новые возможности C# 13 — что стоит знать разработчику',
        excerpt:
          'Разбираем extension members, field keyword, params collections и другие крутые фичи нового стандарта языка.',
        category: 'csharp',
        tags: ['C# 13', 'language features', '.NET 10'],
        readTime: 8,
        date: '2025-03-15',
        slug: 'csharp-13-new-features',
        featured: true,
      },
      {
        id: 2,
        title: 'Angular Signals: полное руководство с практическими примерами',
        excerpt:
          'От computed() и effect() до linkedSignal и resource API — всё что нужно знать о реактивности в Angular 21.',
        category: 'angular',
        tags: ['Angular 21', 'Signals', 'Reactivity'],
        readTime: 12,
        date: '2025-03-10',
        slug: 'angular-signals-guide',
        featured: true,
      },
      {
        id: 3,
        title: 'Minimal API в .NET 10: производительность и новый синтаксис',
        excerpt:
          'AOT компиляция, Native AOT endpoints, улучшенный OpenAPI — строим production-ready API с минимальным бойлерплейтом.',
        category: 'dotnet',
        tags: ['.NET 10', 'Minimal API', 'Performance'],
        readTime: 10,
        date: '2025-03-05',
        slug: 'dotnet10-minimal-api',
        featured: true,
      },
      {
        id: 4,
        title: 'Чистая архитектура на .NET 10 + Angular: полный стек',
        excerpt:
          'Проектируем слоёную архитектуру для enterprise-приложений: домен, приложение, инфраструктура, UI.',
        category: 'architecture',
        tags: ['Clean Architecture', 'CQRS', 'Full-Stack'],
        readTime: 15,
        date: '2025-02-28',
        slug: 'clean-architecture-dotnet-angular',
      },
      {
        id: 5,
        title: 'RxJS в Angular 21: когда Signals недостаточно',
        excerpt:
          'WebSockets, сложные async-потоки, interop с Signals — практическое руководство по RxJS в современном Angular.',
        category: 'angular',
        tags: ['RxJS', 'Angular 21', 'Observables'],
        readTime: 9,
        date: '2025-02-20',
        slug: 'rxjs-angular-21',
      },
      {
        id: 6,
        title: 'Docker + GitHub Actions для .NET 10 приложений',
        excerpt:
          'Полный CI/CD пайплайн: мультистейдж Docker образы, кэширование слоёв, деплой в Kubernetes.',
        category: 'devops',
        tags: ['Docker', 'CI/CD', 'Kubernetes'],
        readTime: 11,
        date: '2025-02-15',
        slug: 'docker-github-actions-dotnet',
      },
    ];
  }

  getStats() {
    return {
      posts: 48,
      readers: '12K',
      topics: 5,
      experience: 8,
    };
  }
}
