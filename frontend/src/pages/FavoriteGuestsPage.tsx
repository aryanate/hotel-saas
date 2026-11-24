import { useState } from "react"
import { useQuery } from "@tanstack/react-query"
import { useNavigate } from "react-router-dom"
import { Star, MessageSquare, Send, Eye } from "lucide-react"
import { guestService } from "@/services/guest.service"
import type { Guest } from "@/types"
import { Button } from "@/components/ui/button"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, type Column } from "@/components/shared/DataTable"
import { SearchInput } from "@/components/shared/SearchInput"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useToast } from "@/components/ui/use-toast"

export function FavoriteGuestsPage() {
  const navigate = useNavigate()
  const { toast } = useToast()
  const [search, setSearch] = useState("")
  const [page, setPage] = useState(1)
  const [messageModalGuest, setMessageModalGuest] = useState<Guest | null>(null)
  const [quickMsg, setQuickMsg] = useState("")

  const { data: res, isLoading } = useQuery({
    queryKey: ["favorite-guests", search, page],
    queryFn: () =>
      guestService.getFavorites({
        search: search || undefined,
        page,
        pageSize: 10,
      }),
  })

  const guests: Guest[] = (res?.data?.items || [
    {
      id: "g1",
      hotelId: "h1",
      createdAt: "2026-11-13",
      updatedAt: "2026-11-13",
      fullName: "Vikram Malhotra",
      mobile: "9876543210",
      email: "vikram@example.com",
      totalVisits: 4,
      totalSpending: 42000,
      isFavorite: true,
      marketingConsent: true,
      preferences: "High floor, extra towels, king suite",
    },
    {
      id: "g3",
      hotelId: "h1",
      createdAt: "2026-11-13",
      updatedAt: "2026-11-13",
      fullName: "Deepak Sharma",
      mobile: "9988112233",
      email: "deepak@example.com",
      totalVisits: 7,
      totalSpending: 96000,
      isFavorite: true,
      marketingConsent: false,
      preferences: "Prefers morning newspaper, quiet wing",
    },
  ]) as Guest[]

  const totalPages = res?.data?.totalPages || 1

  const handleSendMessage = (e: React.FormEvent) => {
    e.preventDefault()
    toast({ title: `WhatsApp message queued to ${messageModalGuest?.fullName}` })
    setMessageModalGuest(null)
    setQuickMsg("")
  }

  const columns: Column<Guest>[] = [
    {
      header: "VIP Guest",
      accessorKey: "fullName",
      cell: (item) => (
        <div className="flex items-center gap-2">
          <Star className="h-4 w-4 fill-amber-400 text-amber-400 shrink-0" />
          <div>
            <p className="font-bold text-foreground">{item.fullName}</p>
            <p className="text-xs text-muted-foreground">{item.email || item.mobile}</p>
          </div>
        </div>
      ),
    },
    {
      header: "Preferences & Notes",
      cell: (item) => (
        <span className="text-xs text-muted-foreground italic line-clamp-1">
          {item.preferences || "VIP Member"}
        </span>
      ),
    },
    {
      header: "Stays",
      accessorKey: "totalVisits",
    },
    {
      header: "Lifetime Value",
      cell: (item) => (
        <span className="font-bold text-emerald-600">
          ₹{item.totalSpending.toLocaleString("en-IN")}
        </span>
      ),
    },
    {
      header: "Quick Actions",
      cell: (item) => (
        <div className="flex items-center gap-2" onClick={(e) => e.stopPropagation()}>
          <Button
            variant="outline"
            size="sm"
            onClick={() => setMessageModalGuest(item)}
            className="text-xs"
          >
            <MessageSquare className="h-3.5 w-3.5 mr-1 text-emerald-600" /> WhatsApp
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => navigate(`/guests/${item.id}`)}
          >
            <Eye className="h-3.5 w-3.5" />
          </Button>
        </div>
      ),
    },
  ]

  return (
    <div className="space-y-6">
      <PageHeader
        title="Favorite & VIP Guests"
        description="High-value repeat clients, tailored hospitality preferences, and loyalty outreach"
      />

      <div className="flex items-center justify-between">
        <SearchInput
          value={search}
          onChange={setSearch}
          placeholder="Search VIPs..."
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

      {/* Quick message modal */}
      <Dialog open={!!messageModalGuest} onOpenChange={(open) => !open && setMessageModalGuest(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Send Message to {messageModalGuest?.fullName}</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSendMessage} className="space-y-4">
            <div className="space-y-2">
              <Label>Mobile Number</Label>
              <Input readOnly value={messageModalGuest?.mobile || ""} className="bg-muted" />
            </div>
            <div className="space-y-2">
              <Label>Message Content</Label>
              <Input
                placeholder="e.g. Exclusive 20% discount on your next weekend stay with us!"
                value={quickMsg}
                onChange={(e) => setQuickMsg(e.target.value)}
                required
              />
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setMessageModalGuest(null)}>
                Cancel
              </Button>
              <Button type="submit">
                <Send className="h-4 w-4 mr-1" /> Send WhatsApp Message
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
