<?php

namespace App\Core;

use PDO;
use PDOException;

final class Database
{
    private static ?PDO $pdo = null;
    private static array $config = [];

    /** Вызывается один раз в public/index.php */
    public static function configure(array $config): void
    {
        self::$config = $config;
        self::$pdo = null;
    }

    public static function connection(): PDO
    {
        if (self::$pdo === null) {
            $c = self::$config;
            $driver = $c['driver'] ?? 'mysql';

            if ($driver === 'sqlite') {
                $dbPath = $c['name'];
                self::$pdo = new PDO('sqlite:' . $dbPath, null, null, [
                    PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
                    PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
                    PDO::ATTR_EMULATE_PREPARES => false,
                ]);
                self::$pdo->exec('PRAGMA foreign_keys = ON;');
                // Регистрация пользовательской функции CONCAT_WS для совместимости SQLite с запросами MySQL
                self::$pdo->sqliteCreateFunction('CONCAT_WS', function ($separator, ...$args) {
                    $parts = [];
                    foreach ($args as $a) {
                        if ($a !== null && $a !== '') {
                            $parts[] = (string) $a;
                        }
                    }
                    return implode($separator, $parts);
                });
                self::$pdo->sqliteCreateFunction('LOWER', function ($s) {
                    return mb_strtolower((string) $s, 'UTF-8');
                });
            } else {
                // charset=utf8mb4 — иначе кириллица превратится в «????»
                $dsn = sprintf(
                    'mysql:host=%s;port=%d;dbname=%s;charset=utf8mb4',
                    $c['host'] ?? '127.0.0.1',
                    $c['port'] ?? 3306,
                    $c['name'] ?? 'dentallink_db'
                );

                try {
                    self::$pdo = new PDO($dsn, $c['user'] ?? 'root', $c['pass'] ?? '', [
                        PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION, // ошибки SQL → исключения
                        PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC, // строки как ассоциативные массивы
                        PDO::ATTR_EMULATE_PREPARES => false, // настоящие prepared statements
                    ]);
                } catch (PDOException $e) {
                    if (!empty($c['sqlite_fallback'])) {
                        self::$pdo = new PDO('sqlite:' . $c['sqlite_fallback'], null, null, [
                            PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
                            PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
                            PDO::ATTR_EMULATE_PREPARES => false,
                        ]);
                        self::$pdo->exec('PRAGMA foreign_keys = ON;');
                        self::$pdo->sqliteCreateFunction('CONCAT_WS', function ($separator, ...$args) {
                            $parts = [];
                            foreach ($args as $a) {
                                if ($a !== null && $a !== '') {
                                    $parts[] = (string) $a;
                                }
                            }
                            return implode($separator, $parts);
                        });
                        self::$pdo->sqliteCreateFunction('LOWER', function ($s) {
                            return mb_strtolower((string) $s, 'UTF-8');
                        });
                    } else {
                        throw $e;
                    }
                }
            }
        }

        return self::$pdo;
    }

    public static function reset(): void
    {
        self::$pdo = null;
    }
}
