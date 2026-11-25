<?php

namespace App\Core;

final class Router
{
    private array $routes = [];

    public function get(string $path, array|callable $handler): void
    {
        $this->addRoute('GET', $path, $handler);
    }

    public function post(string $path, array|callable $handler): void
    {
        $this->addRoute('POST', $path, $handler);
    }

    private function addRoute(string $method, string $path, array|callable $handler): void
    {
        $regex = preg_replace('/\{([a-zA-Z0-9_]+)\}/', '(?P<$1>[^/]+)', $path);
        $regex = '#^' . $regex . '$#u';

        $this->routes[] = [
            'method'  => strtoupper($method),
            'path'    => $path,
            'regex'   => $regex,
            'handler' => $handler,
        ];
    }

    public function dispatch(string $method, string $uri): void
    {
        $method = strtoupper($method);
        $path = parse_url($uri, PHP_URL_PATH) ?: '/';

        foreach ($this->routes as $route) {
            if ($route['method'] !== $method) {
                continue;
            }

            if (preg_match($route['regex'], $path, $matches)) {
                $params = [];
                foreach ($matches as $key => $val) {
                    if (!is_int($key)) {
                        $params[$key] = is_numeric($val) ? (int) $val : $val;
                    }
                }

                $handler = $route['handler'];
                if (is_callable($handler)) {
                    call_user_func_array($handler, array_values($params));
                    return;
                }

                if (is_array($handler) && count($handler) === 2) {
                    [$class, $action] = $handler;
                    $controller = new $class();
                    call_user_func_array([$controller, $action], array_values($params));
                    return;
                }
            }
        }

        // Маршрут не найден
        http_response_code(404);
        echo (new View(BASE_PATH . '/views'))->render('errors/error', [
            'title'   => 'Страница не найдена',
            'code'    => 404,
            'message' => 'Запрашиваемый адрес ' . htmlspecialchars($path) . ' не существует на сервере.',
        ]);
    }
}
