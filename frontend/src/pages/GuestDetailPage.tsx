import { useState } from "react"
import { useParams, useNavigate } from "react-router-dom"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { Star, Plus, CheckCircle, Shield, ExternalLink } from "lucide-react"
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
  const navigate = useNavigate()
  const { toast } = useToast()
  const queryClient = useQueryClient()
  const [newNote, setNewNote] = useState("")

  const { data: res, isLoading } = useQuery({
    queryKey: ["guest", id],
    queryFn: () => guestService.getGuestById(id!),
    enabled: !!id,
  })

  const { data: historyRes } = useQuery({
    queryKey: ["guest-history", id],
    queryFn: () => guestService.getGuestHistory(id!),
    enabled: !!id,
  })

  const guest = res?.data
  const history = historyRes?.data || []

  const toggleFavoriteMutation = useMutation({
    mutationFn: () => guestService.toggleFavorite(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["guest", id] })
      queryClient.invalidateQueries({ queryKey: ["guests"] })
      queryClient.invalidateQueries({ queryKey: ["favorite-guests"] })
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

  if (!guest) {
    return (
      <div className="text-center py-16 space-y-4">
        <p className="text-lg font-semibold">Guest profile not found</p>
        <Button variant="outline" onClick={() => navigate("/guests")}>
          Back to Guest Directory
        </Button>
      </div>
    )
  }

  const formatDate = (d?: string) => (d ? new Date(d).toLocaleDateString("en-IN", { day: "2-digit", month: "short", year: "numeric" }) : "-")

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
              {history.length === 0 ? (
                <p className="text-xs text-muted-foreground py-3">No stays recorded yet.</p>
              ) : (
                <div className="divide-y">
                  {history.map((b) => (
                    <div
                      key={b.id}
                      onClick={() => navigate(`/bookings/${b.id}`)}
                      className="py-3 flex items-center justify-between text-sm cursor-pointer hover:bg-muted/40 px-2 rounded"
                    >
                      <div>
                        <p className="font-semibold text-primary">Booking #{b.bookingReference}</p>
                        <p className="text-xs text-muted-foreground">
                          {formatDate(b.checkInDate)} to {formatDate(b.checkOutDate)} ({Math.max(1, b.totalNights)} nights)
                        </p>
                      </div>
                      <StatusBadge status={b.status} />
                    </div>
                  ))}
                </div>
              )}
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
                  <div key={doc.id} className="p-3 border rounded-lg space-y-2 text-sm bg-slate-50">
                    <div className="flex items-center justify-between">
                      <span className="font-semibold">
                        {doc.documentType === "MaskedAadhaar" ? "Aadhaar Card" : doc.documentType}
                      </span>
                      <span className="text-xs text-emerald-600 flex items-center gap-1 font-medium">
                        <CheckCircle className="h-3 w-3" /> {doc.isVerified ? "Verified" : "Pending"}
                      </span>
                    </div>
                    <p className="text-xs text-muted-foreground font-mono">
                      {doc.documentNumber || "Number on file"}
                    </p>
                    {doc.storageUrl && (
                      <a
                        href={doc.storageUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="inline-flex items-center gap-1 text-xs text-primary font-medium hover:underline"
                      >
                        <ExternalLink className="h-3 w-3" /> View Uploaded Document
                      </a>
                    )}
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
                    <p className="text-foreground">{n.note || n.noteText}</p>
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
