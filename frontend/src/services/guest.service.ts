import { get, put, post } from "./api"
import type { Guest, GuestDocument, GuestNote, Booking, PagedResponse } from "@/types"

export interface GuestFilterParams {
  search?: string
  page?: number
  pageSize?: number
  isFavorite?: boolean
}

export const guestService = {
  async getGuests(params?: GuestFilterParams) {
    return get<PagedResponse<Guest>>("/api/guests", { params })
  },

  async getGuestById(id: string) {
    return get<Guest>(`/api/guests/${id}`)
  },

  async updateGuest(id: string, payload: Partial<Guest>) {
    return put<Guest>(`/api/guests/${id}`, payload)
  },

  async getGuestHistory(id: string) {
    return get<Booking[]>(`/api/guests/${id}/history`)
  },

  async getFavorites(params?: { search?: string; page?: number; pageSize?: number }) {
    return get<PagedResponse<Guest>>("/api/guests/favorites", { params })
  },

  async toggleFavorite(id: string) {
    return post<{ isFavorite: boolean }>(`/api/guests/${id}/favorite`)
  },

  async addNote(id: string, noteText: string) {
    return post<GuestNote>(`/api/guests/${id}/notes`, { noteText })
  },

  async getDocuments(id: string) {
    return get<GuestDocument[]>(`/api/guests/${id}/documents`)
  },

  async getDocumentUrl(documentId: string) {
    return get<{ signedUrl: string }>(`/api/guests/documents/${documentId}/url`)
  },
}
