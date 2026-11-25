import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { servicesApi } from '../api'
import { useBookingStore } from '../store/useBookingStore'
import { useLanguageStore } from '../store/useLanguageStore'
import { useNavigate } from 'react-router-dom'
import { Card } from '../components/ui/Card'
import { Button } from '../components/ui/Button'
import { Badge } from '../components/ui/Badge'
import { Clock, Tag, ArrowRight } from 'lucide-react'

export function ServicesPage() {
  const [selectedCategory, setSelectedCategory] = useState<number | null>(null)
  const { t } = useLanguageStore()
  const { setSelectedService, setStep } = useBookingStore()
  const navigate = useNavigate()

  const { data: categories = [] } = useQuery({
    queryKey: ['serviceCategories'],
    queryFn: servicesApi.getCategories,
  })

  const { data: services = [], isLoading } = useQuery({
    queryKey: ['services', selectedCategory],
    queryFn: () => servicesApi.getServices({ categoryId: selectedCategory ?? undefined }),
  })

  const handleSelectService = (service: any) => {
    setSelectedService(service)
    setStep(2) // Jump to doctor selection
    navigate('/booking')
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8 space-y-8">
      {/* Header */}
      <div className="space-y-2">
        <h1 className="text-3xl sm:text-4xl font-extrabold text-slate-900 tracking-tight">
          Услуги и прейскурант клиники
        </h1>
        <p className="text-base text-slate-500 max-w-2xl">
          Прозрачные цены без скрытых платежей. Длительность процедур кратна 30 минутам для точного планирования.
        </p>
      </div>

      {/* Category Pills */}
      <div className="flex flex-wrap gap-2">
        <button
          onClick={() => setSelectedCategory(null)}
          className={`rounded-xl px-4 py-2 text-xs font-semibold transition-all cursor-pointer ${
            selectedCategory === null
              ? 'bg-blue-600 text-white shadow-sm'
              : 'bg-white border border-slate-200 text-slate-600 hover:bg-slate-50'
          }`}
        >
          Все разделы ({services.length})
        </button>
        {categories.map((cat) => (
          <button
            key={cat.id}
            onClick={() => setSelectedCategory(cat.id)}
            className={`rounded-xl px-4 py-2 text-xs font-semibold transition-all cursor-pointer ${
              selectedCategory === cat.id
                ? 'bg-blue-600 text-white shadow-sm'
                : 'bg-white border border-slate-200 text-slate-600 hover:bg-slate-50'
            }`}
          >
            {cat.name} ({cat.servicesCount})
          </button>
        ))}
      </div>

      {/* Services Table / Cards */}
      {isLoading ? (
        <div className="space-y-4">
          {[1, 2, 3, 4].map((i) => (
            <div key={i} className="h-24 rounded-2xl bg-white border border-slate-200 animate-pulse" />
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {services.map((service) => (
            <Card key={service.id} className="flex flex-col justify-between hover:border-blue-200 transition-all p-5">
              <div className="space-y-2">
                <div className="flex items-center justify-between gap-2">
                  <Badge variant="secondary" className="text-[11px]">
                    {service.categoryName}
                  </Badge>
                  <span className="flex items-center gap-1 text-xs text-slate-500">
                    <Clock className="h-3.5 w-3.5 text-slate-400" />
                    <span>{service.durationMinutes} мин</span>
                  </span>
                </div>

                <h3 className="text-base font-bold text-slate-900">{service.name}</h3>
                <p className="text-xs text-slate-500 leading-relaxed">{service.description}</p>
              </div>

              <div className="flex items-center justify-between pt-4 border-t border-slate-100 mt-4">
                <div>
                  <span className="text-xl font-extrabold text-blue-600">
                    {service.price.toFixed(2)}
                  </span>
                  <span className="text-xs font-semibold text-slate-500 ml-1">
                    {t.somoni}
                  </span>
                </div>

                <Button size="sm" onClick={() => handleSelectService(service)} className="gap-1.5">
                  <span>Выбрать</span>
                  <ArrowRight className="h-3.5 w-3.5" />
                </Button>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
