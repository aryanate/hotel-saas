import { get, post, put } from "./api"
import type { Booking, PagedResponse, BookingStatus } from "@/types"

export interface BookingFilterParams {
  search?: string
  status?: BookingStatus
  startDate?: string
  endDate?: string
  page?: number
  pageSize?: number
}

export interface CreateBookingPayload {
  guestId?: string
  guestName?: string
  mobile?: string
  email?: string
  roomIds: string[]
  checkInDate: string
  checkOutDate: string
  numberOfAdults: number
  numberOfChildren: number
  specialRequests?: string
  source?: string
}

export const bookingService = {
  async getBookings(params?: BookingFilterParams) {
    return get<PagedResponse<Booking>>("/api/bookings", { params })
  },

  async createBooking(payload: CreateBookingPayload) {
    return post<Booking>("/api/bookings", payload)
  },

  async getBookingById(id: string) {
    return get<Booking>(`/api/bookings/${id}`)
  },

  async updateBooking(id: string, payload: Partial<CreateBookingPayload>) {
    return put<Booking>(`/api/bookings/${id}`, payload)
  },

  async confirmBooking(id: string) {
    return post<Booking>(`/api/bookings/${id}/confirm`)
  },

  async checkIn(id: string, roomAllocation?: { roomId: string }) {
    return post<Booking>(`/api/bookings/${id}/check-in`, roomAllocation)
  },

  async checkOut(id: string) {
    return post<Booking>(`/api/bookings/${id}/check-out`)
  },

  async cancelBooking(id: string, reason?: string) {
    return post<Booking>(`/api/bookings/${id}/cancel`, { reason })
  },

  async getTodayBookings() {
    return get<{ checkIns: Booking[]; checkOuts: Booking[] }>("/api/bookings/today")
  },

  async sendCheckInLink(id: string) {
    return post<{ checkInUrl: string }>(`/api/bookings/${id}/send-checkin-link`)
  },
}
