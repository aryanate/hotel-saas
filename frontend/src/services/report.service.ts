import { get } from "./api"
import type { DashboardStats, GSTReport, OccupancyReport } from "@/types"

export const reportService = {
  async getDashboard() {
    return get<DashboardStats>("/api/reports/dashboard")
  },

  async getRevenue(params?: { startDate?: string; endDate?: string }) {
    return get<{ labels: string[]; data: number[]; totalRevenue: number }>("/api/reports/revenue", { params })
  },

  async getOccupancy(params?: { startDate?: string; endDate?: string }) {
    return get<OccupancyReport[]>("/api/reports/occupancy", { params })
  },

  async getGstReport(params: { month: number; year: number }) {
    return get<GSTReport>("/api/reports/gst", { params })
  },
}
