<?php
/** @var string|null $msg */
$msg = $msg ?? ($_GET['msg'] ?? null);

$alerts = [
    'created' => [
        'type' => 'success',
        'title' => 'Успешно!',
        'text'  => 'Специалист успешно добавлен в штат клиники.',
    ],
    'updated' => [
        'type' => 'info',
        'title' => 'Обновлено!',
        'text'  => 'Данные специалиста успешно сохранены.',
    ],
    'deleted' => [
        'type' => 'success',
        'title' => 'Удалено!',
        'text'  => 'Специалист успешно удалён из базы клиники.',
    ],
    'has_appointments' => [
        'type' => 'warning',
        'title' => 'Действие отклонено СУБД (ON DELETE RESTRICT)',
        'text'  => 'Нельзя удалить врача, к которому привязаны записи пациентов на приём или история консультаций. Сначала отмените или перенесите приёмы.',
    ],
    'has_orders' => [
        'type' => 'warning',
        'title' => 'Ограничение внешнего ключа',
        'text'  => 'Нельзя удалить запись, на которую ссылаются связанные документы в базе данных.',
    ],
];

if ($msg && isset($alerts[$msg])):
    $alert = $alerts[$msg];
?>
<div class="alert alert-<?= $alert['type'] ?> alert-dismissible fade show shadow-sm mb-4" role="alert">
    <strong><?= $alert['title'] ?></strong> <?= $this->e($alert['text']) ?>
    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Закрыть"></button>
</div>
<?php endif; ?>
