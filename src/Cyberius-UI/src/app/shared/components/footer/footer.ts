import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-footer',
  imports: [CommonModule],
  templateUrl: './footer.html',
  styleUrl: './footer.css',
})
export class Footer {
  currentYear = new Date().getFullYear();

  links = {
    blog: [
      { label: 'Все статьи', href: '/posts' },
      { label: 'C# & .NET', href: '/dotnet' },
      { label: 'Angular', href: '/angular' },
      { label: 'Архитектура', href: '/architecture' },
    ],
    resources: [
      { label: 'GitHub', href: 'https://github.com' },
      { label: 'Документация .NET', href: 'https://learn.microsoft.com/dotnet' },
      { label: 'Документация Angular', href: 'https://angular.dev' },
      { label: 'RSS Feed', href: '/feed.xml' },
    ],
  };

  social = [
    { name: 'GitHub', href: '#', icon: 'github' },
    { name: 'Telegram', href: '#', icon: 'telegram' },
    { name: 'LinkedIn', href: '#', icon: 'linkedin' },
  ];
}
