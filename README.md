# Real-time мессенджер на ASP.NET Core + SignalR

Современное клиент-серверное приложение для обмена мгновенными сообщениями, изображениями и голосовыми записями.  
Бэкенд построен на **ASP.NET Core 8** с **SignalR**, фронтенд — чистый **HTML/CSS/JS**, всё разворачивается через **Docker Compose**.

## Возможности

- **Регистрация и вход** с хэшированием паролей (BCrypt) и JWT-аутентификацией
- **Обмен сообщениями в реальном времени** (SignalR)
- **Отправка изображений и голосовых сообщений** с кастомным аудиоплеером
- **Поиск собеседников** по никнейму с автодополнением
- **Список активных диалогов** с последним сообщением
- **Ответ на сообщение** с цитированием и предпросмотром вложений
- **Светлая/тёмная тема**, настройка фона чата
- **Изменение никнейма** через панель настроек
- **Адаптивный дизайн** для мобильных устройств
- **Полная контейнеризация** (PostgreSQL, Redis, Nginx, backend)

## Стек технологий

- Backend – C# 12, ASP.NET Core 8, SignalR, Entity Framework Core
- Frontend – HTML5, CSS3, JavaScript (ES6+)
- База данных – PostgreSQL 16
- Кэш – Redis 7 (зарезервирован)
- Прокси-сервер – Nginx
- Валидация – FluentValidation
- Аутентификация – JWT Bearer + BCrypt
- Контейнеризация – Docker, Docker Compose

## Требования

- [Docker](https://docs.docker.com/get-docker/) и [Docker Compose](https://docs.docker.com/compose/install/)
- (опционально) .NET 8 SDK для разработки и миграций

## Быстрый старт

1. **Клонируйте репозиторий**
   ```bash
   git clone https://github.com/lil2faced/Messenger.git
   cd MessengerApp
2. **Запустите контейнеры**
   ```bash
   docker compose up -d --build
3. **Приложение доступно на localhost:80 для http и localhost:443 для https**
