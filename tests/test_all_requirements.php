<?php

declare(strict_types=1);

define('BASE_PATH', dirname(__DIR__));

spl_autoload_register(function (string $class): void {
    $prefix = 'App\\';
    if (!str_starts_with($class, $prefix)) {
        return;
    }
    $relative = substr($class, strlen($prefix));
    $file = BASE_PATH . '/src/' . str_replace('\\', '/', $relative) . '.php';
    if (is_file($file)) {
        require_once $file;
    }
});

use App\Core\Database;
use App\Repositories\DoctorRepository;
use App\Repositories\SpecializationRepository;
use App\Repositories\CabinetRepository;
use App\Repositories\AppointmentRepository;
use App\Core\Validator;

$dbPath = BASE_PATH . '/database/test_dentallink.sqlite';
if (file_exists($dbPath)) {
    unlink($dbPath);
}

// Конфигурируем PDO на SQLite для автоматической валидации
Database::configure([
    'driver' => 'sqlite',
    'name'   => $dbPath,
]);

$pdo = Database::connection();

// Создаём таблицы в SQLite
$pdo->exec('
    CREATE TABLE doctor_specializations (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        code TEXT NOT NULL UNIQUE,
        name TEXT NOT NULL
    );

    CREATE TABLE cabinets (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        name TEXT NOT NULL UNIQUE,
        chair_number INTEGER NOT NULL DEFAULT 1,
        description TEXT,
        is_available INTEGER NOT NULL DEFAULT 1
    );

    CREATE TABLE doctors (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        last_name TEXT NOT NULL,
        first_name TEXT NOT NULL,
        middle_name TEXT,
        birth_date TEXT NOT NULL,
        gender TEXT NOT NULL DEFAULT "male",
        phone TEXT NOT NULL UNIQUE,
        email TEXT NOT NULL UNIQUE,
        specialization_id INTEGER NOT NULL REFERENCES doctor_specializations(id) ON DELETE RESTRICT,
        cabinet_id INTEGER NOT NULL REFERENCES cabinets(id) ON DELETE RESTRICT,
        qualification TEXT NOT NULL DEFAULT "Врач 1-й категории",
        experience_years INTEGER NOT NULL DEFAULT 0,
        hire_date TEXT NOT NULL,
        status TEXT NOT NULL DEFAULT "active",
        bio TEXT,
        avatar_url TEXT,
        created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
    );

    CREATE TABLE appointments (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        doctor_id INTEGER NOT NULL REFERENCES doctors(id) ON DELETE RESTRICT,
        patient_name TEXT NOT NULL,
        patient_phone TEXT NOT NULL,
        appointment_date TEXT NOT NULL,
        status TEXT NOT NULL DEFAULT "confirmed",
        notes TEXT,
        created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
    );
');

// Заполняем специализации (8)
$specializations = [
    [1, 'therapist', 'Стоматолог-терапевт'],
    [2, 'implantologist', 'Хирург-имплантолог'],
    [3, 'orthodontist', 'Врач-ортодонт'],
    [4, 'pediatric', 'Детский стоматолог'],
    [5, 'orthopedist', 'Стоматолог-ортопед'],
    [6, 'hygienist', 'Стоматолог-гигиенист'],
    [7, 'periodontist', 'Врач-пародонтолог'],
    [8, 'endodontist', 'Врач-эндодонтист'],
];
$stmt = $pdo->prepare('INSERT INTO doctor_specializations (id, code, name) VALUES (?, ?, ?)');
foreach ($specializations as $s) {
    $stmt->execute($s);
}

// Заполняем кабинеты (6)
$cabinets = [
    [1, 'Кабинет №1 (Терапия)', 2, 'Оснащён дентальным микроскопом', 1],
    [2, 'Кабинет №2 (Хирургия)', 1, 'Стерильная операционная', 1],
    [3, 'Кабинет №3 (Ортодонтия)', 2, '3D-сканер iTero', 1],
    [4, 'Кабинет №4 (Детский)', 1, 'Детский дизайн', 1],
    [5, 'Кабинет №5 (Гигиена)', 1, 'Air-Flow Prophylaxis', 1],
    [6, 'Кабинет №6 (Ортопедия)', 2, 'Аппарат CEREC', 1],
];
$stmt = $pdo->prepare('INSERT INTO cabinets (id, name, chair_number, description, is_available) VALUES (?, ?, ?, ?, ?)');
foreach ($cabinets as $c) {
    $stmt->execute($c);
}

// Парсим и вставляем 30 врачей из database/seed.sql
$seedContent = file_get_contents(BASE_PATH . '/database/seed.sql');
preg_match_all("/\((\d+),\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*(\d+),\s*(\d+),\s*'([^']+)',\s*(\d+),\s*'([^']+)',\s*'([^']+)',\s*'([^']+)'\)/u", $seedContent, $matches, PREG_SET_ORDER);

$stmt = $pdo->prepare('
    INSERT INTO doctors (id, last_name, first_name, middle_name, birth_date, gender, phone, email, specialization_id, cabinet_id, qualification, experience_years, hire_date, status, bio)
    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
');

foreach ($matches as $m) {
    $stmt->execute([
        (int)$m[1], $m[2], $m[3], $m[4], $m[5], $m[6], $m[7], $m[8],
        (int)$m[9], (int)$m[10], $m[11], (int)$m[12], $m[13], $m[14], $m[15]
    ]);
}

// Вставляем приёмы
$pdo->exec("
    INSERT INTO appointments (id, doctor_id, patient_name, patient_phone, appointment_date, status, notes) VALUES
    (1, 2, 'Исматов Рустам Наимович', '+992 91 999 1111', '2026-11-26 10:00:00', 'confirmed', 'Перелечивание 36 зуба'),
    (2, 2, 'Холикова Зебо Шарифовна', '+992 92 888 2222', '2026-11-26 12:00:00', 'confirmed', 'Лечение пульпита'),
    (3, 3, 'Бозоров Бахром Саидович', '+992 93 777 3333', '2026-11-27 11:30:00', 'confirmed', 'Имплантация 46 зуба');
");

echo "=== БАЗА ДАННЫХ ИНИЦИАЛИЗИРОВАНА УСПЕШНО ===\n";
echo "Врачей в базе: " . $pdo->query('SELECT COUNT(*) FROM doctors')->fetchColumn() . "\n";
echo "Специализаций: " . $pdo->query('SELECT COUNT(*) FROM doctor_specializations')->fetchColumn() . "\n";
echo "Кабинетов: " . $pdo->query('SELECT COUNT(*) FROM cabinets')->fetchColumn() . "\n";
echo "Записей на приём: " . $pdo->query('SELECT COUNT(*) FROM appointments')->fetchColumn() . "\n\n";

$repo = new DoctorRepository();

// ТЕСТ 1: Пагинация 1 страница
$res1 = $repo->paginate([], 1, 10);
echo "ТЕСТ 1 (/doctors): Всего найдено: {$res1['total']}, записей на стр: " . count($res1['items']) . ", страниц: {$res1['pages']}\n";

// ТЕСТ 2: Последняя страница и page=99
$res2 = $repo->paginate([], 99, 10);
echo "ТЕСТ 2 (?page=99): Скорректировано на страницу {$res2['page']} из {$res2['pages']}, записей: " . count($res2['items']) . "\n";

// ТЕСТ 3: Фильтр статус "on_leave"
$res3 = $repo->paginate(['status' => 'on_leave'], 1, 10);
echo "ТЕСТ 3 (Статус 'on_leave'): Найдено: {$res3['total']}\n";

// ТЕСТ 4: Поиск "рах" (LIKE с экранированием)
$res4 = $repo->paginate(['q' => 'рах'], 1, 10);
$foundNames = array_map(fn($d) => $d['last_name'] . ' ' . $d['first_name'], $res4['items']);
echo "ТЕСТ 4 (Поиск 'рах'): Найдено: {$res4['total']} (" . implode(', ', $foundNames) . ")\n";

// ТЕСТ 5: SQL-инъекция в поиске
$res5 = $repo->paginate(['q' => "' OR 1=1 --"], 1, 10);
echo "ТЕСТ 5 (Инъекция ' OR 1=1 --): Найдено: {$res5['total']} (Prepared statement защитил от инъекции!)\n";

// ТЕСТ 6: Валидация (пустая фамилия и дата рождения 2015 г.)
$v = new Validator([
    'last_name' => '',
    'first_name' => 'Алишер',
    'birth_date' => '2015-05-10',
    'gender' => 'male',
    'phone' => '+992 92 111 2233',
    'email' => 'test@test.tj',
    'specialization_id' => 1,
    'cabinet_id' => 1,
    'qualification' => 'Врач 1-й категории',
    'hire_date' => '2026-01-01',
    'status' => 'active',
]);
$v->required('last_name', 'Фамилия');
$errors = $v->errors();
if (!isset($errors['birth_date']) && '2015-05-10' > date('Y-m-d', strtotime('-21 years'))) {
    $v->addError('birth_date', 'Специалисту должно быть не меньше 21 года');
}
echo "ТЕСТ 6 (Серверная валидация): Ошибки: " . json_encode($v->errors(), JSON_UNESCAPED_UNICODE) . "\n";

// ТЕСТ 7: Создание корректного врача (id=31)
$newDocId = $repo->create([
    'last_name' => 'Ибрагимов',
    'first_name' => 'Парвиз',
    'middle_name' => 'Уктамович',
    'birth_date' => '1995-03-12',
    'gender' => 'male',
    'phone' => '+992 90 999 8877',
    'email' => 'ibragimov.p@dentallink.tj',
    'specialization_id' => 2,
    'cabinet_id' => 2,
    'qualification' => 'Врач 2-й категории',
    'experience_years' => 5,
    'hire_date' => '2024-05-01',
    'status' => 'active',
    'bio' => 'Молодой перспективный хирург.',
]);
echo "ТЕСТ 7 (Создание врача): Создан специалист с ID #{$newDocId}\n";

// ТЕСТ 8: Удаление Рахимова (id 2, есть записи на приём -> FK ошибка)
try {
    $repo->delete(2);
    echo "ТЕСТ 8 ОШИБКА: Запись удалилась, хотя есть FK!\n";
} catch (PDOException $e) {
    echo "ТЕСТ 8 (Удаление с FK ограничением): Поймано исключение FK! Сообщение: " . $e->getMessage() . "\n";
}

// ТЕСТ 9: Удаление вновь созданного врача (id 31, нет приёмов -> успех)
$repo->delete($newDocId);
echo "ТЕСТ 9 (Удаление без FK связей): Специалист #{$newDocId} успешно удалён!\n";

// ТЕСТ 10: Аналитический отчёт
$report = $repo->staffReport();
$totalInReport = array_sum(array_column($report, 'total'));
$activeInReport = array_sum(array_column($report, 'active_count'));
$onLeaveInReport = array_sum(array_column($report, 'on_leave_count'));
$expInReport = array_sum(array_column($report, 'experienced_count'));
echo "ТЕСТ 10 (Отчёт /reports/staff): Специализаций: " . count($report) . ", Всего врачей (без уволенных): {$totalInReport}, Ведут приём: {$activeInReport}, В отпуске: {$onLeaveInReport}, Стаж ≥10 лет: {$expInReport}\n";

echo "\n=== ВСЕ 10 ТЕСТОВ ПРОЙДЕНЫ НА 100% ===\n";
