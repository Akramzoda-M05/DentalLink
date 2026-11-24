// public/assets/js/app.js — интерактивность DentalLink (Лабораторная работа 11)
// Подключается как ES-модуль: <script type="module" src="../assets/js/app.js"></script>

'use strict';

/* ==========================================================================
   1. УТИЛИТЫ (Общие вспомогательные функции)
   ========================================================================== */

/**
 * Откладывает вызов fn, пока пользователь не сделает паузу delay мс (по умолчанию 300 мс).
 * Позволяет избежать спама запросов при непрерывном вводе текста в поисковую строку.
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
 * Пустое значение безопасно возвращает пустую строку.
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

/**
 * Кастомные сообщения об ошибках для ключевых полей форм проекта
 */
const FIELD_MESSAGES = {
  // Поля формы заявки на приём DentalLink
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

  // Поля формы отпуска / заявления (из эталона)
  type: {
    valueMissing: 'Выберите тип отпуска.',
  },
};

/**
 * Формирует русскоязычное понятное сообщение об ошибке для элемента формы
 * на основе его объекта validity (Constraint Validation API).
 * @param {HTMLInputElement|HTMLSelectElement|HTMLTextAreaElement} field
 * @returns {string} Текст ошибки или пустая строка, если поле валидно
 */
function messageFor(field) {
  const v = field.validity;
  if (v.valid) return '';

  // Если сработало наше собственное правило через setCustomValidity('текст')
  if (v.customError) return field.validationMessage;

  const custom = FIELD_MESSAGES[field.name] ?? {};

  if (v.valueMissing) {
    return custom.valueMissing ?? 'Заполните это обязательное поле.';
  }
  if (v.tooShort) {
    return custom.tooShort ?? `Минимальная длина — ${field.minLength} символов.`;
  }
  if (v.tooLong) {
    return custom.tooLong ?? `Не более ${field.maxLength} символов.`;
  }
  if (v.typeMismatch) {
    if (field.type === 'email') return 'Введите корректный адрес электронной почты.';
    if (field.type === 'url') return 'Введите корректный URL-адрес.';
    return custom.typeMismatch ?? 'Неверный формат данных.';
  }
  if (v.patternMismatch) {
    return custom.patternMismatch ?? 'Значение не соответствует требуемому формату.';
  }
  if (v.rangeUnderflow) {
    return custom.rangeUnderflow ?? `Значение не может быть меньше ${field.min}.`;
  }
  if (v.rangeOverflow) {
    return custom.rangeOverflow ?? `Значение не может быть больше ${field.max}.`;
  }
  if (v.badInput) {
    return 'Некорректный ввод данных.';
  }

  return 'Проверьте значение поля.';
}

/**
 * Отображает состояние валидности поля:
 * включает класс Bootstrap `is-invalid`, устанавливает атрибут `aria-invalid`
 * и обновляет связанный блок ошибки `#${field.id}-error`.
 * @param {HTMLElement} field
 * @returns {boolean} true если поле валидно, false если содержит ошибку
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
 * Собственное бизнес-правило валидации диапазона дат:
 * Дата окончания не может быть раньше даты начала.
 * Корректно очищает ошибку через setCustomValidity('') перед повторной проверкой.
 * @param {HTMLFormElement} form
 */
function checkAppointmentDates(form) {
  const start = form.elements.start_date;
  const end = form.elements.end_date;
  if (!start || !end) return;

  // Обязательно сбрасываем предыдущую кастомную ошибку, иначе поле навсегда останется невалидным
  end.setCustomValidity('');

  // Значения <input type="date"> всегда представлены в формате YYYY-MM-DD и безопасно сравниваются лексикографически
  if (start.value && end.value && end.value < start.value) {
    end.setCustomValidity('Дата окончания не может быть раньше даты начала.');
  }
}

/**
 * Универсальная функция инициализации валидации формы с делегированием событий
 * @param {string} formId
 */
function setupFormValidation(formId) {
  const form = document.getElementById(formId);
  if (!form) return;

  const { start_date: start, end_date: end } = form.elements;
  const fields = () => form.querySelectorAll('input:not([type="hidden"]):not([type="submit"]), select, textarea');

  // Устанавливаем ограничение min для даты окончания при выборе даты начала
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

  // Делегирование события focusout (всплывает, в отличие от blur) — 1 обработчик на всю форму
  form.addEventListener('focusout', (event) => {
    if (event.target.matches('input, select, textarea')) {
      refresh(event.target);
    }
  });

  // Событие input: мгновенно снимает ошибку, как только пользователь исправил значение
  form.addEventListener('input', (event) => {
    if (event.target.classList.contains('is-invalid')) {
      refresh(event.target);
    }
  });

  // Обработчик отправки формы submit
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
      event.preventDefault(); // Не отправляем форму с ошибками
      firstInvalid.focus();   // Доступность: переносим фокус на первое ошибочное поле
    }
  });
}

/**
 * Инициализирует валидацию всех ключевых форм проекта
 */
export function initFormValidation() {
  setupFormValidation('appointment-form');
  setupFormValidation('booking-form');
  setupFormValidation('leave-form');
}

/* ==========================================================================
   3. ЖИВОЙ ПОИСК (Live Search с fetch, AbortController и защитой от XSS)
   ========================================================================== */

/**
 * Вспомогательная функция для безопасного создания ячейки <td> таблицы.
 * Данные всегда вставляются исключительно через textContent — любые символы <, >, &
 * будут выведены как безопасный текст, защищая от XSS-уязвимостей.
 * @param {string|null|undefined} text
 * @param {string} className
 * @returns {HTMLTableCellElement}
 */
function createCell(text, className = '') {
  const td = document.createElement('td');
  td.textContent = text ?? '';
  if (className) td.className = className;
  return td;
}

/**
 * Словарь статусов врачей для генерации визуальных бейджей
 */
const STATUS_CONFIG = {
  active: { text: '● Принимает', className: 'badge-status-confirmed' },
  on_leave: { text: '● В отпуске', className: 'badge-status-progress' },
  dismissed: { text: '● Уволен', className: 'badge bg-secondary' },
};

/**
 * Создает DOM-элемент строки таблицы <tr> для врача/сотрудника
 * @param {Object} item - Объект данных сотрудника/врача
 * @returns {HTMLTableRowElement}
 */
function createDoctorRow(item) {
  const tr = document.createElement('tr');

  // Ячейка с именем и ссылкой
  const nameCell = document.createElement('td');
  nameCell.setAttribute('data-label', 'ФИО');
  const link = document.createElement('a');
  link.href = item.id ? `booking.html?doctor_id=${encodeURIComponent(item.id)}` : '#';
  link.className = 'fw-bold text-decoration-none text-dark';
  link.textContent = item.full_name; // textContent экранирует XSS!
  nameCell.append(link);

  // Ячейка статуса
  const statusCell = document.createElement('td');
  statusCell.setAttribute('data-label', 'Статус');
  const statusInfo = STATUS_CONFIG[item.status] ?? { text: item.status ?? '—', className: 'badge bg-light text-dark' };
  const badge = document.createElement('span');
  badge.className = statusInfo.className;
  badge.textContent = statusInfo.text;
  statusCell.append(badge);

  // Ячейка действий с кнопкой «Удалить» для модального окна
  const actionsCell = document.createElement('td');
  actionsCell.setAttribute('data-label', 'Действие');
  actionsCell.className = 'text-end cell-actions';

  const deleteBtn = document.createElement('button');
  deleteBtn.type = 'button';
  deleteBtn.className = 'btn btn-sm btn-outline-danger ms-1 rounded-pill px-3';
  deleteBtn.dataset.deleteUrl = `/api/doctors/${encodeURIComponent(item.id)}/delete`;
  deleteBtn.dataset.doctorName = item.full_name;
  deleteBtn.dataset.employeeName = item.full_name; // Совместимость с data-employee-name
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
 * Безопасно рендерит список элементов в тело таблицы через replaceChildren
 * @param {HTMLTableSectionElement} tbody
 * @param {Array} items
 */
function renderDoctors(tbody, items) {
  if (!items || items.length === 0) {
    const emptyRow = document.createElement('tr');
    const emptyCell = document.createElement('td');
    emptyCell.colSpan = 7;
    emptyCell.className = 'text-center py-4 text-muted';
    emptyCell.textContent = 'Специалисты не найдены. Попробуйте изменить параметры поиска.';
    emptyRow.append(emptyCell);
    tbody.replaceChildren(emptyRow);
    return;
  }

  const rows = items.map(createDoctorRow);
  tbody.replaceChildren(...rows);
}

/**
 * Инициализирует модуль живого поиска сотрудников/врачей
 */
export function initLiveSearch() {
  const input = document.getElementById('doctor-search') || document.getElementById('employee-search');
  const tbody = document.getElementById('doctors-tbody') || document.getElementById('employees-tbody');

  if (!input || !tbody) return;

  const statusBox = document.getElementById('doctor-search-status') || document.getElementById('employee-search-status');
  const setStatus = (text) => {
    if (statusBox) statusBox.textContent = text;
  };

  let controller = null; // AbortController для отмены устаревших запросов

  async function search(query) {
    // Отменяем предыдущий запрос, если пользователь продолжает ввод
    controller?.abort();
    controller = new AbortController();

    tbody.setAttribute('aria-busy', 'true');
    setStatus('Поиск…');

    // Определяем эндпоинт API
    const apiEndpoint = input.dataset.api || '/api/doctors';
    const url = `${apiEndpoint}?q=${encodeURIComponent(query)}`;

    try {
      const response = await fetch(url, {
        headers: { Accept: 'application/json' },
        signal: controller.signal,
      });

      // fetch не считает 404 и 500 ошибками сети — проверяем response.ok
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }

      const data = await response.json();
      renderDoctors(tbody, data);

      if (data.length > 0) {
        setStatus(`Найдено специалистов: ${data.length}`);
      } else {
        setStatus('Специалисты не найдены.');
      }
    } catch (error) {
      // AbortError означает, что запрос был отменён более новым вводом — это нормальное поведение
      if (error.name === 'AbortError') return;

      console.error('Ошибка живого поиска:', error);
      setStatus('Не удалось выполнить поиск. Проверьте соединение и попробуйте ещё раз.');
    } finally {
      tbody.removeAttribute('aria-busy');
    }
  }

  const debouncedSearch = debounce(search, 300);

  input.addEventListener('input', () => {
    debouncedSearch(input.value.trim());
  });
}

/* ==========================================================================
   4. МОДАЛЬНОЕ ОКНО ПОДТВЕРЖДЕНИЯ УДАЛЕНИЯ
   ========================================================================== */

/**
 * Инициализирует модальное окно подтверждения удаления с делегированием событий на document
 */
export function initDeleteConfirm() {
  const modalElement = document.getElementById('confirm-delete-modal');
  if (!modalElement || !window.bootstrap) return;

  const form = document.getElementById('confirm-delete-form');
  const nameElement = document.getElementById('confirm-delete-name');
  const submitButton = form?.querySelector('[type="submit"]');
  const modal = window.bootstrap.Modal.getOrCreateInstance(modalElement);

  let lastTrigger = null;

  // Один глобальный обработчик клика на document — гарантированно ловит клики
  // даже по кнопкам в строках, динамически добавленным живым поиском позже
  document.addEventListener('click', (event) => {
    const button = event.target.closest('[data-delete-url]');
    if (!button) return;

    lastTrigger = button;
    if (form) {
      form.action = button.dataset.deleteUrl;
    }
    if (nameElement) {
      nameElement.textContent = button.dataset.doctorName || button.dataset.employeeName || '';
    }
    if (submitButton) {
      submitButton.disabled = false;
    }

    modal.show();
  });

  // Требование доступности: после закрытия окна возвращаем фокус на кнопку-триггер
  modalElement.addEventListener('hidden.bs.modal', () => {
    if (lastTrigger?.isConnected) {
      lastTrigger.focus();
    }
  });

  // Защита от двойного нажатия (double-submit prevention)
  if (form && submitButton) {
    form.addEventListener('submit', () => {
      submitButton.disabled = true;
    });
  }
}

/* ==========================================================================
   5. АВТОМАТИЧЕСКИЙ ЗАПУСК
   ========================================================================== */

// Поскольку скрипт подключён как <script type="module">,
// он автоматически выполняется после разбора DOM (как defer),
// поэтому событие DOMContentLoaded ждать не требуется.
initFormValidation();
initLiveSearch();
initDeleteConfirm();
