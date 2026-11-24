// server.js — универсальный локальный сервер DentalLink
const http = require('node:http');
const fs = require('node:fs');
const path = require('node:path');

const PORT = 8000;
const PUBLIC_DIR = path.join(__dirname, 'public');

const DOCTORS = [
  {
    id: 1,
    full_name: 'Саидов Саид Комилович',
    department: 'Терапевтическое отделение',
    position: 'Главный врач, стоматолог-терапевт',
    specialization: 'therapist',
    experience_years: 10,
    phone: '+992 90 000 0001',
    hire_date: '2015-09-01',
    status: 'active',
  },
  {
    id: 2,
    full_name: 'Рахимов Алишер Баходурович',
    department: 'Терапевтическое отделение',
    position: 'Врач-эндодонтист, микроскопист',
    specialization: 'therapist',
    experience_years: 12,
    phone: '+992 92 700 11 22',
    hire_date: '2018-03-15',
    status: 'active',
  },
  {
    id: 3,
    full_name: 'Умаров Джамшед Каримович',
    department: 'Хирургическое отделение',
    position: 'Хирург-имплантолог, к.м.н.',
    specialization: 'implantologist',
    experience_years: 15,
    phone: '+992 93 500 33 44',
    hire_date: '2016-11-20',
    status: 'active',
  },
  {
    id: 4,
    full_name: 'Турсунова Зарина Мурадовна',
    department: 'Ортодонтическое отделение',
    position: 'Врач-ортодонт высшей категории',
    specialization: 'orthodontist',
    experience_years: 8,
    phone: '+992 98 800 55 66',
    hire_date: '2019-08-01',
    status: 'on_leave',
  },
  {
    id: 5,
    full_name: 'Бобоев Сино Рустамович',
    department: 'Детское отделение',
    position: 'Детский врач-стоматолог',
    specialization: 'pediatric',
    experience_years: 6,
    phone: '+992 91 800 77 88',
    hire_date: '2021-02-10',
    status: 'active',
  },
  {
    id: 6,
    full_name: 'Давлатов Рустам Комронович',
    department: 'Отделение профилактики',
    position: 'Стоматолог-гигиенист',
    specialization: 'hygienist',
    experience_years: 5,
    phone: '+992 90 111 22 33',
    hire_date: '2022-05-18',
    status: 'active',
  },
  {
    id: 7,
    full_name: '<img src=x onerror="alert(\'XSS\')"> Тестов Доктор Врачевич',
    department: 'Тестовая кафедра стоматологии',
    position: 'Врач-стажёр (XSS-тест)',
    specialization: 'therapist',
    experience_years: 1,
    phone: null,
    hire_date: '2024-01-10',
    status: 'active',
  },
];

const mimeTypes = {
  '.html': 'text/html; charset=utf-8',
  '.css': 'text/css; charset=utf-8',
  '.js': 'application/javascript; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.png': 'image/png',
  '.jpg': 'image/jpeg',
  '.svg': 'image/svg+xml',
};

const server = http.createServer((req, res) => {
  const parsed = new URL(req.url, `http://localhost:${PORT}`);
  const pathname = parsed.pathname;

  // Эмуляция API
  if (pathname === '/api/doctors' || pathname === '/api/employees' || pathname.endsWith('/index.php')) {
    const q = (parsed.searchParams.get('q') || '').trim().toLowerCase();
    let result = DOCTORS;
    if (q) {
      result = result.filter(d =>
        d.full_name.toLowerCase().includes(q) ||
        (d.department && d.department.toLowerCase().includes(q)) ||
        (d.position && d.position.toLowerCase().includes(q))
      );
    }
    res.writeHead(200, {
      'Content-Type': 'application/json; charset=utf-8',
      'Access-Control-Allow-Origin': '*',
    });
    return res.end(JSON.stringify(result, null, 2));
  }

  let filePath = path.join(PUBLIC_DIR, pathname);
  if (pathname === '/') {
    filePath = path.join(PUBLIC_DIR, 'static', 'doctors.html');
  }

  fs.stat(filePath, (err, stats) => {
    if (err || !stats.isFile()) {
      res.writeHead(404, { 'Content-Type': 'text/plain; charset=utf-8' });
      return res.end('404 Not Found');
    }
    const ext = path.extname(filePath).toLowerCase();
    const contentType = mimeTypes[ext] || 'application/octet-stream';
    res.writeHead(200, { 'Content-Type': contentType });
    fs.createReadStream(filePath).pipe(res);
  });
});

server.listen(PORT, () => {
  console.log(`=======================================================`);
  console.log(`🚀 Сервер DentalLink успешно запущен!`);
  console.log(`🌐 Адрес каталога врачей: http://localhost:${PORT}/static/doctors.html`);
  console.log(`🌐 Адрес формы записи:   http://localhost:${PORT}/static/appointment-form.html`);
  console.log(`=======================================================`);
});
