import { Component, HostListener, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ThemeService } from '../../../core/services/theme.service';
import { FaIconComponent, FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faArrowRight,
  faArrowRightFromBracket,
  faArrowRightToBracket,
  faBars,
  faBell,
  faBookmark,
  faBurger,
  faCode,
  faMagnifyingGlass,
  faMoon,
  faPenToSquare,
  faSun,
  faTag,
  faUser,
  faUsers,
  faXmark,
} from '@fortawesome/free-solid-svg-icons';
import { LoginModal } from '../../../features/auth/login-modal/login-modal';
import { AuthService } from '../../../core/services/auth.service';
import { RegisterModal } from '../../../features/auth/register-modal/register-modal';
import { FormsModule } from '@angular/forms';
import { NotificationService } from '../../../core/services/notification.service';

type ModalView = 'none' | 'login' | 'register';

@Component({
  selector: 'app-header',
  imports: [CommonModule, RouterModule, LoginModal, RegisterModal, FontAwesomeModule, FormsModule],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header implements OnInit, OnDestroy {
  themeService = inject(ThemeService);
  authService = inject(AuthService);
  notifications = inject(NotificationService);
  private router = inject(Router);

  protected readonly moonIcon = faMoon;
  protected readonly sunIcon = faSun;
  protected readonly faCode = faCode;
  protected readonly faArrowRightToBracket = faArrowRightToBracket;
  protected readonly faArrowRightFromBracket = faArrowRightFromBracket;
  protected readonly faUser = faUser;
  protected readonly faBookmark = faBookmark;
  protected readonly faXmark = faXmark;
  protected readonly faBars = faBars;
  protected readonly faTag = faTag;
  protected readonly faMagnifyingGlass = faMagnifyingGlass;
  protected readonly faArrowRight = faArrowRight;
  protected readonly faPenToSquare = faPenToSquare;

  isScrolled = signal(false);
  isMobileMenuOpen = signal(false);
  showUserMenu = signal(false);
  showSearch = signal(false);
  showNotifications = signal(false);
  searchQuery = signal('');
  modal = signal<ModalView>('none');

  navLinks = [
    { label: 'Главная', href: '/' },
    { label: 'Статьи', href: '/posts' },
    { label: 'Обо мне', href: '/about' },
  ];

  ngOnInit(): void {
    // Подключаем SignalR если уже авторизован
    if (this.authService.isAuthenticated()) {
      this.notifications.connect();
    }
  }

  ngOnDestroy(): void {
    this.notifications.disconnect();
  }

  @HostListener('window:scroll')
  onScroll() {
    this.isScrolled.set(window.scrollY > 20);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const el = event.target as HTMLElement;
    if (!el.closest('.user-menu-wrapper')) this.showUserMenu.set(false);
    if (!el.closest('.search-wrapper')) this.showSearch.set(false);
    if (!el.closest('.notifications-wrapper')) this.showNotifications.set(false);
  }

  @HostListener('document:keydown.escape')
  onEscape() {
    this.showSearch.set(false);
    this.searchQuery.set('');
    this.showNotifications.set(false);
  }

  toggleNotifications(event: MouseEvent): void {
    event.stopPropagation();
    this.showNotifications.update((v) => !v);
    if (this.showNotifications()) {
      this.notifications.markAllRead();
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

  openLoginAndConnect() {
    this.openLogin();
  }

  toggleUserMenu(event: MouseEvent) {
    event.stopPropagation();
    this.showUserMenu.update((v) => !v);
  }
  closeUserMenu() {
    this.showUserMenu.set(false);
  }

  toggleSearch(event: MouseEvent) {
    event.stopPropagation();
    this.showSearch.update((v) => !v);
    if (this.showSearch()) {
      setTimeout(() => document.getElementById('header-search')?.focus(), 50);
    }
  }

  submitSearch(): void {
    const q = this.searchQuery().trim();
    if (!q) return;
    this.router.navigate(['/posts'], { queryParams: { q } });
    this.showSearch.set(false);
    this.searchQuery.set('');
  }

  logout() {
    this.authService.logout();
    this.showUserMenu.set(false);
  }

  protected readonly faBell = faBell;
  protected readonly faUsers = faUsers;
}
