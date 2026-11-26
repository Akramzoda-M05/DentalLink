<?php

declare(strict_types=1);

define('BASE_PATH', dirname(__DIR__));

// Composer — основной PSR-4 загрузчик; fallback сохраняет запуск без vendor/.
$composerAutoload = BASE_PATH . '/vendor/autoload.php';
if (is_file($composerAutoload)) {
    require $composerAutoload;
} else {
    spl_autoload_register(static function (string $class): void {
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
}

use App\Controllers\DoctorController;
use App\Controllers\HomeController;
use App\Controllers\ReportController;
use App\Core\Database;
use App\Core\Router;
use App\Core\View;

$configFile = BASE_PATH . '/config/config.php';
if (!is_file($configFile)) {
    $configFile = BASE_PATH . '/config/config.example.php'; // значения по умолчанию для XAMPP
}
$config = require $configFile;

ini_set('display_errors', $config['debug'] ? '1' : '0');
error_reporting(E_ALL);

Database::configure($config['db']);

// Любое непойманное исключение → страница 500 (подробности — только в режиме debug)
set_exception_handler(function (Throwable $e) use ($config): void {
    error_log((string) $e);
    http_response_code(500);
    echo (new View(BASE_PATH . '/views'))->render('errors/error', [
        'title'   => 'Внутренняя ошибка сервера',
        'code'    => 500,
        'message' => $config['debug'] ? $e->getMessage() : 'Попробуйте позже или обратитесь к администратору.',
    ]);
});

$router = new Router();

// Главная перенаправляет в каталог врачей
$router->get('/', [HomeController::class, 'index']);

// CRUD главной сущности (doctors)
$router->get('/doctors', [DoctorController::class, 'index']);
$router->get('/doctors/create', [DoctorController::class, 'create']);
$router->post('/doctors/create', [DoctorController::class, 'store']);
$router->get('/doctors/{id}', [DoctorController::class, 'show']);
$router->get('/doctors/{id}/edit', [DoctorController::class, 'edit']);
$router->post('/doctors/{id}/edit', [DoctorController::class, 'update']);
$router->post('/doctors/{id}/delete', [DoctorController::class, 'delete']);

// Алиасы /employees (для совместимости с тестами методички)
$router->get('/employees', [DoctorController::class, 'index']);
$router->get('/employees/create', [DoctorController::class, 'create']);
$router->post('/employees/create', [DoctorController::class, 'store']);
$router->get('/employees/{id}', [DoctorController::class, 'show']);
$router->get('/employees/{id}/edit', [DoctorController::class, 'edit']);
$router->post('/employees/{id}/edit', [DoctorController::class, 'update']);
$router->post('/employees/{id}/delete', [DoctorController::class, 'delete']);

// Аналитические отчёты
$router->get('/reports/staff', [ReportController::class, 'staff']);
$router->get('/reports/doctors', [ReportController::class, 'staff']);

// Запуск диспетчера
$router->dispatch($_SERVER['REQUEST_METHOD'] ?? 'GET', $_SERVER['REQUEST_URI'] ?? '/');
