import { get, put } from "./api"
import type { RetentionPolicy, RetentionRecordType } from "@/types"

export const retentionService = {
  async getPolicies() {
    return get<RetentionPolicy[]>("/api/retention/policies")
  },

  async updatePolicies(policies: Array<{ recordType: RetentionRecordType; retentionDays: number; isEnabled: boolean }>) {
    return put<RetentionPolicy[]>("/api/retention/policies", { policies })
  },

  async getDeletionPreview() {
    return get<{ eligibleCount: number; recordSummary: Record<string, number> }>("/api/retention/preview")
  },
}
