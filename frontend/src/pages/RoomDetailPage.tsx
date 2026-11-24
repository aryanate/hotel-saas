import { useState, useEffect } from "react"
import { useParams, useNavigate } from "react-router-dom"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { roomService } from "@/services/room.service"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardHeader, CardTitle, CardContent, CardFooter } from "@/components/ui/card"
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select"
import { PageHeader } from "@/components/shared/PageHeader"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"
import { useToast } from "@/components/ui/use-toast"

export function RoomDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { toast } = useToast()
  const queryClient = useQueryClient()

  const [roomNumber, setRoomNumber] = useState("")
  const [floor, setFloor] = useState("")
  const [roomTypeId, setRoomTypeId] = useState("")
  const [notes, setNotes] = useState("")

  const { data: roomsRes, isLoading: loadingRooms } = useQuery({
    queryKey: ["rooms"],
    queryFn: () => roomService.getRooms(),
  })

  const { data: roomTypesRes } = useQuery({
    queryKey: ["room-types"],
    queryFn: () => roomService.getRoomTypes(),
  })

  const currentRoom = roomsRes?.data?.find((r) => r.id === id)
  const roomTypes = roomTypesRes?.data || []

  useEffect(() => {
    if (currentRoom) {
      setRoomNumber(currentRoom.roomNumber)
      setFloor(currentRoom.floor || "")
      setRoomTypeId(currentRoom.roomTypeId)
      setNotes(currentRoom.notes || "")
    }
  }, [currentRoom])

  const updateMutation = useMutation({
    mutationFn: () =>
      roomService.updateRoom(id!, {
        roomNumber,
        floor,
        roomTypeId,
        notes,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["rooms"] })
      toast({ title: "Room updated successfully" })
      navigate("/rooms")
    },
    onError: () => {
      toast({ title: "Failed to update room", variant: "destructive" })
    },
  })

  if (loadingRooms) {
    return <LoadingSpinner size="lg" className="h-64" />
  }

  if (!currentRoom) {
    return <div>Room not found.</div>
  }

  return (
    <div className="space-y-6 max-w-xl mx-auto">
      <PageHeader
        title={`Edit Room ${currentRoom.roomNumber}`}
        breadcrumbs={[
          { label: "Rooms", href: "/rooms" },
          { label: `Room ${currentRoom.roomNumber}` },
        ]}
      />

      <Card>
        <CardHeader>
          <CardTitle>Room Specifications</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="roomNumber">Room Number</Label>
            <Input
              id="roomNumber"
              value={roomNumber}
              onChange={(e) => setRoomNumber(e.target.value)}
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="floor">Floor / Wing</Label>
            <Input
              id="floor"
              value={floor}
              onChange={(e) => setFloor(e.target.value)}
            />
          </div>

          <div className="space-y-2">
            <Label>Room Type</Label>
            <Select value={roomTypeId} onValueChange={setRoomTypeId}>
              <SelectTrigger>
                <SelectValue placeholder="Select type" />
              </SelectTrigger>
              <SelectContent>
                {roomTypes.map((t) => (
                  <SelectItem key={t.id} value={t.id}>
                    {t.name} (₹{t.basePrice}/night)
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label htmlFor="notes">Housekeeping Notes</Label>
            <Input
              id="notes"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="e.g. Balcony attached, King Bed"
            />
          </div>
        </CardContent>
        <CardFooter className="flex justify-between">
          <Button variant="outline" onClick={() => navigate("/rooms")}>
            Cancel
          </Button>
          <Button onClick={() => updateMutation.mutate()} disabled={updateMutation.isPending}>
            {updateMutation.isPending ? "Saving..." : "Save Changes"}
          </Button>
        </CardFooter>
      </Card>
    </div>
  )
}
