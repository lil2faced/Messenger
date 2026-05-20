// ============================
// Глобальные утилиты
// ============================
const $ = (sel) => document.querySelector(sel);
const $$ = (sel) => document.querySelectorAll(sel);

function escapeHtml(text) {
  if (!text) return '';
  const div = document.createElement('div');
  div.textContent = text;
  return div.innerHTML;
}

function formatTime(dateString) {
  if (!dateString) return '';
  const date = new Date(dateString);
  const now = new Date();
  const isToday = date.toDateString() === now.toDateString();
  if (isToday) return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  return date.toLocaleDateString([], { day: '2-digit', month: '2-digit', year: '2-digit' }) + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

// Нормализация ID для сравнения
function normalizeId(id) {
  if (!id) return '';
  return String(id).replace(/-/g, '').toLowerCase();
}

// ============================
// API-клиент
// ============================
class ApiClient {
  constructor() {
    this.token = localStorage.getItem('token');
  }

  setToken(token) {
    this.token = token;
    localStorage.setItem('token', token);
  }

  clearToken() {
    this.token = null;
    localStorage.removeItem('token');
  }

  async request(url, method = 'GET', body = null, isFormData = false) {
    const headers = {};
    if (this.token) headers['Authorization'] = `Bearer ${this.token}`;
    if (!isFormData) headers['Content-Type'] = 'application/json';

    const options = { method, headers };
    if (body) options.body = isFormData ? body : JSON.stringify(body);

    const response = await fetch(url, options);
    if (!response.ok) {
      const err = await response.json().catch(() => ({ error: 'Ошибка сети' }));
      throw new Error(err.error || err.errors?.join(', ') || 'Ошибка запроса');
    }
    return response.json();
  }

  async login(loginOrEmail, password) {
    const res = await this.request('/api/auth/login', 'POST', { loginOrEmail, password });
    this.setToken(res.token);
    return this.decodeToken(res.token);
  }

  async register(login, email, nickname, password) {
    await this.request('/api/auth/register', 'POST', { login, email, nickname, password });
  }

  decodeToken(token) {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return {
      id: payload.nameid,
      nickname: payload.unique_name
    };
  }

  async searchUsers(query) {
    return this.request(`/api/users/search?query=${encodeURIComponent(query)}`);
  }

  async getContacts() {
    return this.request('/api/messages/contacts');
  }

  async getConversation(otherUserId, skip = 0, take = 100) {
    return this.request(`/api/messages/conversation/${otherUserId}?skip=${skip}&take=${take}`);
  }

  async uploadFile(file, type) {
    const formData = new FormData();
    formData.append('file', file);
    return this.request(`/api/messages/upload?type=${type}`, 'POST', formData, true);
  }

  async updateNickname(nickname) {
    return this.request('/api/users/profile', 'PUT', { nickname });
  }
}

// ============================
// SignalR-клиент
// ============================
class SignalRClient {
  constructor(apiClient) {
    this.api = apiClient;
    this.connection = null;
    this.onReceiveMessage = null;
    this.onMessageSent = null;
    this.onNewContact = null;
  }

  async start() {
    if (this.connection) {
      await this.connection.stop();
    }
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/chat", { accessTokenFactory: () => this.api.token })
      .withAutomaticReconnect()
      .build();

    this.connection.on("ReceiveMessage", (message) => {
      if (this.onReceiveMessage) this.onReceiveMessage(message);
    });
    this.connection.on("MessageSent", (message) => {
      if (this.onMessageSent) this.onMessageSent(message);
    });
    this.connection.on("NewContact", (contact) => {
      if (this.onNewContact) this.onNewContact(contact);
    });
    this.connection.on("Error", (error) => {
      alert(error.message);
    });

    try {
      await this.connection.start();
      console.log("SignalR Connected");
    } catch (err) {
      console.error("SignalR Connection Error:", err);
      throw err;
    }
  }

  async sendMessage(recipientId, text, repliedToMessageId, attachments) {
    await this.connection.invoke("SendMessage", recipientId, text || null, repliedToMessageId || null, attachments || null);
  }
}

// ============================
// UI-менеджер
// ============================
class MessengerUI {
  constructor(api, signalR) {
    this.api = api;
    this.signalR = signalR;
    this.currentUser = null;
    this.activeChatUserId = null;
    this.activeChatNickname = null;
    this.replyToMessageId = null;

    // DOM элементы
    this.authScreen = $('#auth-screen');
    this.chatScreen = $('#chat-screen');
    this.loginForm = $('#login-form');
    this.registerForm = $('#register-form');
    this.loginError = $('#login-error');
    this.regError = $('#reg-error');
    this.logoutBtn = $('#logout-btn');
    this.currentUserNickname = $('#current-user-nickname');
    this.contactList = $('#contact-list');
    this.searchInput = $('#search-input');
    this.searchResults = $('#search-results');
    this.messagesContainer = $('#messages-container');
    this.chatPartnerName = $('#chat-partner-name');
    this.messageInput = $('#message-input');
    this.sendBtn = $('#send-btn');
    this.fileInput = $('#file-input');
    this.attachBtn = $('#attach-btn');
    this.voiceRecordBtn = $('#voice-record-btn');
    this.replyPreview = $('#reply-preview');
    this.replyText = $('#reply-text');
    this.replyAttachmentPreview = $('#reply-attachment-preview');
    this.cancelReplyBtn = $('#cancel-reply');
    this.backBtn = $('#back-to-contacts');
    this.themeToggle = $('#theme-toggle');
    this.imageModal = $('#image-modal');
    this.fullImage = $('#full-image');
    this.chatHeader = $('#chat-header');
    this.noChatPlaceholder = $('#no-chat-placeholder');
    this.messageInputArea = $('#message-input-area');

    // Настройки
    this.settingsBtn = $('#settings-btn');
    this.settingsModal = $('#settings-modal');
    this.newNicknameInput = $('#new-nickname-input');
    this.saveNicknameBtn = $('#save-nickname-btn');
    this.nicknameError = $('#nickname-error');
    this.closeSettingsBtn = $('#close-settings-btn');
    this.customBgColor = $('#custom-bg-color');

    this.init();
  }

  init() {
    // Переключение темы
    this.themeToggle.addEventListener('click', () => {
      document.body.classList.toggle('dark-theme');
      localStorage.setItem('theme', document.body.classList.contains('dark-theme') ? 'dark' : 'light');
    });
    if (localStorage.getItem('theme') === 'dark') {
      document.body.classList.add('dark-theme');
    }

    // Табы авторизации
    $$('.auth-tab').forEach(tab => {
      tab.addEventListener('click', () => this.switchAuthTab(tab.dataset.tab));
    });

    this.loginForm.addEventListener('submit', (e) => this.handleLogin(e));
    this.registerForm.addEventListener('submit', (e) => this.handleRegister(e));

    this.logoutBtn.addEventListener('click', () => this.logout());

    // Поиск
    let searchTimeout;
    this.searchInput.addEventListener('input', () => {
      clearTimeout(searchTimeout);
      searchTimeout = setTimeout(() => this.searchUsers(), 300);
    });

    // Отправка сообщения
    this.sendBtn.addEventListener('click', () => this.sendMessage());
    this.messageInput.addEventListener('keydown', (e) => {
      if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        this.sendMessage();
      }
    });
    this.attachBtn.addEventListener('click', () => this.fileInput.click());
    this.fileInput.addEventListener('change', () => this.sendMessage());

    this.voiceRecordBtn.addEventListener('click', () => this.toggleVoiceRecording());

    this.cancelReplyBtn.addEventListener('click', () => this.cancelReply());

    this.backBtn.addEventListener('click', () => {
      if (window.innerWidth <= 768) this.openSidebar();
    });

    // Ресайз
    this.initResize();

    // Просмотр изображений
    this.messagesContainer.addEventListener('click', (e) => {
      const img = e.target.closest('.attachment-img');
      if (img) {
        e.stopPropagation();
        this.openImageViewer(img.src);
      }
    });

    this.imageModal.addEventListener('click', (e) => {
      if (e.target === this.imageModal || e.target.classList.contains('image-modal-close')) {
        this.closeImageViewer();
      }
    });

    // Настройки
    this.settingsBtn.addEventListener('click', () => this.openSettings());
    this.closeSettingsBtn.addEventListener('click', () => this.closeSettings());
    this.saveNicknameBtn.addEventListener('click', () => this.saveNickname());
    this.customBgColor.addEventListener('input', () => this.changeBgColor(this.customBgColor.value));
    document.querySelectorAll('.bg-option').forEach(el => {
      el.addEventListener('click', () => this.changeBgColor(el.dataset.bg));
    });

    // Применяем сохранённый фон чата
    const savedBg = localStorage.getItem('chatBg');
    if (savedBg) {
      document.documentElement.style.setProperty('--chat-bg', savedBg);
      this.customBgColor.value = savedBg;
    }

    // SignalR обработчики
    this.signalR.onReceiveMessage = (msg) => this.handleIncomingMessage(msg);
    this.signalR.onMessageSent = (msg) => this.handleSentMessage(msg);
    this.signalR.onNewContact = (contact) => this.addOrUpdateContact(contact);

    this.checkAuth();
  }

  // ==================== АВТОРИЗАЦИЯ ====================
  checkAuth() {
    const token = this.api.token;
    if (token) {
      try {
        this.currentUser = this.api.decodeToken(token);
        this.showChat();
        this.loadContacts();
        this.signalR.start().catch(() => this.showAuth());
      } catch (e) {
        this.api.clearToken();
        this.showAuth();
      }
    } else {
      this.showAuth();
    }
  }

  showAuth() {
    this.authScreen.style.display = 'flex';
    this.chatScreen.style.display = 'none';
  }

  showChat() {
    this.authScreen.style.display = 'none';
    this.chatScreen.style.display = 'flex';
    if (this.currentUser) {
      this.currentUserNickname.textContent = this.currentUser.nickname;
    }
    this.setNoChatState();
    if (this.currentUser) {
      this.newNicknameInput.value = this.currentUser.nickname;
    }
  }

  switchAuthTab(tab) {
    $$('.auth-tab').forEach(t => t.classList.remove('active'));
    $$('.auth-form').forEach(f => f.classList.remove('active'));
    document.querySelector(`.auth-tab[data-tab="${tab}"]`).classList.add('active');
    document.getElementById(`${tab}-form`).classList.add('active');
  }

  async handleLogin(e) {
    e.preventDefault();
    this.loginError.textContent = '';
    try {
      this.currentUser = await this.api.login(
        $('#login-username').value,
        $('#login-password').value
      );
      this.showChat();
      await this.signalR.start();
      this.loadContacts();
    } catch (err) {
      this.loginError.textContent = err.message;
    }
  }

  async handleRegister(e) {
    e.preventDefault();
    this.regError.textContent = '';
    try {
      await this.api.register(
        $('#reg-login').value,
        $('#reg-email').value,
        $('#reg-nickname').value,
        $('#reg-password').value
      );
      alert('Регистрация успешна. Теперь войдите.');
      this.switchAuthTab('login');
      $('#login-username').value = $('#reg-login').value;
      $('#login-password').value = '';
    } catch (err) {
      this.regError.textContent = err.message;
    }
  }

  logout() {
    if (this.signalR.connection) {
      this.signalR.connection.stop();
    }
    this.api.clearToken();
    this.currentUser = null;
    this.activeChatUserId = null;
    this.activeChatNickname = null;
    this.replyToMessageId = null;
    this.messagesContainer.innerHTML = '';
    this.setNoChatState();
    this.showAuth();
  }

  // ==================== СОСТОЯНИЯ ЧАТА ====================
  setNoChatState() {
    this.chatHeader.style.display = 'none';
    this.messagesContainer.style.display = 'none';
    this.noChatPlaceholder.style.display = 'flex';
    this.messageInputArea.style.display = 'none';
    this.replyPreview.style.display = 'none';
  }

  setActiveChatState() {
    this.chatHeader.style.display = 'flex';
    this.messagesContainer.style.display = 'flex';
    this.noChatPlaceholder.style.display = 'none';
    this.messageInputArea.style.display = 'flex';
    this.replyPreview.style.display = 'none';
  }

  // ==================== КОНТАКТЫ ====================
  async loadContacts() {
    try {
      const contacts = await this.api.getContacts();
      this.contactList.innerHTML = '';
      contacts.forEach(c => this.addContactToDOM(c));
    } catch (err) {
      console.error('Ошибка загрузки контактов:', err);
    }
  }

  addContactToDOM(contact) {
    const existing = document.querySelector(`.contact-item[data-user-id="${contact.userId}"]`);
    if (existing) {
      existing.querySelector('.contact-last-message').textContent = contact.lastMessage || '';
      existing.querySelector('.contact-time').textContent = formatTime(contact.lastMessageTime);
      return;
    }
    const item = document.createElement('div');
    item.className = 'contact-item';
    item.dataset.userId = contact.userId;
    item.innerHTML = `
      <div class="contact-avatar">${contact.nickname.charAt(0).toUpperCase()}</div>
      <div class="contact-info">
        <div class="contact-nickname">${escapeHtml(contact.nickname)}</div>
        <div class="contact-last-message">${escapeHtml(contact.lastMessage || '')}</div>
      </div>
      <div class="contact-time">${formatTime(contact.lastMessageTime)}</div>
    `;
    item.addEventListener('click', () => this.openChat(contact.userId, contact.nickname));
    this.contactList.prepend(item);
  }

  addOrUpdateContact(contact) {
    const existing = document.querySelector(`.contact-item[data-user-id="${contact.userId}"]`);
    if (existing) {
      existing.querySelector('.contact-last-message').textContent = contact.lastMessage || '';
      existing.querySelector('.contact-time').textContent = formatTime(contact.lastMessageTime);
      this.contactList.prepend(existing);
    } else {
      this.addContactToDOM(contact);
    }
  }

  openChat(userId, nickname) {
    this.activeChatUserId = userId;
    this.activeChatNickname = nickname;
    this.chatPartnerName.textContent = nickname;
    this.messagesContainer.innerHTML = '';
    this.replyToMessageId = null;
    this.setActiveChatState();
    this.loadMessages();
    $$('.contact-item').forEach(el => el.classList.remove('active'));
    const contactEl = document.querySelector(`.contact-item[data-user-id="${userId}"]`);
    if (contactEl) contactEl.classList.add('active');
    if (window.innerWidth <= 768) this.closeSidebar();
  }

  openSidebar() {
    $('#sidebar').classList.add('open');
  }

  closeSidebar() {
    $('#sidebar').classList.remove('open');
  }

  // ==================== СООБЩЕНИЯ ====================
  async loadMessages() {
    if (!this.activeChatUserId) return;
    try {
      const msgs = await this.api.getConversation(this.activeChatUserId);
      this.messagesContainer.innerHTML = '';
      msgs.forEach(msg => {
        const side = msg.isMine ? 'sent' : 'received';
        this.appendMessage(msg, side);
      });
      this.scrollToBottom();
    } catch (err) {
      console.error(err);
    }
  }

  appendMessage(msg, side) {
    const div = document.createElement('div');
    div.className = `message ${side}`;
    div.dataset.messageId = msg.id;
    let content = '';

    // Ответ
    if (msg.repliedToMessageId) {
      let replyHtml = `<div class="reply-quote">↩ ${escapeHtml(msg.repliedToSenderNickname)}: ${escapeHtml(msg.repliedToText || '')}`;
      if (msg.repliedToAttachments && msg.repliedToAttachments.length > 0) {
        msg.repliedToAttachments.forEach(att => {
          if (att.type === 'image') {
            replyHtml += `<img src="${att.fileUrl}" class="reply-attachment-thumb">`;
          }
        });
      }
      replyHtml += `</div>`;
      content += replyHtml;
    }

    // Текст
    if (msg.text) {
      content += `<div class="message-text">${escapeHtml(msg.text)}</div>`;
    }

    // Вложения
    if (msg.attachments && msg.attachments.length) {
      msg.attachments.forEach(att => {
        if (att.type === 'image') {
          content += `<img src="${att.fileUrl}" class="attachment-img" alt="Изображение" loading="lazy">`;
        } else if (att.type === 'voice') {
          content += `<div class="voice-player-wrapper" data-src="${att.fileUrl}"></div>`;
        }
      });
    }

    // Время
    content += `<div class="message-time">${formatTime(msg.createdAt)}</div>`;

    const statusIcon = side === 'sent' ? ` <span class="message-status">${this.getStatusIcon(msg.status)}</span>` : '';
    div.innerHTML = content + statusIcon;

    // Инициализируем плеер для голосовых
    div.querySelectorAll('.voice-player-wrapper').forEach(wrapper => {
      this.initVoicePlayer(wrapper, wrapper.dataset.src);
    });

    div.addEventListener('click', (e) => {
      if (e.target.closest('.voice-player-wrapper') || e.target.closest('.attachment-img')) return;
      if (this.replyToMessageId === msg.id) {
        this.cancelReply();
      } else {
        this.startReply(msg.id, msg.senderNickname, msg.text || 'Вложение', msg.attachments);
      }
    });

    this.messagesContainer.appendChild(div);
  }

  // Кастомный аудиоплеер с range для перемотки
  initVoicePlayer(wrapper, src) {
    const audio = new Audio(src);
    const playerDiv = document.createElement('div');
    playerDiv.className = 'voice-player';
    playerDiv.innerHTML = `
      <button class="play-btn">▶️</button>
      <input type="range" class="progress-range" min="0" max="100" value="0" step="0.1">
      <span class="time">0:00</span>
      <div class="volume-control">
        <span>🔊</span>
        <input type="range" class="volume-range" min="0" max="1" step="0.1" value="1">
      </div>
    `;
    wrapper.innerHTML = '';
    wrapper.appendChild(playerDiv);

    const playBtn = playerDiv.querySelector('.play-btn');
    const progressRange = playerDiv.querySelector('.progress-range');
    const timeDisplay = playerDiv.querySelector('.time');
    const volumeRange = playerDiv.querySelector('.volume-range');

    const updateTimeDisplay = () => {
      const current = audio.currentTime || 0;
      const duration = audio.duration || 0;
      const currentMin = Math.floor(current / 60);
      const currentSec = Math.floor(current % 60).toString().padStart(2, '0');
      const durationMin = Math.floor(duration / 60);
      const durationSec = Math.floor(duration % 60).toString().padStart(2, '0');
      timeDisplay.textContent = `${currentMin}:${currentSec} / ${durationMin}:${durationSec}`;
    };

    audio.addEventListener('loadedmetadata', () => {
      progressRange.max = audio.duration;
      updateTimeDisplay();
    });

    audio.addEventListener('timeupdate', () => {
      progressRange.value = audio.currentTime;
      updateTimeDisplay();
    });

    progressRange.addEventListener('input', () => {
      audio.currentTime = progressRange.value;
      updateTimeDisplay();
    });

    playBtn.addEventListener('click', () => {
      if (audio.paused) {
        audio.play();
        playBtn.textContent = '⏸️';
      } else {
        audio.pause();
        playBtn.textContent = '▶️';
      }
    });

    volumeRange.addEventListener('input', () => {
      audio.volume = volumeRange.value;
    });

    audio.addEventListener('ended', () => {
      playBtn.textContent = '▶️';
      progressRange.value = 0;
    });
  }

  getStatusIcon(status) {
    switch(status) {
      case 'Sent': return '✓';
      case 'Delivered': return '✓✓';
      case 'Read': return '✓✓';
      default: return '';
    }
  }

  startReply(messageId, sender, text, attachments) {
    this.replyToMessageId = messageId;
    this.replyText.textContent = `${sender}: ${text ? text.substring(0, 50) : ''}`;
    this.replyAttachmentPreview.innerHTML = '';
    if (attachments && attachments.length) {
      attachments.forEach(att => {
        if (att.type === 'image') {
          const img = document.createElement('img');
          img.src = att.fileUrl;
          img.className = 'reply-attachment-thumb';
          this.replyAttachmentPreview.appendChild(img);
        }
      });
    }
    this.replyPreview.style.display = 'flex';
  }

  cancelReply() {
    this.replyToMessageId = null;
    this.replyPreview.style.display = 'none';
    this.replyAttachmentPreview.innerHTML = '';
  }

  async sendMessage() {
    const text = this.messageInput.value.trim();
    if (!text && !this.fileInput.files.length) return;
    if (!this.activeChatUserId) return;

    let attachments = [];
    if (this.fileInput.files.length) {
      for (let file of this.fileInput.files) {
        const type = file.type.startsWith('image/') ? 'image' : 'voice';
        const uploadRes = await this.api.uploadFile(file, type);
        attachments.push({ fileUrl: uploadRes.url, type });
      }
      this.fileInput.value = '';
    }

    // Оптимистичное добавление сообщения в UI
    const tempId = 'temp-' + Date.now();
    const optimisticMsg = {
      id: tempId,
      senderId: this.currentUser.id,
      senderNickname: this.currentUser.nickname,
      text: text || null,
      repliedToMessageId: this.replyToMessageId,
      repliedToText: null,
      repliedToSenderNickname: null,
      repliedToAttachments: [],
      status: 'Sent',
      createdAt: new Date().toISOString(),
      attachments: attachments,
      isMine: true
    };

    // Добавляем ответ-превью, если нужно
    if (this.replyToMessageId) {
      const repliedMsg = document.querySelector(`.message[data-message-id="${this.replyToMessageId}"]`);
      if (repliedMsg) {
        const repliedText = repliedMsg.querySelector('.message-text')?.textContent || '';
        const repliedSender = repliedMsg.closest('.message')?.classList.contains('sent') ? this.currentUser.nickname : this.activeChatNickname;
        optimisticMsg.repliedToText = repliedText;
        optimisticMsg.repliedToSenderNickname = repliedSender;
      }
    }

    this.appendMessage(optimisticMsg, 'sent');
    this.messageInput.value = '';
    this.cancelReply();
    this.scrollToBottom();

    // Отправка через SignalR
    try {
      await this.signalR.sendMessage(
        this.activeChatUserId,
        text || null,
        this.replyToMessageId,
        attachments.length ? attachments : null
      );
      // После успешной отправки сервер пришлет MessageSent, где мы обновим статус
    } catch (err) {
      // При ошибке помечаем сообщение как неотправленное (пока просто алерт)
      alert('Ошибка отправки: ' + err.message);
      const tempElement = document.querySelector(`.message[data-message-id="${tempId}"]`);
      if (tempElement) {
        tempElement.querySelector('.message-status').textContent = '⚠️';
      }
    }
  }

  // ================== ОБРАБОТЧИКИ REAL-TIME СООБЩЕНИЙ ==================
  handleIncomingMessage(msg) {
    // Обновляем контакт
    this.addOrUpdateContact({
      userId: msg.senderId,
      nickname: msg.senderNickname,
      lastMessage: msg.text,
      lastMessageTime: msg.createdAt
    });

    // Если чат с отправителем открыт, добавляем сообщение
    if (this.activeChatUserId && normalizeId(this.activeChatUserId) === normalizeId(msg.senderId)) {
      this.appendMessage(msg, 'received');
      this.scrollToBottom();
    }
  }

  handleSentMessage(msg) {
    // Находим временное сообщение и обновляем его ID и статус
    const tempElements = document.querySelectorAll(`.message[data-message-id^="temp-"]`);
    for (const el of tempElements) {
      const msgText = el.querySelector('.message-text')?.textContent;
      if (msgText === msg.text || (msgText === '' && msg.text === null)) {
        // Заменяем временный ID на реальный
        el.dataset.messageId = msg.id;
        // Обновляем статус
        const statusEl = el.querySelector('.message-status');
        if (statusEl) {
          statusEl.textContent = this.getStatusIcon(msg.status);
        }
        // Обновляем время, если нужно
        const timeEl = el.querySelector('.message-time');
        if (timeEl) {
          timeEl.textContent = formatTime(msg.createdAt);
        }
        break;
      }
    }
    // Обновляем контакт получателя
    this.addOrUpdateContact({
      userId: this.activeChatUserId,
      nickname: this.activeChatNickname,
      lastMessage: msg.text,
      lastMessageTime: msg.createdAt
    });
  }

  scrollToBottom() {
    this.messagesContainer.scrollTop = this.messagesContainer.scrollHeight;
  }

  // ==================== ПОИСК ====================
  async searchUsers() {
    const query = this.searchInput.value.trim();
    if (query.length < 2) {
      this.searchResults.classList.remove('active');
      return;
    }
    try {
      const users = await this.api.searchUsers(query);
      this.searchResults.innerHTML = '';
      if (users.length) {
        users.forEach(user => {
          const item = document.createElement('div');
          item.className = 'search-result-item';
          item.textContent = user.nickname;
          item.addEventListener('click', () => {
            this.openChat(user.id, user.nickname);
            this.searchInput.value = '';
            this.searchResults.classList.remove('active');
          });
          this.searchResults.appendChild(item);
        });
        this.searchResults.classList.add('active');
      } else {
        this.searchResults.innerHTML = '<div class="search-result-item" style="color:var(--color-text-secondary)">Никого не найдено</div>';
        this.searchResults.classList.add('active');
      }
    } catch (err) {
      console.error(err);
    }
  }

  // ==================== ГОЛОСОВЫЕ ====================
  toggleVoiceRecording() {
    if (this.mediaRecorder && this.mediaRecorder.state === 'recording') {
      this.mediaRecorder.stop();
      this.voiceRecordBtn.textContent = '🎤';
    } else {
      this.startRecording();
    }
  }

  async startRecording() {
    if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
      alert('Ваш браузер не поддерживает запись голоса. Используйте HTTPS-соединение.');
      return;
    }
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      this.mediaRecorder = new MediaRecorder(stream);
      const chunks = [];
      this.mediaRecorder.ondataavailable = e => chunks.push(e.data);
      this.mediaRecorder.onstop = async () => {
        const blob = new Blob(chunks, { type: 'audio/webm' });
        const file = new File([blob], 'voice.webm', { type: 'audio/webm' });
        const uploadRes = await this.api.uploadFile(file, 'voice');
        await this.signalR.sendMessage(
          this.activeChatUserId,
          null,
          this.replyToMessageId,
          [{ fileUrl: uploadRes.url, type: 'voice' }]
        );
        this.cancelReply();
      };
      this.mediaRecorder.start();
      this.voiceRecordBtn.textContent = '🔴';
    } catch (err) {
      alert('Ошибка доступа к микрофону: ' + (err.message || 'Не удалось получить доступ'));
    }
  }

  // ==================== ИЗОБРАЖЕНИЯ ====================
  openImageViewer(src) {
    this.fullImage.src = src;
    this.imageModal.classList.add('active');
  }

  closeImageViewer() {
    this.imageModal.classList.remove('active');
    this.fullImage.src = '';
  }

  // ==================== НАСТРОЙКИ ====================
  openSettings() {
    this.settingsModal.style.display = 'flex';
    if (this.currentUser) {
      this.newNicknameInput.value = this.currentUser.nickname;
    }
    this.nicknameError.textContent = '';
  }

  closeSettings() {
    this.settingsModal.style.display = 'none';
  }

  async saveNickname() {
    if (!this.currentUser) return;
    const newNick = this.newNicknameInput.value.trim();
    if (!newNick || newNick === this.currentUser.nickname) return;
    try {
      await this.api.updateNickname(newNick);
      this.currentUser.nickname = newNick;
      this.currentUserNickname.textContent = newNick;
      this.nicknameError.textContent = 'Никнейм обновлён.';
    } catch (err) {
      this.nicknameError.textContent = err.message;
    }
  }

  changeBgColor(color) {
    document.documentElement.style.setProperty('--chat-bg', color);
    localStorage.setItem('chatBg', color);
    this.customBgColor.value = color;
  }

  // ==================== РЕСАЙЗ ====================
  initResize() {
    const sidebar = $('#sidebar');
    const handle = $('.sidebar-resize-handle');
    let startX, startWidth;

    const onMouseMove = (e) => {
      const newWidth = startWidth + e.clientX - startX;
      if (newWidth >= 260 && newWidth <= 480) {
        sidebar.style.width = newWidth + 'px';
        document.documentElement.style.setProperty('--sidebar-width', newWidth + 'px');
      }
    };

    const onMouseUp = () => {
      document.removeEventListener('mousemove', onMouseMove);
      document.removeEventListener('mouseup', onMouseUp);
      handle.classList.remove('active');
      document.body.style.cursor = '';
      document.body.style.userSelect = '';
    };

    handle.addEventListener('mousedown', (e) => {
      startX = e.clientX;
      startWidth = sidebar.offsetWidth;
      document.addEventListener('mousemove', onMouseMove);
      document.addEventListener('mouseup', onMouseUp);
      handle.classList.add('active');
      document.body.style.cursor = 'col-resize';
      document.body.style.userSelect = 'none';
      e.preventDefault();
    });
  }
}

// ============================
// ЗАПУСК ПРИЛОЖЕНИЯ
// ============================
document.addEventListener('DOMContentLoaded', () => {
  const api = new ApiClient();
  const signalR = new SignalRClient(api);
  const ui = new MessengerUI(api, signalR);
});