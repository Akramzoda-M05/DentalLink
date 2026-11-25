import { useState, useEffect } from 'react'
import { useQuery, useMutation } from '@tanstack/react-query'
import { servicesApi, doctorsApi, slotsApi, appointmentsApi } from '../api'
import { useBookingStore } from '../store/useBookingStore'
import { useLanguageStore } from '../store/useLanguageStore'
import { Button } from '../components/ui/Button'
import { Card } from '../components/ui/Card'
import { Input } from '../components/ui/Input'
import { Badge } from '../components/ui/Badge'
import {
  Calendar,
  Clock,
  User,
  Phone,
  Mail,
  FileText,
  CheckCircle2,
  AlertCircle,
  Timer,
  ChevronRight,
  ChevronLeft,
  Send,
  Sparkles,
} from 'lucide-react'

export function BookingPage() {
  const {
    step,
    setStep,
    selectedService,
    setSelectedService,
    selectedDoctor,
    setSelectedDoctor,
    selectedDate,
    setSelectedDate,
    selectedSlot,
    setSelectedSlot,
    holdExpiresAt,
    reset,
  } = useBookingStore()

  const { t } = useLanguageStore()

  // Patient form state
  const [patientName, setPatientName] = useState('')
  const [patientPhone, setPatientPhone] = useState('+992')
  const [patientEmail, setPatientEmail] = useState('')
  const [complaint, setComplaint] = useState('')
  const [bookingResult, setBookingResult] = useState<any>(null)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  // Countdown timer for 10-minute hold
  const [timeLeft, setTimeLeft] = useState<number | null>(null)

  useEffect(() => {
    if (!holdExpiresAt) {
      setTimeLeft(null)
      return
    }

    const interval = setInterval(() => {
      const diff = Math.floor((new Date(holdExpiresAt).getTime() - Date.now()) / 1000)
      if (diff <= 0) {
        setTimeLeft(0)
        clearInterval(interval)
      } else {
        setTimeLeft(diff)
      }
    }, 1000)

    return () => clearInterval(interval)
  }, [holdExpiresAt])

  // Queries
  const { data: services = [] } = useQuery({
    queryKey: ['bookingServices'],
    queryFn: () => servicesApi.getServices({ onlyActive: true }),
  })

  const { data: doctors = [] } = useQuery({
    queryKey: ['bookingDoctors'],
    queryFn: () => doctorsApi.getDoctors({ onlyActive: true }),
  })

  const { data: availableSlots = [], isLoading: isLoadingSlots } = useQuery({
    queryKey: ['availableSlots', selectedDoctor?.id, selectedDate],
    queryFn: () =>
      slotsApi.getAvailableSlots({
        doctorId: selectedDoctor?.id,
        date: selectedDate,
      }),
    enabled: !!selectedDoctor && !!selectedDate,
  })

  // Slot Hold Mutation
  const holdMutation = useMutation({
    mutationFn: (slotId: number) => slotsApi.holdSlot(slotId),
    onSuccess: (data, slotId) => {
      const slot = availableSlots.find((s) => s.id === slotId)
      if (slot) {
        setSelectedSlot(slot, data.expiresAt)
        setErrorMessage(null)
      }
    },
    onError: (err: any) => {
      setErrorMessage(err.response?.data?.detail || 'Слот уже занят или удерживается другим пациентом.')
    },
  })

  // Booking Mutation
  const bookMutation = useMutation({
    mutationFn: () =>
      appointmentsApi.book({
        slotId: selectedSlot!.id,
        serviceId: selectedService!.id,
        patientName,
        patientPhone,
        patientEmail: patientEmail || undefined,
        complaint: complaint || undefined,
      }),
    onSuccess: (data) => {
      setBookingResult(data)
      setStep(5)
    },
    onError: (err: any) => {
      setErrorMessage(err.response?.data?.detail || 'Ошибка при сохранении записи. Попробуйте снова.')
    },
  })

  const handleSlotClick = (slot: any) => {
    setErrorMessage(null)
    holdMutation.mutate(slot.id)
  }

  const formatCountdown = (seconds: number) => {
    const m = Math.floor(seconds / 60)
    const s = seconds % 60
    return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-12 sm:px-6 lg:px-8 space-y-8">
      {/* Step Indicators */}
      <div className="flex items-center justify-between border-b border-slate-200 pb-4">
        {[
          { num: 1, label: 'Услуга' },
          { num: 2, label: 'Врач' },
          { num: 3, label: 'Время' },
          { num: 4, label: 'Контакты' },
          { num: 5, label: 'Талон' },
        ].map((s) => (
          <div key={s.num} className="flex items-center gap-2">
            <div
              className={`flex h-8 w-8 items-center justify-center rounded-full text-xs font-bold transition-all ${
                step === s.num
                  ? 'bg-blue-600 text-white ring-4 ring-blue-100 shadow-xs'
                  : step > s.num
                  ? 'bg-emerald-600 text-white'
                  : 'bg-slate-100 text-slate-500'
              }`}
            >
              {step > s.num ? '✓' : s.num}
            </div>
            <span
              className={`hidden sm:block text-xs font-medium ${
                step === s.num ? 'text-slate-900 font-bold' : 'text-slate-400'
              }`}
            >
              {s.label}
            </span>
          </div>
        ))}
      </div>

      {/* Error banner */}
      {errorMessage && (
        <div className="flex items-center gap-2 rounded-2xl bg-rose-50 p-4 text-xs font-medium text-rose-700 border border-rose-200">
          <AlertCircle className="h-5 w-5 shrink-0" />
          <span>{errorMessage}</span>
        </div>
      )}

      {/* STEP 1: SELECT SERVICE */}
      {step === 1 && (
        <div className="space-y-6">
          <div className="space-y-1">
            <h2 className="text-2xl font-bold text-slate-900 tracking-tight">{t.step1Title}</h2>
            <p className="text-sm text-slate-500">Выберите необходимую процедуру из прейскуранта клиники</p>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {services.map((svc) => (
              <Card
                key={svc.id}
                onClick={() => {
                  setSelectedService(svc)
                  setStep(2)
                }}
                className={`cursor-pointer transition-all hover:border-blue-400 hover:shadow-md ${
                  selectedService?.id === svc.id ? 'border-blue-600 ring-2 ring-blue-500/20 bg-blue-50/20' : ''
                }`}
              >
                <div className="flex items-start justify-between gap-2 mb-2">
                  <Badge variant="secondary">{svc.categoryName}</Badge>
                  <span className="text-xs text-slate-400 flex items-center gap-1">
                    <Clock className="h-3 w-3" />
                    {svc.durationMinutes} мин
                  </span>
                </div>
                <h4 className="font-bold text-slate-900 text-sm mb-1">{svc.name}</h4>
                <p className="text-xs text-slate-500 line-clamp-2 mb-3">{svc.description}</p>
                <p className="text-base font-extrabold text-blue-600">
                  {svc.price.toFixed(2)} {t.somoni}
                </p>
              </Card>
            ))}
          </div>
        </div>
      )}

      {/* STEP 2: SELECT DOCTOR */}
      {step === 2 && (
        <div className="space-y-6">
          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <h2 className="text-2xl font-bold text-slate-900 tracking-tight">{t.step2Title}</h2>
              <p className="text-sm text-slate-500">
                Услуга: <span className="font-semibold text-blue-600">{selectedService?.name}</span>
              </p>
            </div>
            <Button variant="outline" size="sm" onClick={() => setStep(1)}>
              <ChevronLeft className="h-4 w-4 mr-1" />
              {t.back}
            </Button>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {doctors.map((doc) => (
              <Card
                key={doc.id}
                onClick={() => {
                  setSelectedDoctor(doc)
                  setStep(3)
                }}
                className={`cursor-pointer transition-all hover:border-blue-400 hover:shadow-md ${
                  selectedDoctor?.id === doc.id ? 'border-blue-600 ring-2 ring-blue-500/20 bg-blue-50/20' : ''
                }`}
              >
                <div className="flex items-center gap-4">
                  <img
                    src={doc.avatarUrl || 'https://images.unsplash.com/photo-1622253692010-333f2da6031d?w=120'}
                    alt={doc.fullName}
                    className="h-16 w-16 rounded-xl object-cover shrink-0"
                  />
                  <div className="space-y-1">
                    <Badge variant="default" className="text-[10px]">
                      {doc.specialization.name}
                    </Badge>
                    <h4 className="font-bold text-slate-900 text-sm leading-tight">{doc.fullName}</h4>
                    <p className="text-xs text-slate-500">{doc.experienceYears} {t.experienceYears}</p>
                  </div>
                </div>
              </Card>
            ))}
          </div>
        </div>
      )}

      {/* STEP 3: DATE & TIME SLOT SELECTION */}
      {step === 3 && (
        <div className="space-y-6">
          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <h2 className="text-2xl font-bold text-slate-900 tracking-tight">{t.step3Title}</h2>
              <p className="text-sm text-slate-500">
                Врач: <span className="font-semibold text-slate-900">{selectedDoctor?.fullName}</span>
              </p>
            </div>
            <Button variant="outline" size="sm" onClick={() => setStep(2)}>
              <ChevronLeft className="h-4 w-4 mr-1" />
              {t.back}
            </Button>
          </div>

          {/* Date Picker Row */}
          <div className="rounded-2xl border border-slate-200 bg-white p-4">
            <label className="block text-xs font-semibold text-slate-700 mb-2">Выберите дату приёма:</label>
            <input
              type="date"
              min={new Date().toISOString().split('T')[0]}
              value={selectedDate}
              onChange={(e) => {
                setSelectedDate(e.target.value)
                setSelectedSlot(null)
              }}
              className="rounded-xl border border-slate-200 px-4 py-2 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
            />
          </div>

          {/* Available Slots Grid */}
          <div className="space-y-3">
            <h4 className="text-xs font-bold text-slate-500 uppercase tracking-wider">
              Свободные слоты на {selectedDate}:
            </h4>

            {isLoadingSlots ? (
              <div className="grid grid-cols-3 sm:grid-cols-6 gap-3">
                {[1, 2, 3, 4, 5, 6].map((i) => (
                  <div key={i} className="h-12 rounded-xl bg-slate-100 animate-pulse" />
                ))}
              </div>
            ) : availableSlots.length === 0 ? (
              <div className="rounded-2xl border border-dashed border-slate-200 p-8 text-center text-sm text-slate-500">
                На выбранную дату свободных окон нет. Пожалуйста, выберите другой день.
              </div>
            ) : (
              <div className="grid grid-cols-2 sm:grid-cols-4 md:grid-cols-6 gap-3">
                {availableSlots.map((slot) => {
                  const isSelected = selectedSlot?.id === slot.id
                  return (
                    <button
                      key={slot.id}
                      onClick={() => handleSlotClick(slot)}
                      className={`flex flex-col items-center justify-center rounded-xl p-3 border text-sm font-semibold transition-all cursor-pointer ${
                        isSelected
                          ? 'border-blue-600 bg-blue-600 text-white shadow-md shadow-blue-500/20'
                          : 'border-slate-200 bg-white text-slate-700 hover:border-blue-400 hover:bg-blue-50/50'
                      }`}
                    >
                      <span className="text-base font-extrabold">{slot.startTime.substring(0, 5)}</span>
                      <span className={`text-[10px] ${isSelected ? 'text-blue-100' : 'text-slate-400'}`}>
                        {slot.cabinetName.split(' ')[0]}
                      </span>
                    </button>
                  )
                })}
              </div>
            )}
          </div>

          {/* Active Hold Countdown Badge */}
          {selectedSlot && timeLeft !== null && timeLeft > 0 && (
            <div className="flex items-center justify-between rounded-2xl bg-amber-50 p-4 border border-amber-200 text-amber-900">
              <div className="flex items-center gap-2">
                <Timer className="h-5 w-5 text-amber-600 animate-pulse" />
                <span className="text-xs font-medium">
                  Слот на <b>{selectedSlot.startTime.substring(0, 5)}</b> временно заблокирован за вами на 10 минут.
                </span>
              </div>
              <span className="font-mono text-sm font-bold bg-amber-200/80 px-2.5 py-1 rounded-lg">
                {formatCountdown(timeLeft)}
              </span>
            </div>
          )}

          {selectedSlot && (
            <div className="flex justify-end pt-4">
              <Button onClick={() => setStep(4)} size="lg" className="gap-2">
                <span>Перейти к контактам</span>
                <ChevronRight className="h-4 w-4" />
              </Button>
            </div>
          )}
        </div>
      )}

      {/* STEP 4: PATIENT DETAILS */}
      {step === 4 && (
        <div className="space-y-6">
          <div className="flex items-center justify-between">
            <div className="space-y-1">
              <h2 className="text-2xl font-bold text-slate-900 tracking-tight">{t.step4Title}</h2>
              <p className="text-sm text-slate-500">Без регистрации и паролей. Запись привязывается к вашему номеру.</p>
            </div>
            <Button variant="outline" size="sm" onClick={() => setStep(3)}>
              <ChevronLeft className="h-4 w-4 mr-1" />
              {t.back}
            </Button>
          </div>

          {/* Booking Summary Box */}
          <div className="rounded-2xl border border-blue-100 bg-blue-50/50 p-4 text-xs text-blue-900 space-y-1.5">
            <p className="font-bold text-sm text-blue-950">Параметры записи:</p>
            <p>Услуга: <b>{selectedService?.name}</b> ({selectedService?.durationMinutes} мин)</p>
            <p>Врач: <b>{selectedDoctor?.fullName}</b> ({selectedDoctor?.specialization.name})</p>
            <p>Дата и время: <b>{selectedDate}</b> в <b>{selectedSlot?.startTime.substring(0, 5)}</b> ({selectedSlot?.cabinetName})</p>
            <p>Стоимость: <b>{selectedService?.price.toFixed(2)} {t.somoni}</b> (оплата в клинике)</p>
          </div>

          <form
            onSubmit={(e) => {
              e.preventDefault()
              bookMutation.mutate()
            }}
            className="space-y-4"
          >
            <Input
              label="Ваше имя и фамилия *"
              required
              placeholder="Алишер Рахимов"
              value={patientName}
              onChange={(e) => setPatientName(e.target.value)}
            />

            <Input
              label="Номер мобильного телефона *"
              type="tel"
              required
              placeholder="+992900000000"
              value={patientPhone}
              onChange={(e) => setPatientPhone(e.target.value)}
              helperText="На этот номер поступит уведомление в Telegram"
            />

            <Input
              label="Электронная почта (необязательно)"
              type="email"
              placeholder="email@example.com"
              value={patientEmail}
              onChange={(e) => setPatientEmail(e.target.value)}
            />

            <div className="space-y-1.5 text-left">
              <label className="block text-xs font-semibold text-slate-700">
                Причина визита / жалобы (необязательно)
              </label>
              <textarea
                rows={3}
                placeholder="Например: болит зуб при горячем, профилактический осмотр..."
                value={complaint}
                onChange={(e) => setComplaint(e.target.value)}
                className="w-full rounded-xl border border-slate-200 p-3 text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
              />
            </div>

            <div className="flex justify-end pt-4">
              <Button type="submit" size="lg" isLoading={bookMutation.isPending} className="w-full sm:w-auto">
                <CheckCircle2 className="h-5 w-5 mr-2" />
                {t.confirmBooking}
              </Button>
            </div>
          </form>
        </div>
      )}

      {/* STEP 5: BOOKING CONFIRMATION & TELEGRAM BOT */}
      {step === 5 && bookingResult && (
        <div className="space-y-6 text-center">
          <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-emerald-100 text-emerald-600">
            <CheckCircle2 className="h-10 w-10" />
          </div>

          <div className="space-y-2">
            <h2 className="text-3xl font-black text-slate-900 tracking-tight">Запись успешно оформлена!</h2>
            <p className="text-sm text-slate-500 max-w-md mx-auto">
              Мы зарезервировали время. Ждём вас в клинике DentalLink без опозданий.
            </p>
          </div>

          {/* Ticket Card */}
          <div className="mx-auto max-w-md rounded-3xl border border-slate-200 bg-white p-6 shadow-lg text-left space-y-4">
            <div className="flex items-center justify-between border-b border-slate-100 pb-3">
              <div>
                <span className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Талон записи</span>
                <p className="text-lg font-black text-blue-600">{bookingResult.appointment.appointmentNumber}</p>
              </div>
              <Badge variant="success">Подтверждена</Badge>
            </div>

            <div className="space-y-2 text-xs text-slate-600">
              <div className="flex justify-between">
                <span className="text-slate-400">Пациент:</span>
                <span className="font-bold text-slate-900">{bookingResult.appointment.patientName}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-400">Врач:</span>
                <span className="font-bold text-slate-900">{bookingResult.appointment.doctorName}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-400">Кабинет:</span>
                <span className="font-bold text-slate-900">{bookingResult.appointment.cabinetName}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-400">Дата и время:</span>
                <span className="font-bold text-blue-600">
                  {bookingResult.appointment.slotDate} в {bookingResult.appointment.startTime.substring(0, 5)}
                </span>
              </div>
            </div>

            {/* Telegram deep link CTA */}
            <div className="rounded-2xl bg-blue-50 p-4 border border-blue-200 space-y-2 text-center">
              <div className="flex items-center justify-center gap-1.5 text-xs font-bold text-blue-900">
                <Send className="h-4 w-4 text-blue-600" />
                <span>Подключите Telegram-бота</span>
              </div>
              <p className="text-[11px] text-blue-700 leading-relaxed">
                Получайте мгновенные напоминания о визите за 24 ч и за 2 ч, подтверждайте или отменяйте запись в один клик.
              </p>
              <a
                href={bookingResult.telegramBotDeepLink}
                target="_blank"
                rel="noreferrer"
                className="inline-flex w-full items-center justify-center rounded-xl bg-blue-600 px-4 py-2 text-xs font-bold text-white hover:bg-blue-700 transition-colors shadow-xs"
              >
                Открыть Telegram Bot
              </a>
            </div>
          </div>

          <div className="pt-4">
            <Button
              variant="outline"
              onClick={() => {
                reset()
                setBookingResult(null)
                setStep(1)
              }}
            >
              Записаться снова
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
