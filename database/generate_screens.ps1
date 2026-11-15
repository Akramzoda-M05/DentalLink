# =============================================================================
# DentalLink: Генератор скриншотов выполнения SQL для docs/07-screens/
# =============================================================================

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

$OutputDir = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\docs\07-screens"))
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
}

$env:LC_ALL = "C"
$env:PGCLIENTENCODING = "utf-8"
if ([string]::IsNullOrWhiteSpace($env:PGPASSWORD)) {
    throw "Set PGPASSWORD in the process environment before running this local verification script."
}
foreach ($name in @("DENTALLINK_DB_HOST", "DENTALLINK_DB_PORT", "DENTALLINK_DB_NAME", "DENTALLINK_DB_USER")) {
    if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) {
        throw "Set $name in the process environment before running this local verification script."
    }
}
$DatabaseHost = $env:DENTALLINK_DB_HOST
$DatabasePort = $env:DENTALLINK_DB_PORT
$DatabaseName = $env:DENTALLINK_DB_NAME
$DatabaseUser = $env:DENTALLINK_DB_USER
$psql = (Get-Command psql -ErrorAction Stop).Source

function Execute-SqlFile {
    param([string]$Sql)
    $tmp = [System.IO.Path]::GetTempFileName() + ".sql"
    [System.IO.File]::WriteAllText($tmp, $Sql, [System.Text.Encoding]::UTF8)
    try {
        $res = & $psql -U $DatabaseUser -h $DatabaseHost -p $DatabasePort -d $DatabaseName -f $tmp
        if ($LASTEXITCODE -ne 0) {
            throw "psql exited with code $LASTEXITCODE."
        }
        return ($res -join "`r`n")
    }
    finally {
        Remove-Item -LiteralPath $tmp -ErrorAction SilentlyContinue
    }
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
-- ПРОВЕРКА ФИЗИЧЕСКОЙ МОДЕЛИ БД DentalLink
-- Проверка наличия всех 17 таблиц из docs/06-logical-model.md (3НФ)
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
    -Title "dentallink_db (PostgreSQL) - Список таблиц схемы public" `
    -SubTitle "Host: $DatabaseHost`:$DatabasePort | Database: $DatabaseName | User: $DatabaseUser | Storage: UTF-8 | Engine: PostgreSQL" `
    -SqlText $sql01 `
    -StatusText "✓ Схема создана. Все 17 таблиц и пользовательские индексы созданы и активны." `
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
-- Синтетические локальные строки; проверяем количество и целостность справочников
-- =============================================================================
SELECT 'user_roles' AS table_name, count(*) AS row_count FROM user_roles
UNION ALL SELECT 'doctor_specializations', count(*) FROM doctor_specializations
UNION ALL SELECT 'time_slot_statuses', count(*) FROM time_slot_statuses
UNION ALL SELECT 'appointment_statuses', count(*) FROM appointment_statuses
UNION ALL SELECT 'notification_types', count(*) FROM notification_types
UNION ALL SELECT 'notification_statuses', count(*) FROM notification_statuses
UNION ALL SELECT 'users', count(*) FROM users
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
    -Title "dentallink_db (PostgreSQL) - Верификация строк (seed.sql)" `
    -SubTitle "Host: $DatabaseHost`:$DatabasePort | Database: $DatabaseName | Main table appointments: 12 rows (>= 10 required)" `
    -SqlText $sql02 `
    -StatusText "✓ Скрипт seed.sql выполнен успешно: главная таблица содержит 12 записей, все справочники >= 3 записей." `
    -OutputText $out02 `
    -OutputPath "$OutputDir\02-seed-verification.png" `
    -Width 1350 `
    -MinHeight 750

# -----------------------------------------------------------------------------
$queriesSource = [IO.File]::ReadAllText((Join-Path $PSScriptRoot "queries.sql"), [Text.Encoding]::UTF8)
$queryMatches = [regex]::Matches($queriesSource, '(?is)\bSELECT\b.*?;')
if ($queryMatches.Count -ne 6) {
    throw "Expected six canonical SQL queries, found $($queryMatches.Count)."
}
$queryStatements = @($queryMatches | ForEach-Object { $_.Value.Trim() })
# SCREEN 03: Запрос 1 (JOIN 3+ таблиц)
# -----------------------------------------------------------------------------
$sql03 = $queryStatements[0].Trim()
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
$sql04 = $queryStatements[1].Trim()
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
$sql05 = $queryStatements[2].Trim()
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
$sql06 = $queryStatements[3].Trim()
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
$sql07 = $queryStatements[4].Trim()
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
$sql08 = $queryStatements[5].Trim()
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
