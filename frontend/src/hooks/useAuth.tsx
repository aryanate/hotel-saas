import * as React from "react"
import { authService, type LoginRequest, type RegisterRequest } from "@/services/auth.service"
import { apiClient } from "@/services/api"
import type { User, Hotel, AuthTokens } from "@/types"

interface AuthContextType {
  user: User | null
  hotel: Hotel | null
  tokens: AuthTokens | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (payload: LoginRequest) => Promise<void>
  register: (payload: RegisterRequest) => Promise<void>
  logout: () => void
  refreshToken: () => Promise<void>
}

const AuthContext = React.createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = React.useState<User | null>(() => {
    const cached = localStorage.getItem("user")
    return cached ? JSON.parse(cached) : null
  })
  const [hotel, setHotel] = React.useState<Hotel | null>(() => {
    const cached = localStorage.getItem("hotel")
    return cached ? JSON.parse(cached) : null
  })
  const [tokens, setTokens] = React.useState<AuthTokens | null>(() => {
    const accessToken = localStorage.getItem("accessToken")
    const refreshToken = localStorage.getItem("refreshToken")
    return accessToken && refreshToken ? { accessToken, refreshToken } : null
  })
  const [isLoading, setIsLoading] = React.useState<boolean>(true)

  React.useEffect(() => {
    async function loadUser() {
      const accessToken = localStorage.getItem("accessToken")
      if (!accessToken) {
        setIsLoading(false)
        return
      }

      try {
        const response = await authService.getMe()
        if (response.data) {
          setUser(response.data.user)
          setHotel(response.data.hotel || null)
          if (response.data.user) localStorage.setItem("user", JSON.stringify(response.data.user))
          if (response.data.hotel) localStorage.setItem("hotel", JSON.stringify(response.data.hotel))
        }
      } catch (err: unknown) {
        const status = (err as { response?: { status?: number } })?.response?.status
        if (status === 401) {
          localStorage.removeItem("accessToken")
          localStorage.removeItem("refreshToken")
          localStorage.removeItem("user")
          localStorage.removeItem("hotel")
          setUser(null)
          setHotel(null)
          setTokens(null)
        }
      } finally {
        setIsLoading(false)
      }
    }

    loadUser()
  }, [])

  const login = async (payload: LoginRequest) => {
    setIsLoading(true)
    try {
      const response = await authService.login(payload)
      if (response.data) {
        const data = response.data
        const accessToken = data.accessToken || data.tokens?.accessToken
        const refreshToken = data.refreshToken || data.tokens?.refreshToken

        if (accessToken) {
          localStorage.setItem("accessToken", accessToken)
          if (refreshToken) {
            localStorage.setItem("refreshToken", refreshToken)
          }
          if (data.user) {
            localStorage.setItem("user", JSON.stringify(data.user))
          }
          if (data.hotel) {
            localStorage.setItem("hotel", JSON.stringify(data.hotel))
          }

          apiClient.defaults.headers.common.Authorization = `Bearer ${accessToken}`
          setUser(data.user)
          setHotel(data.hotel || null)
          setTokens({
            accessToken,
            refreshToken: refreshToken || "",
          })
        }
      }
    } finally {
      setIsLoading(false)
    }
  }

  const register = async (payload: RegisterRequest) => {
    setIsLoading(true)
    try {
      const response = await authService.register(payload)
      if (response.data) {
        const data = response.data
        const accessToken = data.accessToken || data.tokens?.accessToken
        const refreshToken = data.refreshToken || data.tokens?.refreshToken

        if (accessToken) {
          localStorage.setItem("accessToken", accessToken)
          if (refreshToken) {
            localStorage.setItem("refreshToken", refreshToken)
          }
          if (data.user) {
            localStorage.setItem("user", JSON.stringify(data.user))
          }
          if (data.hotel) {
            localStorage.setItem("hotel", JSON.stringify(data.hotel))
          }

          apiClient.defaults.headers.common.Authorization = `Bearer ${accessToken}`
          setUser(data.user)
          setHotel(data.hotel || null)
          setTokens({
            accessToken,
            refreshToken: refreshToken || "",
          })
        }
      }
    } finally {
      setIsLoading(false)
    }
  }

  const logout = () => {
    authService.logout().catch(() => {})
    localStorage.removeItem("accessToken")
    localStorage.removeItem("refreshToken")
    localStorage.removeItem("user")
    localStorage.removeItem("hotel")
    delete apiClient.defaults.headers.common.Authorization
    setUser(null)
    setHotel(null)
    setTokens(null)
    window.location.href = "/login"
  }

  const refreshToken = async () => {
    const currentRefresh = localStorage.getItem("refreshToken")
    if (!currentRefresh) throw new Error("No refresh token")

    const res = await authService.refreshToken(currentRefresh)
    if (res.data) {
      setTokens(res.data)
      localStorage.setItem("accessToken", res.data.accessToken)
      if (res.data.refreshToken) {
        localStorage.setItem("refreshToken", res.data.refreshToken)
      }
    }
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        hotel,
        tokens,
        isAuthenticated: !!tokens?.accessToken,
        isLoading,
        login,
        register,
        logout,
        refreshToken,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = React.useContext(AuthContext)
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider")
  }
  return context
}
