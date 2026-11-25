<?php
/** @var array $rows */
/** @var array $totals */

$overallAvg = $totals['total'] > 0
    ? round(array_sum(array_map(fn($r) => $r['avg_experience'] * $r['total'], $rows)) / $totals['total'], 1)
    : 0;
?>

<div class="d-flex flex-wrap align-items-center justify-content-between gap-3 mb-4">
    <div>
        <h1 class="h3 fw-bold mb-1">Штат клиники по направлениям</h1>
        <p class="text-muted mb-0">Сводный аналитический отчёт с условной агрегацией (GROUP BY и SUM)</p>
    </div>
    <div>
        <a href="<?= $this->url('/doctors') ?>" class="btn btn-outline-primary rounded-pill px-3">
            ← В каталог врачей
        </a>
    </div>
</div>

<div class="card border-0 shadow-sm mb-4" style="border-radius: var(--dl-radius-card); overflow: hidden;">
    <div class="card-body p-4 bg-light border-bottom">
        <div class="row g-3 text-center">
            <div class="col-6 col-md-3">
                <div class="p-3 bg-white rounded-3 shadow-sm">
                    <div class="small text-muted mb-1">Всего специалистов</div>
                    <div class="display-6 fw-bold text-primary"><?= (int) $totals['total'] ?></div>
                </div>
            </div>
            <div class="col-6 col-md-3">
                <div class="p-3 bg-white rounded-3 shadow-sm">
                    <div class="small text-muted mb-1">Ведут приём</div>
                    <div class="display-6 fw-bold text-success"><?= (int) $totals['active'] ?></div>
                </div>
            </div>
            <div class="col-6 col-md-3">
                <div class="p-3 bg-white rounded-3 shadow-sm">
                    <div class="small text-muted mb-1">В отпуске</div>
                    <div class="display-6 fw-bold text-warning"><?= (int) $totals['on_leave'] ?></div>
                </div>
            </div>
            <div class="col-6 col-md-3">
                <div class="p-3 bg-white rounded-3 shadow-sm">
                    <div class="small text-muted mb-1">Опыт ≥ 10 лет</div>
                    <div class="display-6 fw-bold text-info"><?= (int) $totals['experienced'] ?></div>
                </div>
            </div>
        </div>
    </div>

    <div class="table-responsive">
        <table class="table table-hover align-middle mb-0">
            <thead class="table-light">
                <tr>
                    <th scope="col" style="width: 50px;">#</th>
                    <th scope="col">Направление / Специализация</th>
                    <th scope="col" class="text-center">Штат (всего)</th>
                    <th scope="col" class="text-center">Ведут приём</th>
                    <th scope="col" class="text-center">В отпуске</th>
                    <th scope="col" class="text-center">Стаж ≥ 10 лет</th>
                    <th scope="col" class="text-center">Ср. стаж (лет)</th>
                </tr>
            </thead>
            <tbody>
                <?php foreach ($rows as $idx => $r): ?>
                    <tr>
                        <td class="text-muted fw-semibold"><?= $idx + 1 ?></td>
                        <td>
                            <span class="fw-bold text-dark"><?= $this->e($r['specialization_name']) ?></span>
                        </td>
                        <td class="text-center">
                            <span class="badge bg-secondary-subtle text-secondary rounded-pill px-3 py-1 fw-bold fs-6">
                                <?= (int) $r['total'] ?>
                            </span>
                        </td>
                        <td class="text-center">
                            <span class="badge bg-success-subtle text-success rounded-pill px-3 py-1">
                                <?= (int) $r['active_count'] ?>
                            </span>
                        </td>
                        <td class="text-center">
                            <span class="badge bg-warning-subtle text-warning-emphasis rounded-pill px-3 py-1">
                                <?= (int) $r['on_leave_count'] ?>
                            </span>
                        </td>
                        <td class="text-center">
                            <span class="badge bg-info-subtle text-info-emphasis rounded-pill px-3 py-1">
                                <?= (int) $r['experienced_count'] ?>
                            </span>
                        </td>
                        <td class="text-center fw-semibold text-secondary">
                            <?= number_format((float) $r['avg_experience'], 1) ?>
                        </td>
                    </tr>
                <?php endforeach; ?>
            </tbody>
            <tfoot class="table-light fw-bold border-top-2">
                <tr>
                    <td colspan="2" class="text-end py-3">ИТОГО ПО КЛИНИКЕ:</td>
                    <td class="text-center py-3 fs-6 text-primary"><?= (int) $totals['total'] ?></td>
                    <td class="text-center py-3 fs-6 text-success"><?= (int) $totals['active'] ?></td>
                    <td class="text-center py-3 fs-6 text-warning-emphasis"><?= (int) $totals['on_leave'] ?></td>
                    <td class="text-center py-3 fs-6 text-info-emphasis"><?= (int) $totals['experienced'] ?></td>
                    <td class="text-center py-3 fs-6 text-dark"><?= number_format($overallAvg, 1) ?></td>
                </tr>
            </tfoot>
        </table>
    </div>
</div>
