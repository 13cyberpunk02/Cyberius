export function getApiError(err: any, fallback = 'Произошла ошибка'): string {
  // ProblemDetails.detail — наше русское сообщение
  if (err?.error?.detail) return err.error.detail;

  // ValidationProblemDetails — массив ошибок валидации
  if (err?.error?.errors) {
    const errors = err.error.errors;
    if (Array.isArray(errors)) return errors.join(', ');
    // errors — объект { field: [messages] }
    return Object.values(errors).flat().join(', ');
  }

  // ProblemDetails.title — код ошибки как запасной вариант
  if (err?.error?.title) return err.error.title;

  // HTTP статус коды без тела
  if (err?.status === 401) return 'Необходима авторизация';
  if (err?.status === 403) return 'Доступ запрещён';
  if (err?.status === 404) return 'Не найдено';
  if (err?.status === 429) return 'Слишком много запросов. Попробуйте позже';
  if (err?.status === 0) return 'Сервер недоступен. Проверьте подключение';

  return fallback;
}
