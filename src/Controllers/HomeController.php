<?php

declare(strict_types=1);

namespace App\Controllers;

final class HomeController extends Controller
{
    /** GET / — каталог врачей является главной страницей клиники. */
    public function index(): never
    {
        $this->redirect('/doctors');
    }
}
