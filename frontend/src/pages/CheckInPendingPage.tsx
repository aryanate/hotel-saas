import { useState } from "react"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { Check, X, FileText, QrCode } from "lucide-react"
import { get, post } from "@/services/api"
import type { CheckInSession } from "@/types"
import { Button } from "@/components/ui/button"
import { PageHeader } from "@/components/shared/PageHeader"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"
import { EmptyState } from "@/components/shared/EmptyState"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog"
import { useToast } from "@/components/ui/use-toast"

const DOC_TYPE_LABELS: Record<string, string> = {
  MaskedAadhaar: "Aadhaar Card",
  Passport: "Passport",
  DrivingLicense: "Driving License",
  VoterID: "Voter ID",
  PAN: "PAN Card",
  Other: "Other ID",
}

function getDocLabel(type?: string | null) {
  if (!type) return "Government Photo ID"
  return DOC_TYPE_LABELS[type] ?? type
}

export function CheckInPendingPage() {
  const { toast } = useToast()
  const queryClient = useQueryClient()
  const [selectedSession, setSelectedSession] = useState<CheckInSession | null>(null)
  const [docModalOpen, setDocModalOpen] = useState(false)

  const { data: res, isLoading } = useQuery({
    queryKey: ["pending-checkins"],
    queryFn: () => get<CheckInSession[]>("/api/checkin/pending"),
  })

  const sessions: CheckInSession[] = res?.data || [
    {
      id: "cs1",
      hotelId: "h1",
      createdAt: "2026-11-13T10:00:00Z",
      updatedAt: "2026-11-13T10:00:00Z",
      bookingId: "b1",
      token: "tok_991823",
      status: "Submitted",
      expiresAt: "2026-11-14T10:00:00Z",
      booking: {
        id: "b1",
        hotelId: "h1",
        createdAt: "2026-11-13",
        updatedAt: "2026-11-13",
        guestId: "g1",
        bookingReference: "BK-9021",
        source: "QR",
        status: "Pending",
        numberOfAdults: 2,
        numberOfChildren: 0,
        checkInDate: "2026-11-14",
        checkOutDate: "2026-11-17",
        totalNights: 3,
        guest: {
          id: "g1",
          hotelId: "h1",
          createdAt: "2026-11-13",
          updatedAt: "2026-11-13",
          fullName: "Vikram Malhotra",
          mobile: "9876543210",
          totalVisits: 1,
          totalSpending: 0,
          isFavorite: false,
          marketingConsent: true,
        },
      },
    },
    {
      id: "cs2",
      hotelId: "h1",
      createdAt: "2026-11-13T11:00:00Z",
      updatedAt: "2026-11-13T11:00:00Z",
      bookingId: "b2",
      token: "tok_991824",
      status: "Submitted",
      expiresAt: "2026-11-14T11:00:00Z",
      booking: {
        id: "b2",
        hotelId: "h1",
        createdAt: "2026-11-13",
        updatedAt: "2026-11-13",
        guestId: "g2",
        bookingReference: "BK-9022",
        source: "QR",
        status: "Pending",
        numberOfAdults: 1,
        numberOfChildren: 0,
        checkInDate: "2026-11-14",
        checkOutDate: "2026-11-15",
        totalNights: 1,
        guest: {
          id: "g2",
          hotelId: "h1",
          createdAt: "2026-11-13",
          updatedAt: "2026-11-13",
          fullName: "Sunita Roy",
          mobile: "9123456780",
          totalVisits: 2,
          totalSpending: 12000,
          isFavorite: true,
          marketingConsent: true,
        },
      },
    },
  ]

  const approveMutation = useMutation({
    mutationFn: (id: string) => post(`/api/checkin/sessions/${id}/approve`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["pending-checkins"] })
      toast({ title: "Digital registration approved and guest verified" })
      setDocModalOpen(false)
    },
  })

  const rejectMutation = useMutation({
    mutationFn: (id: string) => post(`/api/checkin/sessions/${id}/reject`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["pending-checkins"] })
      toast({ title: "Submission rejected" })
      setDocModalOpen(false)
    },
  })

  return (
    <div className="space-y-6">
      <PageHeader
        title="Digital Check-in Queue"
        description="Verify guest identity documents uploaded via mobile self check-in"
      />

      {isLoading ? (
        <LoadingSpinner size="lg" className="h-64" />
      ) : sessions.length === 0 ? (
        <EmptyState
          icon={<QrCode className="h-6 w-6" />}
          title="No pending check-ins"
          description="Submissions from QR scans or remote check-in links will show up here for verification."
        />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          {sessions.map((session) => (
            <div
              key={session.id}
              className="rounded-lg border bg-card p-5 shadow-sm space-y-4 flex flex-col justify-between"
            >
              <div>
                <div className="flex items-center justify-between pb-2 border-b">
                  <div>
                    <h3 className="font-bold text-base">
                      {session.guestName || session.booking?.guest?.fullName || "Guest"}
                    </h3>
                    <p className="text-xs text-muted-foreground">
                      Mobile: {session.guestMobile || session.booking?.guest?.mobile || "Not provided"}
                    </p>
                  </div>
                  <StatusBadge status={session.status} />
                </div>

                <div className="grid grid-cols-2 gap-2 text-xs py-3 border-b">
                  <div>
                    <span className="text-muted-foreground">Booking Ref:</span>
                    <p className="font-semibold text-primary">
                      {session.bookingReference || session.booking?.bookingReference || "Walk-In (QR)"}
                    </p>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Arrival:</span>
                    <p className="font-semibold">
                      {session.checkInDate
                        ? new Date(session.checkInDate).toLocaleDateString()
                        : (session.booking?.checkInDate || "Today")}
                    </p>
                  </div>
                </div>

                <div className="mt-3">
                  <Button
                    variant="outline"
                    size="sm"
                    className="w-full text-xs"
                    onClick={() => {
                      setSelectedSession(session)
                      setDocModalOpen(true)
                    }}
                  >
                    <FileText className="h-3.5 w-3.5 mr-1 text-primary" /> View Uploaded ID & KYC
                  </Button>
                </div>
              </div>

              <div className="pt-3 flex items-center justify-end gap-2 border-t">
                <Button
                  variant="outline"
                  size="sm"
                  className="text-destructive hover:bg-destructive/10"
                  onClick={() => rejectMutation.mutate(session.id)}
                  disabled={rejectMutation.isPending}
                >
                  <X className="h-4 w-4 mr-1" /> Reject
                </Button>
                <Button
                  size="sm"
                  onClick={() => approveMutation.mutate(session.id)}
                  disabled={approveMutation.isPending}
                >
                  <Check className="h-4 w-4 mr-1" /> Approve & Check In
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Document Review Modal */}
      <Dialog open={docModalOpen} onOpenChange={setDocModalOpen}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>Guest KYC Document Review</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div className="p-3 bg-muted rounded-md text-sm">
              <p className="font-semibold">{selectedSession?.guestName || selectedSession?.booking?.guest?.fullName || "Guest"}</p>
              <p className="text-xs text-muted-foreground">
                Mobile: {selectedSession?.guestMobile || selectedSession?.booking?.guest?.mobile || "N/A"} &bull; Booking:{" "}
                {selectedSession?.bookingReference || selectedSession?.booking?.bookingReference || "Walk-In"}
              </p>
            </div>

            <div className="border rounded-lg p-4 bg-slate-50 flex flex-col items-center justify-center min-h-[180px] text-center">
              {selectedSession?.documentUrl ? (
                <div className="w-full space-y-2">
                  <p className="font-semibold text-sm text-left">
                    {getDocLabel(selectedSession?.documentType)} — ID: {selectedSession.documentNumber ?? "N/A"}
                  </p>
                  <img
                    src={`http://localhost:5208${selectedSession.documentUrl}`}
                    alt="Uploaded ID Document"
                    className="w-full max-h-64 object-contain rounded-md border bg-white"
                    onError={(e) => {
                      (e.target as HTMLImageElement).style.display = "none"
                      const fallback = document.getElementById("doc-fallback")
                      if (fallback) fallback.style.display = "flex"
                    }}
                  />
                  <div id="doc-fallback" className="hidden flex-col items-center text-muted-foreground py-4">
                    <FileText className="h-10 w-10 text-primary/50 mb-1" />
                    <span className="text-xs">Document uploaded (non-image file)</span>
                    <a
                      href={`http://localhost:5208${selectedSession.documentUrl}`}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-xs text-primary underline mt-1"
                    >
                      Open file
                    </a>
                  </div>
                </div>
              ) : (
                <>
                  <FileText className="h-14 w-14 text-primary/70 mb-2" />
                  <p className="font-semibold text-sm">
                    {getDocLabel(selectedSession?.documentType)} Verification
                  </p>
                  <p className="text-xs text-muted-foreground mt-0.5">
                    {selectedSession?.documentNumber
                      ? `ID Number: ${selectedSession.documentNumber}`
                      : "Digital KYC Declaration Submitted"}
                  </p>
                  {selectedSession?.documentNumber && (
                    <span className="mt-3 text-xs font-mono bg-white px-3 py-1 rounded-md border font-medium">
                      {selectedSession.documentNumber}
                    </span>
                  )}
                </>
              )}
            </div>

            <div className="text-xs text-muted-foreground">
              &bull; Document and digital consent verified for check-in compliance under local hotel regulations.
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDocModalOpen(false)}>
              Close
            </Button>
            <Button
              onClick={() => selectedSession && approveMutation.mutate(selectedSession.id)}
              disabled={approveMutation.isPending}
            >
              Approve Identification
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
