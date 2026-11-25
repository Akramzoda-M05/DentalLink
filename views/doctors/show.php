<?php
/** @var array $doctor */
/** @var array $appointments */

$fullName = trim($doctor['last_name'] . ' ' . $doctor['first_name'] . ' ' . ($doctor['middle_name'] ?? ''));
$statusLabels = [
    'active'    => ['label' => 'Ведёт приём', 'class' => 'badge-status-active'],
    'on_leave'  => ['label' => 'В отпуске', 'class' => 'badge-status-on_leave'],
    'dismissed' => ['label' => 'Уволен', 'class' => 'badge-status-dismissed'],
];
$st = $statusLabels[$doctor['status']] ?? ['label' => $doctor['status'], 'class' => 'bg-secondary text-white'];
?>

<nav aria-label="Навигационная цепочка" class="mb-4">
    <ol class="breadcrumb mb-0">
        <li class="breadcrumb-item"><a href="<?= $this->url('/doctors') ?>" class="text-decoration-none">Врачи</a></li>
        <li class="breadcrumb-item active" aria-current="page"><?= $this->e($fullName) ?></li>
    </ol>
</nav>

<div class="row g-4">
    <!-- Левая колонка: Профиль врача -->
    <div class="col-12 col-lg-8">
        <div class="card border-0 shadow-sm p-4" style="border-radius: var(--dl-radius-card);">
            <div class="d-flex flex-wrap align-items-start justify-content-between gap-3 mb-4">
                <div>
                    <div class="d-flex align-items-center gap-2 mb-2">
                        <span class="badge <?= $st['class'] ?> rounded-pill px-3 py-1">
                            <?= $st['label'] ?>
                        </span>
                        <span class="badge bg-light text-secondary border rounded-pill px-3 py-1">
                            ID: #<?= (int) $doctor['id'] ?>
                        </span>
                    </div>
                    <h1 class="h3 fw-bold mb-1"><?= $this->e($fullName) ?></h1>
                    <p class="text-muted mb-0"><?= $this->e($doctor['qualification']) ?></p>
                </div>
                <div class="d-flex gap-2">
                    <a href="<?= $this->url('/doctors/' . $doctor['id'] . '/edit') ?>" class="btn btn-outline-primary rounded-pill px-3">
                        ✎ Редактировать
                    </a>
                    <button type="button" class="btn btn-outline-danger rounded-pill px-3" data-bs-toggle="modal" data-bs-target="#deleteModal">
                        🗑 Удалить
                    </button>
                </div>
            </div>

            <hr class="text-muted">

            <!-- Таблица подробных сведений (JOIN из 2+ справочников) -->
            <div class="row g-3 mb-4">
                <div class="col-12 col-sm-6">
                    <div class="p-3 bg-light rounded-3">
                        <div class="small text-muted mb-1">Специализация (Справочник 1)</div>
                        <div class="fw-bold fs-6 text-primary"><?= $this->e($doctor['specialization_name']) ?></div>
                        <div class="small text-muted">Код: <code><?= $this->e($doctor['specialization_code']) ?></code></div>
                    </div>
                </div>
                <div class="col-12 col-sm-6">
                    <div class="p-3 bg-light rounded-3">
                        <div class="small text-muted mb-1">Лечебный кабинет (Справочник 2)</div>
                        <div class="fw-bold fs-6 text-dark"><?= $this->e($doctor['cabinet_name']) ?></div>
                        <div class="small text-muted">Кресел в кабинете: <?= (int) $doctor['cabinet_chair_number'] ?></div>
                    </div>
                </div>
                <div class="col-12 col-sm-6 col-md-4">
                    <div class="p-3 bg-light rounded-3">
                        <div class="small text-muted mb-1">Стаж работы</div>
                        <div class="fw-bold fs-5"><?= (int) $doctor['experience_years'] ?> лет</div>
                    </div>
                </div>
                <div class="col-12 col-sm-6 col-md-4">
                    <div class="p-3 bg-light rounded-3">
                        <div class="small text-muted mb-1">Дата найма</div>
                        <div class="fw-bold fs-5"><?= $this->e($doctor['hire_date']) ?></div>
                    </div>
                </div>
                <div class="col-12 col-sm-6 col-md-4">
                    <div class="p-3 bg-light rounded-3">
                        <div class="small text-muted mb-1">Дата рождения / Пол</div>
                        <div class="fw-bold fs-6"><?= $this->e($doctor['birth_date']) ?> (<?= $doctor['gender'] === 'female' ? 'Женский' : 'Мужской' ?>)</div>
                    </div>
                </div>
            </div>

            <!-- Контактная информация -->
            <h5 class="fw-bold mb-3">Контакты</h5>
            <div class="row g-3 mb-4">
                <div class="col-12 col-sm-6">
                    <div class="small text-muted">Телефон для связи</div>
                    <div class="fw-semibold fs-6">
                        <a href="tel:<?= $this->e($doctor['phone']) ?>" class="text-decoration-none text-dark">
                            📞 <?= $this->e($doctor['phone']) ?>
                        </a>
                    </div>
                </div>
                <div class="col-12 col-sm-6">
                    <div class="small text-muted">Корпоративная почта</div>
                    <div class="fw-semibold fs-6">
                        <a href="mailto:<?= $this->e($doctor['email']) ?>" class="text-decoration-none text-dark">
                            ✉ <?= $this->e($doctor['email']) ?>
                        </a>
                    </div>
                </div>
            </div>

            <!-- Биография -->
            <?php if (!empty($doctor['bio'])): ?>
                <h5 class="fw-bold mb-2">О специалисте</h5>
                <p class="text-secondary mb-0"><?= nl2br($this->e($doctor['bio'])) ?></p>
            <?php endif; ?>
        </div>
    </div>

    <!-- Правая колонка: Связанные данные (записи пациентов / проверка FK) -->
    <div class="col-12 col-lg-4">
        <div class="card border-0 shadow-sm p-4" style="border-radius: var(--dl-radius-card);">
            <h5 class="fw-bold mb-3 d-flex align-items-center justify-content-between">
                <span>Записи пациентов</span>
                <span class="badge bg-secondary-subtle text-secondary rounded-pill">
                    <?= count($appointments) ?>
                </span>
            </h5>

            <?php if (empty($appointments)): ?>
                <div class="text-center py-4 text-muted">
                    <div class="small mb-2">На текущий момент приёмов не назначено</div>
                    <span class="badge bg-success-subtle text-success small">Врача можно безопасно удалить</span>
                </div>
            <?php else: ?>
                <div class="alert alert-warning small mb-3 py-2">
                    Врач имеет записи пациентов. Попытка удаления приведёт к срабатыванию <code>ON DELETE RESTRICT</code> в СУБД.
                </div>
                <ul class="list-group list-group-flush small">
                    <?php foreach ($appointments as $app): ?>
                        <li class="list-group-item px-0 py-2">
                            <div class="fw-bold"><?= $this->e($app['patient_name']) ?></div>
                            <div class="text-muted"><?= $this->e($app['appointment_date']) ?> — <?= $this->e($app['status']) ?></div>
                            <?php if (!empty($app['notes'])): ?>
                                <div class="text-secondary fst-italic"><?= $this->e($app['notes']) ?></div>
                            <?php endif; ?>
                        </li>
                    <?php endforeach; ?>
                </ul>
            <?php endif; ?>
        </div>
    </div>
</div>

<!-- Модальное окно подтверждения удаления -->
<div class="modal fade" id="deleteModal" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content" style="border-radius: var(--dl-radius-card);">
            <div class="modal-header border-0 pb-0">
                <h5 class="modal-title fw-bold">Подтверждение удаления</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Закрыть"></button>
            </div>
            <div class="modal-body py-3">
                <p class="mb-2">Вы действительно хотите удалить специалиста:</p>
                <p class="fw-bold mb-3"><?= $this->e($fullName) ?>?</p>
                <div class="alert alert-danger py-2 small mb-0">
                    Это действие нельзя отменить.
                </div>
            </div>
            <div class="modal-footer border-0 pt-0">
                <button type="button" class="btn btn-outline-secondary rounded-pill px-3" data-bs-dismiss="modal">Отмена</button>
                <form method="post" action="<?= $this->url('/doctors/' . $doctor['id'] . '/delete') ?>" style="display: inline;">
                    <button type="submit" class="btn btn-danger rounded-pill px-4 fw-semibold">Удалить из клиники</button>
                </form>
            </div>
        </div>
    </div>
</div>
