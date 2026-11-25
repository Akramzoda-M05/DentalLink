<?php

namespace App\Core;

final class View
{
    private string $basePath;

    public function __construct(string $basePath)
    {
        $this->basePath = rtrim($basePath, '/\\');
    }

    public function render(string $template, array $data = [], ?string $layout = 'layout'): string
    {
        $templatePath = $this->basePath . '/' . str_replace('.', '/', $template) . '.php';
        if (!is_file($templatePath)) {
            throw new \RuntimeException("Шаблон [{$template}] не найден по пути: {$templatePath}");
        }

        extract($data, EXTR_SKIP);

        ob_start();
        require $templatePath;
        $content = ob_get_clean();

        if ($layout !== null) {
            $layoutPath = $this->basePath . '/' . str_replace('.', '/', $layout) . '.php';
            if (is_file($layoutPath)) {
                ob_start();
                require $layoutPath;
                return ob_get_clean();
            }
        }

        return $content;
    }

    public function e(?string $value): string
    {
        return htmlspecialchars((string) $value, ENT_QUOTES | ENT_SUBSTITUTE, 'UTF-8');
    }

    public function url(string $path = '', array $params = []): string
    {
        $url = '/' . ltrim($path, '/');
        if (!empty($params)) {
            $query = http_build_query($params);
            $url .= (str_contains($url, '?') ? '&' : '?') . $query;
        }
        return $url;
    }
}
