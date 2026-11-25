import { apiClient } from './client'
import {
  AppointmentDto,
  AuthResponseDto,
  AvailableSlotDto,
  BookAppointmentResultDto,
  CabinetDto,
  DentalServiceDto,
  DoctorDto,
  DoctorShiftDto,
  HoldSlotResultDto,
  PatientDetailDto,
  PatientDto,
  ServiceCategoryDto,
  UserDto,
} from '../types'

// 1. Auth API
export const authApi = {
  login: async (email: string, password: string): Promise<AuthResponseDto> => {
    const res = await apiClient.post<AuthResponseDto>('/auth/login', { email, password })
    return res.data
  },
  refresh: async (refreshToken: string): Promise<AuthResponseDto> => {
    const res = await apiClient.post<AuthResponseDto>('/auth/refresh', { refreshToken })
    return res.data
  },
  getMe: async (): Promise<UserDto> => {
    const res = await apiClient.get<UserDto>('/auth/me')
    return res.data
  },
  registerStaff: async (data: Record<string, unknown>): Promise<UserDto> => {
    const res = await apiClient.post<UserDto>('/auth/register-staff', data)
    return res.data
  },
}

// 2. Doctors API
export const doctorsApi = {
  getDoctors: async (params?: { specializationId?: number; search?: string; onlyActive?: boolean }): Promise<DoctorDto[]> => {
    const res = await apiClient.get<DoctorDto[]>('/doctors', { params })
    return res.data
  },
  getDoctorById: async (id: number): Promise<DoctorDto> => {
    const res = await apiClient.get<DoctorDto>(`/doctors/${id}`)
    return res.data
  },
  updateDoctor: async (id: number, data: Partial<DoctorDto>): Promise<DoctorDto> => {
    const res = await apiClient.put<DoctorDto>(`/doctors/${id}`, data)
    return res.data
  },
}

// 3. Services API
export const servicesApi = {
  getServices: async (params?: { categoryId?: number; search?: string; onlyActive?: boolean }): Promise<DentalServiceDto[]> => {
    const res = await apiClient.get<DentalServiceDto[]>('/services', { params })
    return res.data
  },
  getCategories: async (): Promise<ServiceCategoryDto[]> => {
    const res = await apiClient.get<ServiceCategoryDto[]>('/services/categories')
    return res.data
  },
  createService: async (data: Partial<DentalServiceDto>): Promise<DentalServiceDto> => {
    const res = await apiClient.post<DentalServiceDto>('/services', data)
    return res.data
  },
  updateService: async (id: number, data: Partial<DentalServiceDto>): Promise<DentalServiceDto> => {
    const res = await apiClient.put<DentalServiceDto>(`/services/${id}`, data)
    return res.data
  },
}

// 4. Cabinets API
export const cabinetsApi = {
  getCabinets: async (onlyAvailable = false): Promise<CabinetDto[]> => {
    const res = await apiClient.get<CabinetDto[]>('/cabinets', { params: { onlyAvailable } })
    return res.data
  },
}

// 5. Schedules API
export const schedulesApi = {
  getSchedule: async (params: { doctorId?: number; cabinetId?: number; startDate?: string; endDate?: string }): Promise<DoctorShiftDto[]> => {
    const res = await apiClient.get<DoctorShiftDto[]>('/schedules', { params })
    return res.data
  },
  generateSchedule: async (data: Record<string, unknown>): Promise<{ generatedSlots: number }> => {
    const res = await apiClient.post<{ generatedSlots: number }>('/schedules/generate', data)
    return res.data
  },
}

// 6. Slots API
export const slotsApi = {
  getAvailableSlots: async (params: { doctorId?: number; specializationId?: number; date?: string }): Promise<AvailableSlotDto[]> => {
    const res = await apiClient.get<AvailableSlotDto[]>('/slots/available', { params })
    return res.data
  },
  holdSlot: async (slotId: number): Promise<HoldSlotResultDto> => {
    const res = await apiClient.post<HoldSlotResultDto>(`/slots/${slotId}/hold`)
    return res.data
  },
  releaseHold: async (slotId: number): Promise<{ released: boolean }> => {
    const res = await apiClient.post<{ released: boolean }>(`/slots/${slotId}/release`)
    return res.data
  },
}

// 7. Appointments API
export const appointmentsApi = {
  book: async (data: {
    slotId: number
    serviceId: number
    patientName: string
    patientPhone: string
    patientEmail?: string
    complaint?: string
  }): Promise<BookAppointmentResultDto> => {
    const res = await apiClient.post<BookAppointmentResultDto>('/appointments/book', data)
    return res.data
  },
  createManual: async (data: {
    slotId: number
    serviceId: number
    patientName: string
    patientPhone: string
    patientEmail?: string
    complaint?: string
  }): Promise<AppointmentDto> => {
    const res = await apiClient.post<AppointmentDto>('/appointments/manual', data)
    return res.data
  },
  getTimeline: async (params: { date: string; cabinetId?: number; doctorId?: number }): Promise<AppointmentDto[]> => {
    const res = await apiClient.get<AppointmentDto[]>('/appointments/timeline', { params })
    return res.data
  },
  getMy: async (phone: string): Promise<AppointmentDto[]> => {
    const res = await apiClient.get<AppointmentDto[]>('/appointments/my', { params: { phone } })
    return res.data
  },
  updateStatus: async (id: number, statusCode: string, doctorNotes?: string): Promise<AppointmentDto> => {
    const res = await apiClient.put<AppointmentDto>(`/appointments/${id}/status`, { statusCode, doctorNotes })
    return res.data
  },
  reschedule: async (id: number, newSlotId: number): Promise<AppointmentDto> => {
    const res = await apiClient.put<AppointmentDto>(`/appointments/${id}/reschedule`, { newSlotId })
    return res.data
  },
  cancel: async (id: number): Promise<AppointmentDto> => {
    const res = await apiClient.post<AppointmentDto>(`/appointments/${id}/cancel`)
    return res.data
  },
}

// 8. Patients API
export const patientsApi = {
  getPatients: async (params?: { search?: string; pageNumber?: number; pageSize?: number }): Promise<{ items: PatientDto[]; totalCount: number }> => {
    const res = await apiClient.get<{ items: PatientDto[]; totalCount: number }>('/patients', { params })
    return res.data
  },
  getPatientById: async (id: number): Promise<PatientDetailDto> => {
    const res = await apiClient.get<PatientDetailDto>(`/patients/${id}`)
    return res.data
  },
}
