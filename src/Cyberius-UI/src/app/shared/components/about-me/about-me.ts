import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SeoService } from '../../../core/services/seo.service';
import { FaIconComponent, IconDefinition } from '@fortawesome/angular-fontawesome';
import { faFileLines, faGear, faPalette, faRocket } from '@fortawesome/free-solid-svg-icons';
import { faGithub, faLinkedinIn, faTelegram } from '@fortawesome/free-brands-svg-icons';

interface SkillGroup {
  title: string;
  icon: IconDefinition;
  skills: string[];
}

interface SocialLink {
  name: string;
  url: string;
  icon: IconDefinition;
  color: string;
}

@Component({
  selector: 'app-about-me',
  imports: [CommonModule, RouterModule, FaIconComponent],
  templateUrl: './about-me.html',
  styleUrl: './about-me.css',
})
export class AboutMe implements OnInit {
  private seo = inject(SeoService);

  ngOnInit(): void {
    this.seo.setPage({
      title: 'Обо мне',
      description: 'Разработчик .NET и Angular. Пишу о backend, frontend и архитектуре.',
    });
  }

  readonly skills: SkillGroup[] = [
    {
      title: 'Backend',
      icon: faGear,
      skills: [
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
      skills: ['Angular 21+', 'TypeScript', 'RxJS', 'Tailwind CSS', 'NgRx Signals'],
    },
    {
      title: 'Инфраструктура',
      icon: faRocket,
      skills: ['Docker', 'GitHub Actions', 'Nginx', 'MinIO', 'Linux'],
    },
  ];

  readonly socials: SocialLink[] = [
    {
      name: 'GitHub',
      url: 'https://github.com/',
      color: '#6e7681',
      icon: faGithub,
    },
    {
      name: 'Telegram',
      url: 'https://t.me/',
      color: '#2AABEE',
      icon: faTelegram,
    },
    {
      name: 'LinkedIn',
      url: 'https://linkedin.com/',
      color: '#0A66C2',
      icon: faLinkedinIn,
    },
  ];
  protected readonly faFileLines = faFileLines;
}
