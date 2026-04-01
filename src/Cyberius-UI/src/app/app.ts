import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeService } from './core/services/theme.service';
import { Header } from './shared/components/header/header';
import { Footer } from './shared/components/footer/footer';
import { ToastContainer } from './shared/components/toast-container/toast-container';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header, Footer, ToastContainer],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit {
  themeService = inject(ThemeService);

  ngOnInit() {
    // Theme is initialized reactively via effect() in ThemeService
  }
}
