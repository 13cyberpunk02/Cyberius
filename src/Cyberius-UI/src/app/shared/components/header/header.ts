import { Component, HostListener, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ThemeService } from '../../../core/services/theme.service';
import { FaIconComponent, FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faBell, faCode, faMoon, faSun } from '@fortawesome/free-solid-svg-icons';
import { LoginModal } from '../login-modal/login-modal';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-header',
  imports: [CommonModule, RouterModule, LoginModal, FontAwesomeModule],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  themeService = inject(ThemeService);
  authService = inject(AuthService);
  moonIcon = faMoon;
  sunIcon = faSun;

  isScrolled = signal(false);
  isMobileMenuOpen = signal(false);
  showLoginModal = signal(false);
  showUserMenu = signal(false);

  navLinks = [
    { label: 'Главная', href: '/' },
    { label: 'Статьи', href: '/posts' },
    { label: 'C# / .NET', href: '/dotnet' },
    { label: 'Angular', href: '/angular' },
    { label: 'Обо мне', href: '/about' },
  ];

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('.user-menu-wrapper')) {
      this.showUserMenu.set(false);
    }
  }

  toggleMenu() {
    this.isMobileMenuOpen.update((v) => !v);
  }
  closeMenu() {
    this.isMobileMenuOpen.set(false);
  }
  openLogin() {
    this.showLoginModal.set(true);
  }
  closeLogin() {
    this.showLoginModal.set(false);
  }
  toggleUserMenu(event: MouseEvent) {
    event.stopPropagation();
    this.showUserMenu.update((v) => !v);
  }

  logout() {
    this.authService.logout();
    this.showUserMenu.set(false);
  }

  protected readonly faCode = faCode;
  protected readonly faBell = faBell;
}
