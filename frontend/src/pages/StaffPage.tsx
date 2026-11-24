import { useState } from "react"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { UserPlus, Trash2, Shield } from "lucide-react"
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
  const [initialPassword, setInitialPassword] = useState("Password123!")
  const [roleName, setRoleName] = useState("Receptionist")

  const { data: staffRes, isLoading } = useQuery({
    queryKey: ["staff-members"],
    queryFn: () => staffService.getStaff(),
  })

  const { data: rolesRes } = useQuery({
    queryKey: ["staff-roles"],
    queryFn: () => staffService.getRoles(),
  })

  const staff: User[] = Array.isArray(staffRes?.data) ? staffRes.data : []
  const availableRoles = rolesRes?.data || [
    { id: "HotelAdmin", name: "Hotel Admin", description: "Full hotel administrative access" },
    { id: "Receptionist", name: "Receptionist", description: "Front desk, check-in, bookings" },
    { id: "Housekeeping", name: "Housekeeping", description: "Room cleaning and status management" },
    { id: "Accountant", name: "Accountant", description: "Invoicing, GST reports, payments" },
  ]

  const inviteMutation = useMutation({
    mutationFn: (payload: InviteStaffPayload) => staffService.inviteStaff(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["staff-members"] })
      toast({ title: "Staff member added successfully" })
      setInviteModalOpen(false)
      setEmail("")
      setFirstName("")
      setLastName("")
      setMobile("")
      setInitialPassword("Password123!")
    },
    onError: (err: unknown) => {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
      toast({ title: msg || "Failed to invite staff", variant: "destructive" })
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
    if (!email || !firstName || !lastName || initialPassword.length < 8) {
      toast({ title: "Fill all required fields (password min 8 chars)", variant: "destructive" })
      return
    }
    inviteMutation.mutate({
      email,
      firstName,
      lastName,
      mobile: mobile || undefined,
      initialPassword,
      roles: [roleName],
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
                    {user.firstName?.[0] || "S"}
                    {user.lastName?.[0] || ""}
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
              <Label htmlFor="sPass">Initial Password *</Label>
              <Input
                id="sPass"
                type="password"
                minLength={8}
                placeholder="Min 8 characters"
                value={initialPassword}
                onChange={(e) => setInitialPassword(e.target.value)}
                required
              />
            </div>

            <div className="space-y-2">
              <Label>Assigned Role *</Label>
              <Select value={roleName} onValueChange={setRoleName}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {availableRoles.map((r) => (
                    <SelectItem key={r.id} value={r.id}>
                      {r.name} — {r.description}
                    </SelectItem>
                  ))}
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
