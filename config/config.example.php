<?php
// Скопируйте: cp config/config.example.php config/config.php — config.php НЕ попадает в Git
return [
    'db' => [
        'driver' => getenv('DB_DRIVER') ?: 'mysql',
        'host'   => getenv('DB_HOST') ?: '127.0.0.1',
        'port'   => (int) (getenv('DB_PORT') ?: 3306),
        'name'   => getenv('DB_NAME') ?: 'dentallink_db',
        'user'   => getenv('DB_USER') ?: 'root',
        'pass'   => getenv('DB_PASS') ?: '', // в XAMPP у root пароль пустой
    ],
    // true — показывать подробности ошибок (только при разработке!)
    'debug' => (getenv('APP_DEBUG') ?: '1') === '1',
];
