import { useState, useEffect } from "react"
import { useParams, useNavigate } from "react-router-dom"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { roomService } from "@/services/room.service"
import type { RoomStatus } from "@/types"
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
  const [floor, setFloor] = useState(1)
  const [roomTypeId, setRoomTypeId] = useState("")
  const [capacity, setCapacity] = useState(2)
  const [pricePerNight, setPricePerNight] = useState(1999)
  const [description, setDescription] = useState("")
  const [status, setStatus] = useState<RoomStatus>("Available")

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
      setFloor(currentRoom.floor ?? 1)
      setRoomTypeId(currentRoom.roomTypeId)
      setCapacity(currentRoom.capacity ?? 2)
      setPricePerNight(currentRoom.pricePerNight ?? 1999)
      setDescription(currentRoom.description || "")
      setStatus(currentRoom.status)
    }
  }, [currentRoom])

  const updateMutation = useMutation({
    mutationFn: () =>
      roomService.updateRoom(id!, {
        roomNumber,
        floor: Number(floor),
        roomTypeId,
        capacity: Number(capacity),
        pricePerNight: Number(pricePerNight),
        description,
        status,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["rooms"] })
      toast({ title: "Room updated successfully" })
      navigate("/rooms")
    },
    onError: (err: unknown) => {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
      toast({ title: msg || "Failed to update room", variant: "destructive" })
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
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="roomNumber">Room Number</Label>
              <Input
                id="roomNumber"
                value={roomNumber}
                onChange={(e) => setRoomNumber(e.target.value)}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="floor">Floor</Label>
              <Input
                id="floor"
                type="number"
                value={floor}
                onChange={(e) => setFloor(Number(e.target.value))}
              />
            </div>
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

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="capacity">Capacity</Label>
              <Input
                id="capacity"
                type="number"
                min="1"
                value={capacity}
                onChange={(e) => setCapacity(Number(e.target.value))}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="pricePerNight">Nightly Rate (₹)</Label>
              <Input
                id="pricePerNight"
                type="number"
                min="0"
                value={pricePerNight}
                onChange={(e) => setPricePerNight(Number(e.target.value))}
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label>Current Status</Label>
            <Select value={status} onValueChange={(v) => setStatus(v as RoomStatus)}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Available">Available</SelectItem>
                <SelectItem value="Occupied">Occupied</SelectItem>
                <SelectItem value="Reserved">Reserved</SelectItem>
                <SelectItem value="Cleaning">Cleaning</SelectItem>
                <SelectItem value="Maintenance">Maintenance</SelectItem>
                <SelectItem value="OutOfOrder">Out of Order</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label htmlFor="description">Description / Housekeeping Notes</Label>
            <Input
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
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
