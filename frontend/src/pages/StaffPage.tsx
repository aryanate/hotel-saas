import { useState } from "react"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { Plus, UserPlus, Trash2, Shield } from "lucide-react"
import { staffService, type InviteStaffPayload } from "@/services/staff.service"
import type { User } from "@/types"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { PageHeader } from "@/components/shared/PageHeader"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"
import { EmptyState } from "@/components/shared/EmptyState"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog"
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select"
import { useToast } from "@/components/ui/use-toast"

export function StaffPage() {
  const { toast } = useToast()
  const queryClient = useQueryClient()
  const [inviteModalOpen, setInviteModalOpen] = useState(false)
  const [email, setEmail] = useState("")
  const [firstName, setFirstName] = useState("")
  const [lastName, setLastName] = useState("")
  const [mobile, setMobile] = useState("")
  const [roleId, setRoleId] = useState("role-receptionist")

  const { data: staffRes, isLoading } = useQuery({
    queryKey: ["staff-members"],
    queryFn: () => staffService.getStaff(),
  })

  const staff: User[] = (staffRes?.data || [
    {
      id: "u1",
      createdAt: "2026-10-01",
      updatedAt: "2026-10-01",
      email: "manager@hotel.com",
      firstName: "Rajeev",
      lastName: "Kapoor",
      mobile: "9876543210",
      isActive: true,
      roles: ["Manager"],
    },
    {
      id: "u2",
      createdAt: "2026-10-05",
      updatedAt: "2026-10-05",
      email: "frontdesk@hotel.com",
      firstName: "Pooja",
      lastName: "Nair",
      mobile: "9123456789",
      isActive: true,
      roles: ["Receptionist"],
    },
    {
      id: "u3",
      createdAt: "2026-10-10",
      updatedAt: "2026-10-10",
      email: "cleaning@hotel.com",
      firstName: "Mohan",
      lastName: "Singh",
      mobile: "9988776655",
      isActive: true,
      roles: ["Housekeeping"],
    },
  ]) as User[]

  const inviteMutation = useMutation({
    mutationFn: (payload: InviteStaffPayload) => staffService.inviteStaff(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["staff-members"] })
      toast({ title: "Staff invitation sent via email" })
      setInviteModalOpen(false)
      setEmail("")
      setFirstName("")
      setLastName("")
      setMobile("")
    },
    onError: () => {
      toast({ title: "Failed to invite staff", variant: "destructive" })
    },
  })

  const removeMutation = useMutation({
    mutationFn: (id: string) => staffService.removeStaff(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["staff-members"] })
      toast({ title: "Staff member removed" })
    },
  })

  const handleInvite = (e: React.FormEvent) => {
    e.preventDefault()
    if (!email || !firstName || !lastName) {
      toast({ title: "Please fill required fields", variant: "destructive" })
      return
    }
    inviteMutation.mutate({
      email,
      firstName,
      lastName,
      mobile: mobile || undefined,
      roleId,
    })
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Staff & User Management"
        description="Invite front-desk operators, managers, housekeepers, and configure access permissions"
      >
        <Button onClick={() => setInviteModalOpen(true)}>
          <UserPlus className="h-4 w-4 mr-1" /> Invite Staff Member
        </Button>
      </PageHeader>

      {isLoading ? (
        <LoadingSpinner size="lg" className="h-64" />
      ) : staff.length === 0 ? (
        <EmptyState
          title="No staff members added"
          description="Add receptionists and operators to grant portal access."
          actionLabel="Invite Staff"
          onAction={() => setInviteModalOpen(true)}
        />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {staff.map((user: User) => (
            <div
              key={user.id}
              className="rounded-lg border bg-card p-5 shadow-sm space-y-4 flex flex-col justify-between"
            >
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <div className="h-10 w-10 rounded-full bg-primary/10 text-primary flex items-center justify-center font-bold">
                    {user.firstName[0]}
                    {user.lastName[0]}
                  </div>
                  <span className="text-xs bg-slate-100 text-slate-800 px-2 py-1 rounded font-semibold border flex items-center gap-1">
                    <Shield className="h-3 w-3 text-primary" /> {user.roles?.[0] || "Staff"}
                  </span>
                </div>

                <div>
                  <h4 className="font-bold text-base">
                    {user.firstName} {user.lastName}
                  </h4>
                  <p className="text-xs text-muted-foreground">{user.email}</p>
                  <p className="text-xs text-muted-foreground mt-0.5">{user.mobile || "No phone"}</p>
                </div>
              </div>

              <div className="pt-3 border-t flex justify-end">
                <Button
                  variant="ghost"
                  size="sm"
                  className="text-destructive text-xs hover:bg-destructive/10"
                  onClick={() => {
                    if (confirm(`Remove access for ${user.firstName}?`)) {
                      removeMutation.mutate(user.id)
                    }
                  }}
                >
                  <Trash2 className="h-3.5 w-3.5 mr-1" /> Remove Access
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Invite Dialog */}
      <Dialog open={inviteModalOpen} onOpenChange={setInviteModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Invite Staff Member</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleInvite} className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-2">
                <Label htmlFor="fname">First Name *</Label>
                <Input
                  id="fname"
                  value={firstName}
                  onChange={(e) => setFirstName(e.target.value)}
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="lname">Last Name *</Label>
                <Input
                  id="lname"
                  value={lastName}
                  onChange={(e) => setLastName(e.target.value)}
                  required
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="sEmail">Email Address *</Label>
              <Input
                id="sEmail"
                type="email"
                placeholder="staff@hotel.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="sPhone">Mobile / Phone</Label>
              <Input
                id="sPhone"
                placeholder="9876543210"
                value={mobile}
                onChange={(e) => setMobile(e.target.value)}
              />
            </div>

            <div className="space-y-2">
              <Label>Assigned Role *</Label>
              <Select value={roleId} onValueChange={setRoleId}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="role-manager">Hotel Manager (Full Control)</SelectItem>
                  <SelectItem value="role-receptionist">Front Desk Receptionist (Check-in/Out, Invoicing)</SelectItem>
                  <SelectItem value="role-housekeeping">Housekeeping (Room Status Updates)</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setInviteModalOpen(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={inviteMutation.isPending}>
                {inviteMutation.isPending ? "Sending..." : "Send Invitation"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
