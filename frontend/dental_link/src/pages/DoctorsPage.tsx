import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { doctorsApi } from '../api'
import { useBookingStore } from '../store/useBookingStore'
import { useLanguageStore } from '../store/useLanguageStore'
import { useNavigate } from 'react-router-dom'
import { Card } from '../components/ui/Card'
import { Button } from '../components/ui/Button'
import { Badge } from '../components/ui/Badge'
import { Stethoscope, Calendar, Award } from 'lucide-react'

export function DoctorsPage() {
  const [selectedSpecialization, setSelectedSpecialization] = useState<number | null>(null)
  const { t } = useLanguageStore()
  const { setSelectedDoctor, setStep } = useBookingStore()
  const navigate = useNavigate()

  const { data: doctors = [], isLoading } = useQuery({
    queryKey: ['doctors', selectedSpecialization],
    queryFn: () => doctorsApi.getDoctors({ specializationId: selectedSpecialization ?? undefined }),
  })

  const specializations = [
    { id: null, name: 'Все врачи' },
    { id: 2, name: 'Терапия' },
    { id: 6, name: 'Имплантология' },
    { id: 4, name: 'Ортодонтия' },
    { id: 7, name: 'Детская стоматология' },
  ]

  const handleBookDoctor = (doc: any) => {
    setSelectedDoctor(doc)
    setStep(3) // Jump to slot selection
    navigate('/booking')
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8 space-y-8">
      {/* Header */}
      <div className="space-y-2">
        <h1 className="text-3xl sm:text-4xl font-extrabold text-slate-900 tracking-tight">
          Врачи-стоматологи клиники
        </h1>
        <p className="text-base text-slate-500 max-w-2xl">
          Команда ведущих специалистов с многолетним стажем работы и международными сертификатами.
        </p>
      </div>

      {/* Specialization Filter Tabs */}
      <div className="flex flex-wrap gap-2">
        {specializations.map((spec) => (
          <button
            key={String(spec.id)}
            onClick={() => setSelectedSpecialization(spec.id)}
            className={`rounded-xl px-4 py-2 text-xs font-semibold transition-all cursor-pointer ${
              selectedSpecialization === spec.id
                ? 'bg-blue-600 text-white shadow-sm'
                : 'bg-white border border-slate-200 text-slate-600 hover:bg-slate-50'
            }`}
          >
            {spec.name}
          </button>
        ))}
      </div>

      {/* Doctors Grid */}
      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-72 rounded-2xl bg-white border border-slate-200 animate-pulse p-6" />
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {doctors.map((doctor) => (
            <Card key={doctor.id} className="flex flex-col justify-between hover:shadow-md transition-all">
              <div className="space-y-4">
                <div className="flex items-start gap-4">
                  <img
                    src={doctor.avatarUrl || 'https://images.unsplash.com/photo-1622253692010-333f2da6031d?w=200'}
                    alt={doctor.fullName}
                    className="h-20 w-20 rounded-2xl object-cover border border-slate-100 shadow-xs shrink-0"
                  />
                  <div>
                    <Badge variant="default" className="mb-1.5">
                      {doctor.specialization.name}
                    </Badge>
                    <h3 className="text-base font-bold text-slate-900 leading-snug">
                      {doctor.fullName}
                    </h3>
                    <p className="text-xs text-slate-500 flex items-center gap-1 mt-1">
                      <Award className="h-3.5 w-3.5 text-amber-500" />
                      <span>{doctor.experienceYears} {t.experienceYears}</span>
                    </p>
                  </div>
                </div>

                <div className="text-xs text-slate-600 space-y-1.5 border-t border-slate-100 pt-3">
                  <p className="font-semibold text-slate-700">{doctor.qualification}</p>
                  <p className="text-slate-500 line-clamp-3">{doctor.bio}</p>
                </div>
              </div>

              <div className="pt-6 border-t border-slate-100 mt-4">
                <Button onClick={() => handleBookDoctor(doctor)} className="w-full gap-2">
                  <Calendar className="h-4 w-4" />
                  <span>Записаться на приём</span>
                </Button>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
