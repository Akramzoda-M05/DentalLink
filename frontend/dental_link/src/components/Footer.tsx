import { Link } from 'react-router-dom'
import { useLanguageStore } from '../store/useLanguageStore'
import { Phone, MapPin, Clock, MessageSquare } from 'lucide-react'

export function Footer() {
  const { t } = useLanguageStore()

  return (
    <footer className="border-t border-slate-200/80 bg-white text-slate-600 mt-auto">
      <div className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
          {/* Col 1: Brand */}
          <div className="space-y-3">
            <div className="flex items-center gap-2">
              <span className="text-2xl">🦷</span>
              <span className="text-xl font-black text-slate-900 tracking-tight">
                Dental<span className="text-blue-600">Link</span>
              </span>
            </div>
            <p className="text-xs text-slate-500 leading-relaxed">
              {t.clinicTagline}. Профессиональное лечение зубов, имплантация и ортодонтия европейского уровня.
            </p>
            <div className="inline-flex items-center gap-2 rounded-xl bg-blue-50 px-3 py-1.5 text-xs font-semibold text-blue-700">
              <MessageSquare className="h-4 w-4 text-blue-600" />
              <span>Telegram Bot: @dentallink_bot</span>
            </div>
          </div>

          {/* Col 2: Navigation */}
          <div>
            <h4 className="text-sm font-bold text-slate-900 mb-3 tracking-wide">Навигация</h4>
            <ul className="space-y-2 text-sm">
              <li>
                <Link to="/" className="hover:text-blue-600 transition-colors">
                  {t.navHome}
                </Link>
              </li>
              <li>
                <Link to="/doctors" className="hover:text-blue-600 transition-colors">
                  {t.navDoctors}
                </Link>
              </li>
              <li>
                <Link to="/services" className="hover:text-blue-600 transition-colors">
                  {t.navServices}
                </Link>
              </li>
              <li>
                <Link to="/booking" className="hover:text-blue-600 transition-colors">
                  {t.navBook}
                </Link>
              </li>
              <li>
                <Link to="/my-appointments" className="hover:text-blue-600 transition-colors">
                  {t.navMyAppointments}
                </Link>
              </li>
            </ul>
          </div>

          {/* Col 3: Contacts */}
          <div>
            <h4 className="text-sm font-bold text-slate-900 mb-3 tracking-wide">Контакты</h4>
            <ul className="space-y-2 text-sm">
              <li className="flex items-center gap-2">
                <Phone className="h-4 w-4 text-blue-600 shrink-0" />
                <span>+992 (44) 600-00-00</span>
              </li>
              <li className="flex items-center gap-2">
                <MapPin className="h-4 w-4 text-blue-600 shrink-0" />
                <span>г. Душанбе, пр. Рудаки, 105</span>
              </li>
              <li className="flex items-center gap-2">
                <Clock className="h-4 w-4 text-blue-600 shrink-0" />
                <span>Ежедневно 09:00 – 21:00</span>
              </li>
            </ul>
          </div>

          {/* Col 4: Guarantees */}
          <div className="space-y-2 text-xs text-slate-500">
            <h4 className="text-sm font-bold text-slate-900 mb-3 tracking-wide">Безопасность и качество</h4>
            <p>100% защита от двойных записей (Distributed Lock). Стерилизация по стандартам «АнтиСПИД» и «АнтиГепатит».</p>
            <p className="pt-2 text-slate-400">© 2026 DentalLink. Все права защищены.</p>
          </div>
        </div>
      </div>
    </footer>
  )
}
