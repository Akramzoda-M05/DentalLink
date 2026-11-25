import { useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { useLanguageStore } from '../store/useLanguageStore'
import { useAuthStore } from '../store/useAuthStore'
import { LanguageSwitcher } from './LanguageSwitcher'
import { LoginModal } from '../features/auth/LoginModal'
import { Button } from './ui/Button'
import { Menu, X, Shield, CalendarCheck, Sparkles } from 'lucide-react'

export function Navbar() {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false)
  const [isLoginModalOpen, setIsLoginModalOpen] = useState(false)

  const { t } = useLanguageStore()
  const { isAuthenticated, user } = useAuthStore()
  const location = useLocation()

  const navLinks = [
    { href: '/', label: t.navHome },
    { href: '/doctors', label: t.navDoctors },
    { href: '/services', label: t.navServices },
    { href: '/my-appointments', label: t.navMyAppointments },
  ]

  const isActive = (path: string) => location.pathname === path

  return (
    <>
      <header className="sticky top-0 z-40 w-full border-b border-slate-200/80 bg-white/95 backdrop-blur-md">
        <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
          {/* Logo */}
          <Link to="/" className="flex items-center gap-2.5">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-600 text-white shadow-md shadow-blue-500/20">
              <span className="text-xl font-black">🦷</span>
            </div>
            <div>
              <span className="text-xl font-bold tracking-tight text-slate-900">
                Dental<span className="text-blue-600">Link</span>
              </span>
              <span className="hidden sm:block text-[10px] font-medium text-slate-400 uppercase tracking-widest">
                Digital Clinic
              </span>
            </div>
          </Link>

          {/* Desktop Nav */}
          <nav className="hidden md:flex items-center gap-1">
            {navLinks.map((link) => (
              <Link
                key={link.href}
                to={link.href}
                className={`rounded-xl px-3.5 py-2 text-sm font-medium transition-colors ${
                  isActive(link.href)
                    ? 'bg-blue-50 text-blue-700 font-semibold'
                    : 'text-slate-600 hover:text-slate-900 hover:bg-slate-50'
                }`}
              >
                {link.label}
              </Link>
            ))}
          </nav>

          {/* Right side tools */}
          <div className="hidden md:flex items-center gap-3">
            <LanguageSwitcher />

            {isAuthenticated ? (
              <Link to={user?.roleCode === 'doctor' ? '/staff/doctor-queue' : '/staff/timeline'}>
                <Button variant="outline" size="sm" className="gap-2">
                  <Shield className="h-4 w-4 text-blue-600" />
                  <span>{user?.fullName.split(' ')[0]} ({user?.roleName})</span>
                </Button>
              </Link>
            ) : (
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setIsLoginModalOpen(true)}
                className="text-slate-600"
              >
                <Shield className="h-4 w-4 text-slate-400 mr-1.5" />
                <span>{t.navStaffLogin}</span>
              </Button>
            )}

            <Link to="/booking">
              <Button size="sm" className="gap-2 font-semibold">
                <CalendarCheck className="h-4 w-4" />
                <span>{t.navBook}</span>
              </Button>
            </Link>
          </div>

          {/* Mobile hamburger */}
          <div className="flex md:hidden items-center gap-2">
            <LanguageSwitcher />
            <button
              onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
              className="rounded-lg p-2 text-slate-600 hover:bg-slate-100"
            >
              {isMobileMenuOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
            </button>
          </div>
        </div>

        {/* Mobile menu dropdown */}
        {isMobileMenuOpen && (
          <div className="border-b border-slate-200 bg-white px-4 pt-2 pb-6 md:hidden space-y-3">
            <div className="flex flex-col space-y-1">
              {navLinks.map((link) => (
                <Link
                  key={link.href}
                  to={link.href}
                  onClick={() => setIsMobileMenuOpen(false)}
                  className={`rounded-xl px-3 py-2 text-base font-medium ${
                    isActive(link.href) ? 'bg-blue-50 text-blue-700 font-semibold' : 'text-slate-600'
                  }`}
                >
                  {link.label}
                </Link>
              ))}
            </div>

            <div className="pt-2 border-t border-slate-100 flex flex-col gap-2">
              <Link to="/booking" onClick={() => setIsMobileMenuOpen(false)}>
                <Button className="w-full justify-center">
                  <CalendarCheck className="h-4 w-4 mr-2" />
                  {t.navBook}
                </Button>
              </Link>

              {isAuthenticated ? (
                <Link
                  to={user?.roleCode === 'doctor' ? '/staff/doctor-queue' : '/staff/timeline'}
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  <Button variant="outline" className="w-full">
                    {t.navStaffPortal} ({user?.fullName})
                  </Button>
                </Link>
              ) : (
                <Button
                  variant="outline"
                  onClick={() => {
                    setIsMobileMenuOpen(false)
                    setIsLoginModalOpen(true)
                  }}
                  className="w-full"
                >
                  <Shield className="h-4 w-4 mr-2 text-slate-400" />
                  {t.navStaffLogin}
                </Button>
              )}
            </div>
          </div>
        )}
      </header>

      <LoginModal open={isLoginModalOpen} onClose={() => setIsLoginModalOpen(false)} />
    </>
  )
}
