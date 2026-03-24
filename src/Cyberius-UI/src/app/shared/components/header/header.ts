import { Component, HostListener, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ThemeService } from '../../../core/services/theme.service';

@Component({
  selector: 'app-header',
  imports: [CommonModule, RouterModule],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  themeService = inject(ThemeService);

  isScrolled = signal(false);
  isMobileMenuOpen = signal(false);

  navLinks = [
    { label: 'Главная', href: '/' },
    { label: 'Статьи', href: '/posts' },
    { label: 'C# / .NET', href: '/dotnet' },
    { label: 'Angular', href: '/angular' },
    { label: 'Обо мне', href: '/about' },
  ];

  @HostListener('window:scroll')
  onScroll() {
    this.isScrolled.set(window.scrollY > 20);
  }

  toggleMenu() {
    this.isMobileMenuOpen.update((v) => !v);
  }

  closeMenu() {
    this.isMobileMenuOpen.set(false);
  }
}
