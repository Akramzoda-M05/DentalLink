<?php
/** @var array $doctors */
/** @var int $total */
/** @var int $page */
/** @var int $pages */
/** @var array $filters */
/** @var array $specializations */
/** @var array $cabinets */
/** @var array $statusCounts */

$statusLabels = [
    'active'    => ['label' => 'Ведёт приём', 'class' => 'badge-status-active'],
    'on_leave'  => ['label' => 'В отпуске', 'class' => 'badge-status-on_leave'],
    'dismissed' => ['label' => 'Уволен', 'class' => 'badge-status-dismissed'],
];
?>

<div class="d-flex flex-wrap align-items-center justify-content-between gap-3 mb-4">
    <div>
        <h1 class="h3 fw-bold mb-1">Каталог врачей-стоматологов</h1>
        <p class="text-muted mb-0">Управление штатом клиники DentalLink, поиск специалистов и онлайн-запись</p>
    </div>
    <div class="d-flex align-items-center gap-2 flex-wrap">
        <span class="badge bg-primary-subtle text-primary fw-semibold px-3 py-2 rounded-pill">
            Найдено: <?= (int) $total ?>
        </span>
        <a href="<?= $this->url('/doctors/create') ?>" class="btn btn-primary rounded-pill px-3 fw-semibold">
            + Добавить врача
        </a>
    </div>
</div>

<!-- Панель фильтров и поиска -->
<section class="card border-0 shadow-sm mb-4" style="border-radius: var(--dl-radius-card);">
    <div class="card-body p-3 p-md-4">
        <form method="get" action="<?= $this->url('/doctors') ?>">
            <div class="row g-3 align-items-end">
                <!-- Текстовый поиск LIKE -->
                <div class="col-12 col-md-4">
                    <label for="filter-q" class="form-label small fw-semibold text-muted mb-1">Поиск по ФИО</label>
                    <input type="search" class="form-control rounded-pill" id="filter-q" name="q"
                           placeholder="Например: Рахимов или Саид..."
                           value="<?= $this->e($filters['q']) ?>">
                </div>

                <!-- Фильтр 1: Специализация -->
                <div class="col-12 col-sm-6 col-md-3">
                    <label for="filter-spec" class="form-label small fw-semibold text-muted mb-1">Специализация</label>
                    <select class="form-select rounded-pill" id="filter-spec" name="specialization_id">
                        <option value="">Все специализации</option>
                        <?php foreach ($specializations as $s): ?>
                            <option value="<?= (int) $s['id'] ?>" <?= ((string)$s['id'] === (string)$filters['specialization_id']) ? 'selected' : '' ?>>
                                <?= $this->e($s['name']) ?>
                            </option>
                        <?php endforeach; ?>
                    </select>
                </div>

                <!-- Фильтр 2: Статус -->
                <div class="col-12 col-sm-6 col-md-2">
                    <label for="filter-status" class="form-label small fw-semibold text-muted mb-1">Статус</label>
                    <select class="form-select rounded-pill" id="filter-status" name="status">
                        <option value="">Все статусы</option>
                        <option value="active" <?= ($filters['status'] === 'active') ? 'selected' : '' ?>>Ведёт приём</option>
                        <option value="on_leave" <?= ($filters['status'] === 'on_leave') ? 'selected' : '' ?>>В отпуске</option>
                        <option value="dismissed" <?= ($filters['status'] === 'dismissed') ? 'selected' : '' ?>>Уволен</option>
                    </select>
                </div>

                <!-- Фильтр 3: Опыт работы -->
                <div class="col-12 col-sm-6 col-md-2">
                    <label for="filter-exp" class="form-label small fw-semibold text-muted mb-1">Стаж от</label>
                    <select class="form-select rounded-pill" id="filter-exp" name="min_experience">
                        <option value="">Любой стаж</option>
                        <option value="5" <?= ((string)$filters['min_experience'] === '5') ? 'selected' : '' ?>>От 5 лет</option>
                        <option value="10" <?= ((string)$filters['min_experience'] === '10') ? 'selected' : '' ?>>От 10 лет</option>
                        <option value="15" <?= ((string)$filters['min_experience'] === '15') ? 'selected' : '' ?>>От 15 лет</option>
                    </select>
                </div>

                <!-- Кнопки Найти и Сбросить -->
                <div class="col-12 col-sm-6 col-md-1 d-flex gap-2">
                    <button type="submit" class="btn btn-primary rounded-pill w-100 fw-semibold" title="Применить фильтры">
                        Найти
                    </button>
                    <?php if (!empty($filters['q']) || !empty($filters['specialization_id']) || !empty($filters['status']) || !empty($filters['min_experience'])): ?>
                        <a href="<?= $this->url('/doctors') ?>" class="btn btn-outline-secondary rounded-pill px-3" title="Сбросить все фильтры">
                            ✕
                        </a>
                    <?php endif; ?>
                </div>
            </div>
        </form>
    </div>
</section>

<!-- Список врачей в табличном виде -->
<div class="card border-0 shadow-sm mb-4" style="border-radius: var(--dl-radius-card); overflow: hidden;">
    <div class="table-responsive">
        <table class="table table-hover align-middle mb-0">
            <thead class="table-light">
                <tr>
                    <th scope="col" style="width: 50px;">#</th>
                    <th scope="col">ФИО специалиста</th>
                    <th scope="col">Специализация</th>
                    <th scope="col">Кабинет</th>
                    <th scope="col">Стаж</th>
                    <th scope="col">Контакты</th>
                    <th scope="col">Статус</th>
                    <th scope="col" class="text-end">Действия</th>
                </tr>
            </thead>
            <tbody>
                <?php if (empty($doctors)): ?>
                    <tr>
                        <td colspan="8" class="text-center py-5 text-muted">
                            <div class="fs-4 mb-2">🔍 Ни одного специалиста не найдено</div>
                            <p class="mb-0">Попробуйте изменить параметры поиска или <a href="<?= $this->url('/doctors') ?>">сбросить фильтры</a>.</p>
                        </td>
                    </tr>
                <?php else: ?>
                    <?php foreach ($doctors as $d): ?>
                        <?php
                            $fullName = trim($d['last_name'] . ' ' . $d['first_name'] . ' ' . ($d['middle_name'] ?? ''));
                            $st = $statusLabels[$d['status']] ?? ['label' => $d['status'], 'class' => 'bg-secondary text-white'];
                        ?>
                        <tr>
                            <td class="text-muted fw-semibold"><?= (int) $d['id'] ?></td>
                            <td>
                                <a href="<?= $this->url('/doctors/' . $d['id']) ?>" class="fw-bold text-decoration-none text-dark d-block">
                                    <?= $this->e($fullName) ?>
                                </a>
                                <span class="small text-muted"><?= $this->e($d['qualification']) ?></span>
                            </td>
                            <td>
                                <span class="badge bg-info-subtle text-info-emphasis rounded-pill">
                                    <?= $this->e($d['specialization_name']) ?>
                                </span>
                            </td>
                            <td>
                                <span class="small"><?= $this->e($d['cabinet_name']) ?></span>
                            </td>
                            <td>
                                <span class="fw-semibold"><?= (int) $d['experience_years'] ?></span> лет
                            </td>
                            <td>
                                <div class="small"><?= $this->e($d['phone']) ?></div>
                                <div class="small text-muted"><?= $this->e($d['email']) ?></div>
                            </td>
                            <td>
                                <span class="badge <?= $st['class'] ?> rounded-pill px-2 py-1">
                                    <?= $st['label'] ?>
                                </span>
                            </td>
                            <td class="text-end">
                                <div class="btn-group btn-group-sm">
                                    <a href="<?= $this->url('/doctors/' . $d['id']) ?>" class="btn btn-outline-primary" title="Просмотр карточки">
                                        Профиль
                                    </a>
                                    <a href="<?= $this->url('/doctors/' . $d['id'] . '/edit') ?>" class="btn btn-outline-secondary" title="Редактировать">
                                        ✎
                                    </a>
                                    <button type="button" class="btn btn-outline-danger" data-bs-toggle="modal" data-bs-target="#deleteModal<?= (int) $d['id'] ?>" title="Удалить">
                                        🗑
                                    </button>
                                </div>

                                <!-- Модальное окно подтверждения удаления (строго POST) -->
                                <div class="modal fade text-start" id="deleteModal<?= (int) $d['id'] ?>" tabindex="-1" aria-hidden="true">
                                    <div class="modal-dialog modal-dialog-centered">
                                        <div class="modal-content" style="border-radius: var(--dl-radius-card);">
                                            <div class="modal-header border-0 pb-0">
                                                <h5 class="modal-title fw-bold">Подтверждение удаления</h5>
                                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Закрыть"></button>
                                            </div>
                                            <div class="modal-body py-3">
                                                <p class="mb-2">Вы действительно хотите удалить специалиста:</p>
                                                <p class="fw-bold mb-3"><?= $this->e($fullName) ?> (<?= $this->e($d['specialization_name']) ?>)?</p>
                                                <div class="alert alert-warning py-2 small mb-0">
                                                    Внимание: если к врачу привязаны записи пациентов или расписание, удаление будет отклонено базой данных (ON DELETE RESTRICT).
                                                </div>
                                            </div>
                                            <div class="modal-footer border-0 pt-0">
                                                <button type="button" class="btn btn-outline-secondary rounded-pill px-3" data-bs-dismiss="modal">Отмена</button>
                                                <form method="post" action="<?= $this->url('/doctors/' . $d['id'] . '/delete') ?>" style="display: inline;">
                                                    <button type="submit" class="btn btn-danger rounded-pill px-4 fw-semibold">Удалить</button>
                                                </form>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </td>
                        </tr>
                    <?php endforeach; ?>
                <?php endif; ?>
            </tbody>
        </table>
    </div>

    <!-- Пагинация с сохранением параметров фильтров в ссылках -->
    <?php if ($pages > 1): ?>
        <div class="card-footer bg-white border-0 py-3 d-flex flex-wrap align-items-center justify-content-between gap-2">
            <span class="small text-muted">
                Страница <?= (int) $page ?> из <?= (int) $pages ?> (всего <?= (int) $total ?> специалистов)
            </span>
            <nav aria-label="Пагинация каталога">
                <ul class="pagination pagination-sm mb-0">
                    <!-- Первая страница -->
                    <li class="page-item <?= ($page <= 1) ? 'disabled' : '' ?>">
                        <a class="page-link" href="<?= $this->url('/doctors', array_merge($filters, ['page' => 1])) ?>">«</a>
                    </li>
                    <!-- Предыдущая -->
                    <li class="page-item <?= ($page <= 1) ? 'disabled' : '' ?>">
                        <a class="page-link" href="<?= $this->url('/doctors', array_merge($filters, ['page' => $page - 1])) ?>">‹</a>
                    </li>

                    <?php for ($p = 1; $p <= $pages; $p++): ?>
                        <li class="page-item <?= ($p === $page) ? 'active' : '' ?>">
                            <a class="page-link" href="<?= $this->url('/doctors', array_merge($filters, ['page' => $p])) ?>">
                                <?= $p ?>
                            </a>
                        </li>
                    <?php endfor; ?>

                    <!-- Следующая -->
                    <li class="page-item <?= ($page >= $pages) ? 'disabled' : '' ?>">
                        <a class="page-link" href="<?= $this->url('/doctors', array_merge($filters, ['page' => $page + 1])) ?>">›</a>
                    </li>
                    <!-- Последняя -->
                    <li class="page-item <?= ($page >= $pages) ? 'disabled' : '' ?>">
                        <a class="page-link" href="<?= $this->url('/doctors', array_merge($filters, ['page' => $pages])) ?>">»</a>
                    </li>
                </ul>
            </nav>
        </div>
    <?php endif; ?>
</div>
