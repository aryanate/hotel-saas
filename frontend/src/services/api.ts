import axios, { type AxiosRequestConfig, type InternalAxiosRequestConfig } from "axios"
import type { ApiResponse } from "@/types"

const getBaseUrl = () => {
  if (import.meta.env.VITE_API_URL) return import.meta.env.VITE_API_URL
  return ""
}

const baseURL = getBaseUrl()

export const apiClient = axios.create({
  baseURL,
  headers: {
    "Content-Type": "application/json",
  },
})

// Request Interceptor: Attach token
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem("accessToken")
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

// Response Interceptor: 401 refresh retry logic
let isRefreshing = false
let failedQueue: Array<{
  resolve: (value?: unknown) => void
  reject: (reason?: unknown) => void
}> = []

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve(token)
    }
  })
  failedQueue = []
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (
        originalRequest.url?.includes("/api/public") ||
        originalRequest.url?.includes("/api/checkin/public") ||
        (typeof window !== "undefined" && window.location.pathname.startsWith("/checkin"))
      ) {
        return Promise.reject(error)
      }

      if (originalRequest.url?.includes("/api/auth/refresh") || originalRequest.url?.includes("/api/auth/login")) {
        localStorage.removeItem("accessToken")
        localStorage.removeItem("refreshToken")
        return Promise.reject(error)
      }

      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        })
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`
            return apiClient(originalRequest)
          })
          .catch((err) => Promise.reject(err))
      }

      originalRequest._retry = true
      isRefreshing = true

      const refreshToken = localStorage.getItem("refreshToken")
      if (!refreshToken) {
        isRefreshing = false
        localStorage.removeItem("accessToken")
        localStorage.removeItem("refreshToken")
        window.location.href = "/login"
        return Promise.reject(error)
      }

      try {
        const { data } = await axios.post(`${baseURL}/api/auth/refresh`, {
          refreshToken,
        })

        const newAccessToken = data.data?.accessToken || data.accessToken
        const newRefreshToken = data.data?.refreshToken || data.refreshToken

        if (newAccessToken) {
          localStorage.setItem("accessToken", newAccessToken)
          if (newRefreshToken) {
            localStorage.setItem("refreshToken", newRefreshToken)
          }
          apiClient.defaults.headers.common.Authorization = `Bearer ${newAccessToken}`
          originalRequest.headers.Authorization = `Bearer ${newAccessToken}`
          processQueue(null, newAccessToken)
          return apiClient(originalRequest)
        }
      } catch (refreshErr) {
        processQueue(refreshErr, null)
        localStorage.removeItem("accessToken")
        localStorage.removeItem("refreshToken")
        window.location.href = "/login"
        return Promise.reject(refreshErr)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  }
)

// Helper methods returning ApiResponse<T>
export async function get<T>(url: string, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
  const response = await apiClient.get<ApiResponse<T> | T>(url, config)
  if (response.data && typeof response.data === "object" && "success" in response.data) {
    return response.data as ApiResponse<T>
  }
  return { success: true, data: response.data as T }
}

export async function post<T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
  const response = await apiClient.post<ApiResponse<T> | T>(url, data, config)
  if (response.data && typeof response.data === "object" && "success" in response.data) {
    return response.data as ApiResponse<T>
  }
  return { success: true, data: response.data as T }
}

export async function put<T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
  const response = await apiClient.put<ApiResponse<T> | T>(url, data, config)
  if (response.data && typeof response.data === "object" && "success" in response.data) {
    return response.data as ApiResponse<T>
  }
  return { success: true, data: response.data as T }
}

export async function del<T>(url: string, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
  const response = await apiClient.delete<ApiResponse<T> | T>(url, config)
  if (response.data && typeof response.data === "object" && "success" in response.data) {
    return response.data as ApiResponse<T>
  }
  return { success: true, data: response.data as T }
}
