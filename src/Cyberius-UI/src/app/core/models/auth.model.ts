export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  userName: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
}

export interface RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
}

// Полный профиль пользователя с /me endpoint
export interface UserProfile {
  userId: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  avatarUrl: string | null;
  dateOfBirth: string;
  joinedDate: string;
  roles: string[] | null;
}

// Краткая модель для auth state (из JWT)
export interface User {
  id: string;
  email: string;
  userName?: string;
}

export interface AuthState {
  user: User | null;
  profile: UserProfile | null; // полный профиль
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
}

// ── Validation ──────────────────────────────────────────────────
export interface FieldError {
  message: string;
}

export type FormErrors<T> = Partial<Record<keyof T, string>>;
