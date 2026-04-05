import { Injectable, inject, signal, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { ToastService } from './toast.service';
import { Router } from '@angular/router';

export interface AppNotification {
  id: string;
  type: 'CommentReply' | 'CommentReaction' | 'PostReaction';
  message: string;
  postSlug: string | null;
  postTitle: string | null;
  actorName: string;
  actorAvatarUrl: string | null;
  createdAt: string;
  read: boolean;
}

@Injectable({ providedIn: 'root' })
export class NotificationService implements OnDestroy {
  private auth = inject(AuthService);
  private toast = inject(ToastService);
  private router = inject(Router);

  private hub: signalR.HubConnection | null = null;

  readonly notifications = signal<AppNotification[]>([]);
  readonly unreadCount = signal(0);

  connect(): void {
    if (this.hub?.state === signalR.HubConnectionState.Connected) return;

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5273/hubs/notifications', {
        accessTokenFactory: () => {
          // Берём токен из localStorage напрямую (signal может быть null если истёк)
          return localStorage.getItem('blog_access_token') ?? '';
        },
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.hub.on('ReceiveNotification', (notification: AppNotification) => {
      this.addNotification({ ...notification, read: false });
      this.showToast(notification);
    });

    this.hub.onreconnected(() => console.log('SignalR reconnected'));
    this.hub.onclose(() => console.log('SignalR disconnected'));

    this.hub.start().catch((err) => console.warn('SignalR connection failed:', err));
  }

  disconnect(): void {
    this.hub?.stop();
    this.hub = null;
  }

  markAllRead(): void {
    this.notifications.update((list) => list.map((n) => ({ ...n, read: true })));
    this.unreadCount.set(0);
  }

  markRead(id: string): void {
    this.notifications.update((list) => list.map((n) => (n.id === id ? { ...n, read: true } : n)));
    this.unreadCount.set(this.notifications().filter((n) => !n.read).length);
  }

  clearAll(): void {
    this.notifications.set([]);
    this.unreadCount.set(0);
  }

  ngOnDestroy(): void {
    this.disconnect();
  }

  private addNotification(n: AppNotification): void {
    this.notifications.update((list) => [n, ...list].slice(0, 50)); // макс 50
    this.unreadCount.update((c) => c + 1);
  }

  private showToast(n: AppNotification): void {
    const icon = this.getIcon(n.type);
    const msg = `${icon} ${n.message}`;

    if (n.postSlug) {
      // Клик по toast → переход на статью
      this.toast.info(msg);
    } else {
      this.toast.info(msg);
    }
  }

  private getIcon(type: string): string {
    switch (type) {
      case 'CommentReply':
        return '💬';
      case 'CommentReaction':
        return '❤️';
      case 'PostReaction':
        return '🔥';
      default:
        return '🔔';
    }
  }
}
