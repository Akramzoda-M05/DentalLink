<?php
// public/api/doctors/index.php — ВРЕМЕННАЯ заглушка API для лабораторной работы 11.
// В лекции 14 маршрут /api/doctors появится в front controller, а эту папку нужно удалить.
declare(strict_types=1);

header('Content-Type: application/json; charset=utf-8');

$doctors = [
    [
        'id' => 1,
        'full_name' => 'Саидов Саид Комилович',
        'department' => 'Терапевтическое отделение',
        'position' => 'Главный врач, стоматолог-терапевт',
        'specialization' => 'Терапевтическая стоматология',
        'phone' => '+992 90 000 0001',
        'hire_date' => '2015-09-01',
        'status' => 'active',
    ],
    [
        'id' => 2,
        'full_name' => 'Рахимов Алишер Баходурович',
        'department' => 'Терапевтическое отделение',
        'position' => 'Врач-эндодонтист, микроскопист',
        'specialization' => 'Эндодонтия',
        'phone' => '+992 92 700 11 22',
        'hire_date' => '2018-03-15',
        'status' => 'active',
    ],
    [
        'id' => 3,
        'full_name' => 'Умаров Джамшед Каримович',
        'department' => 'Хирургическое отделение',
        'position' => 'Хирург-имплантолог, к.м.н.',
        'specialization' => 'Хирургическая стоматология',
        'phone' => '+992 93 500 33 44',
        'hire_date' => '2016-11-20',
        'status' => 'active',
    ],
    [
        'id' => 4,
        'full_name' => 'Турсунова Зарина Мурадовна',
        'department' => 'Ортодонтическое отделение',
        'position' => 'Врач-ортодонт высшей категории',
        'specialization' => 'Ортодонтия (брекеты, элайнеры)',
        'phone' => '+992 98 800 55 66',
        'hire_date' => '2019-08-01',
        'status' => 'on_leave',
    ],
    [
        'id' => 5,
        'full_name' => 'Бобоев Сино Рустамович',
        'department' => 'Детское отделение',
        'position' => 'Детский врач-стоматолог',
        'specialization' => 'Детская стоматология',
        'phone' => '+992 91 800 77 88',
        'hire_date' => '2021-02-10',
        'status' => 'active',
    ],
    [
        'id' => 6,
        'full_name' => 'Давлатов Рустам Комронович',
        'department' => 'Отделение профилактики',
        'position' => 'Стоматолог-гигиенист',
        'specialization' => 'Профессиональная гигиена и отбеливание',
        'phone' => '+992 90 111 22 33',
        'hire_date' => '2022-05-18',
        'status' => 'active',
    ],
    // Запись для проверки экранирования (XSS): такое «имя» не должно выполниться как HTML
    [
        'id' => 7,
        'full_name' => '<img src=x onerror="alert(\'XSS\')"> Тестов Доктор Врачевич',
        'department' => 'Тестовая кафедра стоматологии',
        'position' => 'Врач-стажёр (XSS-тест)',
        'specialization' => 'Тестирование безопасности',
        'phone' => null,
        'hire_date' => '2024-01-10',
        'status' => 'active',
    ],
];

$q = mb_strtolower(trim((string) ($_GET['q'] ?? '')));

if ($q !== '') {
    $doctors = array_values(array_filter(
        $doctors,
        fn (array $d): bool => str_contains(mb_strtolower($d['full_name']), $q)
            || str_contains(mb_strtolower($d['department']), $q)
            || str_contains(mb_strtolower($d['position']), $q)
    ));
}

echo json_encode($doctors, JSON_UNESCAPED_UNICODE | JSON_PRETTY_PRINT);
