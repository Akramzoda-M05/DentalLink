<?php

namespace App\Repositories;

use App\Core\Database;
use PDO;

final class DoctorRepository
{
    /** Поля, которые разрешено сохранять из формы (белый список безопасности) */
    public const FIELDS = [
        'last_name',
        'first_name',
        'middle_name',
        'birth_date',
        'gender',
        'phone',
        'email',
        'specialization_id',
        'cabinet_id',
        'qualification',
        'experience_years',
        'hire_date',
        'status',
        'bio',
    ];

    /** Общая часть SELECT: врач + специализация + кабинет (JOIN 2 справочников) */
    private const SELECT = '
        SELECT d.*,
               s.name AS specialization_name,
               s.code AS specialization_code,
               c.name AS cabinet_name,
               c.chair_number AS cabinet_chair_number
        FROM doctors d
        JOIN doctor_specializations s ON s.id = d.specialization_id
        JOIN cabinets c ON c.id = d.cabinet_id';

    private PDO $db;

    public function __construct(?PDO $db = null)
    {
        $this->db = $db ?? Database::connection();
    }

    /**
     * Поиск и фильтрация с безопасным связыванием параметров
     */
    private function buildWhere(array $filters): array
    {
        $conditions = [];
        $params = [];

        $q = trim((string) ($filters['q'] ?? ''));
        if ($q !== '') {
            // Экранирование спецсимволов LIKE (% и _)
            // Поиск по ФИО целиком (находит и «Рахимов», и «Саидов Саид»)
            $conditions[] = "LOWER(CONCAT_WS(' ', d.last_name, d.first_name, d.middle_name)) LIKE :q";
            $params[':q'] = '%' . mb_strtolower(addcslashes($q, '%_\\'), 'UTF-8') . '%';
        }

        if (!empty($filters['specialization_id'])) {
            $conditions[] = 'd.specialization_id = :specialization_id';
            $params[':specialization_id'] = (int) $filters['specialization_id'];
        }

        if (!empty($filters['cabinet_id'])) {
            $conditions[] = 'd.cabinet_id = :cabinet_id';
            $params[':cabinet_id'] = (int) $filters['cabinet_id'];
        }

        if (!empty($filters['status'])) {
            $conditions[] = 'd.status = :status';
            $params[':status'] = (string) $filters['status'];
        }

        if (!empty($filters['min_experience'])) {
            $conditions[] = 'd.experience_years >= :min_experience';
            $params[':min_experience'] = (int) $filters['min_experience'];
        }

        $where = $conditions ? 'WHERE ' . implode(' AND ', $conditions) : '';
        return [$where, $params];
    }

    /**
     * Пагинация с безопасной передачей LIMIT и OFFSET через PDO::PARAM_INT
     */
    public function paginate(array $filters, int $page, int $perPage = 10): array
    {
        [$where, $params] = $this->buildWhere($filters);

        // 1) Сколько всего записей подходит под фильтр
        $countSql = "SELECT COUNT(*) FROM doctors d {$where}";
        $stmt = $this->db->prepare($countSql);
        $stmt->execute($params);
        $total = (int) $stmt->fetchColumn();

        // 2) Расчёт количества страниц и нормализация номера страницы (минимум 1, максимум $pages)
        $pages = max(1, (int) ceil($total / $perPage));
        $page = min(max(1, $page), $pages);
        $offset = ($page - 1) * $perPage;

        // 3) Запрос нужной страницы
        $sql = self::SELECT . " {$where} ORDER BY d.last_name, d.first_name, d.id LIMIT :limit OFFSET :offset";
        $stmt = $this->db->prepare($sql);

        foreach ($params as $name => $value) {
            $stmt->bindValue($name, $value);
        }
        $stmt->bindValue(':limit', $perPage, PDO::PARAM_INT);
        $stmt->bindValue(':offset', $offset, PDO::PARAM_INT);
        $stmt->execute();

        return [
            'items' => $stmt->fetchAll(),
            'total' => $total,
            'page'  => $page,
            'pages' => $pages,
        ];
    }

    /**
     * Получение всех врачей (например, для API)
     */
    public function all(array $filters = []): array
    {
        [$where, $params] = $this->buildWhere($filters);
        $sql = self::SELECT . " {$where} ORDER BY d.last_name, d.first_name, d.id";
        $stmt = $this->db->prepare($sql);
        $stmt->execute($params);
        return $stmt->fetchAll();
    }

    /**
     * Поиск одного врача по ID
     */
    public function find(int $id): ?array
    {
        $stmt = $this->db->prepare(self::SELECT . ' WHERE d.id = :id LIMIT 1');
        $stmt->execute([':id' => $id]);
        $row = $stmt->fetch();
        return $row ?: null;
    }

    /**
     * Добавление врача
     */
    public function create(array $data): int
    {
        $columns = implode(', ', self::FIELDS);
        $placeholders = ':' . implode(', :', self::FIELDS);

        $stmt = $this->db->prepare("INSERT INTO doctors ({$columns}) VALUES ({$placeholders})");
        $stmt->execute($this->onlyFields($data));

        return (int) $this->db->lastInsertId();
    }

    /**
     * Обновление данных врача
     */
    public function update(int $id, array $data): void
    {
        $set = implode(', ', array_map(fn ($f) => "{$f} = :{$f}", self::FIELDS));
        $stmt = $this->db->prepare("UPDATE doctors SET {$set} WHERE id = :id");
        $stmt->execute($this->onlyFields($data) + ['id' => $id]);
    }

    /**
     * Удаление врача
     */
    public function delete(int $id): void
    {
        $stmt = $this->db->prepare('DELETE FROM doctors WHERE id = :id');
        $stmt->execute([':id' => $id]);
    }

    /**
     * Проверка уникальности e-mail (исключая самого врача при редактировании)
     */
    public function emailExists(string $email, ?int $excludeId = null): bool
    {
        $sql = 'SELECT 1 FROM doctors WHERE email = :email';
        $params = [':email' => $email];

        if ($excludeId !== null) {
            $sql .= ' AND id <> :exclude_id';
            $params[':exclude_id'] = $excludeId;
        }

        $stmt = $this->db->prepare($sql . ' LIMIT 1');
        $stmt->execute($params);
        return (bool) $stmt->fetchColumn();
    }

    /**
     * Проверка уникальности телефона
     */
    public function phoneExists(string $phone, ?int $excludeId = null): bool
    {
        $sql = 'SELECT 1 FROM doctors WHERE phone = :phone';
        $params = [':phone' => $phone];

        if ($excludeId !== null) {
            $sql .= ' AND id <> :exclude_id';
            $params[':exclude_id'] = $excludeId;
        }

        $stmt = $this->db->prepare($sql . ' LIMIT 1');
        $stmt->execute($params);
        return (bool) $stmt->fetchColumn();
    }

    /**
     * Подсчёт врачей по статусу для информационной панели
     */
    public function countByStatus(): array
    {
        $stmt = $this->db->query('SELECT status, COUNT(*) as cnt FROM doctors GROUP BY status');
        $res = [];
        while ($row = $stmt->fetch()) {
            $res[$row['status']] = (int) $row['cnt'];
        }
        return $res;
    }

    /**
     * Отчёт «Штат стоматологической клиники по направлениям» с условной агрегацией
     */
    public function staffReport(): array
    {
        $sql = "
            SELECT s.id AS specialization_id,
                   s.name AS specialization_name,
                   COUNT(d.id) AS total,
                   COALESCE(SUM(d.status = 'active'), 0) AS active_count,
                   COALESCE(SUM(d.status = 'on_leave'), 0) AS on_leave_count,
                   COALESCE(SUM(d.experience_years >= 10), 0) AS experienced_count,
                   COALESCE(ROUND(AVG(d.experience_years), 1), 0) AS avg_experience
            FROM doctor_specializations s
            LEFT JOIN doctors d ON d.specialization_id = s.id AND d.status <> 'dismissed'
            GROUP BY s.id, s.name
            ORDER BY s.name";

        return $this->db->query($sql)->fetchAll();
    }

    /**
     * Фильтрация входного массива по белому списку полей FIELDS
     */
    public function onlyFields(array $data): array
    {
        $clean = [];
        foreach (self::FIELDS as $field) {
            $val = $data[$field] ?? null;
            if ($val === '') {
                // Пустые необязательные поля превращаются в null
                $clean[$field] = in_array($field, ['middle_name', 'bio'], true) ? null : '';
            } else {
                $clean[$field] = $val;
            }
        }
        return $clean;
    }
}
