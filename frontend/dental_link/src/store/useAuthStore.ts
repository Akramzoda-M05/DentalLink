import { create } from 'zustand'
import { AuthResponseDto, UserDto } from '../types'

interface AuthState {
  user: UserDto | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  setAuth: (data: AuthResponseDto) => void
  logout: () => void
}

const savedAccessToken = localStorage.getItem('dl_access_token')
const savedRefreshToken = localStorage.getItem('dl_refresh_token')
const savedUser = localStorage.getItem('dl_user')

export const useAuthStore = create<AuthState>((set) => ({
  user: savedUser ? JSON.parse(savedUser) : null,
  accessToken: savedAccessToken,
  refreshToken: savedRefreshToken,
  isAuthenticated: !!savedAccessToken,

  setAuth: (data: AuthResponseDto) => {
    localStorage.setItem('dl_access_token', data.accessToken)
    localStorage.setItem('dl_refresh_token', data.refreshToken)
    localStorage.setItem('dl_user', JSON.stringify(data.user))
    set({
      user: data.user,
      accessToken: data.accessToken,
      refreshToken: data.refreshToken,
      isAuthenticated: true,
    })
  },

  logout: () => {
    localStorage.removeItem('dl_access_token')
    localStorage.removeItem('dl_refresh_token')
    localStorage.removeItem('dl_user')
    set({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
    })
  },
}))
