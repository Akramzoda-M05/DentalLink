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
        $stmt = $this->db->prepare('
            SELECT a.*, p.full_name AS patient_full_name
            FROM appointments a
            LEFT JOIN patients p ON p.id = a.patient_id
            WHERE a.doctor_id = :doctor_id
            ORDER BY a.appointment_date DESC
        ');
        $stmt->execute([':doctor_id' => $doctorId]);
        return $stmt->fetchAll();
    }
}
