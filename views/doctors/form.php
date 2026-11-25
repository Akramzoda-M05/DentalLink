<?php
/** @var array $doctor */
/** @var array $errors */
/** @var int|null $id */
/** @var array $specializations */
/** @var array $cabinets */

$action = $id ? '/doctors/' . $id . '/edit' : '/doctors/create';
$val = fn (string $field): string => $this->e($doctor[$field] ?? '');
$invalid = fn (string $field): string => isset($errors[$field]) ? ' is-invalid' : '';
$error = function (string $field) use ($errors): string {
    return isset($errors[$field])
        ? '<div class="invalid-feedback" id="' . $field . '-error">' . $this->e($errors[$field]) . '</div>'
        : '';
};
?>

<nav aria-label="Навигационная цепочка" class="mb-4">
    <ol class="breadcrumb mb-0">
        <li class="breadcrumb-item"><a href="<?= $this->url('/doctors') ?>" class="text-decoration-none">Врачи</a></li>
        <li class="breadcrumb-item active" aria-current="page">
            <?= $id ? 'Редактирование врача #' . (int) $id : 'Новый специалист' ?>
        </li>
    </ol>
</nav>

<div class="row justify-content-center">
    <div class="col-12 col-lg-10">
        <div class="card border-0 shadow-sm p-4 p-md-5" style="border-radius: var(--dl-radius-card);">
            <div class="mb-4">
                <span class="badge bg-primary-subtle text-primary fw-semibold px-3 py-2 rounded-pill mb-2">
                    <?= $id ? 'Обновление данных' : 'Регистрация в штате' ?>
                </span>
                <h1 class="h3 fw-bold mb-1">
                    <?= $id ? 'Редактирование профиля специалиста' : 'Добавление нового врача в DentalLink' ?>
                </h1>
                <p class="text-muted mb-0">
                    Все поля со звёздочкой (<span class="text-danger">*</span>) проверяются на сервере. При ошибках возвращается HTTP-код 422 с сохранением введённых данных.
                </p>
            </div>

            <?php if (!empty($errors)): ?>
                <div class="alert alert-danger shadow-sm mb-4">
                    <h6 class="fw-bold mb-1">Пожалуйста, исправьте ошибки в форме:</h6>
                    <ul class="mb-0 ps-3 small">
                        <?php foreach ($errors as $field => $msg): ?>
                            <li><?= $this->e($msg) ?></li>
                        <?php endforeach; ?>
                    </ul>
                </div>
            <?php endif; ?>

            <form class="needs-validation" method="post" action="<?= $this->url($action) ?>" novalidate>
                <!-- Блок 1: ФИО -->
                <h5 class="fw-bold mb-3 text-secondary">1. Персональные данные</h5>
                <div class="row g-3 mb-4">
                    <div class="col-12 col-md-4">
                        <label class="form-label fw-semibold" for="last_name">Фамилия <span class="text-danger">*</span></label>
                        <input class="form-control<?= $invalid('last_name') ?>" id="last_name" name="last_name" required maxlength="60"
                               placeholder="Например: Рахимов"
                               value="<?= $val('last_name') ?>">
                        <?= $error('last_name') ?>
                    </div>
                    <div class="col-12 col-md-4">
                        <label class="form-label fw-semibold" for="first_name">Имя <span class="text-danger">*</span></label>
                        <input class="form-control<?= $invalid('first_name') ?>" id="first_name" name="first_name" required maxlength="60"
                               placeholder="Например: Алишер"
                               value="<?= $val('first_name') ?>">
                        <?= $error('first_name') ?>
                    </div>
                    <div class="col-12 col-md-4">
                        <label class="form-label fw-semibold" for="middle_name">Отчество</label>
                        <input class="form-control<?= $invalid('middle_name') ?>" id="middle_name" name="middle_name" maxlength="60"
                               placeholder="Например: Баходурович"
                               value="<?= $val('middle_name') ?>">
                        <?= $error('middle_name') ?>
                    </div>
                    <div class="col-12 col-md-6">
                        <label class="form-label fw-semibold" for="birth_date">Дата рождения <span class="text-danger">*</span></label>
                        <input type="date" class="form-control<?= $invalid('birth_date') ?>" id="birth_date" name="birth_date" required
                               value="<?= $val('birth_date') ?>">
                        <?= $error('birth_date') ?>
                        <div class="form-text small text-muted">Специалист должен быть не моложе 21 года.</div>
                    </div>
                    <div class="col-12 col-md-6">
                        <label class="form-label fw-semibold" for="gender">Пол <span class="text-danger">*</span></label>
                        <select class="form-select<?= $invalid('gender') ?>" id="gender" name="gender" required>
                            <option value="male" <?= ($doctor['gender'] ?? 'male') === 'male' ? 'selected' : '' ?>>Мужской</option>
                            <option value="female" <?= ($doctor['gender'] ?? '') === 'female' ? 'selected' : '' ?>>Женский</option>
                        </select>
                        <?= $error('gender') ?>
                    </div>
                </div>

                <!-- Блок 2: Контакты -->
                <h5 class="fw-bold mb-3 text-secondary">2. Контактная информация</h5>
                <div class="row g-3 mb-4">
                    <div class="col-12 col-md-6">
                        <label class="form-label fw-semibold" for="phone">Номер телефона <span class="text-danger">*</span></label>
                        <input type="tel" class="form-control<?= $invalid('phone') ?>" id="phone" name="phone" required
                               placeholder="+992 92 700 1122"
                               value="<?= $val('phone') ?>">
                        <?= $error('phone') ?>
                        <div class="form-text small text-muted">Уникальный телефон врача в формате: +992 XX XXX XXXX.</div>
                    </div>
                    <div class="col-12 col-md-6">
                        <label class="form-label fw-semibold" for="email">Корпоративный E-mail <span class="text-danger">*</span></label>
                        <input type="email" class="form-control<?= $invalid('email') ?>" id="email" name="email" required maxlength="120"
                               placeholder="doctor@dentallink.tj"
                               value="<?= $val('email') ?>">
                        <?= $error('email') ?>
                        <div class="form-text small text-muted">Проверяется на уникальность в базе данных клиники.</div>
                    </div>
                </div>

                <!-- Блок 3: Специализация и кабинет -->
                <h5 class="fw-bold mb-3 text-secondary">3. Должность и рабочее место</h5>
                <div class="row g-3 mb-4">
                    <div class="col-12 col-md-6">
                        <label class="form-label fw-semibold" for="specialization_id">Специализация (Справочник 1) <span class="text-danger">*</span></label>
                        <select class="form-select<?= $invalid('specialization_id') ?>" id="specialization_id" name="specialization_id" required>
                            <option value="">Выберите специализацию</option>
                            <?php foreach ($specializations as $s): ?>
                                <option value="<?= (int) $s['id'] ?>" <?= ((string)$s['id'] === (string)($doctor['specialization_id'] ?? '')) ? 'selected' : '' ?>>
                                    <?= $this->e($s['name']) ?>
                                </option>
                            <?php endforeach; ?>
                        </select>
                        <?= $error('specialization_id') ?>
                    </div>
                    <div class="col-12 col-md-6">
                        <label class="form-label fw-semibold" for="cabinet_id">Лечебный кабинет (Справочник 2) <span class="text-danger">*</span></label>
                        <select class="form-select<?= $invalid('cabinet_id') ?>" id="cabinet_id" name="cabinet_id" required>
                            <option value="">Выберите кабинет</option>
                            <?php foreach ($cabinets as $c): ?>
                                <option value="<?= (int) $c['id'] ?>" <?= ((string)$c['id'] === (string)($doctor['cabinet_id'] ?? '')) ? 'selected' : '' ?>>
                                    <?= $this->e($c['name']) ?> (Кресел: <?= (int) $c['chair_number'] ?>)
                                </option>
                            <?php endforeach; ?>
                        </select>
                        <?= $error('cabinet_id') ?>
                    </div>
                    <div class="col-12 col-md-6">
                        <label class="form-label fw-semibold" for="qualification">Квалификационная категория <span class="text-danger">*</span></label>
                        <select class="form-select<?= $invalid('qualification') ?>" id="qualification" name="qualification" required>
                            <?php
                            $quals = [
                                'Врач 2-й категории',
                                'Врач 1-й категории',
                                'Врач высшей категории',
                                'Кандидат медицинских наук',
                                'Доктор медицинских наук',
                            ];
                            $currQual = $doctor['qualification'] ?? 'Врач 1-й категории';
                            foreach ($quals as $q):
                            ?>
                                <option value="<?= $this->e($q) ?>" <?= ($currQual === $q) ? 'selected' : '' ?>><?= $this->e($q) ?></option>
                            <?php endforeach; ?>
                        </select>
                        <?= $error('qualification') ?>
                    </div>
                    <div class="col-12 col-md-3">
                        <label class="form-label fw-semibold" for="experience_years">Стаж работы (лет) <span class="text-danger">*</span></label>
                        <input type="number" min="0" max="60" class="form-control<?= $invalid('experience_years') ?>" id="experience_years" name="experience_years" required
                               value="<?= $val('experience_years') ?: '0' ?>">
                        <?= $error('experience_years') ?>
                    </div>
                    <div class="col-12 col-md-3">
                        <label class="form-label fw-semibold" for="hire_date">Дата найма <span class="text-danger">*</span></label>
                        <input type="date" class="form-control<?= $invalid('hire_date') ?>" id="hire_date" name="hire_date" required
                               value="<?= $val('hire_date') ?: date('Y-m-d') ?>">
                        <?= $error('hire_date') ?>
                    </div>
                    <div class="col-12 col-md-6">
                        <label class="form-label fw-semibold" for="status">Статус специалиста <span class="text-danger">*</span></label>
                        <select class="form-select<?= $invalid('status') ?>" id="status" name="status" required>
                            <option value="active" <?= (($doctor['status'] ?? 'active') === 'active') ? 'selected' : '' ?>>Ведёт приём (active)</option>
                            <option value="on_leave" <?= (($doctor['status'] ?? '') === 'on_leave') ? 'selected' : '' ?>>В отпуске (on_leave)</option>
                            <option value="dismissed" <?= (($doctor['status'] ?? '') === 'dismissed') ? 'selected' : '' ?>>Уволен (dismissed)</option>
                        </select>
                        <?= $error('status') ?>
                    </div>
                    <div class="col-12">
                        <label class="form-label fw-semibold" for="bio">Краткая биография и специализация</label>
                        <textarea class="form-control<?= $invalid('bio') ?>" id="bio" name="bio" rows="3"
                                  placeholder="Укажите методики лечения, сертификаты, владение микроскопом..."><?= $val('bio') ?></textarea>
                        <?= $error('bio') ?>
                    </div>
                </div>

                <!-- Кнопки управления формой -->
                <div class="d-flex flex-wrap gap-2 justify-content-end pt-3 border-top">
                    <a href="<?= $id ? $this->url('/doctors/' . $id) : $this->url('/doctors') ?>" class="btn btn-outline-secondary rounded-pill px-4">
                        Отмена
                    </a>
                    <button type="submit" class="btn btn-primary rounded-pill px-4 fw-semibold">
                        <?= $id ? 'Сохранить изменения' : 'Добавить специалиста' ?>
                    </button>
                </div>
            </form>
        </div>
    </div>
</div>
