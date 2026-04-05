import { Component, HostListener, inject, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeService } from './core/services/theme.service';
import { Header } from './shared/components/header/header';
import { Footer } from './shared/components/footer/footer';
import { ToastContainer } from './shared/components/toast-container/toast-container';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { faAnglesUp } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header, Footer, ToastContainer, FaIconComponent],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit {
  themeService = inject(ThemeService);
  showScrollTop = signal(false);

  ngOnInit() {
    // Theme is initialized reactively via effect() in ThemeService
  }

  scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  @HostListener('window:scroll')
  onScroll(): void {
    this.showScrollTop.set(window.scrollY > 400);
  }

  protected readonly faAnglesUp = faAnglesUp;
}
