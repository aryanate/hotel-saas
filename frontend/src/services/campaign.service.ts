import { get, post, put, del } from "./api"
import type { Campaign, CampaignRecipient, CampaignStatus } from "@/types"

export interface CreateCampaignPayload {
  name: string
  occasion: string
  messageTemplate: string
  scheduledAt?: string
  onlyConsentGuests?: boolean
}

export const campaignService = {
  async getCampaigns(params?: { status?: CampaignStatus }) {
    return get<Campaign[]>("/api/campaigns", { params })
  },

  async getCampaignById(id: string) {
    return get<Campaign>(`/api/campaigns/${id}`)
  },

  async createCampaign(payload: CreateCampaignPayload) {
    return post<Campaign>("/api/campaigns", payload)
  },

  async updateCampaign(id: string, payload: Partial<CreateCampaignPayload>) {
    return put<Campaign>(`/api/campaigns/${id}`, payload)
  },

  async scheduleCampaign(id: string, scheduledAt: string) {
    return post<Campaign>(`/api/campaigns/${id}/schedule`, { scheduledAt })
  },

  async sendCampaign(id: string) {
    return post<Campaign>(`/api/campaigns/${id}/send`)
  },

  async deleteCampaign(id: string) {
    return del<boolean>(`/api/campaigns/${id}`)
  },

  async getRecipients(id: string) {
    return get<CampaignRecipient[]>(`/api/campaigns/${id}/recipients`)
  },
}
