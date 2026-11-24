import { useState } from "react"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { Plus, Layers } from "lucide-react"
import { roomService } from "@/services/room.service"
import type { RoomType } from "@/types"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { PageHeader } from "@/components/shared/PageHeader"
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

export function RoomTypesPage() {
  const { toast } = useToast()
  const queryClient = useQueryClient()
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [name, setName] = useState("")
  const [description, setDescription] = useState("")
  const [basePrice, setBasePrice] = useState(2500)
  const [capacityAdults, setCapacityAdults] = useState(2)
  const [capacityChildren, setCapacityChildren] = useState(1)

  const { data: res, isLoading } = useQuery({
    queryKey: ["room-types"],
    queryFn: () => roomService.getRoomTypes(),
  })

  const roomTypes = res?.data || []

  const createMutation = useMutation({
    mutationFn: () =>
      roomService.createRoomType({
        name,
        description,
        basePrice,
        capacityAdults,
        capacityChildren,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["room-types"] })
      toast({ title: "Room type created successfully" })
      setIsModalOpen(false)
      setName("")
      setDescription("")
    },
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!name) return
    createMutation.mutate()
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Room Categories & Types"
        description="Configure pricing, occupancy limits, and luxury amenities for your rooms"
        breadcrumbs={[{ label: "Rooms", href: "/rooms" }, { label: "Room Types" }]}
      >
        <Button onClick={() => setIsModalOpen(true)}>
          <Plus className="h-4 w-4 mr-1" /> Add Room Type
        </Button>
      </PageHeader>

      {isLoading ? (
        <LoadingSpinner size="lg" className="h-64" />
      ) : roomTypes.length === 0 ? (
        <EmptyState
          icon={<Layers className="h-6 w-6" />}
          title="No room types configured"
          description="Define standard, deluxe, or suite tiers to set room rates and capacities."
          actionLabel="Create Room Type"
          onAction={() => setIsModalOpen(true)}
        />
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {roomTypes.map((type: RoomType) => (
            <div key={type.id} className="rounded-lg border bg-card p-6 shadow-sm space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="font-bold text-lg">{type.name}</h3>
                <span className="text-lg font-extrabold text-primary">₹{type.basePrice}<span className="text-xs font-normal text-muted-foreground">/night</span></span>
              </div>
              <p className="text-sm text-muted-foreground">{type.description || "No description provided."}</p>
              <div className="pt-2 border-t text-xs text-muted-foreground flex items-center justify-between">
                <span>Max Adults: {type.capacityAdults}</span>
                <span>Max Children: {type.capacityChildren}</span>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Add Dialog */}
      <Dialog open={isModalOpen} onOpenChange={setIsModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add Room Category</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Category Name *</Label>
              <Input
                id="name"
                placeholder="e.g. Deluxe Suite, Executive King"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="desc">Description</Label>
              <Input
                id="desc"
                placeholder="e.g. Spacious with city view, king bed"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />
            </div>
            <div className="grid grid-cols-3 gap-3">
              <div className="space-y-2">
                <Label htmlFor="price">Base Rate (₹)</Label>
                <Input
                  id="price"
                  type="number"
                  value={basePrice}
                  onChange={(e) => setBasePrice(Number(e.target.value))}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="adults">Adults</Label>
                <Input
                  id="adults"
                  type="number"
                  value={capacityAdults}
                  onChange={(e) => setCapacityAdults(Number(e.target.value))}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="children">Children</Label>
                <Input
                  id="children"
                  type="number"
                  value={capacityChildren}
                  onChange={(e) => setCapacityChildren(Number(e.target.value))}
                />
              </div>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setIsModalOpen(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={createMutation.isPending}>
                {createMutation.isPending ? "Saving..." : "Create Category"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
