<?php

namespace App\Repositories;

use App\Core\Database;
use PDO;

final class CabinetRepository
{
    private PDO $db;

    public function __construct(?PDO $db = null)
    {
        $this->db = $db ?? Database::connection();
    }

    /**
     * Получение всех лечебных кабинетов
     */
    public function all(): array
    {
        $stmt = $this->db->query('SELECT id, name, chair_number, description, is_available FROM cabinets ORDER BY id ASC');
        return $stmt->fetchAll();
    }

    /**
     * Список ID кабинетов для валидатора (in rule)
     */
    public function ids(): array
    {
        $stmt = $this->db->query('SELECT id FROM cabinets');
        return $stmt->fetchAll(PDO::FETCH_COLUMN);
    }

    /**
     * Поиск кабинета по ID
     */
    public function find(int $id): ?array
    {
        $stmt = $this->db->prepare('SELECT id, name, chair_number, description, is_available FROM cabinets WHERE id = :id LIMIT 1');
        $stmt->execute([':id' => $id]);
        $row = $stmt->fetch();
        return $row ?: null;
    }
}
