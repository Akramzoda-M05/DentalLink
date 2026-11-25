<?php

namespace App\Controllers;

use App\Core\View;
use App\Repositories\DoctorRepository;

final class ReportController
{
    private DoctorRepository $doctors;
    private View $view;

    public function __construct()
    {
        $this->doctors = new DoctorRepository();
        $this->view = new View(BASE_PATH . '/views');
    }

    /**
     * GET /reports/staff или /reports/doctors — аналитический отчёт по штату клиники
     */
    public function staff(): void
    {
        $rows = $this->doctors->staffReport();

        $totals = [
            'total'       => 0,
            'active'      => 0,
            'on_leave'    => 0,
            'experienced' => 0,
        ];

        foreach ($rows as $row) {
            $totals['total'] += (int) $row['total'];
            $totals['active'] += (int) $row['active_count'];
            $totals['on_leave'] += (int) $row['on_leave_count'];
            $totals['experienced'] += (int) $row['experienced_count'];
        }

        echo $this->view->render('reports/staff', [
            'title'  => 'Отчёт: Штат специалистов стоматологии по направлениям',
            'rows'   => $rows,
            'totals' => $totals,
        ]);
    }
}
