import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { faGithub, faLinkedin, faTelegram } from '@fortawesome/free-brands-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faCode } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-footer',
  imports: [CommonModule, FontAwesomeModule],
  templateUrl: './footer.html',
  styleUrl: './footer.css',
})
export class Footer {
  currentYear = new Date().getFullYear();
  githubIcon = faGithub;
  tgIcon = faTelegram;
  linkedinIcon = faLinkedin;
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
  protected readonly faCode = faCode;
}
