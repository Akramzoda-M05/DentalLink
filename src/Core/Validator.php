<?php

namespace App\Core;

final class Validator
{
    private array $data;
    private array $errors = [];

    public function __construct(array $data)
    {
        $this->data = $data;
    }

    public function required(string $field, string $label): self
    {
        if (isset($this->errors[$field])) {
            return $this;
        }

        $val = trim((string) ($this->data[$field] ?? ''));
        if ($val === '') {
            $this->errors[$field] = "Поле «{$label}» обязательно для заполнения";
        }

        return $this;
    }

    public function maxLength(string $field, int $max, string $label): self
    {
        if (isset($this->errors[$field])) {
            return $this;
        }

        $val = trim((string) ($this->data[$field] ?? ''));
        if ($val !== '' && mb_strlen($val, 'UTF-8') > $max) {
            $this->errors[$field] = "Поле «{$label}» не должно превышать {$max} символов";
        }

        return $this;
    }

    public function email(string $field, string $label = 'E-mail'): self
    {
        if (isset($this->errors[$field])) {
            return $this;
        }

        $val = trim((string) ($this->data[$field] ?? ''));
        if ($val !== '' && !filter_var($val, FILTER_VALIDATE_EMAIL)) {
            $this->errors[$field] = "Поле «{$label}» содержит некорректный адрес e-mail";
        }

        return $this;
    }

    public function date(string $field, string $label): self
    {
        if (isset($this->errors[$field])) {
            return $this;
        }

        $val = trim((string) ($this->data[$field] ?? ''));
        if ($val === '') {
            return $this;
        }

        // Строго ГГГГ-ММ-ДД
        if (!preg_match('/^\d{4}-\d{2}-\d{2}$/', $val)) {
            $this->errors[$field] = "Поле «{$label}» должно быть датой в формате ГГГГ-ММ-ДД";
            return $this;
        }

        [$year, $month, $day] = explode('-', $val);
        if (!checkdate((int) $month, (int) $day, (int) $year)) {
            $this->errors[$field] = "Поле «{$label}» содержит несуществующую календарную дату";
        }

        return $this;
    }

    public function in(string $field, array $allowed, string $label): self
    {
        if (isset($this->errors[$field])) {
            return $this;
        }

        $val = $this->data[$field] ?? null;
        if ($val === null || $val === '') {
            return $this;
        }

        $allowedStr = array_map('strval', $allowed);
        if (!in_array((string) $val, $allowedStr, true)) {
            $this->errors[$field] = "Значение поля «{$label}» недопустимо";
        }

        return $this;
    }

    public function pattern(string $field, string $regex, string $message): self
    {
        if (isset($this->errors[$field])) {
            return $this;
        }

        $val = trim((string) ($this->data[$field] ?? ''));
        if ($val !== '' && !preg_match($regex, $val)) {
            $this->errors[$field] = $message;
        }

        return $this;
    }

    public function addError(string $field, string $message): self
    {
        if (!isset($this->errors[$field])) {
            $this->errors[$field] = $message;
        }

        return $this;
    }

    public function errors(): array
    {
        return $this->errors;
    }

    public function fails(): bool
    {
        return !empty($this->errors);
    }

    public function passes(): bool
    {
        return empty($this->errors);
    }
}
