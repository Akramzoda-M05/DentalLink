-- =============================================================================
-- ИС «Онлайн-запись в стоматологию» (DentalLink)
-- Лабораторная работа 7: Выборка и манипуляция данными (SQL-запросы)
-- СУБД: PostgreSQL (14+)
-- Файл: database/queries.sql
-- Кодировка: UTF-8
--
-- Соответствие требованиям лабораторной работы:
--  1. JOIN трёх таблиц (с детальным разбором связей)
--  2. LIKE (поиск по строковому шаблону)
--  3. GROUP BY + COUNT (агрегация и аналитика)
--  4. LEFT JOIN (сохранение строк без совпадений)
--  5. Фильтр по статусу / категории (выборка по критерию)
--  6. LIMIT / OFFSET (постраничный вывод)
-- У каждого запроса приведён комментарий со ссылкой на функциональное требование (ФТ)
-- =============================================================================

-- =============================================================================
-- ЗАПРОС 1: JOIN трёх и более таблиц
-- Ссылка на ФТ: ФТ-05 (Интерактивная сетка расписания Timeline для ресепшена)
--                ФТ-06 (Просмотр персонального расписания приёмов врачом)
-- Бизнес-смысл:
--  Формирование оперативного реестра приёмов на конкретную дату (12 ноября 2026 г.)
--  с объединением данных из 5 таблиц: запись на приём (appointments), пациент (patients),
--  врач (doctors), учётная запись пользователя (users), кабинет (cabinets) и слот (time_slots).
-- =============================================================================
SELECT
    a.appointment_number                        AS "Номер талона",
    ts.slot_date                                AS "Дата приёма",
    to_char(ts.start_time, 'HH24:MI') || ' - ' ||
    to_char(ts.end_time, 'HH24:MI')            AS "Время визита",
    p.full_name                                 AS "Пациент",
    p.phone                                     AS "Телефон пациента",
    u_doc.full_name                             AS "Лечащий врач",
    d.specialization                            AS "Специализация",
    c.name                                      AS "Кабинет",
    a.status                                    AS "Статус визита",
    CASE
        WHEN a.is_confirmed_by_patient THEN 'Да (Telegram)'
        ELSE 'Нет'
    END                                         AS "Подтверждено пациентом"
FROM appointments a
JOIN patients p       ON a.patient_id = p.id
JOIN doctors d        ON a.doctor_id = d.id
JOIN users u_doc      ON d.user_id = u_doc.id
JOIN cabinets c       ON a.cabinet_id = c.id
JOIN time_slots ts    ON a.slot_id = ts.id
WHERE ts.slot_date = '2026-11-12'
ORDER BY ts.start_time ASC, c.name ASC;


-- =============================================================================
-- ЗАПРОС 2: Поиск по текстовому шаблону с использованием LIKE / ILIKE
-- Ссылка на ФТ: ФТ-02 (Просмотр каталога врачей и стоматологических услуг)
--                ФТ-10 (Быстрый поиск пациента регистратором по номеру или фамилии)
-- Бизнес-смысл:
--  Быстрый поиск пациента в картотеке клиники по части фамилии (без учёта регистра)
--  или по префиксу мобильного номера операторов Таджикистана (+992 90...).
-- =============================================================================
SELECT
    p.id                                        AS "ID пациента",
    p.full_name                                 AS "ФИО пациента",
    p.phone                                     AS "Номер телефона",
    COALESCE(p.email, 'Не указан')              AS "E-mail",
    to_char(p.birth_date, 'DD.MM.YYYY')         AS "Дата рождения",
    p.notes                                     AS "Медицинские примечания",
    to_char(p.created_at, 'DD.MM.YYYY HH24:MI') AS "Дата первого обращения"
FROM patients p
WHERE p.full_name ILIKE '%Каримов%'
   OR p.phone LIKE '+99290%'
ORDER BY p.full_name ASC;


-- =============================================================================
-- ЗАПРОС 3: Агрегация с группировкой (GROUP BY + COUNT)
-- Ссылка на ФТ: ФТ-11 (Экспорт аналитического отчёта по загрузке врачей и посещаемости)
--                ФТ-08 (Управление графиком смен и планирование загрузки)
-- Бизнес-смысл:
--  Сводная статистика работы врачей: общее число записей, количество успешно
--  завершённых приёмов, отмен, неявок и суммарный доход клиники от оказанных услуг.
-- =============================================================================
SELECT
    u.full_name                                 AS "Врач-стоматолог",
    d.specialization                            AS "Специализация",
    COUNT(a.id)                                 AS "Всего записей",
    COUNT(CASE WHEN a.status = 'completed' THEN 1 END)   AS "Завершено приёмов",
    COUNT(CASE WHEN a.status = 'cancelled' THEN 1 END)   AS "Отменено пациентом",
    COUNT(CASE WHEN a.status = 'no_show' THEN 1 END)     AS "Неявок",
    COALESCE(SUM(
        CASE WHEN a.status = 'completed' THEN aps.quantity * aps.price_at_booking ELSE 0 END
    ), 0.00)                                    AS "Выручка (сомони, TJS)"
FROM doctors d
JOIN users u                ON d.user_id = u.id
LEFT JOIN appointments a    ON d.id = a.doctor_id
LEFT JOIN appointment_services aps ON a.id = aps.appointment_id
GROUP BY d.id, u.full_name, d.specialization
ORDER BY "Выручка (сомони, TJS)" DESC, "Всего записей" DESC;


-- =============================================================================
-- ЗАПРОС 4: Внешнее объединение (LEFT JOIN)
-- Ссылка на ФТ: ФТ-02 (Просмотр каталога врачей и стоматологических услуг)
--                ФТ-09 (Управление справочниками услуг, цен и категорий)
-- Бизнес-смысл:
--  Вывод полного прейскуранта категорий с подсчётом количества активных услуг
--  в каждой категории и диапазона цен. Категории без услуг (если таковые появятся)
--  не отсекаются из отчёта благодаря использованию LEFT JOIN.
-- =============================================================================
SELECT
    sc.id                                       AS "ID категории",
    sc.name                                     AS "Наименование категории",
    sc.sort_order                               AS "Порядок сортировки",
    COUNT(ds.id)                                AS "Количество услуг",
    COALESCE(MIN(ds.price), 0.00)               AS "Мин. цена (TJS)",
    COALESCE(MAX(ds.price), 0.00)               AS "Макс. цена (TJS)",
    COALESCE(ROUND(AVG(ds.price), 2), 0.00)     AS "Средняя цена (TJS)"
FROM service_categories sc
LEFT JOIN dental_services ds ON sc.id = ds.category_id AND ds.is_active = TRUE
GROUP BY sc.id, sc.name, sc.sort_order
ORDER BY sc.sort_order ASC;


-- =============================================================================
-- ЗАПРОС 5: Фильтрация по статусу и категории (WHERE status / category)
-- Ссылка на ФТ: ФТ-03 (Интерактивный календарь свободных слотов в реальном времени)
--                ФТ-04 (Онлайн-бронирование временного слота пациентом)
-- Бизнес-смысл:
--  Выборка актуальных доступных слотов для записи (со статусом 'free' или 'reserved'),
--  к врачам терапевтического профиля ('therapist') на выбранную дату (2026-11-12).
--  Исключаются слоты, уже занятые подтверждёнными визитами ('booked').
-- =============================================================================
SELECT
    ts.id                                       AS "ID слота",
    u.full_name                                 AS "Врач-терапевт",
    c.name                                      AS "Кабинет",
    ts.slot_date                                AS "Дата слота",
    to_char(ts.start_time, 'HH24:MI')           AS "Время начала",
    to_char(ts.end_time, 'HH24:MI')             AS "Время окончания",
    ts.status                                   AS "Текущий статус",
    CASE
        WHEN ts.status = 'free' THEN 'Доступен для бронирования'
        WHEN ts.status = 'reserved' THEN 'Временно удерживается (до 10 мин)'
        ELSE 'Занят'
    END                                         AS "Примечание для сайта"
FROM time_slots ts
JOIN doctors d  ON ts.doctor_id = d.id
JOIN users u    ON d.user_id = u.id
JOIN cabinets c ON ts.cabinet_id = c.id
WHERE ts.slot_date = '2026-11-12'
  AND d.specialization = 'therapist'
  AND ts.status IN ('free', 'reserved')
ORDER BY ts.start_time ASC;


-- =============================================================================
-- ЗАПРОС 6: Постраничная выборка (PAGINATION: LIMIT / OFFSET)
-- Ссылка на ФТ: ФТ-05 (Журнал приёмов пациентов в панели администратора)
--                ФТ-10 (Просмотр истории визитов пациентов)
-- Бизнес-смысл:
--  Постраничная загрузка архива завершённых приёмов с подсчётом итоговой стоимости
--  оказанных услуг (Страница 1: размер страницы 5 записей, сортировка от новых к старым).
-- =============================================================================
SELECT
    a.id                                        AS "ID записи",
    a.appointment_number                        AS "Талон",
    p.full_name                                 AS "Пациент",
    u.full_name                                 AS "Врач",
    c.name                                      AS "Кабинет",
    to_char(a.created_at, 'DD.MM.YYYY HH24:MI') AS "Дата создания",
    a.status                                    AS "Статус",
    COUNT(aps.service_id)                       AS "Кол-во услуг",
    COALESCE(SUM(aps.quantity * aps.price_at_booking), 0.00) AS "Сумма к оплате (TJS)"
FROM appointments a
JOIN patients p     ON a.patient_id = p.id
JOIN doctors d      ON a.doctor_id = d.id
JOIN users u        ON d.user_id = u.id
JOIN cabinets c     ON a.cabinet_id = c.id
LEFT JOIN appointment_services aps ON a.id = aps.appointment_id
WHERE a.status = 'completed'
GROUP BY a.id, a.appointment_number, p.full_name, u.full_name, c.name, a.created_at, a.status
ORDER BY a.created_at DESC
LIMIT 5 OFFSET 0;
