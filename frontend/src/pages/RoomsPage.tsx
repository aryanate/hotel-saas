import { useState } from "react"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { Plus, Filter, BedDouble, Edit3, Trash2 } from "lucide-react"
import { useNavigate } from "react-router-dom"
import { roomService } from "@/services/room.service"
import type { Room, RoomStatus } from "@/types"
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
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select"
import { useToast } from "@/components/ui/use-toast"

export function RoomsPage() {
  const navigate = useNavigate()
  const { toast } = useToast()
  const queryClient = useQueryClient()
  const [statusFilter, setStatusFilter] = useState<string>("all")
  const [isAddModalOpen, setIsAddModalOpen] = useState(false)
  const [newRoomNumber, setNewRoomNumber] = useState("")
  const [newRoomFloor, setNewRoomFloor] = useState("")
  const [newRoomTypeId, setNewRoomTypeId] = useState("")

  const { data: roomsRes, isLoading } = useQuery({
    queryKey: ["rooms", statusFilter],
    queryFn: () =>
      roomService.getRooms(
        statusFilter === "all" ? undefined : { status: statusFilter as RoomStatus }
      ),
  })

  const { data: roomTypesRes } = useQuery({
    queryKey: ["room-types"],
    queryFn: () => roomService.getRoomTypes(),
  })

  const rooms = roomsRes?.data || []
  const roomTypes = roomTypesRes?.data || []

  const createRoomMutation = useMutation({
    mutationFn: (payload: { roomNumber: string; roomTypeId: string; floor?: string }) =>
      roomService.createRoom(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["rooms"] })
      toast({ title: "Room added successfully" })
      setIsAddModalOpen(false)
      setNewRoomNumber("")
      setNewRoomFloor("")
    },
    onError: () => {
      toast({ title: "Failed to add room", variant: "destructive" })
    },
  })

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: RoomStatus }) =>
      roomService.updateRoomStatus(id, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["rooms"] })
      toast({ title: "Room status updated" })
    },
  })

  const deleteRoomMutation = useMutation({
    mutationFn: (id: string) => roomService.deleteRoom(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["rooms"] })
      toast({ title: "Room deleted" })
    },
  })

  const handleAddRoom = (e: React.FormEvent) => {
    e.preventDefault()
    if (!newRoomNumber || !newRoomTypeId) {
      toast({ title: "Please fill required fields", variant: "destructive" })
      return
    }
    createRoomMutation.mutate({
      roomNumber: newRoomNumber,
      roomTypeId: newRoomTypeId,
      floor: newRoomFloor,
    })
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Rooms Inventory"
        description="Monitor room occupancy, assign cleaning, and configure rooms"
      >
        <Button variant="outline" onClick={() => navigate("/rooms/types")}>
          Manage Room Types
        </Button>
        <Button onClick={() => setIsAddModalOpen(true)}>
          <Plus className="h-4 w-4 mr-1" /> Add Room
        </Button>
      </PageHeader>

      {/* Filter bar */}
      <div className="flex flex-wrap items-center gap-2 pb-2">
        <span className="text-sm text-muted-foreground flex items-center gap-1 mr-2">
          <Filter className="h-4 w-4" /> Filter Status:
        </span>
        {["all", "Available", "Occupied", "Reserved", "Cleaning", "Maintenance"].map((status) => (
          <Button
            key={status}
            variant={statusFilter === status ? "default" : "outline"}
            size="sm"
            onClick={() => setStatusFilter(status)}
          >
            {status}
          </Button>
        ))}
      </div>

      {isLoading ? (
        <LoadingSpinner size="lg" className="h-64" />
      ) : rooms.length === 0 ? (
        <EmptyState
          icon={<BedDouble className="h-6 w-6" />}
          title="No rooms found"
          description="Create rooms to start receiving guest bookings and tracking occupancy."
          actionLabel="Add First Room"
          onAction={() => setIsAddModalOpen(true)}
        />
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
          {rooms.map((room: Room) => (
            <div
              key={room.id}
              className="rounded-lg border bg-card p-4 shadow-sm flex flex-col justify-between hover:border-primary/50 transition-colors"
            >
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <span className="text-xl font-bold tracking-tight">Room {room.roomNumber}</span>
                  <StatusBadge status={room.status} />
                </div>
                <div className="text-xs text-muted-foreground">
                  <p>Type: {room.roomType?.name || "Standard"}</p>
                  <p>Floor: {room.floor || "1st Floor"}</p>
                </div>
              </div>

              <div className="pt-4 border-t mt-4 flex items-center justify-between">
                <Select
                  value={room.status}
                  onValueChange={(val) =>
                    updateStatusMutation.mutate({ id: room.id, status: val as RoomStatus })
                  }
                >
                  <SelectTrigger className="h-8 text-xs w-32">
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

                <div className="flex items-center gap-1">
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-8 w-8"
                    onClick={() => navigate(`/rooms/${room.id}`)}
                  >
                    <Edit3 className="h-3.5 w-3.5" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-8 w-8 text-destructive"
                    onClick={() => {
                      if (confirm("Delete this room?")) {
                        deleteRoomMutation.mutate(room.id)
                      }
                    }}
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                  </Button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Add Room Dialog */}
      <Dialog open={isAddModalOpen} onOpenChange={setIsAddModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add New Room</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleAddRoom} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="roomNumber">Room Number / Identifier *</Label>
              <Input
                id="roomNumber"
                placeholder="e.g. 101, 204B"
                value={newRoomNumber}
                onChange={(e) => setNewRoomNumber(e.target.value)}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="floor">Floor / Wing</Label>
              <Input
                id="floor"
                placeholder="e.g. 1st Floor, East Wing"
                value={newRoomFloor}
                onChange={(e) => setNewRoomFloor(e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label>Room Category / Type *</Label>
              <Select value={newRoomTypeId} onValueChange={setNewRoomTypeId}>
                <SelectTrigger>
                  <SelectValue placeholder="Select room type" />
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
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setIsAddModalOpen(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={createRoomMutation.isPending}>
                {createRoomMutation.isPending ? "Creating..." : "Save Room"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
