import { get, put, post } from "./api"
import type { Hotel } from "@/types"

export const hotelService = {
  async getMyHotel() {
    return get<Hotel>("/api/hotels/me")
  },

  async updateHotel(payload: Partial<Hotel>) {
    return put<Hotel>("/api/hotels/me", payload)
  },

  async getQrCode(hostUrl?: string) {
    const params = hostUrl ? `?hostUrl=${encodeURIComponent(hostUrl)}` : ""
    return get<{ qrCodeUrl: string; qrCodeBase64?: string; checkInUrl: string; publicToken: string; localIp?: string }>(`/api/hotels/qrcode${params}`)
  },

  async seedDemoData() {
    return post<boolean>("/api/hotels/me/seed")
  },

  async uploadLogo(formData: FormData) {
    return post<{ logoUrl: string }>("/api/hotels/logo", formData, {
      headers: { "Content-Type": "multipart/form-data" },
    })
  },
}
