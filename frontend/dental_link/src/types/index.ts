export interface UserDto {
  id: number
  fullName: string
  email: string
  phone: string
  roleCode: 'admin' | 'receptionist' | 'doctor'
  roleName: string
  doctorId?: number | null
}

export interface AuthResponseDto {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: UserDto
}

export interface SpecializationDto {
  id: number
  code: string
  name: string
}

export interface DoctorDto {
  id: number
  userId: number
  fullName: string
  email: string
  phone: string
  specialization: SpecializationDto
  qualification?: string | null
  experienceYears: number
  bio?: string | null
  avatarUrl?: string | null
  isActive: boolean
}

export interface ServiceCategoryDto {
  id: number
  name: string
  description?: string | null
  sortOrder: number
  servicesCount: number
}

export interface DentalServiceDto {
  id: number
  categoryId: number
  categoryName: string
  name: string
  description?: string | null
  price: number
  durationMinutes: number
  isActive: boolean
}

export interface CabinetDto {
  id: number
  name: string
  chairNumber: number
  description?: string | null
  isAvailable: boolean
}

export interface AvailableSlotDto {
  id: number
  doctorId: number
  doctorName: string
  cabinetId: number
  cabinetName: string
  slotDate: string
  startTime: string
  endTime: string
}

export interface HoldSlotResultDto {
  slotId: number
  expiresAt: string
  message: string
}

export interface AppointmentServiceItemDto {
  serviceId: number
  serviceName: string
  price: number
  durationMinutes: number
}

export interface AppointmentDto {
  id: number
  appointmentNumber: string
  patientId: number
  patientName: string
  patientPhone: string
  doctorId: number
  doctorName: string
  doctorSpecialization: string
  cabinetId: number
  cabinetName: string
  slotId: number
  slotDate: string
  startTime: string
  endTime: string
  statusId: number
  statusCode: 'confirmed' | 'arrived' | 'in_progress' | 'completed' | 'cancelled' | 'no_show'
  statusName: string
  patientComplaint?: string | null
  doctorNotes?: string | null
  isConfirmedByPatient: boolean
  createdAt: string
  services: AppointmentServiceItemDto[]
}

export interface BookAppointmentResultDto {
  appointment: AppointmentDto
  telegramBotDeepLink: string
  message: string
}

export interface PatientDto {
  id: number
  fullName: string
  phone: string
  email?: string | null
  birthDate?: string | null
  telegramChatId?: number | null
  notes?: string | null
  createdAt: string
  totalAppointmentsCount: number
}

export interface PatientDetailDto {
  id: number
  fullName: string
  phone: string
  email?: string | null
  birthDate?: string | null
  telegramChatId?: number | null
  notes?: string | null
  createdAt: string
  appointments: AppointmentDto[]
}

export interface TimeSlotSummaryDto {
  id: number
  startTime: string
  endTime: string
  statusId: number
  statusCode: string
  statusName: string
  reservedUntil?: string | null
}

export interface DoctorShiftDto {
  id: number
  doctorId: number
  doctorName: string
  cabinetId: number
  cabinetName: string
  workDate: string
  startTime: string
  endTime: string
  slots: TimeSlotSummaryDto[]
}
