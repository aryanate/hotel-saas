import { get, post, put, del } from "./api"
import type { User } from "@/types"

export interface InviteStaffPayload {
  email: string
  firstName: string
  lastName: string
  mobile?: string
  initialPassword: string
  roles: string[]
}

export const staffService = {
  async getStaff() {
    return get<User[]>("/api/staff")
  },

  async inviteStaff(payload: InviteStaffPayload) {
    return post<User>("/api/staff/invite", payload)
  },

  async updateStaff(id: string, payload: Partial<User>) {
    return put<User>(`/api/staff/${id}`, payload)
  },

  async removeStaff(id: string) {
    return del(`/api/staff/${id}`)
  },

  async assignRoles(userId: string, roles: string[]) {
    return put(`/api/staff/${userId}/roles`, { roles })
  },

  async getRoles() {
    return get<Array<{ id: string; name: string; description: string }>>("/api/staff/roles")
  },
}
