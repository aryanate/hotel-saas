import { useState } from "react"
import { useParams } from "react-router-dom"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { Star, FileText, Send, Plus, CheckCircle, Shield } from "lucide-react"
import { guestService } from "@/services/guest.service"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"
import { useToast } from "@/components/ui/use-toast"

export function GuestDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { toast } = useToast()
  const queryClient = useQueryClient()
  const [newNote, setNewNote] = useState("")

  const { data: res, isLoading } = useQuery({
    queryKey: ["guest", id],
    queryFn: () => guestService.getGuestById(id!),
  })

  const { data: historyRes } = useQuery({
    queryKey: ["guest-history", id],
    queryFn: () => guestService.getGuestHistory(id!),
  })

  const guest = res?.data || {
    id: id || "g1",
    fullName: "Vikram Malhotra",
    mobile: "9876543210",
    email: "vikram@example.com",
    address: "B-402, Highgrove Apartments, Pune, MH",
    totalVisits: 4,
    totalSpending: 42000,
    isFavorite: true,
    marketingConsent: true,
    preferences: "Requires high floor, extra mineral water, no feather pillows.",
    notes: [
      {
        id: "n1",
        noteText: "Guest celebrated anniversary during last stay. Prefers corner rooms.",
        createdAt: "2026-10-15T14:20:00Z",
      },
    ],
    documents: [
      {
        id: "d1",
        documentType: "MaskedAadhaar",
        documentNumberMasked: "XXXX-XXXX-8921",
        fileUrl: "/docs/aadhaar_masked.pdf",
        verifiedAt: "2026-10-15T14:00:00Z",
      },
    ],
  }

  const history = historyRes?.data || [
    {
      id: "b1",
      bookingReference: "BK-8021",
      checkInDate: "2026-10-15",
      checkOutDate: "2026-10-18",
      status: "CheckedOut",
      totalNights: 3,
    },
    {
      id: "b2",
      bookingReference: "BK-7120",
      checkInDate: "2026-08-01",
      checkOutDate: "2026-08-03",
      status: "CheckedOut",
      totalNights: 2,
    },
  ]

  const toggleFavoriteMutation = useMutation({
    mutationFn: () => guestService.toggleFavorite(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["guest", id] })
      toast({ title: "VIP / Favorite status updated" })
    },
  })

  const addNoteMutation = useMutation({
    mutationFn: (text: string) => guestService.addNote(id!, text),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["guest", id] })
      toast({ title: "Note added" })
      setNewNote("")
    },
  })

  const handleAddNote = (e: React.FormEvent) => {
    e.preventDefault()
    if (!newNote.trim()) return
    addNoteMutation.mutate(newNote)
  }

  if (isLoading) {
    return <LoadingSpinner size="lg" className="h-64" />
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={guest.fullName}
        description={`Registered Mobile: ${guest.mobile}`}
        breadcrumbs={[{ label: "Guests", href: "/guests" }, { label: guest.fullName }]}
      >
        <Button
          variant={guest.isFavorite ? "default" : "outline"}
          onClick={() => toggleFavoriteMutation.mutate()}
          className="gap-2"
        >
          <Star className={`h-4 w-4 ${guest.isFavorite ? "fill-amber-400 text-amber-400" : ""}`} />
          {guest.isFavorite ? "VIP Favorite" : "Mark as Favorite"}
        </Button>
      </PageHeader>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Profile and Preferences */}
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="text-base font-semibold">Guest Profile & Metrics</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 py-2 border-b">
                <div>
                  <p className="text-xs text-muted-foreground">Total Stays</p>
                  <p className="text-lg font-bold">{guest.totalVisits}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Lifetime Spend</p>
                  <p className="text-lg font-bold">₹{guest.totalSpending.toLocaleString("en-IN")}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Marketing Consent</p>
                  <p className="text-sm font-semibold text-emerald-600">
                    {guest.marketingConsent ? "Opted In" : "Opted Out"}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Email</p>
                  <p className="text-sm font-medium truncate">{guest.email || "N/A"}</p>
                </div>
              </div>

              <div>
                <p className="text-xs text-muted-foreground">Registered Address</p>
                <p className="text-sm mt-1">{guest.address || "Not specified"}</p>
              </div>

              {guest.preferences && (
                <div className="p-3 bg-muted rounded-md">
                  <p className="text-xs font-semibold text-muted-foreground">Special Preferences & VIP Tags</p>
                  <p className="text-sm mt-1">{guest.preferences}</p>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Past Bookings */}
          <Card>
            <CardHeader>
              <CardTitle className="text-base font-semibold">Stay History</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="divide-y">
                {history.map((b) => (
                  <div key={b.id} className="py-3 flex items-center justify-between text-sm">
                    <div>
                      <p className="font-semibold text-primary">Booking #{b.bookingReference}</p>
                      <p className="text-xs text-muted-foreground">
                        {b.checkInDate} to {b.checkOutDate} ({b.totalNights} nights)
                      </p>
                    </div>
                    <StatusBadge status={b.status} />
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Notes and Government IDs */}
        <div className="space-y-6">
          {/* Identity Documents */}
          <Card>
            <CardHeader>
              <CardTitle className="text-base font-semibold flex items-center gap-2">
                <Shield className="h-4 w-4 text-emerald-600" /> Identity Documents
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {guest.documents && guest.documents.length > 0 ? (
                guest.documents.map((doc) => (
                  <div key={doc.id} className="p-3 border rounded-lg space-y-1 text-sm bg-slate-50">
                    <div className="flex items-center justify-between">
                      <span className="font-semibold">{doc.documentType}</span>
                      <span className="text-xs text-emerald-600 flex items-center gap-1 font-medium">
                        <CheckCircle className="h-3 w-3" /> Verified
                      </span>
                    </div>
                    <p className="text-xs text-muted-foreground font-mono">{doc.documentNumberMasked}</p>
                  </div>
                ))
              ) : (
                <p className="text-xs text-muted-foreground">No documents uploaded yet.</p>
              )}
            </CardContent>
          </Card>

          {/* Internal Staff Notes */}
          <Card>
            <CardHeader>
              <CardTitle className="text-base font-semibold">Staff Notes</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <form onSubmit={handleAddNote} className="space-y-2">
                <Input
                  placeholder="Add a private note about guest..."
                  value={newNote}
                  onChange={(e) => setNewNote(e.target.value)}
                />
                <Button type="submit" size="sm" className="w-full" disabled={addNoteMutation.isPending}>
                  <Plus className="h-4 w-4 mr-1" /> Add Note
                </Button>
              </form>

              <div className="space-y-2 pt-2 border-t">
                {guest.notes?.map((n) => (
                  <div key={n.id} className="p-2.5 rounded bg-muted/60 text-xs space-y-1">
                    <p className="text-foreground">{n.noteText}</p>
                    <p className="text-muted-foreground text-[10px]">
                      {new Date(n.createdAt).toLocaleString()}
                    </p>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
