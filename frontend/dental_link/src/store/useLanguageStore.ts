import { create } from 'zustand'
import { Language, translations } from '../utils/i18n'

interface LanguageState {
  language: Language
  setLanguage: (lang: Language) => void
  t: typeof translations['ru']
}

export const useLanguageStore = create<LanguageState>((set) => ({
  language: (localStorage.getItem('dentallink_lang') as Language) || 'ru',
  setLanguage: (lang: Language) => {
    localStorage.setItem('dentallink_lang', lang)
    set({ language: lang, t: translations[lang] })
  },
  t: translations[(localStorage.getItem('dentallink_lang') as Language) || 'ru'] || translations['ru'],
}))
