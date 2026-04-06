import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { faGithub, faLinkedin, faTelegram } from '@fortawesome/free-brands-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faCode } from '@fortawesome/free-solid-svg-icons';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-footer',
  imports: [CommonModule, FontAwesomeModule, RouterLink],
  templateUrl: './footer.html',
  styleUrl: './footer.css',
})
export class Footer {
  currentYear = new Date().getFullYear();
  protected readonly faCode = faCode;
  links = {
    blog: [
      { label: 'Все статьи', slug: null },
      { label: 'C# & .NET', slug: 'csharp' },
      { label: 'Angular', slug: 'angular' },
      { label: 'Архитектура', slug: 'architecture' },
    ],
    resources: [
      { label: 'GitHub', href: 'https://github.com' },
      { label: 'Документация .NET', href: 'https://learn.microsoft.com/dotnet' },
      { label: 'Документация Angular', href: 'https://angular.dev' },
      { label: 'RSS Feed', href: 'http://localhost:5273/feed.xml' },
    ],
  };

  social = [
    {
      name: 'GitHub',
      href: 'https://github.com/13cyberpunk02',
      icon: faGithub,
    },
    {
      name: 'Telegram',
      href: 'https://t.me/cyberpunk92',
      icon: faTelegram,
    },
    {
      name: 'LinkedIn',
      href: 'https://www.linkedin.com/in/salavat-sabirov-a43b69152/',
      icon: faLinkedin,
    },
  ];
}
