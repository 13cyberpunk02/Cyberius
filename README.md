# Cyberius

> Полноценный блог-платформа для разработчиков на **C# .NET 10** + **Angular 21+**

![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet)
![Angular](https://img.shields.io/badge/Angular-21+-DD0031?style=flat-square&logo=angular)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=flat-square&logo=postgresql)
![MinIO](https://img.shields.io/badge/MinIO-S3-C72E49?style=flat-square&logo=minio)

---

## О проекте

Cyberius — это блог-платформа для публикации технических статей. Авторы могут писать статьи с богатым контентом (заголовки, параграфы, код, изображения, таблицы, цитаты), а читатели — комментировать, реагировать и сохранять статьи в закладки.

Проект построен на принципах **Clean Architecture** и разделён на независимые слои.

---

## Стек технологий

### Backend
| Технология | Версия | Назначение |
|---|---|---|
| .NET | 10 | Runtime |
| ASP.NET Core Minimal API | 10 | HTTP API |
| Entity Framework Core | 9 | ORM |
| PostgreSQL | 16 | База данных |
| MinIO | latest | Хранилище файлов (S3-совместимое) |
| ASP.NET Identity | — | Управление пользователями |
| SignalR | — | Real-time уведомления |
| JWT Bearer | — | Аутентификация |
| BCrypt.Net | — | Хэширование паролей |

### Frontend
| Технология | Версия | Назначение |
|---|---|---|
| Angular | 21+ | SPA фреймворк |
| TypeScript | 5+ | Язык |
| Tailwind CSS | v4 | Стилизация |
| Angular Signals | — | Реактивное состояние |
| @microsoft/signalr | — | Real-time уведомления |
| FontAwesome | 6 | Иконки |

---

## Архитектура

```
src/
├── Cyberius.Domain/          # Сущности, интерфейсы, бизнес-правила
├── Cyberius.Application/     # Use cases, сервисы, DTO
├── Cyberius.Infrastructure/  # EF Core, MinIO, Email, репозитории
├── Cyberius.Api/             # Minimal API endpoints, SignalR Hub
└── Cyberius-UI/
    └── src/app/
        ├── core/                 # Сервисы, модели, интерсепторы, гварды
        ├── shared/               # Переиспользуемые компоненты
        └── features/             # Страницы по фичам


```

### Слои и зависимости

```
Api → Application → Domain
Infrastructure → Domain
```

`Domain` не зависит ни от кого. `Application` знает только о `Domain`. `Infrastructure` и `Api` зависят от `Application`.

---

## Возможности

### Для читателей
- 📖 Чтение статей с богатым контентом (код с подсветкой синтаксиса, изображения, таблицы, цитаты, видео)
- 🔍 Полнотекстовый поиск через PostgreSQL `tsvector`
- 💬 Комментарии с вложенными ответами
- 👍 Реакции на статьи и комментарии (Like, Heart, Fire, Clap, Thinking)
- 🔖 Закладки (сохранённые статьи, хранятся в localStorage)
- 📡 RSS-лента (`/feed.xml`)
- 🔔 Real-time уведомления через SignalR (новые комментарии, реакции)

### Для авторов
- ✍️ Редактор статей с блоковой структурой контента
- 📝 Черновики — автосохранение в localStorage
- 📊 Статистика: просмотры за 30 дней, топ статей, реакции
- 🖼️ Загрузка изображений в MinIO
- ⌨️ Горячие клавиши: `Ctrl+S` — сохранить, `Ctrl+Enter` — опубликовать
- 🔒 Защита от случайного закрытия страницы при несохранённых изменениях

### Для администраторов
- 👥 Управление пользователями (смена ролей, блокировка, удаление)
- 🏷️ Управление категориями
- 🗑️ Удаление любых статей

### Безопасность
- JWT access token (15 мин) + Refresh token (7 дней, opaque)
- Проактивное обновление токенов через HTTP-интерцептор
- Rate limiting: публичные запросы, мутации, комментарии, реакции, просмотры
- Подтверждение email при регистрации
- Сброс пароля через email

---

## Быстрый старт

### Требования

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- [Docker](https://www.docker.com/) (для PostgreSQL и MinIO)
- [Angular CLI](https://angular.io/cli) `npm install -g @angular/cli`

### 1. Запуск инфраструктуры

```bash
docker run -d \
  --name cyberius-postgres \
  -e POSTGRES_USER=root \
  -e POSTGRES_PASSWORD=root123 \
  -e POSTGRES_DB=cyberius_db \
  -p 5432:5432 \
  postgres:16

docker run -d \
  --name cyberius-minio \
  -e MINIO_ROOT_USER=minioadmin \
  -e MINIO_ROOT_PASSWORD=minioadmin \
  -p 9000:9000 \
  -p 9001:9001 \
  minio/minio server /data --console-address ":9001"
```

### 2. Настройка бэкенда

Создай файл `src/Cyberius.Api/Environments/AppEnv.json`:

```json
{
  "DatabaseSettings": {
    "ConnectionString": "Host=localhost;Port=5432;Database=cyberius_db;Username=root;Password=root123"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-chars-here",
    "Issuer": "https://localhost:7071",
    "Audience": "http://localhost:4200",
    "AccessTokenLifetimeInMinutes": 15,
    "RefreshTokenLifetimeInDays": 7
  },
  "MinioSettings": {
    "Endpoint": "localhost:9000",
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin",
    "BucketName": "avatars",
    "UseSSL": false,
    "PublicBaseUrl": "http://localhost:5273"
  },
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true,
    "From": "your@gmail.com",
    "DisplayName": "Cyberius",
    "UserName": "your@gmail.com",
    "Password": "your-gmail-app-password"
  }
}
```

> **Gmail App Password**: Google Account → Security → 2-Step Verification → App passwords

### 3. Применение миграций

```bash
cd src/Cyberius.Api
dotnet ef database update --project ../Cyberius.Infrastructure
```

### 4. Запуск бэкенда

```bash
cd src/Cyberius.Api
dotnet run
```

API будет доступен по адресу `http://localhost:5273`

### 5. Запуск фронтенда

```bash
cd frontend
npm install
ng serve
```

Приложение будет доступно по адресу `http://localhost:4200`

---

## Структура API

### Аутентификация
| Метод | Путь | Описание |
|---|---|---|
| POST | `/api/auth/register` | Регистрация |
| POST | `/api/auth/login` | Вход |
| POST | `/api/auth/refresh-token` | Обновление токена |
| POST | `/api/auth/logout/{userId}` | Выход |
| POST | `/api/auth/forgot-password` | Запрос сброса пароля |
| POST | `/api/auth/reset-password` | Сброс пароля |
| GET | `/api/auth/confirm-email` | Подтверждение email |
| POST | `/api/auth/resend-confirmation` | Повторная отправка письма |

### Статьи
| Метод | Путь | Описание |
|---|---|---|
| GET | `/api/posts` | Список опубликованных |
| GET | `/api/posts/slug/{slug}` | Статья по slug |
| GET | `/api/posts/search?q=...` | Поиск |
| GET | `/api/posts/category/{id}` | По категории |
| GET | `/api/posts/{id}/related` | Похожие статьи |
| GET | `/api/posts/{id}/neighbors` | Предыдущая/следующая |
| POST | `/api/posts` | Создать статью `[auth]` |
| PUT | `/api/posts/{id}` | Обновить `[auth]` |
| POST | `/api/posts/{id}/publish` | Опубликовать `[auth]` |
| POST | `/api/posts/{id}/react/{type}` | Реакция `[auth]` |
| POST | `/api/posts/{id}/view` | Трекинг просмотра |
| DELETE | `/api/posts/{id}` | Удалить `[auth]` |

### Комментарии
| Метод | Путь | Описание |
|---|---|---|
| GET | `/api/comments/post/{postId}` | Комментарии к статье |
| POST | `/api/comments` | Написать комментарий `[auth]` |
| PUT | `/api/comments/{id}` | Редактировать `[auth]` |
| DELETE | `/api/comments/{id}` | Удалить `[auth]` |
| POST | `/api/comments/{id}/react/{type}` | Реакция `[auth]` |

### Прочее
| Метод | Путь | Описание |
|---|---|---|
| GET | `/api/categories` | Все категории |
| GET | `/api/stats/author/{id}` | Статистика автора `[auth]` |
| GET | `/api/users/{id}` | Публичный профиль |
| GET | `/feed.xml` | RSS лента |
| GET/POST | `/api/admin/users` | Управление пользователями `[admin]` |

### WebSocket
| Путь | Описание |
|---|---|
| `/hubs/notifications` | SignalR уведомления `[auth]` |

---

## Роли пользователей

| Роль | Возможности |
|---|---|
| `User` | Читать, комментировать, реагировать, писать статьи |
| `Manager` | Всё что User + управление пользователями |
| `Admin` | Всё что Manager + удаление пользователей и любых статей |

---

## Типы блоков контента

Статьи состоят из блоков — каждый блок имеет тип и контент:

| Тип | Описание |
|---|---|
| `Paragraph` | Текстовый абзац |
| `Heading1/2/3` | Заголовки уровней 1-3 |
| `Code` | Блок кода с подсветкой синтаксиса |
| `Image` | Изображение с подписью и лайтбоксом |
| `Quote` | Цитата |
| `Callout` | Выделенный блок (Info/Warning/Danger/Success) |
| `Divider` | Разделитель |
| `VideoEmbed` | Встроенное видео (YouTube/Vimeo) |
| `Table` | Таблица |

---

## Переменные окружения

Все настройки хранятся в `Cyberius.Api/Environments/AppEnv.json` и не попадают в Git (добавлен в `.gitignore`).

| Секция | Описание |
|---|---|
| `DatabaseSettings` | Подключение к PostgreSQL |
| `JwtSettings` | Параметры JWT токенов |
| `MinioSettings` | Подключение к MinIO |
| `EmailSettings` | SMTP для отправки писем |

---

## Разработка

### Создание миграции

```bash
dotnet ef migrations add MigrationName \
  --project src/Cyberius.Infrastructure \
  --startup-project src/Cyberius.Api
```

### Структура фронтенда

```
src/app/
├── core/
│   ├── models/          # TypeScript интерфейсы
│   ├── services/        # HTTP сервисы, AuthService, ThemeService
│   ├── guards/          # auth, guest, admin guards
│   └── interceptors/    # JWT interceptor с авто-refresh
├── shared/
│   └── components/      # Header, Footer, Toast, Modals, Confirm Dialog
└── features/
    ├── home/            # Главная страница
    ├── posts/           # Список, детальная, редактор, черновики
    ├── profile/         # Профиль с статистикой
    ├── users/           # Публичный профиль пользователя
    ├── admin/           # Управление категориями и пользователями
    ├── about/           # Страница "Обо мне"
    ├── saved/           # Сохранённые статьи
    └── auth/            # Сброс и подтверждение пароля
```

### Темы оформления

Приложение поддерживает тёмную и светлую темы. Переключение через кнопку в хедере. Тема сохраняется в `localStorage`.

---

## Лицензия

MIT
