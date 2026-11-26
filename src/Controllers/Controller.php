<?php

declare(strict_types=1);

namespace App\Controllers;

use App\Core\View;

abstract class Controller
{
    protected View $view;

    public function __construct()
    {
        $this->view = new View(BASE_PATH . '/views');
    }

    protected function render(string $template, array $data = [], int $status = 200, string $layout = 'layout'): void
    {
        http_response_code($status);
        echo $this->view->render($template, $data, $layout);
    }

    /** Перенаправление (HTTP 302) — после успешной отправки формы (шаблон PRG). */
    protected function redirect(string $path): never
    {
        header('Location: ' . $this->view->url($path));
        exit;
    }

    /** Ответ в формате JSON (для API). */
    protected function json(array $data, int $status = 200): never
    {
        http_response_code($status);
        header('Content-Type: application/json; charset=utf-8');
        echo json_encode($data, JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
        exit;
    }

    /** Страница ошибки (403, 404, 419) и завершение скрипта. */
    protected function abort(int $code, string $message = '', string $title = ''): never
    {
        $titles = [
            403 => 'Доступ запрещён',
            404 => 'Страница не найдена',
            419 => 'Страница устарела',
            500 => 'Внутренняя ошибка сервера',
        ];

        $this->render('errors/error', [
            'title'   => $title !== '' ? $title : ($titles[$code] ?? 'Ошибка'),
            'code'    => $code,
            'message' => $message !== '' ? $message : 'Запрос не может быть выполнен.',
        ], $code);
        exit;
    }
}
