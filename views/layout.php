<!doctype html>
<html lang="ru">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title><?= isset($title) ? $this->e($title) : 'DentalLink — Стоматологическая клиника' ?></title>
  <link rel="preconnect" href="https://fonts.googleapis.com">
  <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
  <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
  <!-- Bootstrap 5.3.3 -->
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet">
  <!-- DentalLink UI-kit стили -->
  <link href="/assets/css/app.css" rel="stylesheet">
  <style>
    :root {
      --dl-primary: #0284c7;
      --dl-card-bg: #f8fafc;
      --dl-radius-card: 16px;
    }
    body {
      font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
      background-color: #f1f5f9;
      color: #1e293b;
      min-height: 100vh;
      display: flex;
      flex-direction: column;
    }
    .mockup-header {
      background: #ffffff;
      border-bottom: 1px solid #e2e8f0;
      padding: 0.75rem 0;
    }
    .mockup-logo {
      font-size: 1.25rem;
      font-weight: 700;
      color: #0284c7;
      text-decoration: none;
    }
    .nav-pill-btn {
      color: #475569;
      padding: 0.4rem 0.9rem;
      border-radius: 9999px;
      text-decoration: none;
      font-weight: 500;
      font-size: 0.9rem;
      transition: all 0.15s ease-in-out;
    }
    .nav-pill-btn:hover, .nav-pill-btn.active {
      background-color: #e0f2fe;
      color: #0284c7;
    }
    .app-footer {
      background: #ffffff;
      border-top: 1px solid #e2e8f0;
      color: #64748b;
    }
    .badge-status-active { background-color: #dcfce7; color: #166534; }
    .badge-status-on_leave { background-color: #fef9c3; color: #854d0e; }
    .badge-status-dismissed { background-color: #fee2e2; color: #991b1b; }
  </style>
</head>
<body>
  <header class="mockup-header shadow-sm">
    <div class="container d-flex flex-wrap align-items-center justify-content-between gap-3">
      <a class="mockup-logo d-flex align-items-center gap-2" href="<?= $this->url('/doctors') ?>">
        <span>DentalLink 🦷</span>
        <span class="badge bg-primary-subtle text-primary rounded-pill small px-2">Лаб. 13</span>
      </a>

      <nav aria-label="Основная навигация" class="d-flex align-items-center gap-2 flex-wrap">
        <a href="<?= $this->url('/doctors') ?>" class="nav-pill-btn <?= (str_starts_with($_SERVER['REQUEST_URI'] ?? '', '/doctors') && !str_contains($_SERVER['REQUEST_URI'] ?? '', '/create')) ? 'active' : '' ?>">Врачи</a>
        <a href="<?= $this->url('/doctors/create') ?>" class="nav-pill-btn <?= str_contains($_SERVER['REQUEST_URI'] ?? '', '/create') ? 'active' : '' ?>">+ Добавить врача</a>
        <a href="<?= $this->url('/reports/staff') ?>" class="nav-pill-btn <?= str_starts_with($_SERVER['REQUEST_URI'] ?? '', '/reports') ? 'active' : '' ?>">Отчёт: Штат</a>
        <a href="/static/doctors.html" class="nav-pill-btn text-muted" target="_blank">Статика (лаб. 11)</a>
      </nav>
    </div>
  </header>

  <main class="container py-4 flex-grow-1">
    <?php require BASE_PATH . '/views/partials/notice.php'; ?>
    <?= $content ?>
  </main>

  <footer class="app-footer py-4 mt-auto">
    <div class="container text-center">
      <p class="mb-1"><strong>DentalLink</strong> — Лабораторная работа 13: Работа с БД (PDO, Репозитории, CRUD, Поиск, Пагинация).</p>
      <p class="mb-0 small text-muted">Архитектура MVC, Prepared Statements, PRG-паттерн, строгая типизация PHP 8.2.</p>
    </div>
  </footer>

  <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
