import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-hero',
  imports: [CommonModule],
  templateUrl: './hero.html',
  styleUrl: './hero.css',
})
export class Hero implements OnInit, OnDestroy {
  private interval: ReturnType<typeof setInterval> | null = null;

  codeLines = [
    { text: 'var app = WebApplication.CreateBuilder(args)', color: 'text-slate-300' },
    { text: '    .AddServices()', color: 'text-sky-400' },
    { text: '    .ConfigureOpenApi()', color: 'text-cyan-400' },
    { text: '    .Build();', color: 'text-slate-300' },
    { text: '', color: '' },
    { text: 'app.MapGet("/posts", async (IPostsService svc)', color: 'text-slate-300' },
    { text: '    => await svc.GetAllAsync());', color: 'text-emerald-400' },
    { text: '', color: '' },
    { text: 'await app.RunAsync();', color: 'text-amber-400' },
  ];

  visibleLines = signal(0);

  ngOnInit() {
    let count = 0;
    this.interval = setInterval(() => {
      count++;
      this.visibleLines.set(count);
      if (count >= this.codeLines.length) {
        clearInterval(this.interval!);
      }
    }, 120);
  }

  ngOnDestroy() {
    if (this.interval) clearInterval(this.interval);
  }

  techBadges = [
    { label: 'C# 13', color: 'badge-blue' },
    { label: '.NET 10', color: 'badge-purple' },
    { label: 'Angular 21', color: 'badge-rose' },
    { label: 'Minimal API', color: 'badge-teal' },
    { label: 'Signals', color: 'badge-amber' },
  ];
}
