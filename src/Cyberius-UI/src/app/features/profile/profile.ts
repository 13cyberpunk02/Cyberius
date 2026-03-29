import { Component, inject, signal, computed, OnInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { UserProfile } from '../../core/models/auth.model';
import { HttpClient } from '@angular/common/http';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faAlarmClock,
  faAt,
  faCalendar,
  faCamera,
  faCameraAlt,
  faExclamationTriangle,
  faFloppyDisk,
  faInfo,
  faInfoCircle,
  faKey,
  faShield,
  faShieldAlt,
  faShieldHalved,
  faShieldHeart,
  faShieldVirus,
  faSpinner,
  faThumbsUp,
  faUserCheck,
  faXmark,
} from '@fortawesome/free-solid-svg-icons';

interface UpdateProfileForm {
  firstName: string;
  lastName: string;
  userName: string;
  dateOfBirth: string;
}

type SaveStatus = 'idle' | 'saving' | 'success' | 'error';

const FILES_BASE_URL = 'http://localhost:5273/api/files/';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, FaIconComponent],
  templateUrl: './profile.html',
})
export class Profile implements OnInit {
  private auth = inject(AuthService);
  private http = inject(HttpClient);

  @ViewChild('avatarInput') avatarInput!: ElementRef<HTMLInputElement>;

  profile = signal<UserProfile | null>(null);
  saveStatus = signal<SaveStatus>('idle');
  errorMsg = signal('');
  activeTab = signal<'info' | 'security'>('info');
  avatarPreview = signal<string | null>(null);
  avatarFile = signal<File | null>(null);

  // Полный URL аватара — берём из превью (новый) или строим из profile.avatarUrl
  fullAvatarUrl = computed(() => {
    const preview = this.avatarPreview();
    if (preview) return preview;

    const avatarPath = this.profile()?.avatarUrl;
    if (!avatarPath) return null;

    // Если уже полный URL — возвращаем как есть
    if (avatarPath.startsWith('http')) return avatarPath;

    return FILES_BASE_URL + avatarPath;
  });

  form: UpdateProfileForm = {
    firstName: '',
    lastName: '',
    userName: '',
    dateOfBirth: '',
  };

  get initials(): string {
    return this.auth.initials();
  }
  get displayName(): string {
    return this.auth.displayName();
  }

  ngOnInit(): void {
    const cached = this.auth.profile();
    if (cached) {
      this.profile.set(cached);
      this.fillForm(cached);
    }

    this.auth.fetchMe().subscribe({
      next: (p) => {
        this.profile.set(p);
        this.fillForm(p);
      },
      error: () => {},
    });
  }

  private fillForm(p: UserProfile): void {
    this.form = {
      firstName: p.firstName,
      lastName: p.lastName,
      userName: p.username,
      dateOfBirth: p.dateOfBirth.split('T')[0],
    };
  }

  // ── Avatar pick ─────────────────────────────────────────────────
  openAvatarPicker(): void {
    this.avatarInput.nativeElement.click();
  }

  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    // Валидация: только изображения, не более 5 MB
    if (!file.type.startsWith('image/')) {
      this.errorMsg.set('Выберите изображение (jpg, png, webp)');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.errorMsg.set('Размер файла не должен превышать 5 МБ');
      return;
    }

    this.errorMsg.set('');
    this.avatarFile.set(file);

    // Показываем превью
    const reader = new FileReader();
    reader.onload = (e) => this.avatarPreview.set(e.target?.result as string);
    reader.readAsDataURL(file);
  }

  clearAvatar(): void {
    this.avatarPreview.set(null);
    this.avatarFile.set(null);
    if (this.avatarInput) this.avatarInput.nativeElement.value = '';
  }

  // ── Save ────────────────────────────────────────────────────────
  save(): void {
    const userId = this.auth.user()?.id;
    if (!userId) return;

    this.saveStatus.set('saving');
    this.errorMsg.set('');

    // Собираем multipart/form-data
    const formData = new FormData();
    formData.append('firstName', this.form.firstName);
    formData.append('lastName', this.form.lastName);
    formData.append('userName', this.form.userName);
    formData.append('dateOfBirth', this.form.dateOfBirth);

    // Файл добавляем только если выбран
    const avatar = this.avatarFile();
    if (avatar) {
      formData.append('avatar', avatar, avatar.name);
    }

    this.http.put(`${this.auth.API}/users/${userId}`, formData).subscribe({
      next: () => {
        this.saveStatus.set('success');
        this.clearAvatar();
        // Обновляем профиль с сервера чтобы подтянуть новый avatarUrl
        this.auth.fetchMe().subscribe({
          next: (p) => this.profile.set(p),
        });
        setTimeout(() => this.saveStatus.set('idle'), 3000);
      },
      error: (err) => {
        this.saveStatus.set('error');
        this.errorMsg.set(
          err?.error?.message ?? err?.error?.title ?? 'Не удалось сохранить изменения',
        );
      },
    });
  }

  formatDate(dateStr: string): string {
    return new Intl.DateTimeFormat('ru-RU', {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
    }).format(new Date(dateStr));
  }

  protected readonly faCamera = faCamera;
  protected readonly faAt = faAt;
  protected readonly faCalendar = faCalendar;
  protected readonly faInfoCircle = faInfoCircle;
  protected readonly faXmark = faXmark;
  protected readonly faExclamationTriangle = faExclamationTriangle;
  protected readonly faThumbsUp = faThumbsUp;
  protected readonly faSpinner = faSpinner;
  protected readonly faKey = faKey;
  protected readonly faShieldAlt = faShieldAlt;
  protected readonly faFloppyDisk = faFloppyDisk;
  protected readonly faUserCheck = faUserCheck;
  protected readonly faShieldVirus = faShieldVirus;
}
