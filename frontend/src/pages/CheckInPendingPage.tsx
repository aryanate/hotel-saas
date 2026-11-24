import { useState } from "react"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { Check, X, FileText, QrCode } from "lucide-react"
import { get, post } from "@/services/api"
import { roomService } from "@/services/room.service"
import type { CheckInSession } from "@/types"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
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
  const [selectedRoomId, setSelectedRoomId] = useState<string>("")

  const { data: res, isLoading } = useQuery({
    queryKey: ["pending-checkins"],
    queryFn: () => get<CheckInSession[]>("/api/checkin/pending"),
  })

  const { data: roomsRes } = useQuery({
    queryKey: ["rooms", "Available"],
    queryFn: () => roomService.getRooms({ status: "Available" }),
  })

  const sessions: CheckInSession[] = Array.isArray(res?.data) ? res.data : []
  const availableRooms = roomsRes?.data || []

  const approveMutation = useMutation({
    mutationFn: ({ id, roomId }: { id: string; roomId?: string }) =>
      post(`/api/checkin/sessions/${id}/approve`, roomId ? { roomId } : {}),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["pending-checkins"] })
      queryClient.invalidateQueries({ queryKey: ["rooms"] })
      queryClient.invalidateQueries({ queryKey: ["bookings"] })
      queryClient.invalidateQueries({ queryKey: ["dashboard-stats"] })
      toast({ title: "Digital registration approved and guest checked in" })
      setDocModalOpen(false)
      setSelectedRoomId("")
    },
    onError: () => {
      toast({ title: "Failed to approve check-in", variant: "destructive" })
    },
  })

  const rejectMutation = useMutation({
    mutationFn: (id: string) => post(`/api/checkin/sessions/${id}/reject`, {}),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["pending-checkins"] })
      toast({ title: "Submission rejected" })
      setDocModalOpen(false)
    },
    onError: () => {
      toast({ title: "Failed to reject submission", variant: "destructive" })
    },
  })

  const openReviewModal = (session: CheckInSession) => {
    setSelectedSession(session)
    const needsRoom = !session.roomNumbers || session.roomNumbers.length === 0
    if (needsRoom && availableRooms.length > 0) {
      setSelectedRoomId(availableRooms[0].id)
    } else {
      setSelectedRoomId("")
    }
    setDocModalOpen(true)
  }

  const handleCardApprove = (session: CheckInSession) => {
    const hasAssignedRoom = session.roomNumbers && session.roomNumbers.length > 0
    if (hasAssignedRoom) {
      approveMutation.mutate({ id: session.id })
    } else {
      openReviewModal(session)
    }
  }

  const handleModalApprove = () => {
    if (!selectedSession) return
    const hasAssignedRoom = selectedSession.roomNumbers && selectedSession.roomNumbers.length > 0
    if (!hasAssignedRoom && availableRooms.length > 0 && !selectedRoomId) {
      toast({ title: "Please select a room to assign before approving", variant: "destructive" })
      return
    }
    approveMutation.mutate({
      id: selectedSession.id,
      roomId: selectedRoomId || undefined,
    })
  }

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
                    <span className="text-muted-foreground">Room:</span>
                    <p className="font-semibold">
                      {session.roomNumbers && session.roomNumbers.length > 0
                        ? `Room ${session.roomNumbers.join(", ")}`
                        : "Needs Assignment"}
                    </p>
                  </div>
                </div>

                <div className="mt-3">
                  <Button
                    variant="outline"
                    size="sm"
                    className="w-full text-xs"
                    onClick={() => openReviewModal(session)}
                  >
                    <FileText className="h-3.5 w-3.5 mr-1 text-primary" /> View Uploaded ID &amp; Assign Room
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
                  onClick={() => handleCardApprove(session)}
                  disabled={approveMutation.isPending}
                >
                  <Check className="h-4 w-4 mr-1" />{" "}
                  {session.roomNumbers && session.roomNumbers.length > 0
                    ? "Approve & Check In"
                    : "Assign Room & Approve"}
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
              {(() => {
                const urls =
                  selectedSession?.documentUrls && selectedSession.documentUrls.length > 0
                    ? selectedSession.documentUrls
                    : selectedSession?.documentUrl
                      ? [selectedSession.documentUrl]
                      : []

                if (urls.length === 0) {
                  return (
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
                  )
                }

                return (
                  <div className="w-full space-y-3">
                    <div className="flex items-center justify-between">
                      <p className="font-semibold text-sm text-left">
                        {getDocLabel(selectedSession?.documentType)} — ID: {selectedSession?.documentNumber ?? "N/A"}
                      </p>
                      <span className="text-[11px] text-muted-foreground">
                        {urls.length} {urls.length === 1 ? "file" : "files"} attached
                      </span>
                    </div>
                    <div className={urls.length > 1 ? "grid grid-cols-1 sm:grid-cols-2 gap-3" : "space-y-2"}>
                      {urls.map((url, idx) => {
                        const isPdf = url.toLowerCase().endsWith(".pdf")
                        return isPdf ? (
                          <div
                            key={url}
                            className="flex flex-col items-center justify-center p-4 rounded-md border bg-white text-muted-foreground min-h-[140px]"
                          >
                            <FileText className="h-10 w-10 text-primary/60 mb-1.5" />
                            <span className="text-xs font-medium text-foreground">
                              PDF Document #{idx + 1}
                            </span>
                            <a
                              href={url}
                              target="_blank"
                              rel="noopener noreferrer"
                              className="text-xs text-primary underline mt-1 font-medium"
                            >
                              Open / View PDF
                            </a>
                          </div>
                        ) : (
                          <a
                            key={url}
                            href={url}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="block rounded-md border bg-white overflow-hidden hover:ring-2 hover:ring-primary/40 transition"
                            title="Click to open full size"
                          >
                            <img
                              src={url}
                              alt={`Uploaded ID Document ${idx + 1}`}
                              className="w-full max-h-56 object-contain bg-white"
                            />
                          </a>
                        )
                      })}
                    </div>
                  </div>
                )
              })()}
            </div>

            {selectedSession?.roomNumbers && selectedSession.roomNumbers.length > 0 ? (
              <div className="p-2.5 rounded-md bg-emerald-50 border border-emerald-200 text-xs text-emerald-800">
                Pre-assigned Room: <strong>Room {selectedSession.roomNumbers.join(", ")}</strong>
              </div>
            ) : availableRooms.length > 0 ? (
              <div className="space-y-1.5">
                <Label htmlFor="assignRoom" className="text-xs font-semibold">
                  Assign Room *
                </Label>
                <select
                  id="assignRoom"
                  value={selectedRoomId}
                  onChange={(e) => setSelectedRoomId(e.target.value)}
                  className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-xs"
                >
                  <option value="">-- Select Available Room --</option>
                  {availableRooms.map((r) => (
                    <option key={r.id} value={r.id}>
                      Room {r.roomNumber} — {r.roomTypeName || "Standard"} (₹{r.pricePerNight}/night)
                    </option>
                  ))}
                </select>
              </div>
            ) : (
              <div className="p-2.5 rounded-md bg-amber-50 border border-amber-200 text-xs text-amber-800">
                No rooms currently marked Available — guest will be checked in without room assignment.
              </div>
            )}

            <div className="text-xs text-muted-foreground">
              &bull; Document and digital consent verified for check-in compliance under local hotel regulations.
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDocModalOpen(false)}>
              Close
            </Button>
            <Button
              onClick={handleModalApprove}
              disabled={approveMutation.isPending}
            >
              {approveMutation.isPending
                ? "Approving..."
                : selectedSession?.roomNumbers && selectedSession.roomNumbers.length > 0
                  ? "Approve & Check In"
                  : "Approve & Assign Room"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
