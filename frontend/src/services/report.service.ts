import { get } from "./api"
import type { DashboardStats, GSTReport, OccupancyReport, RevenueReport } from "@/types"

export const reportService = {
  async getDashboard() {
    return get<DashboardStats>("/api/reports/dashboard")
  },

  async getRevenue(params?: { fromDate?: string; toDate?: string }) {
    return get<RevenueReport>("/api/reports/revenue", { params })
  },

  async getOccupancy(params?: { fromDate?: string; toDate?: string }) {
    return get<OccupancyReport>("/api/reports/occupancy", { params })
  },

  async getGstReport(params: { month: number; year: number }) {
    return get<GSTReport>("/api/reports/gst", { params })
  },
}
