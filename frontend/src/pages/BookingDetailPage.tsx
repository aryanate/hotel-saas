import { useState } from "react"
import { useParams, useNavigate } from "react-router-dom"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import {
  CheckCircle2,
  LogOut,
  XCircle,
  FileText,
  User,
  Calendar,
  Bed,
  QrCode,
  Share2,
} from "lucide-react"
import { bookingService } from "@/services/booking.service"
import { Button } from "@/components/ui/button"
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"
import { ConfirmDialog } from "@/components/shared/ConfirmDialog"
import { useToast } from "@/components/ui/use-toast"

export function BookingDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { toast } = useToast()
  const queryClient = useQueryClient()

  const [confirmCancelOpen, setConfirmCancelOpen] = useState(false)
  const [confirmCheckOutOpen, setConfirmCheckOutOpen] = useState(false)

  const { data: res, isLoading } = useQuery({
    queryKey: ["booking", id],
    queryFn: () => bookingService.getBookingById(id!),
  })

  const booking = res?.data || {
    id: id || "b1",
    bookingReference: "BK-9021",
    status: "Confirmed",
    source: "Manual",
    checkInDate: "2026-11-15",
    checkOutDate: "2026-11-18",
    totalNights: 3,
    numberOfAdults: 2,
    numberOfChildren: 1,
    specialRequests: "Quiet room away from elevator, extra towels please.",
    guest: {
      id: "g1",
      fullName: "Vikram Malhotra",
      mobile: "9876543210",
      email: "vikram@example.com",
    },
    bookingRooms: [
      { id: "br1", roomId: "r1", room: { roomNumber: "204" }, ratePerNight: 3500 },
    ],
  }

  const confirmMutation = useMutation({
    mutationFn: () => bookingService.confirmBooking(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["booking", id] })
      toast({ title: "Booking confirmed" })
    },
  })

  const checkInMutation = useMutation({
    mutationFn: () => bookingService.checkIn(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["booking", id] })
      toast({ title: "Guest checked in successfully" })
    },
  })

  const checkOutMutation = useMutation({
    mutationFn: () => bookingService.checkOut(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["booking", id] })
      toast({ title: "Guest checked out" })
      navigate(`/billing/new?bookingId=${id}`)
    },
  })

  const cancelMutation = useMutation({
    mutationFn: () => bookingService.cancelBooking(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["booking", id] })
      toast({ title: "Booking cancelled" })
    },
  })

  const sendCheckInLinkMutation = useMutation({
    mutationFn: () => bookingService.sendCheckInLink(id!),
    onSuccess: () => {
      toast({ title: "Check-in link dispatched via WhatsApp/SMS" })
    },
  })

  if (isLoading) {
    return <LoadingSpinner size="lg" className="h-64" />
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={`Booking #${booking.bookingReference}`}
        breadcrumbs={[
          { label: "Bookings", href: "/bookings" },
          { label: booking.bookingReference },
        ]}
      >
        <div className="flex flex-wrap items-center gap-2">
          {booking.status === "Pending" && (
            <Button onClick={() => confirmMutation.mutate()} disabled={confirmMutation.isPending}>
              <CheckCircle2 className="h-4 w-4 mr-1" /> Confirm Booking
            </Button>
          )}

          {booking.status === "Confirmed" && (
            <Button onClick={() => checkInMutation.mutate()} disabled={checkInMutation.isPending}>
              <CheckCircle2 className="h-4 w-4 mr-1" /> Check In Guest
            </Button>
          )}

          {booking.status === "CheckedIn" && (
            <Button
              variant="default"
              onClick={() => setConfirmCheckOutOpen(true)}
              className="bg-amber-600 hover:bg-amber-700"
            >
              <LogOut className="h-4 w-4 mr-1" /> Check Out & Invoice
            </Button>
          )}

          <Button
            variant="outline"
            onClick={() => sendCheckInLinkMutation.mutate()}
            disabled={sendCheckInLinkMutation.isPending}
          >
            <Share2 className="h-4 w-4 mr-1" /> Send Mobile QR Link
          </Button>

          <Button
            variant="outline"
            onClick={() => navigate(`/billing/new?bookingId=${booking.id}`)}
          >
            <FileText className="h-4 w-4 mr-1" /> Generate Invoice
          </Button>

          {booking.status !== "Cancelled" && booking.status !== "CheckedOut" && (
            <Button
              variant="destructive"
              onClick={() => setConfirmCancelOpen(true)}
            >
              <XCircle className="h-4 w-4 mr-1" /> Cancel
            </Button>
          )}
        </div>
      </PageHeader>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Main Details */}
        <div className="md:col-span-2 space-y-6">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-base font-semibold">Reservation Information</CardTitle>
              <StatusBadge status={booking.status} />
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 sm:grid-cols-3 gap-4 py-2 border-b">
                <div>
                  <p className="text-xs text-muted-foreground">Check-in Date</p>
                  <p className="text-sm font-semibold">{booking.checkInDate}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Check-out Date</p>
                  <p className="text-sm font-semibold">{booking.checkOutDate}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Total Duration</p>
                  <p className="text-sm font-semibold">{booking.totalNights} Nights</p>
                </div>
              </div>

              <div className="grid grid-cols-2 sm:grid-cols-3 gap-4 py-2 border-b">
                <div>
                  <p className="text-xs text-muted-foreground">Adults</p>
                  <p className="text-sm font-medium">{booking.numberOfAdults}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Children</p>
                  <p className="text-sm font-medium">{booking.numberOfChildren}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Booking Source</p>
                  <p className="text-sm font-medium">{booking.source}</p>
                </div>
              </div>

              {booking.specialRequests && (
                <div className="pt-2">
                  <p className="text-xs text-muted-foreground font-medium">Guest Special Requests</p>
                  <p className="text-sm mt-1 p-3 bg-muted rounded-md text-foreground">
                    {booking.specialRequests}
                  </p>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Room Allocation */}
          <Card>
            <CardHeader>
              <CardTitle className="text-base font-semibold">Assigned Rooms</CardTitle>
            </CardHeader>
            <CardContent>
              {booking.bookingRooms && booking.bookingRooms.length > 0 ? (
                <div className="divide-y">
                  {booking.bookingRooms.map((br) => (
                    <div key={br.id} className="py-3 flex items-center justify-between text-sm">
                      <div className="flex items-center gap-3">
                        <div className="p-2 rounded bg-primary/10 text-primary">
                          <Bed className="h-5 w-5" />
                        </div>
                        <div>
                          <p className="font-bold">Room {br.room?.roomNumber || "Unassigned"}</p>
                          <p className="text-xs text-muted-foreground">Rate: ₹{br.ratePerNight}/night</p>
                        </div>
                      </div>
                      <span className="font-semibold">₹{br.ratePerNight * booking.totalNights}</span>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-sm text-muted-foreground">No rooms assigned yet.</p>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Guest profile sidebar */}
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="text-base font-semibold">Primary Guest Profile</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center gap-3">
                <div className="h-10 w-10 rounded-full bg-primary/10 text-primary flex items-center justify-center font-bold">
                  <User className="h-5 w-5" />
                </div>
                <div>
                  <h4 className="font-semibold text-sm">{booking.guest?.fullName}</h4>
                  <p className="text-xs text-muted-foreground">{booking.guest?.mobile}</p>
                </div>
              </div>
              <div className="text-xs space-y-1 pt-2 border-t text-muted-foreground">
                <p>Email: {booking.guest?.email || "Not provided"}</p>
              </div>
              <Button
                variant="outline"
                size="sm"
                className="w-full mt-2"
                onClick={() => navigate(`/guests/${booking.guest?.id}`)}
              >
                View Full Guest History
              </Button>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="text-base font-semibold">Digital Self Check-in</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <p className="text-xs text-muted-foreground">
                Guests can complete KYC and upload Gov IDs remotely prior to arrival.
              </p>
              <Button
                variant="secondary"
                size="sm"
                className="w-full"
                onClick={() => sendCheckInLinkMutation.mutate()}
              >
                <QrCode className="h-4 w-4 mr-2" /> Share Check-In Link
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>

      <ConfirmDialog
        open={confirmCancelOpen}
        onOpenChange={setConfirmCancelOpen}
        title="Cancel Booking"
        description="Are you sure you want to cancel this reservation? The assigned rooms will be released immediately."
        variant="destructive"
        confirmText="Cancel Booking"
        onConfirm={() => cancelMutation.mutate()}
        isLoading={cancelMutation.isPending}
      />

      <ConfirmDialog
        open={confirmCheckOutOpen}
        onOpenChange={setConfirmCheckOutOpen}
        title="Check-Out Guest"
        description="Proceed to mark guest as checked-out and review final invoice charges?"
        confirmText="Proceed to Invoicing"
        onConfirm={() => checkOutMutation.mutate()}
        isLoading={checkOutMutation.isPending}
      />
    </div>
  )
}
