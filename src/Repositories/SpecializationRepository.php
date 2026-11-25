<?php

namespace App\Repositories;

use App\Core\Database;
use PDO;

final class SpecializationRepository
{
    private PDO $db;

    public function __construct(?PDO $db = null)
    {
        $this->db = $db ?? Database::connection();
    }

    /**
     * Получение всех специализаций для выпадающих списков
     */
    public function all(): array
    {
        $stmt = $this->db->query('SELECT id, code, name FROM doctor_specializations ORDER BY name ASC');
        return $stmt->fetchAll();
    }

    /**
     * Список ID специализаций для валидатора (in rule)
     */
    public function ids(): array
    {
        $stmt = $this->db->query('SELECT id FROM doctor_specializations');
        return $stmt->fetchAll(PDO::FETCH_COLUMN);
    }

    /**
     * Поиск специализации по ID
     */
    public function find(int $id): ?array
    {
        $stmt = $this->db->prepare('SELECT id, code, name FROM doctor_specializations WHERE id = :id LIMIT 1');
        $stmt->execute([':id' => $id]);
        $row = $stmt->fetch();
        return $row ?: null;
    }
}
