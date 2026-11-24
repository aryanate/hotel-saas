import { useState } from "react"
import { useQuery } from "@tanstack/react-query"
import { useNavigate } from "react-router-dom"
import { Plus, CalendarCheck2 } from "lucide-react"
import { bookingService } from "@/services/booking.service"
import type { Booking, BookingStatus } from "@/types"
import { Button } from "@/components/ui/button"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, type Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchInput } from "@/components/shared/SearchInput"
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select"

export function BookingsPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState("")
  const [status, setStatus] = useState<string>("all")
  const [page, setPage] = useState(1)

  const { data: res, isLoading } = useQuery({
    queryKey: ["bookings", search, status, page],
    queryFn: () =>
      bookingService.getBookings({
        search: search || undefined,
        status: status === "all" ? undefined : (status as BookingStatus),
        page,
        pageSize: 10,
      }),
  })

  const bookings: Booking[] = res?.data?.items || []

  const totalPages = res?.data?.totalPages || 1

  const formatDate = (dateStr?: string) =>
    dateStr ? new Date(dateStr).toLocaleDateString("en-IN", { day: "2-digit", month: "short" }) : "-"

  const columns: Column<Booking>[] = [
    {
      header: "Reference",
      accessorKey: "bookingReference",
      cell: (item) => <span className="font-semibold text-primary">{item.bookingReference}</span>,
    },
    {
      header: "Guest",
      cell: (item) => (
        <div>
          <p className="font-medium">{item.guestName || item.guest?.fullName || "Guest"}</p>
          <p className="text-xs text-muted-foreground">{item.guestMobile || item.guest?.mobile || "-"}</p>
        </div>
      ),
    },
    {
      header: "Dates",
      cell: (item) => (
        <div className="text-xs">
          <p>
            {formatDate(item.checkInDate)} &rarr; {formatDate(item.checkOutDate)}
          </p>
          <p className="text-muted-foreground">{Math.max(1, item.totalNights)} nights</p>
        </div>
      ),
    },
    {
      header: "Rooms / Guests",
      cell: (item) => (
        <div className="text-xs">
          <p className="font-medium">
            {item.rooms && item.rooms.length > 0
              ? item.rooms.map((r) => `Room ${r.roomNumber}`).join(", ")
              : "Walk-in / Unassigned"}
          </p>
          <p className="text-muted-foreground">
            {item.numberOfAdults} Ad{item.numberOfChildren > 0 ? `, ${item.numberOfChildren} Ch` : ""}
          </p>
        </div>
      ),
    },
    {
      header: "Source",
      accessorKey: "source",
      cell: (item) => <span className="text-xs font-medium text-muted-foreground">{item.source}</span>,
    },
    {
      header: "Status",
      cell: (item) => <StatusBadge status={item.status} />,
    },
  ]

  return (
    <div className="space-y-6">
      <PageHeader
        title="Reservations & Bookings"
        description="Search, view, and process all guest reservations"
      >
        <Button onClick={() => navigate("/bookings/new")}>
          <Plus className="h-4 w-4 mr-1" /> New Booking
        </Button>
      </PageHeader>

      <div className="flex flex-col sm:flex-row items-center justify-between gap-4">
        <SearchInput
          value={search}
          onChange={setSearch}
          placeholder="Search by reference or guest name..."
        />
        <div className="flex items-center gap-2 w-full sm:w-auto">
          <Select value={status} onValueChange={setStatus}>
            <SelectTrigger className="w-40">
              <SelectValue placeholder="All Statuses" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Statuses</SelectItem>
              <SelectItem value="Pending">Pending</SelectItem>
              <SelectItem value="Confirmed">Confirmed</SelectItem>
              <SelectItem value="CheckedIn">Checked In</SelectItem>
              <SelectItem value="CheckedOut">Checked Out</SelectItem>
              <SelectItem value="Cancelled">Cancelled</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      <DataTable
        data={bookings}
        columns={columns}
        isLoading={isLoading}
        pageNumber={page}
        totalPages={totalPages}
        onPageChange={setPage}
        onRowClick={(item) => navigate(`/bookings/${item.id}`)}
      />
    </div>
  )
}
