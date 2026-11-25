import { create } from 'zustand'
import { AvailableSlotDto, DentalServiceDto, DoctorDto } from '../types'

interface BookingState {
  step: number
  selectedService: DentalServiceDto | null
  selectedDoctor: DoctorDto | null
  selectedDate: string
  selectedSlot: AvailableSlotDto | null
  holdExpiresAt: string | null

  setStep: (step: number) => void
  setSelectedService: (service: DentalServiceDto | null) => void
  setSelectedDoctor: (doctor: DoctorDto | null) => void
  setSelectedDate: (date: string) => void
  setSelectedSlot: (slot: AvailableSlotDto | null, expiresAt?: string | null) => void
  reset: () => void
}

export const useBookingStore = create<BookingState>((set) => ({
  step: 1,
  selectedService: null,
  selectedDoctor: null,
  selectedDate: new Date().toISOString().split('T')[0],
  selectedSlot: null,
  holdExpiresAt: null,

  setStep: (step) => set({ step }),
  setSelectedService: (service) => set({ selectedService: service }),
  setSelectedDoctor: (doctor) => set({ selectedDoctor: doctor }),
  setSelectedDate: (date) => set({ selectedDate: date }),
  setSelectedSlot: (slot, expiresAt) => set({ selectedSlot: slot, holdExpiresAt: expiresAt ?? null }),

  reset: () =>
    set({
      step: 1,
      selectedService: null,
      selectedDoctor: null,
      selectedDate: new Date().toISOString().split('T')[0],
      selectedSlot: null,
      holdExpiresAt: null,
    }),
}))
