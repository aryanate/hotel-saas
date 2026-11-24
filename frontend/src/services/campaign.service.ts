import { get, post, put } from "./api"
import type { Campaign, CampaignRecipient, PagedResponse, MessageChannel } from "@/types"

export interface CreateCampaignPayload {
  title: string
  subject: string
  channel: MessageChannel
  messageContent: string
  scheduledFor?: string
  recipientFilters?: {
    onlyFavorites?: boolean
    minVisits?: number
    minSpend?: number
  }
}

export const campaignService = {
  async getCampaigns(params?: { page?: number; pageSize?: number }) {
    return get<PagedResponse<Campaign>>("/api/campaigns", { params })
  },

  async createCampaign(payload: CreateCampaignPayload) {
    return post<Campaign>("/api/campaigns", payload)
  },

  async updateCampaign(id: string, payload: Partial<CreateCampaignPayload>) {
    return put<Campaign>(`/api/campaigns/${id}`, payload)
  },

  async scheduleCampaign(id: string, scheduledFor: string) {
    return post<Campaign>(`/api/campaigns/${id}/schedule`, { scheduledFor })
  },

  async sendCampaign(id: string) {
    return post<Campaign>(`/api/campaigns/${id}/send`)
  },

  async getRecipients(id: string) {
    return get<CampaignRecipient[]>(`/api/campaigns/${id}/recipients`)
  },
}
