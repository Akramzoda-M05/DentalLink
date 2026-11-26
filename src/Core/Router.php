<?php

declare(strict_types=1);

namespace App\Core;

/**
 * Маршрутизатор: метод + шаблон пути → метод контроллера.
 * '/doctors/{id}/edit' → '#^/doctors/(?P<id>\d+)/edit$#'
 */
final class Router
{
    /** @var list<array{method: string, regex: string, handler: array{0: class-string, 1: string}|callable}> */
    private array $routes = [];

    public function get(string $path, array|callable $handler): void
    {
        $this->add('GET', $path, $handler);
    }

    public function post(string $path, array|callable $handler): void
    {
        $this->add('POST', $path, $handler);
    }

    private function add(string $method, string $path, array|callable $handler): void
    {
        $this->routes[] = [
            'method'  => $method,
            'regex'   => $this->toRegex($path),
            'handler' => $handler,
        ];
    }

    /**
     * '/doctors/{id}/edit'  →  '#^/doctors/(?P<id>\d+)/edit$#'
     * Параметры в фигурных скобках — только цифры (идентификаторы).
     */
    private function toRegex(string $path): string
    {
        $pattern = preg_replace('#\{(\w+)\}#', '(?P<$1>\d+)', $path);

        return '#^' . $pattern . '$#';
    }

    /**
     * Базовый путь приложения.
     * php -S localhost:8000 -t public → пустая строка.
     * XAMPP http://localhost/DentalLink/public → /DentalLink/public.
     */
    public static function basePath(): string
    {
        $dir = str_replace('\\', '/', dirname($_SERVER['SCRIPT_NAME'] ?? '/'));

        return rtrim($dir, '/');
    }

    public function dispatch(string $method, string $uri): void
    {
        $path = $this->normalizePath($uri);
        $allowedMethods = [];

        foreach ($this->routes as $route) {
            if (!preg_match($route['regex'], $path, $matches)) {
                continue;
            }

            if ($route['method'] !== $method) {
                $allowedMethods[] = $route['method'];
                continue;
            }

            // Оставляем только именованные параметры: ['id' => '15'] → [15]
            $params = array_filter($matches, 'is_string', ARRAY_FILTER_USE_KEY);
            $params = array_map('intval', array_values($params));

            $handler = $route['handler'];
            if (is_array($handler)) {
                [$class, $action] = $handler;
                (new $class())->$action(...$params);
                return;
            }

            $handler(...$params);
            return;
        }

        if ($allowedMethods !== []) {
            http_response_code(405);
            $allowedMethods = array_values(array_unique($allowedMethods));
            header('Allow: ' . implode(', ', $allowedMethods));
            $views = defined('BASE_PATH') ? BASE_PATH . '/views' : dirname(__DIR__, 2) . '/views';
            echo (new View($views))->render('errors/error', [
                'title'   => 'Метод не поддерживается',
                'code'    => 405,
                'message' => 'Для этого адреса доступны методы: ' . implode(', ', $allowedMethods) . '.',
            ]);
            return;
        }

        http_response_code(404);
        $views = defined('BASE_PATH') ? BASE_PATH . '/views' : dirname(__DIR__, 2) . '/views';
        echo (new View($views))->render('errors/error', [
            'title'   => 'Страница не найдена',
            'code'    => 404,
            'message' => 'Проверьте адрес или вернитесь на главную.',
        ]);
    }

    /**
     * Убирает строку запроса, завершающий слеш и базовый путь.
     */
    private function normalizePath(string $uri): string
    {
        $path = parse_url($uri, PHP_URL_PATH) ?: '/';
        $path = rawurldecode($path);

        $base = self::basePath();
        if ($base !== '' && str_starts_with($path, $base)) {
            $path = substr($path, strlen($base)) ?: '/';
        }

        if ($path === '') {
            $path = '/';
        }

        if ($path !== '/' && str_ends_with($path, '/')) {
            $path = rtrim($path, '/');
        }

        return $path === '' ? '/' : $path;
    }
}
