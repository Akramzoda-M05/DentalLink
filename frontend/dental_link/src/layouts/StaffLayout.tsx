import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuthStore } from '../store/useAuthStore'
import { LanguageSwitcher } from '../components/LanguageSwitcher'
import {
  CalendarDays,
  Users,
  Stethoscope,
  Sparkles,
  Layers,
  LogOut,
  ArrowLeft,
  CalendarRange,
} from 'lucide-react'

export function StaffLayout() {
  const { user, logout } = useAuthStore()
  const location = useLocation()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/')
  }

  const role = user?.roleCode

  const menuItems = [
    {
      to: '/staff/timeline',
      label: 'Сетка приёмов (Timeline)',
      icon: CalendarDays,
      roles: ['admin', 'receptionist', 'doctor'],
    },
    {
      to: '/staff/doctor-queue',
      label: 'Приём пациентов (Врач)',
      icon: Stethoscope,
      roles: ['admin', 'doctor'],
    },
    {
      to: '/staff/patients',
      label: 'База пациентов (CRM)',
      icon: Users,
      roles: ['admin', 'receptionist'],
    },
    {
      to: '/staff/schedules',
      label: 'График смен и слоты',
      icon: CalendarRange,
      roles: ['admin'],
    },
    {
      to: '/staff/services',
      label: 'Услуги и прейскурант',
      icon: Layers,
      roles: ['admin'],
    },
  ]

  const filteredMenu = menuItems.filter((item) => role && item.roles.includes(role))

  return (
    <div className="flex min-h-screen bg-slate-100/70 text-slate-900">
      {/* Sidebar */}
      <aside className="w-64 border-r border-slate-200 bg-white flex flex-col shrink-0">
        {/* Brand */}
        <div className="h-16 border-b border-slate-200 px-6 flex items-center justify-between">
          <Link to="/" className="flex items-center gap-2">
            <span className="text-xl">🦷</span>
            <span className="text-lg font-bold tracking-tight text-slate-900">
              Dental<span className="text-blue-600">Link</span>
            </span>
          </Link>
          <span className="rounded-md bg-blue-50 px-2 py-0.5 text-[10px] font-bold text-blue-700 uppercase">
            {role}
          </span>
        </div>

        {/* Navigation */}
        <nav className="flex-1 p-4 space-y-1 overflow-y-auto">
          {filteredMenu.map((item) => {
            const Icon = item.icon
            const isActive = location.pathname === item.to
            return (
              <Link
                key={item.to}
                to={item.to}
                className={`flex items-center gap-3 rounded-xl px-3.5 py-2.5 text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-blue-600 text-white shadow-sm shadow-blue-500/20 font-semibold'
                    : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900'
                }`}
              >
                <Icon className={`h-4 w-4 shrink-0 ${isActive ? 'text-white' : 'text-slate-400'}`} />
                <span>{item.label}</span>
              </Link>
            )
          })}
        </nav>

        {/* User Card & Logout */}
        <div className="border-t border-slate-200 p-4 space-y-3">
          <div className="flex items-center justify-between">
            <div className="truncate">
              <p className="text-sm font-semibold text-slate-900 truncate">{user?.fullName}</p>
              <p className="text-xs text-slate-500 truncate">{user?.roleName}</p>
            </div>
            <LanguageSwitcher />
          </div>

          <div className="flex items-center gap-2">
            <Link
              to="/"
              className="flex-1 inline-flex items-center justify-center gap-1.5 rounded-xl border border-slate-200 bg-white px-3 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50"
            >
              <ArrowLeft className="h-3.5 w-3.5" />
              <span>На сайт</span>
            </Link>
            <button
              onClick={handleLogout}
              className="inline-flex items-center justify-center rounded-xl border border-rose-200 bg-rose-50 px-3 py-2 text-xs font-semibold text-rose-700 hover:bg-rose-100 transition-colors cursor-pointer"
              title="Выйти"
            >
              <LogOut className="h-4 w-4" />
            </button>
          </div>
        </div>
      </aside>

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col overflow-x-hidden">
        <header className="h-16 border-b border-slate-200 bg-white/80 backdrop-blur-md px-6 flex items-center justify-between">
          <div className="flex items-center gap-2 text-sm text-slate-500">
            <span>Кабинет клиники</span>
            <span>/</span>
            <span className="font-semibold text-slate-900 capitalize">
              {filteredMenu.find((m) => m.to === location.pathname)?.label || 'Управление'}
            </span>
          </div>

          <div className="flex items-center gap-3">
            <span className="flex h-2.5 w-2.5 rounded-full bg-emerald-500 animate-pulse" />
            <span className="text-xs font-medium text-slate-600">Система активна: PostgreSQL + Redis</span>
          </div>
        </header>

        <main className="flex-1 p-6 overflow-y-auto">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
