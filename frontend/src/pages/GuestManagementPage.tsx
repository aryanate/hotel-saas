import { useState } from "react"
import { useQuery } from "@tanstack/react-query"
import { useNavigate } from "react-router-dom"
import { Star, Eye } from "lucide-react"
import { guestService } from "@/services/guest.service"
import type { Guest } from "@/types"
import { Button } from "@/components/ui/button"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, type Column } from "@/components/shared/DataTable"
import { SearchInput } from "@/components/shared/SearchInput"

export function GuestManagementPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)

  const { data: res, isLoading } = useQuery({
    queryKey: ["guests", search, page],
    queryFn: () =>
      guestService.getGuests({
        search: search || undefined,
        page,
        pageSize: 10,
      }),
  })

  const guests: Guest[] = res?.data?.items || []

  const totalPages = res?.data?.totalPages || 1

  const columns: Column<Guest>[] = [
    {
      header: "Guest Name",
      accessorKey: "fullName",
      cell: (item) => (
        <div className="flex items-center gap-2">
          {item.isFavorite && <Star className="h-4 w-4 fill-amber-400 text-amber-400" />}
          <div>
            <p className="font-semibold text-foreground">{item.fullName}</p>
            <p className="text-xs text-muted-foreground">{item.email || "No email"}</p>
          </div>
        </div>
      ),
    },
    {
      header: "Mobile Number",
      accessorKey: "mobile",
    },
    {
      header: "Total Stays",
      accessorKey: "totalVisits",
      cell: (item) => <span>{item.totalVisits} stays</span>,
    },
    {
      header: "Total Spend",
      cell: (item) => <span className="font-semibold">₹{item.totalSpending.toLocaleString("en-IN")}</span>,
    },
    {
      header: "Last Visit",
      cell: (item) => (
        <span className="text-xs text-muted-foreground">
          {item.lastVisitAt ? new Date(item.lastVisitAt).toLocaleDateString() : "Never"}
        </span>
      ),
    },
    {
      header: "Actions",
      cell: (item) => (
        <Button
          variant="ghost"
          size="sm"
          onClick={() => navigate(`/guests/${item.id}`)}
        >
          <Eye className="h-4 w-4 mr-1" /> View Profile
        </Button>
      ),
    },
  ]

  return (
    <div className="space-y-6">
      <PageHeader
        title="Guest Directory"
        description="Comprehensive guest registry, stay history, ID verifications, and VIP status"
      />

      <div className="flex items-center justify-between">
        <SearchInput
          value={search}
          onChange={setSearch}
          placeholder="Search guests by name or phone..."
        />
      </div>

      <DataTable
        data={guests}
        columns={columns}
        isLoading={isLoading}
        pageNumber={page}
        totalPages={totalPages}
        onPageChange={setPage}
        onRowClick={(item) => navigate(`/guests/${item.id}`)}
      />
    </div>
  )
}
