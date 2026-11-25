-- =============================================================================
-- ИС «Онлайн-запись в стоматологию» (DentalLink)
-- Лабораторная работа 13: База данных MySQL/MariaDB (XAMPP / PDO)
-- Файл: database/schema.sql
-- Кодировка: UTF-8 (utf8mb4)
-- =============================================================================

CREATE DATABASE IF NOT EXISTS dentallink_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE dentallink_db;

SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS notifications;
DROP TABLE IF EXISTS appointment_services;
DROP TABLE IF EXISTS appointments;
DROP TABLE IF EXISTS patients;
DROP TABLE IF EXISTS time_slots;
DROP TABLE IF EXISTS doctor_schedules;
DROP TABLE IF EXISTS dental_services;
DROP TABLE IF EXISTS service_categories;
DROP TABLE IF EXISTS doctors;
DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS cabinets;
DROP TABLE IF EXISTS notification_statuses;
DROP TABLE IF EXISTS notification_types;
DROP TABLE IF EXISTS appointment_statuses;
DROP TABLE IF EXISTS time_slot_statuses;
DROP TABLE IF EXISTS doctor_specializations;
DROP TABLE IF EXISTS user_roles;

SET FOREIGN_KEY_CHECKS = 1;

-- -----------------------------------------------------------------------------
-- 1. Справочники
-- -----------------------------------------------------------------------------
CREATE TABLE user_roles (
    id INT PRIMARY KEY AUTO_INCREMENT,
    code VARCHAR(32) NOT NULL UNIQUE,
    name VARCHAR(80) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE doctor_specializations (
    id INT PRIMARY KEY AUTO_INCREMENT,
    code VARCHAR(32) NOT NULL UNIQUE,
    name VARCHAR(80) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE cabinets (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(80) NOT NULL UNIQUE,
    chair_number INT NOT NULL DEFAULT 1,
    description VARCHAR(255) NULL,
    is_available BOOLEAN NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE time_slot_statuses (
    id INT PRIMARY KEY AUTO_INCREMENT,
    code VARCHAR(32) NOT NULL UNIQUE,
    name VARCHAR(80) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE appointment_statuses (
    id INT PRIMARY KEY AUTO_INCREMENT,
    code VARCHAR(32) NOT NULL UNIQUE,
    name VARCHAR(80) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE notification_types (
    id INT PRIMARY KEY AUTO_INCREMENT,
    code VARCHAR(32) NOT NULL UNIQUE,
    name VARCHAR(80) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE notification_statuses (
    id INT PRIMARY KEY AUTO_INCREMENT,
    code VARCHAR(32) NOT NULL UNIQUE,
    name VARCHAR(80) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 2. Пользователи системы (администраторы, регистраторы)
-- -----------------------------------------------------------------------------
CREATE TABLE users (
    id INT PRIMARY KEY AUTO_INCREMENT,
    full_name VARCHAR(120) NOT NULL,
    email VARCHAR(120) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    phone VARCHAR(30) NOT NULL UNIQUE,
    role_id INT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_users_role FOREIGN KEY (role_id)
        REFERENCES user_roles (id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 3. Главная сущность: Врачи-стоматологи (doctors)
-- -----------------------------------------------------------------------------
CREATE TABLE doctors (
    id INT PRIMARY KEY AUTO_INCREMENT,
    last_name VARCHAR(60) NOT NULL,
    first_name VARCHAR(60) NOT NULL,
    middle_name VARCHAR(60) NULL,
    birth_date DATE NOT NULL,
    gender VARCHAR(10) NOT NULL DEFAULT 'male',
    phone VARCHAR(30) NOT NULL UNIQUE,
    email VARCHAR(120) NOT NULL UNIQUE,
    specialization_id INT NOT NULL,
    cabinet_id INT NOT NULL,
    qualification VARCHAR(100) NOT NULL DEFAULT 'Врач 1-й категории',
    experience_years INT NOT NULL DEFAULT 0,
    hire_date DATE NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'active',
    bio TEXT NULL,
    avatar_url VARCHAR(255) NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_doctors_specialization FOREIGN KEY (specialization_id)
        REFERENCES doctor_specializations (id) ON DELETE RESTRICT,
    CONSTRAINT fk_doctors_cabinet FOREIGN KEY (cabinet_id)
        REFERENCES cabinets (id) ON DELETE RESTRICT,
    CONSTRAINT chk_doctors_experience CHECK (experience_years >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 4. Услуги и категории услуг
-- -----------------------------------------------------------------------------
CREATE TABLE service_categories (
    id INT PRIMARY KEY AUTO_INCREMENT,
    code VARCHAR(32) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    display_order INT NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE dental_services (
    id INT PRIMARY KEY AUTO_INCREMENT,
    category_id INT NOT NULL,
    code VARCHAR(32) NOT NULL UNIQUE,
    name VARCHAR(150) NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    duration_minutes INT NOT NULL DEFAULT 30,
    is_active BOOLEAN NOT NULL DEFAULT 1,
    CONSTRAINT fk_services_category FOREIGN KEY (category_id)
        REFERENCES service_categories (id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 5. Пациенты
-- -----------------------------------------------------------------------------
CREATE TABLE patients (
    id INT PRIMARY KEY AUTO_INCREMENT,
    full_name VARCHAR(120) NOT NULL,
    phone VARCHAR(30) NOT NULL UNIQUE,
    email VARCHAR(120) NULL,
    birth_date DATE NULL,
    gender VARCHAR(10) NOT NULL DEFAULT 'male',
    address VARCHAR(255) NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 6. Записи на приём (appointments) — ссылаются на врачей с ON DELETE RESTRICT
-- -----------------------------------------------------------------------------
CREATE TABLE appointments (
    id INT PRIMARY KEY AUTO_INCREMENT,
    doctor_id INT NOT NULL,
    patient_id INT NULL,
    patient_name VARCHAR(120) NOT NULL,
    patient_phone VARCHAR(30) NOT NULL,
    appointment_date DATETIME NOT NULL,
    status VARCHAR(30) NOT NULL DEFAULT 'confirmed',
    notes TEXT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_appointments_doctor FOREIGN KEY (doctor_id)
        REFERENCES doctors (id) ON DELETE RESTRICT,
    CONSTRAINT fk_appointments_patient FOREIGN KEY (patient_id)
        REFERENCES patients (id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- 7. Индексы для ускорения поиска и фильтрации
-- -----------------------------------------------------------------------------
CREATE INDEX idx_doctors_specialization ON doctors(specialization_id);
CREATE INDEX idx_doctors_cabinet ON doctors(cabinet_id);
CREATE INDEX idx_doctors_status ON doctors(status);
CREATE INDEX idx_doctors_last_name ON doctors(last_name);
CREATE INDEX idx_appointments_doctor ON appointments(doctor_id);
