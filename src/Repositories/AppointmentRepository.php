<?php

namespace App\Repositories;

use App\Core\Database;
use PDO;

final class AppointmentRepository
{
    private PDO $db;

    public function __construct(?PDO $db = null)
    {
        $this->db = $db ?? Database::connection();
    }

    /**
     * Количество записей пациентов к данному врачу
     */
    public function countByDoctor(int $doctorId): int
    {
        $stmt = $this->db->prepare('SELECT COUNT(*) FROM appointments WHERE doctor_id = :doctor_id');
        $stmt->execute([':doctor_id' => $doctorId]);
        return (int) $stmt->fetchColumn();
    }

    /**
     * Список приёмов врача
     */
    public function allByDoctor(int $doctorId): array
    {
        // В таблице appointments уже хранится снимок ФИО и телефона пациента.
        // Это также работает с учебной SQLite-схемой, где таблицы patients может не быть.
        $stmt = $this->db->prepare('
            SELECT *
            FROM appointments
            WHERE doctor_id = :doctor_id
            ORDER BY appointment_date DESC
        ');
        $stmt->execute([':doctor_id' => $doctorId]);
        return $stmt->fetchAll();
    }
}
