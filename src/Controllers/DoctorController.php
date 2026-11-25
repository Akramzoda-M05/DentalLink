<?php

namespace App\Controllers;

use App\Core\Validator;
use App\Core\View;
use App\Repositories\AppointmentRepository;
use App\Repositories\CabinetRepository;
use App\Repositories\DoctorRepository;
use App\Repositories\SpecializationRepository;
use PDOException;

final class DoctorController
{
    private DoctorRepository $doctors;
    private SpecializationRepository $specializations;
    private CabinetRepository $cabinets;
    private AppointmentRepository $appointments;
    private View $view;

    public function __construct()
    {
        $this->doctors = new DoctorRepository();
        $this->specializations = new SpecializationRepository();
        $this->cabinets = new CabinetRepository();
        $this->appointments = new AppointmentRepository();
        $this->view = new View(BASE_PATH . '/views');
    }

    /**
     * GET /doctors или /employees — каталог врачей с пагинацией и фильтрами
     */
    public function index(): void
    {
        $filters = [
            'q'                 => trim((string) ($_GET['q'] ?? '')),
            'specialization_id' => $_GET['specialization_id'] ?? '',
            'cabinet_id'        => $_GET['cabinet_id'] ?? '',
            'status'            => $_GET['status'] ?? '',
            'min_experience'    => $_GET['min_experience'] ?? '',
        ];

        $page = (int) ($_GET['page'] ?? 1);
        $perPage = 10;

        $pagination = $this->doctors->paginate($filters, $page, $perPage);
        $statusCounts = $this->doctors->countByStatus();
        $allSpecializations = $this->specializations->all();
        $allCabinets = $this->cabinets->all();

        echo $this->view->render('doctors/index', [
            'title'          => 'Врачи — Каталог специалистов DentalLink',
            'doctors'        => $pagination['items'],
            'total'          => $pagination['total'],
            'page'           => $pagination['page'],
            'pages'          => $pagination['pages'],
            'filters'        => $filters,
            'specializations'=> $allSpecializations,
            'cabinets'       => $allCabinets,
            'statusCounts'   => $statusCounts,
            'msg'            => $_GET['msg'] ?? null,
        ]);
    }

    /**
     * GET /doctors/{id} или /employees/{id} — карточка врача
     */
    public function show(int $id): void
    {
        $doctor = $this->findOrFail($id);
        $doctorAppointments = $this->appointments->allByDoctor($id);

        echo $this->view->render('doctors/show', [
            'title'        => $doctor['last_name'] . ' ' . $doctor['first_name'] . ' — Профиль врача',
            'doctor'       => $doctor,
            'appointments' => $doctorAppointments,
            'msg'          => $_GET['msg'] ?? null,
        ]);
    }

    /**
     * GET /doctors/create или /employees/create — форма добавления
     */
    public function create(): void
    {
        $this->renderForm([], [], null, 200);
    }

    /**
     * POST /doctors/create или /employees/create — сохранение нового врача (PRG)
     */
    public function store(): void
    {
        [$data, $errors] = $this->validate($_POST, null);

        if ($errors) {
            $this->renderForm($_POST, $errors, null, 422);
            return;
        }

        $id = $this->doctors->create($data);
        $this->redirect('/doctors/' . $id . '?msg=created');
    }

    /**
     * GET /doctors/{id}/edit или /employees/{id}/edit — форма редактирования
     */
    public function edit(int $id): void
    {
        $doctor = $this->findOrFail($id);
        $this->renderForm($doctor, [], $id, 200);
    }

    /**
     * POST /doctors/{id}/edit или /employees/{id}/edit — сохранение изменений (PRG)
     */
    public function update(int $id): void
    {
        $this->findOrFail($id);
        [$data, $errors] = $this->validate($_POST, $id);

        if ($errors) {
            $this->renderForm($_POST, $errors, $id, 422);
            return;
        }

        $this->doctors->update($id, $data);
        $this->redirect('/doctors/' . $id . '?msg=updated');
    }

    /**
     * POST /doctors/{id}/delete или /employees/{id}/delete — удаление врача с защитой внешних ключей
     */
    public function delete(int $id): void
    {
        $this->findOrFail($id);

        try {
            $this->doctors->delete($id);
        } catch (PDOException $e) {
            // Код 1451 в MySQL / MariaDB — нарушение ограничения внешнего ключа (ON DELETE RESTRICT)
            // Код 19 или 23503 в SQLite / PostgreSQL
            $sqlCode = $e->errorInfo[1] ?? null;
            if ($sqlCode === 1451 || $sqlCode === 19 || str_contains($e->getMessage(), 'FOREIGN KEY')) {
                $this->redirect('/doctors/' . $id . '?msg=has_appointments');
                return;
            }
            throw $e;
        }

        $this->redirect('/doctors?msg=deleted');
    }

    /**
     * Вспомогательный метод рендеринга формы
     */
    private function renderForm(array $doctor, array $errors, ?int $id, int $httpCode = 200): void
    {
        http_response_code($httpCode);
        echo $this->view->render('doctors/form', [
            'title'           => $id ? 'Редактирование профиля специалиста' : 'Регистрация нового врача',
            'doctor'          => $doctor,
            'errors'          => $errors,
            'id'              => $id,
            'specializations' => $this->specializations->all(),
            'cabinets'        => $this->cabinets->all(),
        ]);
    }

    /**
     * Серверная валидация входных данных
     */
    private function validate(array $input, ?int $id): array
    {
        $v = new Validator($input);

        $v->required('last_name', 'Фамилия')->maxLength('last_name', 60, 'Фамилия')
          ->required('first_name', 'Имя')->maxLength('first_name', 60, 'Имя')
          ->maxLength('middle_name', 60, 'Отчество')
          ->required('birth_date', 'Дата рождения')->date('birth_date', 'Дата рождения')
          ->required('gender', 'Пол')->in('gender', ['male', 'female'], 'Пол')
          ->required('phone', 'Телефон')->pattern('phone', '/^[0-9+()\s\-]{5,20}$/', 'Телефон: от 5 до 20 символов (цифры, +, -, скобки)')
          ->required('email', 'E-mail')->email('email', 'E-mail')->maxLength('email', 120, 'E-mail')
          ->required('specialization_id', 'Специализация')->in('specialization_id', $this->specializations->ids(), 'Специализация')
          ->required('cabinet_id', 'Лечебный кабинет')->in('cabinet_id', $this->cabinets->ids(), 'Лечебный кабинет')
          ->required('qualification', 'Квалификация')->maxLength('qualification', 100, 'Квалификация')
          ->required('hire_date', 'Дата найма')->date('hire_date', 'Дата найма')
          ->required('status', 'Статус')->in('status', ['active', 'on_leave', 'dismissed'], 'Статус');

        $errors = $v->errors();
        $birthDate = trim((string) ($input['birth_date'] ?? ''));
        $hireDate = trim((string) ($input['hire_date'] ?? ''));
        $email = trim((string) ($input['email'] ?? ''));
        $phone = trim((string) ($input['phone'] ?? ''));

        // Бизнес-правило 1: Врач должен быть совершеннолетним (не младше 21 года)
        if (!isset($errors['birth_date']) && $birthDate !== '') {
            $minAgeDate = date('Y-m-d', strtotime('-21 years'));
            if ($birthDate > $minAgeDate) {
                $v->addError('birth_date', 'Специалисту должно быть не меньше 21 года');
            }
        }

        // Бизнес-правило 2: Дата найма не может быть в будущем
        if (!isset($errors['hire_date']) && $hireDate !== '') {
            if ($hireDate > date('Y-m-d')) {
                $v->addError('hire_date', 'Дата найма не может быть позже сегодняшнего дня');
            }
        }

        // Бизнес-правило 3: Стаж работы не может быть отрицательным
        $exp = $input['experience_years'] ?? '';
        if ($exp !== '' && (!is_numeric($exp) || (int) $exp < 0)) {
            $v->addError('experience_years', 'Стаж работы должен быть неотрицательным числом');
        }

        // Бизнес-правило 4: Проверка уникальности email в БД
        if (!isset($errors['email']) && $email !== '' && $this->doctors->emailExists($email, $id)) {
            $v->addError('email', 'Специалист с таким e-mail уже зарегистрирован в клинике');
        }

        // Бизнес-правило 5: Проверка уникальности телефона в БД
        if (!isset($errors['phone']) && $phone !== '' && $this->doctors->phoneExists($phone, $id)) {
            $v->addError('phone', 'Специалист с таким номером телефона уже существует');
        }

        $cleanData = $input;
        $cleanData['experience_years'] = (int) ($input['experience_years'] ?? 0);
        $cleanData['specialization_id'] = (int) ($input['specialization_id'] ?? 0);
        $cleanData['cabinet_id'] = (int) ($input['cabinet_id'] ?? 0);

        return [$cleanData, $v->errors()];
    }

    private function findOrFail(int $id): array
    {
        $doctor = $this->doctors->find($id);
        if (!$doctor) {
            http_response_code(404);
            echo $this->view->render('errors/error', [
                'title'   => 'Врач не найден',
                'code'    => 404,
                'message' => "Специалист с идентификатором #{$id} не найден в базе данных клиники.",
            ]);
            exit;
        }
        return $doctor;
    }

    private function redirect(string $url): void
    {
        header('Location: ' . $url);
        exit;
    }
}
