import { useLanguageStore } from '../store/useLanguageStore'
import { Language } from '../utils/i18n'
import { Globe } from 'lucide-react'

export function LanguageSwitcher() {
  const { language, setLanguage } = useLanguageStore()

  const languages: { code: Language; label: string }[] = [
    { code: 'ru', label: 'Рус' },
    { code: 'tg', label: 'Тоҷ' },
    { code: 'en', label: 'Eng' },
  ]

  return (
    <div className="flex items-center gap-1 rounded-xl border border-slate-200 bg-white p-1 shadow-xs">
      <Globe className="h-4 w-4 ml-1.5 text-slate-400" />
      <div className="flex gap-0.5">
        {languages.map((lang) => (
          <button
            key={lang.code}
            onClick={() => setLanguage(lang.code)}
            className={`rounded-lg px-2 py-1 text-xs font-semibold transition-colors cursor-pointer ${
              language === lang.code
                ? 'bg-blue-600 text-white shadow-xs'
                : 'text-slate-600 hover:text-slate-900 hover:bg-slate-100'
            }`}
          >
            {lang.label}
          </button>
        ))}
      </div>
    </div>
  )
}
