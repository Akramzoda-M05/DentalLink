<?php

declare(strict_types=1);

namespace App\Core;

use RuntimeException;

/**
 * Шаблонизатор на чистом PHP.
 * Внутри шаблона $this — этот объект: e(), url(), partial(), date(), label().
 */
final class View
{
    /** Русские подписи для значений ENUM. */
    public const LABELS = [
        'doctor_status' => [
            'active'    => 'Ведёт приём',
            'on_leave'  => 'В отпуске',
            'dismissed' => 'Уволен',
        ],
        'gender' => [
            'male'   => 'Мужской',
            'female' => 'Женский',
            'm'      => 'Мужской',
            'f'      => 'Женский',
        ],
        'appointment_status' => [
            'confirmed' => 'Подтверждено',
            'pending'   => 'Ожидает',
            'cancelled' => 'Отменено',
            'completed' => 'Завершено',
        ],
    ];

    private string $viewsPath;

    public function __construct(string $viewsPath)
    {
        $this->viewsPath = rtrim($viewsPath, '/\\');
    }

    /**
     * Сначала рисует страницу, потом вставляет её в макет как $content.
     */
    public function render(string $template, array $data = [], ?string $layout = 'layout'): string
    {
        $content = $this->renderFile($template, $data);
        if ($layout === null) {
            return $content;
        }

        return $this->renderFile($layout, $data + ['content' => $content]);
    }

    private function renderFile(string $template, array $data): string
    {
        $relative = str_replace('.', '/', $template);
        $file = $this->viewsPath . '/' . $relative . '.php';
        if (!is_file($file)) {
            throw new RuntimeException("Шаблон не найден: {$template}");
        }

        // extract() превращает ['doctors' => [...]] в переменную $doctors
        extract($data, EXTR_SKIP);

        ob_start();
        require $file;

        return (string) ob_get_clean();
    }

    /** Фрагмент из views/partials/. */
    public function partial(string $name, array $data = []): string
    {
        return $this->renderFile('partials/' . $name, $data);
    }

    /** Экранирование для вывода в HTML — защита от XSS. */
    public function e(mixed $value): string
    {
        return htmlspecialchars((string) $value, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8');
    }

    /**
     * Полный адрес с учётом базового пути.
     * url('/doctors') при XAMPP → '/DentalLink/public/doctors'.
     * Второй аргумент добавляет query string (пагинация и фильтры).
     *
     * @param array<string, mixed> $query
     */
    public function url(string $path = '/', array $query = []): string
    {
        if ($path === '') {
            $path = '/';
        }
        if ($path[0] !== '/') {
            $path = '/' . $path;
        }

        $url = Router::basePath() . $path;
        if ($query !== []) {
            $url .= (str_contains($path, '?') ? '&' : '?') . http_build_query($query);
        }

        return $url;
    }

    /** 2026-11-02 → 02.11.2026 */
    public function date(mixed $value): string
    {
        $raw = trim((string) $value);
        if ($raw === '') {
            return '';
        }

        $timestamp = strtotime($raw);
        if ($timestamp === false) {
            return $this->e($raw);
        }

        return date('d.m.Y', $timestamp);
    }

    /** label('doctor_status', 'on_leave') → «В отпуске» */
    public function label(string $group, mixed $value): string
    {
        $key = (string) $value;

        return self::LABELS[$group][$key] ?? $key;
    }

    /** @return array<string, string> */
    public function labels(string $group): array
    {
        return self::LABELS[$group] ?? [];
    }

    /** «Рахимов Алишер» → «РА» */
    public function initials(?string $lastName, ?string $firstName): string
    {
        $last = mb_strtoupper(mb_substr((string) $lastName, 0, 1));
        $first = mb_strtoupper(mb_substr((string) $firstName, 0, 1));

        return $last . $first;
    }
}
