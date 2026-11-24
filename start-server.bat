@echo off
chcp 65001 > nul
echo ===================================================
echo 🦷 Запуск проекта DentalLink (Лабораторная работа 11)
echo ===================================================

where php >nul 2>nul
if %errorlevel% equ 0 (
    echo [OK] Обнаружен PHP. Запускаем встроенный PHP-сервер...
    start http://localhost:8000/static/doctors.html
    php -S localhost:8000 -t public
) else (
    echo [INFO] PHP не найден в PATH. Запускаем Node.js сервер...
    start http://localhost:8000/static/doctors.html
    node server.js
)
pause
