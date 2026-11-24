import { get, post, put, del } from "./api"
import type { Room, RoomType, RoomStatus } from "@/types"

export interface CreateRoomPayload {
  roomNumber: string
  roomTypeId: string
  floor?: string
  notes?: string
}

export interface CreateRoomTypePayload {
  name: string
  description?: string
  basePrice: number
  capacityAdults: number
  capacityChildren: number
  amenities?: string[]
}

export const roomService = {
  async getRooms(params?: { status?: RoomStatus; floor?: string; roomTypeId?: string }) {
    return get<Room[]>("/api/rooms", { params })
  },

  async createRoom(payload: CreateRoomPayload) {
    return post<Room>("/api/rooms", payload)
  },

  async updateRoom(id: string, payload: Partial<CreateRoomPayload>) {
    return put<Room>(`/api/rooms/${id}`, payload)
  },

  async deleteRoom(id: string) {
    return del(`/api/rooms/${id}`)
  },

  async updateRoomStatus(id: string, status: RoomStatus) {
    return put<Room>(`/api/rooms/${id}/status`, { status })
  },

  async getRoomTypes() {
    return get<RoomType[]>("/api/rooms/types")
  },

  async createRoomType(payload: CreateRoomTypePayload) {
    return post<RoomType>("/api/rooms/types", payload)
  },

  async getAvailability(checkIn: string, checkOut: string) {
    return get<{ availableRooms: Room[]; totalAvailable: number }>("/api/rooms/availability", {
      params: { checkIn, checkOut },
    })
  },
}
