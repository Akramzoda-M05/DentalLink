import { Link } from 'react-router-dom'
import { useLanguageStore } from '../store/useLanguageStore'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import {
  CalendarCheck,
  Clock,
  ShieldCheck,
  Sparkles,
  Award,
  Users,
  ChevronRight,
  PhoneCall,
} from 'lucide-react'

export function HomePage() {
  const { t } = useLanguageStore()

  const stats = [
    { label: 'Довольных пациентов', value: '15 000+' },
    { label: 'Опытных стоматологов', value: '10 врачей' },
    { label: 'Кабинетов и установок', value: '5 кресел' },
    { label: 'Защита от очередей', value: '100% онлайн' },
  ]

  const specializations = [
    { title: 'Терапия и кариес', icon: '✨', desc: 'Эстетическая реставрация и лечение каналов под микроскопом' },
    { title: 'Имплантация под ключ', icon: '🦷', desc: 'Установка имплантатов ведущих брендов Straumann и Osstem' },
    { title: 'Ортодонтия и прикус', icon: '📐', desc: 'Брекеты Damon и незаметные прозрачные 3D-элайнеры' },
    { title: 'Детская стоматология', icon: '🧸', desc: 'Лечение в игровой форме без боли и слёз для малышей' },
    { title: 'Хирургия', icon: '⚡', desc: 'Безболезненное атравматичное удаление и синус-лифтинг' },
    { title: 'Профгигиена', icon: '💎', desc: 'Швейцарский протокол Air-Flow и бережное отбеливание' },
  ]

  return (
    <div className="space-y-16 py-8 sm:py-12">
      {/* Hero Section */}
      <section className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="relative overflow-hidden rounded-3xl bg-linear-to-br from-blue-700 via-blue-600 to-indigo-800 px-6 py-16 sm:px-12 sm:py-24 text-white shadow-xl shadow-blue-900/10">
          <div className="relative z-10 max-w-2xl space-y-6">
            <div className="inline-flex items-center gap-2 rounded-full bg-white/10 px-3.5 py-1 text-xs font-semibold backdrop-blur-md">
              <Sparkles className="h-4 w-4 text-amber-300" />
              <span>Европейские стандарты стоматологии в Душанбе</span>
            </div>

            <h1 className="text-4xl sm:text-5xl lg:text-6xl font-black tracking-tight leading-[1.1]">
              {t.heroTitle}
            </h1>

            <p className="text-base sm:text-lg text-blue-100 font-normal leading-relaxed">
              {t.heroSubtitle}
            </p>

            <div className="flex flex-col sm:flex-row gap-3 pt-2">
              <Link to="/booking">
                <Button size="lg" className="w-full sm:w-auto bg-white text-blue-700 hover:bg-blue-50 shadow-md">
                  <CalendarCheck className="h-5 w-5 mr-2" />
                  {t.heroCtaBook}
                </Button>
              </Link>
              <Link to="/doctors">
                <Button size="lg" variant="outline" className="w-full sm:w-auto border-white/30 text-white bg-white/10 hover:bg-white/20">
                  {t.heroCtaDoctors}
                </Button>
              </Link>
            </div>
          </div>

          {/* Background decorative circles */}
          <div className="absolute -right-20 -top-20 h-96 w-96 rounded-full bg-white/5 blur-3xl pointer-events-none" />
          <div className="absolute right-10 bottom-0 h-64 w-64 rounded-full bg-blue-400/10 blur-2xl pointer-events-none" />
        </div>
      </section>

      {/* Stats Counter */}
      <section className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          {stats.map((s, idx) => (
            <div key={idx} className="rounded-2xl border border-slate-200 bg-white p-6 text-center shadow-xs">
              <p className="text-2xl sm:text-3xl font-extrabold text-blue-600">{s.value}</p>
              <p className="mt-1 text-xs sm:text-sm font-medium text-slate-500">{s.label}</p>
            </div>
          ))}
        </div>
      </section>

      {/* Specializations Grid */}
      <section className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8 space-y-8">
        <div className="text-center space-y-2 max-w-xl mx-auto">
          <h2 className="text-2xl sm:text-3xl font-bold text-slate-900 tracking-tight">
            Основные медицинские направления
          </h2>
          <p className="text-sm text-slate-500">
            Оказываем весь комплекс стоматологических услуг с использованием передового оборудования
          </p>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {specializations.map((spec, i) => (
            <Card key={i} className="hover:border-blue-300 hover:shadow-md transition-all group">
              <div className="text-3xl mb-3">{spec.icon}</div>
              <h3 className="text-base font-bold text-slate-900 mb-1 group-hover:text-blue-600 transition-colors">
                {spec.title}
              </h3>
              <p className="text-xs text-slate-500 leading-relaxed mb-4">{spec.desc}</p>
              <Link
                to="/booking"
                className="inline-flex items-center text-xs font-semibold text-blue-600 hover:text-blue-700"
              >
                <span>Записаться онлайн</span>
                <ChevronRight className="h-3.5 w-3.5 ml-1" />
              </Link>
            </Card>
          ))}
        </div>
      </section>

      {/* Why Choose Us */}
      <section className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="rounded-3xl border border-slate-200 bg-slate-900 text-white p-8 sm:p-12">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div className="flex items-start gap-4">
              <div className="rounded-xl bg-blue-500/20 p-3 text-blue-400">
                <Clock className="h-6 w-6" />
              </div>
              <div className="space-y-1">
                <h4 className="font-bold text-base">Точно по времени</h4>
                <p className="text-xs text-slate-400 leading-relaxed">
                  Слот бронируется персонально за вами с шагом 30 минут без ожидания в коридорах.
                </p>
              </div>
            </div>

            <div className="flex items-start gap-4">
              <div className="rounded-xl bg-emerald-500/20 p-3 text-emerald-400">
                <ShieldCheck className="h-6 w-6" />
              </div>
              <div className="space-y-1">
                <h4 className="font-bold text-base">Защита от накладок</h4>
                <p className="text-xs text-slate-400 leading-relaxed">
                  Распределённая блокировка слота гарантирует, что выбранное время закрепится за вами.
                </p>
              </div>
            </div>

            <div className="flex items-start gap-4">
              <div className="rounded-xl bg-amber-500/20 p-3 text-amber-400">
                <PhoneCall className="h-6 w-6" />
              </div>
              <div className="space-y-1">
                <h4 className="font-bold text-base">Telegram-напоминания</h4>
                <p className="text-xs text-slate-400 leading-relaxed">
                  Бот напомнит о визите за 24 и 2 часа, позволяя подтвердить или перенести приём в один клик.
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>
  )
}
