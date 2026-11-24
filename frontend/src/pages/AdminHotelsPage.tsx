import { useState } from "react"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { Building2, Power, CheckCircle2, ShieldAlert } from "lucide-react"
import { get, put } from "@/services/api"
import type { Hotel } from "@/types"
import { Button } from "@/components/ui/button"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, type Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchInput } from "@/components/shared/SearchInput"
import { useToast } from "@/components/ui/use-toast"

export function AdminHotelsPage() {
  const { toast } = useToast()
  const queryClient = useQueryClient()
  const [search, setSearch] = useState("")

  const { data: res, isLoading } = useQuery({
    queryKey: ["admin-all-hotels"],
    queryFn: () => get<Hotel[]>("/api/admin/hotels"),
  })

  const hotels: Hotel[] = res?.data || [
    {
      id: "h1",
      createdAt: "2026-10-01",
      updatedAt: "2026-10-01",
      name: "Grand Royal Palace",
      ownerName: "Rajeev Kapoor",
      email: "contact@grandroyal.com",
      mobile: "9876543210",
      address: "123 Luxury Ave",
      city: "Mumbai",
      state: "Maharashtra",
      pinCode: "400001",
      numberOfRooms: 40,
      publicToken: "tok-1",
      invoicePrefix: "INV",
      invoiceSequence: 12,
      checkInTime: "14:00",
      checkOutTime: "11:00",
      currency: "INR",
      timeZone: "Asia/Kolkata",
      isActive: true,
      isDeleted: false,
    },
    {
      id: "h2",
      createdAt: "2026-10-15",
      updatedAt: "2026-10-15",
      name: "Seaside Heritage Resort",
      ownerName: "Anita Rao",
      email: "info@seaside.com",
      mobile: "9123456780",
      address: "Beach Road",
      city: "Goa",
      state: "Goa",
      pinCode: "403001",
      numberOfRooms: 65,
      publicToken: "tok-2",
      invoicePrefix: "SEA",
      invoiceSequence: 34,
      checkInTime: "14:00",
      checkOutTime: "11:00",
      currency: "INR",
      timeZone: "Asia/Kolkata",
      isActive: true,
      isDeleted: false,
    },
    {
      id: "h3",
      createdAt: "2026-11-01",
      updatedAt: "2026-11-01",
      name: "Mountain Mist Haven",
      ownerName: "Devendra Negi",
      email: "mist@mountain.com",
      mobile: "9988771122",
      address: "Old Manali Road",
      city: "Manali",
      state: "Himachal Pradesh",
      pinCode: "175131",
      numberOfRooms: 25,
      publicToken: "tok-3",
      invoicePrefix: "MMH",
      invoiceSequence: 5,
      checkInTime: "14:00",
      checkOutTime: "11:00",
      currency: "INR",
      timeZone: "Asia/Kolkata",
      isActive: false,
      isDeleted: false,
    },
  ]

  const toggleStatusMutation = useMutation({
    mutationFn: ({ id, active }: { id: string; active: boolean }) =>
      put(`/api/admin/hotels/${id}/status`, { isActive: active }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-all-hotels"] })
      toast({ title: "Hotel tenancy status updated" })
    },
  })

  const filtered = hotels.filter(
    (h) =>
      h.name.toLowerCase().includes(search.toLowerCase()) ||
      h.city.toLowerCase().includes(search.toLowerCase()) ||
      h.ownerName.toLowerCase().includes(search.toLowerCase())
  )

  const columns: Column<Hotel>[] = [
    {
      header: "Property Name",
      accessorKey: "name",
      cell: (item) => (
        <div>
          <p className="font-bold text-foreground">{item.name}</p>
          <p className="text-xs text-muted-foreground">
            {item.city}, {item.state}
          </p>
        </div>
      ),
    },
    {
      header: "Owner / Contact",
      cell: (item) => (
        <div>
          <p className="font-medium text-xs">{item.ownerName}</p>
          <p className="text-xs text-muted-foreground">{item.email}</p>
          <p className="text-xs text-muted-foreground">{item.mobile}</p>
        </div>
      ),
    },
    {
      header: "Capacity",
      cell: (item) => <span className="text-xs font-semibold">{item.numberOfRooms} Rooms</span>,
    },
    {
      header: "Status",
      cell: (item) => <StatusBadge status={item.isActive ? "Active" : "Suspended"} />,
    },
    {
      header: "Actions",
      cell: (item) => (
        <Button
          variant={item.isActive ? "destructive" : "default"}
          size="sm"
          className="text-xs"
          onClick={() =>
            toggleStatusMutation.mutate({ id: item.id, active: !item.isActive })
          }
        >
          <Power className="h-3.5 w-3.5 mr-1" />
          {item.isActive ? "Suspend Tenant" : "Activate Tenant"}
        </Button>
      ),
    },
  ]

  return (
    <div className="space-y-6">
      <PageHeader
        title="Tenant Properties Directory"
        description="Monitor all subscribed properties and manage tenant access permissions"
        breadcrumbs={[{ label: "Admin", href: "/admin/dashboard" }, { label: "Hotels" }]}
      />

      <div className="flex items-center justify-between">
        <SearchInput
          value={search}
          onChange={setSearch}
          placeholder="Filter by hotel, owner, or city..."
        />
      </div>

      <DataTable
        data={filtered}
        columns={columns}
        isLoading={isLoading}
      />
    </div>
  )
}
