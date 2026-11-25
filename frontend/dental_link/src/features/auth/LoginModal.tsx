import React, { useState } from 'react'
import { Dialog } from '../../components/ui/Dialog'
import { Input } from '../../components/ui/Input'
import { Button } from '../../components/ui/Button'
import { authApi } from '../../api'
import { useAuthStore } from '../../store/useAuthStore'
import { useLanguageStore } from '../../store/useLanguageStore'
import { Lock, Mail, AlertCircle } from 'lucide-react'
import { useNavigate } from 'react-router-dom'

interface LoginModalProps {
  open: boolean
  onClose: () => void
}

export function LoginModal({ open, onClose }: LoginModalProps) {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  const { setAuth } = useAuthStore()
  const { t } = useLanguageStore()
  const navigate = useNavigate()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    setIsLoading(true)

    try {
      const data = await authApi.login(email, password)
      setAuth(data)
      onClose()

      // Navigate according to staff role
      if (data.user.roleCode === 'doctor') {
        navigate('/staff/doctor-queue')
      } else {
        navigate('/staff/timeline')
      }
    } catch (err: any) {
      setError(err.response?.data?.detail || 'Неверный email или пароль.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Dialog open={open} onClose={onClose} title={t.navStaffLogin} maxWidth="sm">
      <form onSubmit={handleSubmit} className="space-y-4 pt-2">
        {error && (
          <div className="flex items-center gap-2 rounded-xl bg-red-50 p-3 text-xs text-red-700 border border-red-200">
            <AlertCircle className="h-4 w-4 shrink-0" />
            <span>{error}</span>
          </div>
        )}

        <div className="relative">
          <Input
            label="Email"
            type="email"
            placeholder="admin@dentallink.tj"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
        </div>

        <div className="relative">
          <Input
            label="Пароль"
            type="password"
            placeholder="••••••••"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
        </div>

        <div className="rounded-xl bg-slate-50 p-3 text-xs text-slate-500 border border-slate-100">
          <p className="font-semibold text-slate-700 mb-1">Тестовые аккаунты (пароль: Password123!):</p>
          <ul className="list-disc list-inside space-y-0.5">
            <li>Администратор: <span className="font-mono text-blue-600">admin@dentallink.tj</span></li>
            <li>Ресепшен: <span className="font-mono text-blue-600">reception@dentallink.tj</span></li>
            <li>Врач: <span className="font-mono text-blue-600">a.rahimov@dentallink.tj</span></li>
          </ul>
        </div>

        <div className="flex justify-end gap-2 pt-2">
          <Button type="button" variant="outline" onClick={onClose}>
            Отмена
          </Button>
          <Button type="submit" isLoading={isLoading} className="w-full">
            Войти в систему
          </Button>
        </div>
      </form>
    </Dialog>
  )
}
