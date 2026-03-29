import { Component, inject, signal, computed, OnInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { UserProfile } from '../../core/models/auth.model';
import { HttpClient } from '@angular/common/http';
import { ChangePasswordModal } from '../../shared/components/change-password-modal/change-password-modal';
import {
  faCamera,
  faAt,
  faCalendar,
  faInfoCircle,
  faXmark,
  faUserCheck,
  faShieldVirus,
  faFloppyDisk,
  faSpinner,
  faExclamationTriangle,
  faThumbsUp,
  faKey,
  faShieldAlt,
} from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

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
  imports: [CommonModule, FormsModule, FontAwesomeModule, ChangePasswordModal],
  templateUrl: './profile.html',
})
export class Profile implements OnInit {
  private auth = inject(AuthService);
  private http = inject(HttpClient);

  @ViewChild('avatarInput') avatarInput!: ElementRef<HTMLInputElement>;

  // Icons
  faCamera = faCamera;
  faAt = faAt;
  faCalendar = faCalendar;
  faInfoCircle = faInfoCircle;
  faXmark = faXmark;
  faUserCheck = faUserCheck;
  faShieldVirus = faShieldVirus;
  faFloppyDisk = faFloppyDisk;
  faSpinner = faSpinner;
  faExclamationTriangle = faExclamationTriangle;
  faThumbsUp = faThumbsUp;
  faKey = faKey;
  faShieldAlt = faShieldAlt;

  profile = signal<UserProfile | null>(null);
  saveStatus = signal<SaveStatus>('idle');
  errorMsg = signal('');
  activeTab = signal<'info' | 'security'>('info');
  avatarPreview = signal<string | null>(null);
  avatarFile = signal<File | null>(null);
  showChangePassword = signal(false);

  fullAvatarUrl = computed(() => {
    const preview = this.avatarPreview();
    if (preview) return preview;
    const avatarPath = this.profile()?.avatarUrl;
    if (!avatarPath) return null;
    if (avatarPath.startsWith('http')) return avatarPath;
    return FILES_BASE_URL + avatarPath;
  });

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

  form: UpdateProfileForm = {
    firstName: '',
    lastName: '',
    userName: '',
    dateOfBirth: '',
  };

  // ── Avatar ─────────────────────────────────────────────────────
  openAvatarPicker(): void {
    this.avatarInput.nativeElement.click();
  }

  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

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
    const reader = new FileReader();
    reader.onload = (e) => this.avatarPreview.set(e.target?.result as string);
    reader.readAsDataURL(file);
  }

  clearAvatar(): void {
    this.avatarPreview.set(null);
    this.avatarFile.set(null);
    if (this.avatarInput) this.avatarInput.nativeElement.value = '';
  }

  // ── Save profile ───────────────────────────────────────────────
  save(): void {
    const userId = this.auth.user()?.id;
    if (!userId) return;

    this.saveStatus.set('saving');
    this.errorMsg.set('');

    const formData = new FormData();
    formData.append('firstName', this.form.firstName);
    formData.append('lastName', this.form.lastName);
    formData.append('userName', this.form.userName);
    formData.append('dateOfBirth', this.form.dateOfBirth);

    const avatar = this.avatarFile();
    if (avatar) formData.append('avatar', avatar, avatar.name);

    this.http.put(`${this.auth.API}/users/${userId}`, formData).subscribe({
      next: () => {
        this.saveStatus.set('success');
        this.clearAvatar();
        this.auth.fetchMe().subscribe({ next: (p) => this.profile.set(p) });
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
}
