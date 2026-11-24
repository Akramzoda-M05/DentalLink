// public/assets/js/app.js — интерактивность DentalLink (Лабораторная работа 11)
// Подключается как ES-модуль: <script type="module" src="../assets/js/app.js"></script>

'use strict';

/* ==========================================================================
   1. УТИЛИТЫ (Общие вспомогательные функции)
   ========================================================================== */

/**
 * Откладывает вызов fn, пока пользователь не сделает паузу delay мс (по умолчанию 300 мс).
 * @param {Function} fn - Вызываемая функция
 * @param {number} delay - Задержка в миллисекундах
 * @returns {Function} Функция с дебаунсом
 */
export function debounce(fn, delay = 300) {
  let timerId;
  return (...args) => {
    clearTimeout(timerId);
    timerId = setTimeout(() => fn(...args), delay);
  };
}

/**
 * Преобразует дату из формата ISO 'YYYY-MM-DD' в локализованный вид 'DD.MM.YYYY'.
 * @param {string} isoDate - Дата в формате YYYY-MM-DD
 * @returns {string} Дата в формате DD.MM.YYYY
 */
export function formatDate(isoDate) {
  if (!isoDate || typeof isoDate !== 'string') return '';
  const parts = isoDate.split('-');
  if (parts.length !== 3) return isoDate;
  const [year, month, day] = parts;
  return `${day}.${month}.${year}`;
}

/* ==========================================================================
   2. ВАЛИДАЦИЯ ФОРМ (Constraint Validation API)
   ========================================================================== */

const FIELD_MESSAGES = {
  patient_name: {
    valueMissing: 'Укажите ФИО пациента.',
    tooShort: 'ФИО должно содержать не менее 3 символов.',
  },
  patient_phone: {
    valueMissing: 'Укажите контактный номер телефона.',
    patternMismatch: 'Введите корректный номер телефона (например, +992 90 000 0000).',
  },
  doctor_id: {
    valueMissing: 'Выберите лечащего врача из списка.',
  },
  service_type: {
    valueMissing: 'Выберите необходимую медицинскую услугу.',
  },
  start_date: {
    valueMissing: 'Укажите дату начала визита.',
    rangeUnderflow: 'Дата визита не может быть в прошлом.',
  },
  end_date: {
    valueMissing: 'Укажите дату окончания курса лечения.',
  },
  notes: {
    tooLong: 'Описание жалобы не должно превышать 255 символов.',
  },
  type: {
    valueMissing: 'Выберите тип отпуска.',
  },
};

/**
 * Возвращает русскоязычный текст ошибки для поля формы
 * @param {HTMLInputElement|HTMLSelectElement|HTMLTextAreaElement} field
 * @returns {string}
 */
function messageFor(field) {
  const v = field.validity;
  if (v.valid) return '';

  if (v.customError) return field.validationMessage;

  const custom = FIELD_MESSAGES[field.name] ?? {};

  if (v.valueMissing) return custom.valueMissing ?? 'Заполните это обязательное поле.';
  if (v.tooShort) return custom.tooShort ?? `Минимальная длина — ${field.minLength} символов.`;
  if (v.tooLong) return custom.tooLong ?? `Не более ${field.maxLength} символов.`;
  if (v.typeMismatch) {
    if (field.type === 'email') return 'Введите корректный адрес электронной почты.';
    return custom.typeMismatch ?? 'Неверный формат данных.';
  }
  if (v.patternMismatch) return custom.patternMismatch ?? 'Значение не соответствует требуемому формату.';
  if (v.rangeUnderflow) return custom.rangeUnderflow ?? `Значение не может быть меньше ${field.min}.`;
  if (v.rangeOverflow) return custom.rangeOverflow ?? `Значение не может быть больше ${field.max}.`;
  if (v.badInput) return 'Некорректный ввод данных.';

  return 'Проверьте значение поля.';
}

/**
 * Подсвечивает валидность поля: класс is-invalid, aria-invalid, текст ошибки
 * @param {HTMLElement} field
 * @returns {boolean}
 */
function showFieldState(field) {
  if (!field || !field.id) return true;
  const message = messageFor(field);
  const feedback = document.getElementById(`${field.id}-error`);

  const hasError = message !== '';
  field.classList.toggle('is-invalid', hasError);

  if (hasError) {
    field.setAttribute('aria-invalid', 'true');
  } else {
    field.removeAttribute('aria-invalid');
  }

  if (feedback) {
    feedback.textContent = message;
  }

  return !hasError;
}

/**
 * Собственное правило валидации дат на два поля:
 * Дата окончания не может быть раньше даты начала.
 * @param {HTMLFormElement} form
 */
function checkAppointmentDates(form) {
  const start = form.elements.start_date;
  const end = form.elements.end_date;
  if (!start || !end) return;

  // Обязательный сброс ошибки, чтобы не блокировать форму навсегда
  end.setCustomValidity('');

  if (start.value && end.value && end.value < start.value) {
    end.setCustomValidity('Дата окончания не может быть раньше даты начала.');
  }
}

/**
 * Настройка валидации формы с делегированием focusout и input
 * @param {string} formId
 */
function setupFormValidation(formId) {
  const form = document.getElementById(formId);
  if (!form) return;

  const { start_date: start, end_date: end } = form.elements;
  const fields = () => form.querySelectorAll('input:not([type="hidden"]):not([type="submit"]), select, textarea');

  if (start && end) {
    start.addEventListener('change', () => {
      end.min = start.value;
      if (end.value && end.classList.contains('is-invalid')) {
        checkAppointmentDates(form);
        showFieldState(end);
      }
    });
  }

  const refresh = (field) => {
    checkAppointmentDates(form);
    showFieldState(field);
    if (end && field !== end && end.classList.contains('is-invalid')) {
      showFieldState(end);
    }
  };

  // Делегирование focusout
  form.addEventListener('focusout', (event) => {
    if (event.target.matches('input, select, textarea')) {
      refresh(event.target);
    }
  });

  // Мгновенное снятие ошибки при вводе
  form.addEventListener('input', (event) => {
    if (event.target.classList.contains('is-invalid')) {
      refresh(event.target);
    }
  });

  // Отправка формы submit
  form.addEventListener('submit', (event) => {
    checkAppointmentDates(form);
    let firstInvalid = null;

    fields().forEach((field) => {
      const isValid = showFieldState(field);
      if (!isValid && !firstInvalid) {
        firstInvalid = field;
      }
    });

    if (firstInvalid) {
      event.preventDefault();
      firstInvalid.focus();
    }
  });
}

export function initFormValidation() {
  setupFormValidation('appointment-form');
  setupFormValidation('booking-form');
  setupFormValidation('leave-form');
}

/* ==========================================================================
   3. ЖИВОЙ ПОИСК (Live Search)
   ========================================================================== */

function createCell(text, className = '') {
  const td = document.createElement('td');
  td.textContent = text ?? '';
  if (className) td.className = className;
  return td;
}

const STATUS_CONFIG = {
  active: { text: '● Принимает', className: 'badge-status-confirmed' },
  on_leave: { text: '● В отпуске', className: 'badge-status-progress' },
  dismissed: { text: '● Уволен', className: 'badge bg-secondary' },
};

function createDoctorRow(item) {
  const tr = document.createElement('tr');

  const nameCell = document.createElement('td');
  nameCell.setAttribute('data-label', 'ФИО');
  const link = document.createElement('a');
  link.href = item.id ? `booking.html?doctor_id=${encodeURIComponent(item.id)}` : '#';
  link.className = 'fw-bold text-decoration-none text-dark';
  link.textContent = item.full_name; // Защита от XSS
  nameCell.append(link);

  const statusCell = document.createElement('td');
  statusCell.setAttribute('data-label', 'Статус');
  const statusInfo = STATUS_CONFIG[item.status] ?? { text: item.status ?? '—', className: 'badge bg-light text-dark' };
  const badge = document.createElement('span');
  badge.className = statusInfo.className;
  badge.textContent = statusInfo.text;
  statusCell.append(badge);

  const actionsCell = document.createElement('td');
  actionsCell.setAttribute('data-label', 'Действие');
  actionsCell.className = 'text-end cell-actions';

  const deleteBtn = document.createElement('button');
  deleteBtn.type = 'button';
  deleteBtn.className = 'btn btn-sm btn-outline-danger ms-1 rounded-pill px-3';
  deleteBtn.dataset.deleteUrl = `/api/doctors/${encodeURIComponent(item.id)}/delete`;
  deleteBtn.dataset.doctorName = item.full_name;
  deleteBtn.dataset.employeeName = item.full_name;
  deleteBtn.textContent = 'Удалить';

  actionsCell.append(deleteBtn);

  tr.append(
    nameCell,
    createCell(item.department ?? '—', 'd-none d-sm-table-cell'),
    createCell(item.position ?? item.specialization ?? '—'),
    createCell(item.phone ?? '—', 'd-none d-md-table-cell'),
    createCell(formatDate(item.hire_date), 'd-none d-lg-table-cell'),
    statusCell,
    actionsCell
  );

  return tr;
}

/**
 * Создаёт красивую карточку врача в стиле UI-kit DentalLink (безопасно, без innerHTML)
 * @param {Object} item
 * @returns {HTMLElement}
 */
function createDoctorCard(item) {
  const col = document.createElement('div');
  col.className = 'col-12 col-md-6 col-xl-4 doctor-card-item';
  col.dataset.doctorName = (item.full_name || '').toLowerCase();
  col.dataset.doctorSpec = (item.specialization || item.position || '').toLowerCase();

  const article = document.createElement('article');
  article.className = 'doctor-catalog-card h-100';

  const avatar = document.createElement('div');
  avatar.className = 'doctor-photo-placeholder';
  avatar.setAttribute('role', 'img');
  avatar.setAttribute('aria-label', `Фото ${item.full_name}`);
  // Генерация инициалов
  const initials = (item.full_name || 'ДЛ')
    .replace(/<[^>]*>/g, '') // убираем теги для инициалов
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map(w => w[0] || '')
    .join('')
    .toUpperCase() || 'ДЛ';
  avatar.textContent = initials;

  const content = document.createElement('div');
  content.className = 'doctor-card-content';

  const statusWrap = document.createElement('div');
  statusWrap.className = 'mb-2';
  const statusInfo = STATUS_CONFIG[item.status] ?? { text: '● Принимает', className: 'badge-status-confirmed' };
  const badge = document.createElement('span');
  badge.className = statusInfo.className;
  badge.textContent = statusInfo.text;
  statusWrap.append(badge);

  const title = document.createElement('h3');
  title.className = 'doctor-name-title';
  title.textContent = item.full_name; // textContent безопасен от XSS!

  const spec = document.createElement('div');
  spec.className = 'doctor-spec-text';
  spec.textContent = item.position ?? item.specialization ?? 'Стоматолог-специалист';

  const exp = document.createElement('div');
  exp.className = 'doctor-exp-text';
  exp.textContent = `${item.department ?? 'Клиника DentalLink'} · ${item.phone ?? 'Приём по записи'}`;

  const btnWrap = document.createElement('div');
  btnWrap.className = 'd-flex gap-2 mt-auto pt-2';

  const link = document.createElement('a');
  link.href = `booking.html?doctor_id=${encodeURIComponent(item.id)}`;
  link.className = 'btn-pill-action text-decoration-none text-center flex-grow-1';
  link.textContent = 'Подробнее';

  const deleteBtn = document.createElement('button');
  deleteBtn.type = 'button';
  deleteBtn.className = 'btn btn-sm btn-outline-danger rounded-pill px-3';
  deleteBtn.dataset.deleteUrl = `/api/doctors/${encodeURIComponent(item.id)}/delete`;
  deleteBtn.dataset.doctorName = item.full_name;
  deleteBtn.dataset.employeeName = item.full_name;
  deleteBtn.textContent = 'Удалить';

  btnWrap.append(link, deleteBtn);

  content.append(statusWrap, title, spec, exp, btnWrap);
  article.append(avatar, content);
  col.append(article);

  return col;
}

function renderDoctors(tbody, cardsContainer, items) {
  // Рендеринг в таблицу
  if (tbody) {
    if (!items || items.length === 0) {
      const emptyRow = document.createElement('tr');
      const emptyCell = document.createElement('td');
      emptyCell.colSpan = 7;
      emptyCell.className = 'text-center py-4 text-muted';
      emptyCell.textContent = 'Специалисты не найдены. Попробуйте изменить параметры поиска.';
      emptyRow.append(emptyCell);
      tbody.replaceChildren(emptyRow);
    } else {
      const rows = items.map(createDoctorRow);
      tbody.replaceChildren(...rows);
    }
  }

  // Рендеринг в сетку карточек
  if (cardsContainer) {
    if (!items || items.length === 0) {
      const emptyMsg = document.createElement('div');
      emptyMsg.className = 'col-12 text-center py-5 text-muted';
      emptyMsg.textContent = 'Специалисты не найдены. Попробуйте изменить параметры поиска.';
      cardsContainer.replaceChildren(emptyMsg);
    } else {
      const cards = items.map(createDoctorCard);
      cardsContainer.replaceChildren(...cards);
    }
  }
}

export function initLiveSearch() {
  const input = document.getElementById('doctor-search') || document.getElementById('employee-search') || document.getElementById('header-search');
  const headerSearch = document.getElementById('header-search');
  const tbody = document.getElementById('doctors-tbody') || document.getElementById('employees-tbody');
  const cardsContainer = document.getElementById('doctors-cards-container');

  if (!input && !headerSearch) return;

  const statusBox = document.getElementById('doctor-search-status') || document.getElementById('employee-search-status');
  const countBadge = document.getElementById('doctors-count-badge');

  const setStatus = (text) => {
    if (statusBox) statusBox.textContent = text;
  };

  let controller = null;

  async function search(query) {
    controller?.abort();
    controller = new AbortController();

    if (tbody) tbody.setAttribute('aria-busy', 'true');
    setStatus('Поиск специалистов…');

    const apiEndpoint = (input && input.dataset.api) || '/api/doctors';
    const url = `${apiEndpoint}?q=${encodeURIComponent(query)}`;

    try {
      const response = await fetch(url, {
        headers: { Accept: 'application/json' },
        signal: controller.signal,
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }

      const data = await response.json();
      renderDoctors(tbody, cardsContainer, data);

      if (data.length > 0) {
        setStatus(`Найдено специалистов: ${data.length}`);
        if (countBadge) countBadge.textContent = `${data.length} врачей`;
      } else {
        setStatus('Специалисты не найдены.');
        if (countBadge) countBadge.textContent = '0 врачей';
      }
    } catch (error) {
      if (error.name === 'AbortError') return;
      console.error('Ошибка живого поиска:', error);
      setStatus('Не удалось выполнить поиск. Проверьте соединение и попробуйте ещё раз.');
    } finally {
      if (tbody) tbody.removeAttribute('aria-busy');
    }
  }

  const debouncedSearch = debounce(search, 300);

  // Слушаем основной инпут
  if (input) {
    input.addEventListener('input', () => {
      const val = input.value.trim();
      if (headerSearch && headerSearch !== input) headerSearch.value = input.value;
      debouncedSearch(val);
    });
  }

  // Если есть инпут в шапке, синхронизируем с основным
  if (headerSearch && headerSearch !== input) {
    headerSearch.addEventListener('input', () => {
      const val = headerSearch.value.trim();
      if (input) input.value = headerSearch.value;
      debouncedSearch(val);
    });
  }
}

/* ==========================================================================
   4. МОДАЛЬНОЕ ОКНО ПОДТВЕРЖДЕНИЯ УДАЛЕНИЯ
   ========================================================================== */

export function initDeleteConfirm() {
  const modalElement = document.getElementById('confirm-delete-modal');
  if (!modalElement || !window.bootstrap) return;

  const form = document.getElementById('confirm-delete-form');
  const nameElement = document.getElementById('confirm-delete-name');
  const submitButton = form?.querySelector('[type="submit"]');
  const modal = window.bootstrap.Modal.getOrCreateInstance(modalElement);

  let lastTrigger = null;

  // Делегирование на уровне document — ловит клики даже по созданным динамически карточкам и строкам
  document.addEventListener('click', (event) => {
    const button = event.target.closest('[data-delete-url]');
    if (!button) return;

    lastTrigger = button;
    if (form) form.action = button.dataset.deleteUrl;
    if (nameElement) {
      nameElement.textContent = button.dataset.doctorName || button.dataset.employeeName || '';
    }
    if (submitButton) submitButton.disabled = false;

    modal.show();
  });

  // Возврат фокуса при закрытии
  modalElement.addEventListener('hidden.bs.modal', () => {
    if (lastTrigger?.isConnected) {
      lastTrigger.focus();
    }
  });

  // Защита от двойного клика
  if (form && submitButton) {
    form.addEventListener('submit', () => {
      submitButton.disabled = true;
    });
  }
}

/* ==========================================================================
   5. ПЕРЕКЛЮЧЕНИЕ ВИДОВ (Карточки / Таблица) в каталоге
   ========================================================================== */

function initViewToggle() {
  const btnCards = document.getElementById('view-cards-btn');
  const btnTable = document.getElementById('view-table-btn');
  const secCards = document.getElementById('doctors-cards-section');
  const secTable = document.getElementById('doctors-table-section');

  if (!btnCards || !btnTable || !secCards || !secTable) return;

  btnCards.addEventListener('click', () => {
    btnCards.classList.add('active');
    btnCards.setAttribute('aria-selected', 'true');
    btnTable.classList.remove('active');
    btnTable.setAttribute('aria-selected', 'false');
    secCards.classList.remove('d-none');
    secTable.classList.add('d-none');
  });

  btnTable.addEventListener('click', () => {
    btnTable.classList.add('active');
    btnTable.setAttribute('aria-selected', 'true');
    btnCards.classList.remove('active');
    btnCards.setAttribute('aria-selected', 'false');
    secTable.classList.remove('d-none');
    secCards.classList.add('d-none');
  });
}

// Запуск при подключении как ES-модуль
initFormValidation();
initLiveSearch();
initDeleteConfirm();
initViewToggle();
