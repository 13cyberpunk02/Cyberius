// ══════════════════════════════════════════════════════════
// Конфиг страницы «Обо мне» — редактируй только этот файл
// ══════════════════════════════════════════════════════════

import { IconDefinition } from '@fortawesome/angular-fontawesome';
import { faGithub, faLinkedin, faTelegram } from '@fortawesome/free-brands-svg-icons';
import { faGear, faPalette, faRocket } from '@fortawesome/free-solid-svg-icons';

export interface AuthorConfig {
  firstName: string;
  lastName: string;
  role: string;
  bio: string[];
  avatarInitial: string;
  avatarUrl?: string;
  socials: {
    name: string;
    url: string;
    color: string;
    icon: IconDefinition;
  }[];
  skills: {
    title: string;
    icon: IconDefinition;
    items: string[];
  }[];
}

export const AUTHOR_CONFIG: AuthorConfig = {
  firstName: 'Salavat',
  lastName: 'Sabirov',
  role: 'Full-stack .NET & Angular Developer',
  bio: [
    'Разрабатываю backend на C# / .NET и frontend на Angular. ' +
      'Этот блог — место где я делюсь практическим опытом, разбираю архитектурные решения ' +
      'и слежу за новинками экосистемы .NET и Angular.',

    'Пишу о разработке на C# 14 / .NET 10 и Angular 21+ — ' +
      'от архитектуры и паттернов до конкретных фишек языка и фреймворка.',

    'Если нашли ошибку или хотите обсудить — пишите в комментариях или в Telegram.',
  ],
  avatarInitial: 'S',
  // avatarUrl: 'https://example.com/your-photo.jpg',

  socials: [
    {
      name: 'GitHub',
      url: 'https://github.com/13cyberpunk02',
      color: '#6e7681',
      icon: faGithub,
    },
    {
      name: 'Telegram',
      url: 'https://t.me/cyberpunk92',
      color: '#2AABEE',
      icon: faTelegram,
    },
    {
      name: 'LinkedIn',
      url: 'https://www.linkedin.com/in/salavat-sabirov-a43b69152/',
      color: '#0A66C2',
      icon: faLinkedin,
    },
  ],

  skills: [
    {
      title: 'Backend',
      icon: faGear,
      items: [
        'C# 14',
        '.NET 10',
        'ASP.NET Core',
        'Entity Framework Core',
        'PostgreSQL',
        'Redis',
        'Docker',
      ],
    },
    {
      title: 'Frontend',
      icon: faPalette,
      items: ['Angular 21+', 'TypeScript', 'RxJS', 'Tailwind CSS v4', 'NgRx Signals'],
    },
    {
      title: 'Инфраструктура',
      icon: faRocket,
      items: ['Docker', 'GitHub Actions', 'Nginx', 'MinIO', 'Linux'],
    },
  ],
};
