import { LoginRequest, RegisterRequest, FormErrors } from '../models/auth.model';

// ── Helpers ────────────────────────────────────────────────────────────────

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;
const USERNAME_RE = /^[a-zA-Z0-9_.-]{3,32}$/;
// Минимум 8 символов, хотя бы 1 цифра, 1 буква
const PASSWORD_RE = /^(?=.*[A-Za-z])(?=.*\d).{8,}$/;

// Минимальный возраст — 13 лет
const MIN_AGE = 13;

function calcAge(dob: string): number {
  const birth = new Date(dob);
  const now = new Date();
  let age = now.getFullYear() - birth.getFullYear();
  const m = now.getMonth() - birth.getMonth();
  if (m < 0 || (m === 0 && now.getDate() < birth.getDate())) age--;
  return age;
}

// ── Login validation ───────────────────────────────────────────────────────

export function validateLogin(data: LoginRequest): FormErrors<LoginRequest> {
  const errors: FormErrors<LoginRequest> = {};

  if (!data.email.trim()) {
    errors.email = 'Введите email';
  } else if (!EMAIL_RE.test(data.email.trim())) {
    errors.email = 'Некорректный формат email';
  }

  if (!data.password) {
    errors.password = 'Введите пароль';
  } else if (data.password.length < 8) {
    errors.password = 'Пароль должен быть не менее 8 символов';
  }

  return errors;
}

// ── Register validation ────────────────────────────────────────────────────

export function validateRegister(data: RegisterRequest): FormErrors<RegisterRequest> {
  const errors: FormErrors<RegisterRequest> = {};

  // Email
  if (!data.email.trim()) {
    errors.email = 'Введите email';
  } else if (!EMAIL_RE.test(data.email.trim())) {
    errors.email = 'Некорректный формат email';
  }

  // Username
  if (!data.userName.trim()) {
    errors.userName = 'Введите имя пользователя';
  } else if (!USERNAME_RE.test(data.userName.trim())) {
    errors.userName = 'От 3 до 32 символов: буквы, цифры, _ . -';
  }

  // First name
  if (!data.firstName.trim()) {
    errors.firstName = 'Введите имя';
  } else if (data.firstName.trim().length < 2) {
    errors.firstName = 'Имя должно быть не менее 2 символов';
  }

  // Last name
  if (!data.lastName.trim()) {
    errors.lastName = 'Введите фамилию';
  } else if (data.lastName.trim().length < 2) {
    errors.lastName = 'Фамилия должна быть не менее 2 символов';
  }

  // Password
  if (!data.password) {
    errors.password = 'Введите пароль';
  } else if (!PASSWORD_RE.test(data.password)) {
    errors.password = 'Минимум 8 символов, буква и цифра';
  }

  // Confirm password
  if (!data.confirmPassword) {
    errors.confirmPassword = 'Подтвердите пароль';
  } else if (data.password !== data.confirmPassword) {
    errors.confirmPassword = 'Пароли не совпадают';
  }

  // Date of birth
  if (!data.dateOfBirth) {
    errors.dateOfBirth = 'Введите дату рождения';
  } else {
    const age = calcAge(data.dateOfBirth);
    if (isNaN(age) || age < 0) {
      errors.dateOfBirth = 'Некорректная дата';
    } else if (age < MIN_AGE) {
      errors.dateOfBirth = `Минимальный возраст — ${MIN_AGE} лет`;
    } else if (age > 120) {
      errors.dateOfBirth = 'Некорректная дата рождения';
    }
  }

  return errors;
}

export function isValid<T>(errors: FormErrors<T>): boolean {
  return Object.keys(errors).length === 0;
}
