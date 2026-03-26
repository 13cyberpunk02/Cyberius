import { Component, HostListener, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ThemeService } from '../../../core/services/theme.service';
import { FaIconComponent, FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faArrowRightFromBracket,
  faArrowRightToBracket,
  faBars,
  faBell,
  faBookmark,
  faBurger,
  faCode,
  faMoon,
  faSun,
  faUser,
  faXmark,
} from '@fortawesome/free-solid-svg-icons';
import { LoginModal } from '../login-modal/login-modal';
import { AuthService } from '../../../core/services/auth.service';
import { RegisterModal } from '../register-modal/register-modal';

type ModalView = 'none' | 'login' | 'register';

@Component({
  selector: 'app-header',
  imports: [CommonModule, RouterModule, LoginModal, RegisterModal, FontAwesomeModule],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  themeService = inject(ThemeService);
  authService = inject(AuthService);

  protected readonly moonIcon = faMoon;
  protected readonly sunIcon = faSun;
  protected readonly faCode = faCode;
  protected readonly faBell = faBell;
  protected readonly faArrowRightToBracket = faArrowRightToBracket;
  protected readonly faArrowRightFromBracket = faArrowRightFromBracket;
  protected readonly faUser = faUser;
  protected readonly faBookmark = faBookmark;
  protected readonly faBurger = faBurger;
  protected readonly faXmark = faXmark;
  protected readonly faBars = faBars;

  isScrolled = signal(false);
  isMobileMenuOpen = signal(false);
  showUserMenu = signal(false);
  modal = signal<ModalView>('none');

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

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!(event.target as HTMLElement).closest('.user-menu-wrapper')) {
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
    this.modal.set('login');
  }
  openRegister() {
    this.modal.set('register');
  }
  closeModal() {
    this.modal.set('none');
  }

  toggleUserMenu(event: MouseEvent) {
    event.stopPropagation();
    this.showUserMenu.update((v) => !v);
  }

  logout() {
    this.authService.logout();
    this.showUserMenu.set(false);
  }
}
