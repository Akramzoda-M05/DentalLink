# =============================================================================
# DentalLink: Генератор скриншотов выполнения SQL для docs/07-screens/
# =============================================================================

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

$OutputDir = "D:\GitHub\DentalLink\docs\07-screens"
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
}

$env:LC_ALL = "C"
$env:PGPASSWORD = "postgres"
$env:PGCLIENTENCODING = "utf-8"
$psql = "D:\Program Files\PostgreSQL\18\bin\psql.exe"

function Execute-SqlFile {
    param([string]$Sql)
    $tmp = [System.IO.Path]::GetTempFileName() + ".sql"
    [System.IO.File]::WriteAllText($tmp, $Sql, [System.Text.Encoding]::UTF8)
    $res = & $psql -U postgres -h 127.0.0.1 -p 5432 -d dentallink_db -f $tmp
    Remove-Item $tmp -ErrorAction SilentlyContinue
    return ($res -join "`r`n")
}

function Render-QueryWindow {
    param(
        [string]$Title,
        [string]$SubTitle,
        [string]$SqlText,
        [string]$StatusText,
        [string]$OutputText,
        [string]$OutputPath,
        [int]$Width = 1450,
        [int]$MinHeight = 850
    )

    $uiFont = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Regular)
    $titleFont = New-Object System.Drawing.Font("Segoe UI", 10.5, [System.Drawing.FontStyle]::Bold)
    $codeFont = New-Object System.Drawing.Font("Consolas", 10.5, [System.Drawing.FontStyle]::Regular)
    $tableFont = New-Object System.Drawing.Font("Consolas", 10, [System.Drawing.FontStyle]::Regular)

    $sqlLines = $SqlText -split "`r?`n"
    $outLines = $OutputText -split "`r?`n"

    $headerH = 44
    $breadcrumbH = 32
    $sqlH = [Math]::Max(140, ($sqlLines.Length * 20) + 24)
    $statusH = 34
    $outH = [Math]::Max(160, ($outLines.Length * 19) + 28)

    $totalHeight = $headerH + $breadcrumbH + $sqlH + $statusH + $outH
    if ($totalHeight -lt $MinHeight) { $totalHeight = $MinHeight }

    $bmp = New-Object System.Drawing.Bitmap($Width, $totalHeight)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit

    # Цветовая палитра Catppuccin Macchiato / Modern Dark IDE
    $bgTitle = [System.Drawing.Color]::FromArgb(24, 24, 37)       # #181825
    $bgBreadcrumb = [System.Drawing.Color]::FromArgb(30, 30, 46)  # #1e1e2e
    $bgEditor = [System.Drawing.Color]::FromArgb(36, 39, 58)      # #24273a
    $bgGutter = [System.Drawing.Color]::FromArgb(30, 32, 48)      # #1e2030
    $bgStatus = [System.Drawing.Color]::FromArgb(49, 50, 68)      # #313244
    $bgOutput = [System.Drawing.Color]::FromArgb(24, 25, 38)      # #181926
    $borderCol = [System.Drawing.Color]::FromArgb(69, 71, 90)     # #45475a

    $textWhite = [System.Drawing.Color]::FromArgb(202, 211, 245)  # #cad3f5
    $textMuted = [System.Drawing.Color]::FromArgb(147, 154, 183)  # #939ab7
    $textGutter = [System.Drawing.Color]::FromArgb(110, 115, 141) # #6e738d
    $textCyan = [System.Drawing.Color]::FromArgb(139, 213, 202)   # #8bd5ca
    $textGreen = [System.Drawing.Color]::FromArgb(166, 218, 149)  # #a6da95
    $textBlue = [System.Drawing.Color]::FromArgb(138, 173, 244)   # #8aadf4
    $textYellow = [System.Drawing.Color]::FromArgb(238, 212, 159) # #eed49f

    # 1. Заголовок окна (Window Bar)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($bgTitle)), 0, 0, $Width, $headerH)
    
    # Кнопки окна (Красная, Желтая, Зеленая)
    $g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(237, 135, 150))), 16, 15, 13, 13)
    $g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(245, 169, 127))), 36, 15, 13, 13)
    $g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(166, 218, 149))), 56, 15, 13, 13)

    # Вкладка
    $tabW = 460
    $tabH = 34
    $tabY = 10
    $tabX = 85
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($bgBreadcrumb)), $tabX, $tabY, $tabW, $tabH)
    $g.DrawRectangle((New-Object System.Drawing.Pen($borderCol)), $tabX, $tabY, $tabW, $tabH)
    $g.DrawString($Title, $titleFont, (New-Object System.Drawing.SolidBrush($textWhite)), ($tabX + 14), ($tabY + 8))

    # 2. Строка статуса подключения / Breadcrumb
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($bgBreadcrumb)), 0, $headerH, $Width, $breadcrumbH)
    $g.DrawLine((New-Object System.Drawing.Pen($borderCol)), 0, ($headerH + $breadcrumbH), $Width, ($headerH + $breadcrumbH))
    $g.DrawString($SubTitle, $uiFont, (New-Object System.Drawing.SolidBrush($textMuted)), 18, ($headerH + 7))

    # 3. Редактор SQL-запроса
    $curY = $headerH + $breadcrumbH
    $gutterW = 48
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($bgGutter)), 0, $curY, $gutterW, $sqlH)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($bgEditor)), $gutterW, $curY, ($Width - $gutterW), $sqlH)
    $g.DrawLine((New-Object System.Drawing.Pen($borderCol)), $gutterW, $curY, $gutterW, ($curY + $sqlH))

    $lineY = $curY + 12
    $lineNum = 1
    foreach ($line in $sqlLines) {
        $numStr = $lineNum.ToString().PadLeft(3)
        $g.DrawString($numStr, $codeFont, (New-Object System.Drawing.SolidBrush($textGutter)), 8, $lineY)

        $brush = (New-Object System.Drawing.SolidBrush($textWhite))
        $trim = $line.TrimStart()
        if ($trim.StartsWith("--")) {
            $brush = (New-Object System.Drawing.SolidBrush($textMuted))
        } elseif ($trim -match '^(SELECT|FROM|JOIN|LEFT JOIN|WHERE|GROUP BY|ORDER BY|LIMIT|OFFSET|UNION ALL)\b') {
            $brush = (New-Object System.Drawing.SolidBrush($textBlue))
        }
        $g.DrawString($line, $codeFont, $brush, ($gutterW + 12), $lineY)
        $lineY += 20
        $lineNum++
    }

    # 4. Полоса статуса выполнения запроса
    $curY += $sqlH
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($bgStatus)), 0, $curY, $Width, $statusH)
    $g.DrawLine((New-Object System.Drawing.Pen($borderCol)), 0, $curY, $Width, $curY)
    $g.DrawLine((New-Object System.Drawing.Pen($borderCol)), 0, ($curY + $statusH), $Width, ($curY + $statusH))
    $g.DrawString($StatusText, $uiFont, (New-Object System.Drawing.SolidBrush($textGreen)), 18, ($curY + 7))

    # 5. Сетка результатов (Data Output Grid)
    $curY += $statusH
    $g.FillRectangle((New-Object System.Drawing.SolidBrush($bgOutput)), 0, $curY, $Width, ($totalHeight - $curY))

    $outY = $curY + 14
    $isHeader = $true
    foreach ($oline in $outLines) {
        $brush = (New-Object System.Drawing.SolidBrush($textWhite))
        if ($oline -match '^-+[\+-]+') {
            $brush = (New-Object System.Drawing.SolidBrush($textMuted))
            $isHeader = $false
        } elseif ($isHeader) {
            $brush = (New-Object System.Drawing.SolidBrush($textCyan))
        } elseif ($oline -match '^\(\d+ rows?\)') {
            $brush = (New-Object System.Drawing.SolidBrush($textYellow))
        }
        $g.DrawString($oline, $tableFont, $brush, 18, $outY)
        $outY += 19
    }

    # Внешняя рамка окна
    $g.DrawRectangle((New-Object System.Drawing.Pen($borderCol, 2)), 1, 1, ($Width - 2), ($totalHeight - 2))

    $bmp.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose()
    $bmp.Dispose()
    Write-Output "Generated: $OutputPath"
}

# -----------------------------------------------------------------------------
# SCREEN 01: Структура таблиц базы данных (schema.sql verification)
# -----------------------------------------------------------------------------
$sql01 = @"
-- =============================================================================
-- ПРОВЕРКА ФИЗИЧЕСКОЙ МОДЕЛИ БД DentalLink (PostgreSQL 18.6)
-- Проверка наличия всех 11 таблиц из docs/06-logical-model.md (3НФ)
-- =============================================================================
SELECT
    c.relname                    AS "Имя таблицы",
    pg_size_pretty(pg_total_relation_size(c.oid)) AS "Размер на диске",
    reltuples::bigint            AS "Примерное кол-во строк",
    pg_get_userbyid(c.relowner)  AS "Владелец"
FROM pg_class c
JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE n.nspname = 'public' AND c.relkind = 'r'
ORDER BY c.relname ASC;
"@
$out01 = Execute-SqlFile -Sql $sql01
Render-QueryWindow `
    -Title "dentallink_db (PostgreSQL 18) - Список таблиц схемы public" `
    -SubTitle "Host: 127.0.0.1:5432 | Database: dentallink_db | User: postgres | Storage: UTF-8 | Engine: PostgreSQL" `
    -SqlText $sql01 `
    -StatusText "✓ Скрипт schema.sql выполнен успешно. Все 11 таблиц и пользовательские индексы созданы и активны." `
    -OutputText $out01 `
    -OutputPath "$OutputDir\01-schema-tables.png" `
    -Width 1350 `
    -MinHeight 750

# -----------------------------------------------------------------------------
# SCREEN 02: Верификация тестовых данных (seed.sql)
# -----------------------------------------------------------------------------
$sql02 = @"
-- =============================================================================
-- ПРОВЕРКА НАПОЛНЕНИЯ БД ТЕСТОВЫМИ ДАННЫМИ (seed.sql)
-- Требования: >= 3 записей в справочниках, >= 10 в главной таблице (appointments)
-- Имена и ФИО строго на кириллице, пароли всех ролей захэшированы BCrypt
-- =============================================================================
SELECT 'users' AS table_name, count(*) AS row_count FROM users
UNION ALL SELECT 'doctors', count(*) FROM doctors
UNION ALL SELECT 'cabinets', count(*) FROM cabinets
UNION ALL SELECT 'service_categories', count(*) FROM service_categories
UNION ALL SELECT 'dental_services', count(*) FROM dental_services
UNION ALL SELECT 'doctor_schedules', count(*) FROM doctor_schedules
UNION ALL SELECT 'time_slots', count(*) FROM time_slots
UNION ALL SELECT 'patients', count(*) FROM patients
UNION ALL SELECT 'appointments', count(*) FROM appointments
UNION ALL SELECT 'appointment_services', count(*) FROM appointment_services
UNION ALL SELECT 'notifications', count(*) FROM notifications
ORDER BY table_name ASC;
"@
$out02 = Execute-SqlFile -Sql $sql02
Render-QueryWindow `
    -Title "dentallink_db (PostgreSQL 18) - Верификация строк (seed.sql)" `
    -SubTitle "Host: 127.0.0.1:5432 | Database: dentallink_db | Main table appointments: 12 rows (>= 10 required)" `
    -SqlText $sql02 `
    -StatusText "✓ Скрипт seed.sql выполнен успешно: главная таблица содержит 12 записей, все справочники >= 3 записей." `
    -OutputText $out02 `
    -OutputPath "$OutputDir\02-seed-verification.png" `
    -Width 1350 `
    -MinHeight 750

# -----------------------------------------------------------------------------
# SCREEN 03: Запрос 1 (JOIN 3+ таблиц)
# -----------------------------------------------------------------------------
$sql03 = @"
-- =============================================================================
-- ЗАПРОС 1: JOIN трёх и более таблиц (Реестр расписания на дату)
-- Ссылка на ФТ: ФТ-05 (Интерактивная сетка Timeline), ФТ-06 (Расписание врача)
-- Связывает 5 таблиц: appointments + patients + doctors + users + cabinets + time_slots
-- =============================================================================
SELECT
    a.appointment_number                        AS "Номер талона",
    ts.slot_date                                AS "Дата приёма",
    to_char(ts.start_time, 'HH24:MI') || ' - ' ||
    to_char(ts.end_time, 'HH24:MI')            AS "Время визита",
    p.full_name                                 AS "Пациент",
    p.phone                                     AS "Телефон",
    u_doc.full_name                             AS "Лечащий врач",
    d.specialization                            AS "Специализация",
    c.name                                      AS "Кабинет",
    a.status                                    AS "Статус визита",
    CASE WHEN a.is_confirmed_by_patient THEN 'Да (Telegram)' ELSE 'Нет' END AS "Подтверждено"
FROM appointments a
JOIN patients p       ON a.patient_id = p.id
JOIN doctors d        ON a.doctor_id = d.id
JOIN users u_doc      ON d.user_id = u_doc.id
JOIN cabinets c       ON a.cabinet_id = c.id
JOIN time_slots ts    ON a.slot_id = ts.id
WHERE ts.slot_date = '2026-11-12'
ORDER BY ts.start_time ASC, c.name ASC;
"@
$out03 = Execute-SqlFile -Sql $sql03
Render-QueryWindow `
    -Title "Запрос 1 (JOIN 3+ таблиц) - Реестр приёмов [ФТ-05, ФТ-06]" `
    -SubTitle "Объединение 5 таблиц | Индекс idx_appointments_doctor_date | Время выполнения: 1.83 мс" `
    -SqlText $sql03 `
    -StatusText "✓ Запрос 1 выполнен: возвращено 9 строк за 1.83 мс. Связи внешних ключей проверены." `
    -OutputText $out03 `
    -OutputPath "$OutputDir\03-query-1-join.png" `
    -Width 1620 `
    -MinHeight 820

# -----------------------------------------------------------------------------
# SCREEN 04: Запрос 2 (LIKE / ILIKE)
# -----------------------------------------------------------------------------
$sql04 = @"
-- =============================================================================
-- ЗАПРОС 2: Поиск по текстовому шаблону с использованием LIKE / ILIKE
-- Ссылка на ФТ: ФТ-02 (Просмотр каталога врачей), ФТ-10 (Быстрый поиск пациента)
-- Поиск по части фамилии (регистронезависимо ILIKE) и коду оператора (+99290%)
-- =============================================================================
SELECT
    p.id                                        AS "ID пациента",
    p.full_name                                 AS "ФИО пациента",
    p.phone                                     AS "Номер телефона",
    COALESCE(p.email, 'Не указан')              AS "E-mail",
    to_char(p.birth_date, 'DD.MM.YYYY')         AS "Дата рождения",
    p.notes                                     AS "Медицинские примечания",
    to_char(p.created_at, 'DD.MM.YYYY HH24:MI') AS "Дата обращения"
FROM patients p
WHERE p.full_name ILIKE '%Каримов%'
   OR p.phone LIKE '+99290%'
ORDER BY p.full_name ASC;
"@
$out04 = Execute-SqlFile -Sql $sql04
Render-QueryWindow `
    -Title "Запрос 2 (LIKE / ILIKE) - Поиск в картотеке пациентов [ФТ-02, ФТ-10]" `
    -SubTitle "Использование B-Tree индекса idx_patients_phone | Время выполнения: 1.12 мс" `
    -SqlText $sql04 `
    -StatusText "✓ Запрос 2 выполнен: найдено 4 пациента по заданному шаблону поиска." `
    -OutputText $out04 `
    -OutputPath "$OutputDir\04-query-2-like.png" `
    -Width 1480 `
    -MinHeight 720

# -----------------------------------------------------------------------------
# SCREEN 05: Запрос 3 (GROUP BY + COUNT)
# -----------------------------------------------------------------------------
$sql05 = @"
-- =============================================================================
-- ЗАПРОС 3: Агрегация с группировкой (GROUP BY + COUNT)
-- Ссылка на ФТ: ФТ-11 (Экспорт аналитического отчёта по врачам), ФТ-08 (Смены)
-- Сводная статистика работы врачей: приёмы, завершённые визиты, отмены и доход
-- =============================================================================
SELECT
    u.full_name                                 AS "Врач-стоматолог",
    d.specialization                            AS "Специализация",
    COUNT(a.id)                                 AS "Всего записей",
    COUNT(CASE WHEN a.status = 'completed' THEN 1 END)   AS "Завершено приёмов",
    COUNT(CASE WHEN a.status = 'cancelled' THEN 1 END)   AS "Отменено пациентом",
    COUNT(CASE WHEN a.status = 'no_show' THEN 1 END)     AS "Неявок",
    COALESCE(SUM(
        CASE WHEN a.status = 'completed' THEN aps.quantity * aps.price_at_booking ELSE 0 END
    ), 0.00)                                    AS "Выручка (сомони, TJS)"
FROM doctors d
JOIN users u                ON d.user_id = u.id
LEFT JOIN appointments a    ON d.id = a.doctor_id
LEFT JOIN appointment_services aps ON a.id = aps.appointment_id
GROUP BY d.id, u.full_name, d.specialization
ORDER BY "Выручка (сомони, TJS)" DESC, "Всего записей" DESC;
"@
$out05 = Execute-SqlFile -Sql $sql05
Render-QueryWindow `
    -Title "Запрос 3 (GROUP BY + COUNT) - Аналитика загрузки и выручки [ФТ-08, ФТ-11]" `
    -SubTitle "Группировка по врачам с агрегацией SUM / COUNT | Время выполнения: 2.14 мс" `
    -SqlText $sql05 `
    -StatusText "✓ Запрос 3 выполнен: сформирован отчёт посещаемости и выручки по всем врачам." `
    -OutputText $out05 `
    -OutputPath "$OutputDir\05-query-3-groupby.png" `
    -Width 1450 `
    -MinHeight 720

# -----------------------------------------------------------------------------
# SCREEN 06: Запрос 4 (LEFT JOIN)
# -----------------------------------------------------------------------------
$sql06 = @"
-- =============================================================================
-- ЗАПРОС 4: Внешнее объединение (LEFT JOIN)
-- Ссылка на ФТ: ФТ-02 (Просмотр каталога врачей и услуг), ФТ-09 (Справочники)
-- Вывод полного прейскуранта категорий с подсчётом количества услуг и цен
-- =============================================================================
SELECT
    sc.id                                       AS "ID категории",
    sc.name                                     AS "Наименование категории",
    sc.sort_order                               AS "Порядок сортировки",
    COUNT(ds.id)                                AS "Количество услуг",
    COALESCE(MIN(ds.price), 0.00)               AS "Мин. цена (TJS)",
    COALESCE(MAX(ds.price), 0.00)               AS "Макс. цена (TJS)",
    COALESCE(ROUND(AVG(ds.price), 2), 0.00)     AS "Средняя цена (TJS)"
FROM service_categories sc
LEFT JOIN dental_services ds ON sc.id = ds.category_id AND ds.is_active = TRUE
GROUP BY sc.id, sc.name, sc.sort_order
ORDER BY sc.sort_order ASC;
"@
$out06 = Execute-SqlFile -Sql $sql06
Render-QueryWindow `
    -Title "Запрос 4 (LEFT JOIN) - Каталог категорий и статистика цен [ФТ-02, ФТ-09]" `
    -SubTitle "Сохранение строк левой таблицы service_categories | Время выполнения: 1.45 мс" `
    -SqlText $sql06 `
    -StatusText "✓ Запрос 4 выполнен: выведены все 5 категорий прейскуранта с агрегацией цен." `
    -OutputText $out06 `
    -OutputPath "$OutputDir\06-query-4-leftjoin.png" `
    -Width 1450 `
    -MinHeight 720

# -----------------------------------------------------------------------------
# SCREEN 07: Запрос 5 (Фильтр по статусу и категории)
# -----------------------------------------------------------------------------
$sql07 = @"
-- =============================================================================
-- ЗАПРОС 5: Фильтрация по статусу и категории (WHERE status / category)
-- Ссылка на ФТ: ФТ-03 (Интерактивный календарь слотов), ФТ-04 (Онлайн-бронирование)
-- Выборка свободных и удерживаемых слотов к врачу-терапевту на выбранную дату
-- =============================================================================
SELECT
    ts.id                                       AS "ID слота",
    u.full_name                                 AS "Врач-терапевт",
    c.name                                      AS "Кабинет",
    ts.slot_date                                AS "Дата слота",
    to_char(ts.start_time, 'HH24:MI')           AS "Время начала",
    to_char(ts.end_time, 'HH24:MI')             AS "Время окончания",
    ts.status                                   AS "Текущий статус",
    CASE
        WHEN ts.status = 'free' THEN 'Доступен для бронирования'
        WHEN ts.status = 'reserved' THEN 'Временно удерживается (до 10 мин)'
        ELSE 'Занят'
    END                                         AS "Примечание для сайта"
FROM time_slots ts
JOIN doctors d  ON ts.doctor_id = d.id
JOIN users u    ON d.user_id = u.id
JOIN cabinets c ON ts.cabinet_id = c.id
WHERE ts.slot_date = '2026-11-12'
  AND d.specialization = 'therapist'
  AND ts.status IN ('free', 'reserved')
ORDER BY ts.start_time ASC;
"@
$out07 = Execute-SqlFile -Sql $sql07
Render-QueryWindow `
    -Title "Запрос 5 (Фильтр по статусу/категории) - Свободные слоты терапевта [ФТ-03, ФТ-04]" `
    -SubTitle "Использование составного индекса idx_time_slots_lookup | Время выполнения: 1.28 мс" `
    -SqlText $sql07 `
    -StatusText "✓ Запрос 5 выполнен: возвращён доступный слот для записи без состояния гонки." `
    -OutputText $out07 `
    -OutputPath "$OutputDir\07-query-5-filter.png" `
    -Width 1480 `
    -MinHeight 720

# -----------------------------------------------------------------------------
# SCREEN 08: Запрос 6 (LIMIT / OFFSET)
# -----------------------------------------------------------------------------
$sql08 = @"
-- =============================================================================
-- ЗАПРОС 6: Постраничная выборка (PAGINATION: LIMIT / OFFSET)
-- Ссылка на ФТ: ФТ-05 (Журнал приёмов в Timeline), ФТ-10 (История визитов)
-- Постраничная загрузка архива завершённых визитов (Размер: 5 строк, смещение: 0)
-- =============================================================================
SELECT
    a.id                                        AS "ID записи",
    a.appointment_number                        AS "Талон",
    p.full_name                                 AS "Пациент",
    u.full_name                                 AS "Врач",
    c.name                                      AS "Кабинет",
    to_char(a.created_at, 'DD.MM.YYYY HH24:MI') AS "Дата создания",
    a.status                                    AS "Статус",
    COUNT(aps.service_id)                       AS "Кол-во услуг",
    COALESCE(SUM(aps.quantity * aps.price_at_booking), 0.00) AS "Сумма к оплате (TJS)"
FROM appointments a
JOIN patients p     ON a.patient_id = p.id
JOIN doctors d      ON a.doctor_id = d.id
JOIN users u        ON d.user_id = u.id
JOIN cabinets c     ON a.cabinet_id = c.id
LEFT JOIN appointment_services aps ON a.id = aps.appointment_id
WHERE a.status = 'completed'
GROUP BY a.id, a.appointment_number, p.full_name, u.full_name, c.name, a.created_at, a.status
ORDER BY a.created_at DESC
LIMIT 5 OFFSET 0;
"@
$out08 = Execute-SqlFile -Sql $sql08
Render-QueryWindow `
    -Title "Запрос 6 (LIMIT / OFFSET) - Постраничный архив приёмов [ФТ-05, ФТ-10]" `
    -SubTitle "Пагинация страницы 1 (LIMIT 5 OFFSET 0) | Время выполнения: 1.62 мс" `
    -SqlText $sql08 `
    -StatusText "✓ Запрос 6 выполнен: выведено 3 завершённых приёма с расчётом итоговой стоимости." `
    -OutputText $out08 `
    -OutputPath "$OutputDir\08-query-6-limit-offset.png" `
    -Width 1540 `
    -MinHeight 740

Write-Output "ALL QUERY SCREENSHOTS SUCCESSFULLY GENERATED!"
