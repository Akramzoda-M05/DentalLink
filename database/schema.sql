-- =============================================================================
-- ИС «Онлайн-запись в стоматологию» (DentalLink)
-- Лабораторная работа 7: Физическая модель базы данных
-- СУБД: PostgreSQL (14+)
-- Файл: database/schema.sql
-- Кодировка: UTF-8
-- Описание: Скрипт создания таблиц, связей, ограничений и индексов
-- =============================================================================

-- Включение расширений (при необходимости)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- -----------------------------------------------------------------------------
-- 0. Перезапускаемость скрипта (Idempotent cleanup)
-- Очистка таблиц в порядке, обратном зависимостям внешних ключей
-- -----------------------------------------------------------------------------
DROP TABLE IF EXISTS notifications CASCADE;
DROP TABLE IF EXISTS appointment_services CASCADE;
DROP TABLE IF EXISTS appointments CASCADE;
DROP TABLE IF EXISTS time_slots CASCADE;
DROP TABLE IF EXISTS doctor_schedules CASCADE;
DROP TABLE IF EXISTS dental_services CASCADE;
DROP TABLE IF EXISTS service_categories CASCADE;
DROP TABLE IF EXISTS cabinets CASCADE;
DROP TABLE IF EXISTS doctors CASCADE;
DROP TABLE IF EXISTS users CASCADE;
DROP TABLE IF EXISTS patients CASCADE;

-- -----------------------------------------------------------------------------
-- 1. Таблица `users`: Учётные записи персонала клиники
-- -----------------------------------------------------------------------------
CREATE TABLE users (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    full_name VARCHAR(120) NOT NULL,
    email VARCHAR(120) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    role VARCHAR(20) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Ограничения уникальности (UNIQUE)
    CONSTRAINT uq_users_email UNIQUE (email),
    CONSTRAINT uq_users_phone UNIQUE (phone),

    -- Ограничения проверок (CHECK)
    CONSTRAINT chk_users_role CHECK (role IN ('admin', 'receptionist', 'doctor'))
);

COMMENT ON TABLE users IS 'Учётные записи администраторов, регистраторов и врачей клиники';
COMMENT ON COLUMN users.password_hash IS 'Хэш пароля пользователя (BCrypt / ASP.NET Core Identity)';
COMMENT ON COLUMN users.role IS 'Роль в системе: admin, receptionist, doctor';

-- -----------------------------------------------------------------------------
-- 2. Таблица `doctors`: Профили врачей-стоматологов (расширение users 1:1)
-- -----------------------------------------------------------------------------
CREATE TABLE doctors (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id BIGINT NOT NULL,
    specialization VARCHAR(30) NOT NULL,
    qualification VARCHAR(100),
    experience_years INT NOT NULL DEFAULT 0,
    bio TEXT,
    avatar_url VARCHAR(255),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Связь 1:1 с таблицей пользователей
    CONSTRAINT uq_doctors_user_id UNIQUE (user_id),
    CONSTRAINT fk_doctors_user FOREIGN KEY (user_id)
        REFERENCES users (id) ON DELETE RESTRICT,

    -- Ограничения проверок (CHECK)
    CONSTRAINT chk_doctors_experience CHECK (experience_years >= 0),
    CONSTRAINT chk_doctors_specialization CHECK (specialization IN (
        'dentist', 'therapist', 'orthopedist', 'orthodontist',
        'surgeon', 'implantologist', 'pediatric', 'hygienist'
    ))
);

COMMENT ON TABLE doctors IS 'Медицинские профили врачей клиники';
COMMENT ON COLUMN doctors.specialization IS 'Специализация врача согласно классификатору';

-- -----------------------------------------------------------------------------
-- 3. Таблица `cabinets`: Лечебные кабинеты и стоматологические установки
-- -----------------------------------------------------------------------------
CREATE TABLE cabinets (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    chair_number INT NOT NULL DEFAULT 1,
    description VARCHAR(255),
    is_available BOOLEAN NOT NULL DEFAULT TRUE,

    CONSTRAINT uq_cabinets_name UNIQUE (name),
    CONSTRAINT chk_cabinets_chair_number CHECK (chair_number > 0)
);

COMMENT ON TABLE cabinets IS 'Справочник лечебных кабинетов и стоматологических кресел клиники';

-- -----------------------------------------------------------------------------
-- 4. Таблица `service_categories`: Категории стоматологических услуг
-- -----------------------------------------------------------------------------
CREATE TABLE service_categories (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(255),
    sort_order INT NOT NULL DEFAULT 0,

    CONSTRAINT uq_service_categories_name UNIQUE (name),
    CONSTRAINT chk_service_categories_sort_order CHECK (sort_order >= 0)
);

COMMENT ON TABLE service_categories IS 'Разделы каталога стоматологических услуг';

-- -----------------------------------------------------------------------------
-- 5. Таблица `dental_services`: Прейскурант стоматологических услуг
-- -----------------------------------------------------------------------------
CREATE TABLE dental_services (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    category_id BIGINT NOT NULL,
    name VARCHAR(150) NOT NULL,
    description TEXT,
    price NUMERIC(10,2) NOT NULL,
    duration_minutes INT NOT NULL DEFAULT 30,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_dental_services_category FOREIGN KEY (category_id)
        REFERENCES service_categories (id) ON DELETE RESTRICT,

    CONSTRAINT chk_dental_services_price CHECK (price >= 0),
    CONSTRAINT chk_dental_services_duration CHECK (duration_minutes > 0 AND duration_minutes % 30 = 0)
);

COMMENT ON TABLE dental_services IS 'Каталог медицинских услуг с ценами и длительностью кратно 30 мин';

-- -----------------------------------------------------------------------------
-- 6. Таблица `doctor_schedules`: График рабочих смен врачей
-- -----------------------------------------------------------------------------
CREATE TABLE doctor_schedules (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    doctor_id BIGINT NOT NULL,
    cabinet_id BIGINT NOT NULL,
    work_date DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_schedules_doctor FOREIGN KEY (doctor_id)
        REFERENCES doctors (id) ON DELETE RESTRICT,
    CONSTRAINT fk_schedules_cabinet FOREIGN KEY (cabinet_id)
        REFERENCES cabinets (id) ON DELETE RESTRICT,

    CONSTRAINT chk_schedules_time CHECK (end_time > start_time),
    CONSTRAINT uq_doctor_shift UNIQUE (doctor_id, work_date, start_time)
);

COMMENT ON TABLE doctor_schedules IS 'Планирование смен врачей в привязке к кабинетам';

-- -----------------------------------------------------------------------------
-- 7. Таблица `time_slots`: Временные слоты расписания (шаг 30 минут)
-- -----------------------------------------------------------------------------
CREATE TABLE time_slots (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    schedule_id BIGINT NOT NULL,
    doctor_id BIGINT NOT NULL,
    cabinet_id BIGINT NOT NULL,
    slot_date DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'free',
    reserved_until TIMESTAMPTZ,

    CONSTRAINT fk_time_slots_schedule FOREIGN KEY (schedule_id)
        REFERENCES doctor_schedules (id) ON DELETE CASCADE,
    CONSTRAINT fk_time_slots_doctor FOREIGN KEY (doctor_id)
        REFERENCES doctors (id) ON DELETE RESTRICT,
    CONSTRAINT fk_time_slots_cabinet FOREIGN KEY (cabinet_id)
        REFERENCES cabinets (id) ON DELETE RESTRICT,

    CONSTRAINT chk_time_slots_time CHECK (end_time > start_time),
    CONSTRAINT chk_time_slots_status CHECK (status IN ('free', 'reserved', 'booked')),
    CONSTRAINT uq_time_slots_doctor_interval UNIQUE (doctor_id, slot_date, start_time)
);

COMMENT ON TABLE time_slots IS 'Дискретные интервалы записи пациентов с шагом 30 минут';
COMMENT ON COLUMN time_slots.status IS 'Статус слота: free (свободен), reserved (удержание 10 мин), booked (занят)';

-- -----------------------------------------------------------------------------
-- 8. Таблица `patients`: Картотека пациентов клиники
-- -----------------------------------------------------------------------------
CREATE TABLE patients (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    full_name VARCHAR(120) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    email VARCHAR(120),
    birth_date DATE,
    telegram_chat_id BIGINT,
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT uq_patients_phone UNIQUE (phone),
    CONSTRAINT uq_patients_telegram UNIQUE (telegram_chat_id)
);

COMMENT ON TABLE patients IS 'Реестр пациентов клиники (идентификация по номеру телефона)';

-- -----------------------------------------------------------------------------
-- 9. Таблица `appointments`: Журнал записей на приём
-- -----------------------------------------------------------------------------
CREATE TABLE appointments (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    appointment_number VARCHAR(32) NOT NULL,
    patient_id BIGINT NOT NULL,
    doctor_id BIGINT NOT NULL,
    slot_id BIGINT NOT NULL,
    cabinet_id BIGINT NOT NULL,
    created_by_user_id BIGINT,
    status VARCHAR(20) NOT NULL DEFAULT 'confirmed',
    patient_complaint TEXT,
    doctor_notes TEXT,
    is_confirmed_by_patient BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Ограничения уникальности (UNIQUE)
    CONSTRAINT uq_appointments_number UNIQUE (appointment_number),
    CONSTRAINT uq_appointments_slot UNIQUE (slot_id),

    -- Внешние ключи с явными правилами ON DELETE
    CONSTRAINT fk_appointments_patient FOREIGN KEY (patient_id)
        REFERENCES patients (id) ON DELETE RESTRICT,
    CONSTRAINT fk_appointments_doctor FOREIGN KEY (doctor_id)
        REFERENCES doctors (id) ON DELETE RESTRICT,
    CONSTRAINT fk_appointments_slot FOREIGN KEY (slot_id)
        REFERENCES time_slots (id) ON DELETE RESTRICT,
    CONSTRAINT fk_appointments_cabinet FOREIGN KEY (cabinet_id)
        REFERENCES cabinets (id) ON DELETE RESTRICT,
    CONSTRAINT fk_appointments_created_by FOREIGN KEY (created_by_user_id)
        REFERENCES users (id) ON DELETE SET NULL,

    -- Ограничения проверок (CHECK)
    CONSTRAINT chk_appointments_status CHECK (status IN (
        'confirmed', 'arrived', 'in_progress', 'completed', 'cancelled', 'no_show'
    ))
);

COMMENT ON TABLE appointments IS 'Главная операционная таблица приёмов пациентов';
COMMENT ON COLUMN appointments.created_by_user_id IS 'ID регистратора при ручной записи или NULL при онлайн-бронировании';

-- -----------------------------------------------------------------------------
-- 10. Таблица `appointment_services`: Оказанные услуги в приёме (Связь M:N)
-- -----------------------------------------------------------------------------
CREATE TABLE appointment_services (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    appointment_id BIGINT NOT NULL,
    service_id BIGINT NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    price_at_booking NUMERIC(10,2) NOT NULL,

    CONSTRAINT fk_app_services_appointment FOREIGN KEY (appointment_id)
        REFERENCES appointments (id) ON DELETE CASCADE,
    CONSTRAINT fk_app_services_service FOREIGN KEY (service_id)
        REFERENCES dental_services (id) ON DELETE RESTRICT,

    CONSTRAINT chk_app_services_quantity CHECK (quantity > 0),
    CONSTRAINT chk_app_services_price CHECK (price_at_booking >= 0),
    CONSTRAINT uq_appointment_service UNIQUE (appointment_id, service_id)
);

COMMENT ON TABLE appointment_services IS 'Таблица-связка M:N между приёмами и услугами с фиксацией исторической цены';

-- -----------------------------------------------------------------------------
-- 11. Таблица `notifications`: Очередь и журнал Telegram-уведомлений
-- -----------------------------------------------------------------------------
CREATE TABLE notifications (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    appointment_id BIGINT NOT NULL,
    telegram_chat_id BIGINT NOT NULL,
    type VARCHAR(30) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'pending',
    scheduled_at TIMESTAMPTZ NOT NULL,
    sent_at TIMESTAMPTZ,
    response_payload TEXT,

    CONSTRAINT fk_notifications_appointment FOREIGN KEY (appointment_id)
        REFERENCES appointments (id) ON DELETE CASCADE,

    CONSTRAINT chk_notifications_type CHECK (type IN (
        'confirmation', 'reminder_24h', 'reminder_2h', 'cancellation'
    )),
    CONSTRAINT chk_notifications_status CHECK (status IN ('pending', 'sent', 'failed'))
);

COMMENT ON TABLE notifications IS 'Очередь сообщений и журнал доставки уведомлений через Telegram Bot';

-- -----------------------------------------------------------------------------
-- 12. Индексы для оптимизации поиска и аналитических запросов
-- -----------------------------------------------------------------------------

-- Поиск пациента по номеру телефона (ФТ-04, ФТ-10)
CREATE INDEX idx_patients_phone ON patients (phone);

-- Быстрый поиск свободных слотов врача по дате и статусу (ФТ-03)
CREATE INDEX idx_time_slots_lookup ON time_slots (doctor_id, slot_date, status);

-- Фильтрация приёмов по врачу и дате создания (ФТ-05, ФТ-06)
CREATE INDEX idx_appointments_doctor_date ON appointments (doctor_id, created_at);

-- Выборка приёмов по статусу для интерактивной сетки Timeline (ФТ-05)
CREATE INDEX idx_appointments_status ON appointments (status);

-- Поиск активных услуг по категории (ФТ-02, ФТ-09)
CREATE INDEX idx_dental_services_category_active ON dental_services (category_id, is_active);

-- Выборка неотправленных уведомлений фоновым демоном (ФТ-07)
CREATE INDEX idx_notifications_status_scheduled ON notifications (status, scheduled_at);

-- -----------------------------------------------------------------------------
-- 13. Триггеры для автоматического обновления поля `updated_at`
-- -----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION trigger_set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW
    EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER trg_appointments_updated_at
    BEFORE UPDATE ON appointments
    FOR EACH ROW
    EXECUTE FUNCTION trigger_set_updated_at();
