import { post, get } from "./api"
import type { User, AuthTokens, Hotel } from "@/types"

export interface LoginRequest {
  email: string
  passwordHash?: string
  password?: string
}

export interface RegisterRequest {
  hotelName: string
  ownerName: string
  email: string
  mobile: string
  address: string
  city: string
  state: string
  pinCode: string
  numberOfRooms: number
  password?: string
  gstIn?: string
}

export interface AuthResult {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: User
  hotel?: Hotel
  tokens?: AuthTokens
}

export const authService = {
  async login(payload: LoginRequest) {
    return post<AuthResult>("/api/auth/login", payload)
  },

  async register(payload: RegisterRequest) {
    return post<AuthResult>("/api/auth/register", payload)
  },

  async logout() {
    return post("/api/auth/logout")
  },

  async refreshToken(refreshToken: string) {
    return post<AuthTokens>("/api/auth/refresh", { refreshToken })
  },

  async getMe() {
    return get<{ user: User; hotel: Hotel }>("/api/auth/me")
  },
}
