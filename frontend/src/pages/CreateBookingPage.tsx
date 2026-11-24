import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { useQuery, useMutation } from "@tanstack/react-query"
import { bookingService } from "@/services/booking.service"
import { roomService } from "@/services/room.service"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardHeader, CardTitle, CardContent, CardFooter } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { useToast } from "@/components/ui/use-toast"

const bookingSchema = z.object({
  guestName: z.string().min(2, "Guest name is required"),
  mobile: z.string().min(10, "Valid mobile number is required"),
  email: z.string().email().optional().or(z.literal("")),
  checkInDate: z.string().min(1, "Check-in date required"),
  checkOutDate: z.string().min(1, "Check-out date required"),
  numberOfAdults: z.number().min(1),
  numberOfChildren: z.number().min(0),
  specialRequests: z.string().optional(),
})

type BookingFormValues = z.infer<typeof bookingSchema>

export function CreateBookingPage() {
  const navigate = useNavigate()
  const { toast } = useToast()
  const [selectedRoomIds, setSelectedRoomIds] = useState<string[]>([])

  const today = new Date().toISOString().split("T")[0]
  const tomorrow = new Date(Date.now() + 86400000).toISOString().split("T")[0]

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<BookingFormValues>({
    resolver: zodResolver(bookingSchema),
    defaultValues: {
      checkInDate: today,
      checkOutDate: tomorrow,
      numberOfAdults: 2,
      numberOfChildren: 0,
    },
  })

  const { data: roomsRes } = useQuery({
    queryKey: ["rooms"],
    queryFn: () => roomService.getRooms(),
  })

  const rooms = roomsRes?.data || []
  const availableRooms = rooms.filter((r) => r.status === "Available")

  const createMutation = useMutation({
    mutationFn: (values: BookingFormValues) =>
      bookingService.createBooking({
        guestName: values.guestName,
        guestMobile: values.mobile,
        guestEmail: values.email || undefined,
        checkInDate: values.checkInDate,
        checkOutDate: values.checkOutDate,
        numberOfAdults: values.numberOfAdults,
        numberOfChildren: values.numberOfChildren,
        specialRequests: values.specialRequests,
        roomIds: selectedRoomIds,
        source: "Manual",
      }),
    onSuccess: (res) => {
      toast({ title: "Booking created successfully" })
      if (res.data?.id) {
        navigate(`/bookings/${res.data.id}`)
      } else {
        navigate("/bookings")
      }
    },
    onError: (err: unknown) => {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
      toast({ title: msg || "Failed to create booking", variant: "destructive" })
    },
  })

  const onSubmit = (values: BookingFormValues) => {
    if (selectedRoomIds.length === 0) {
      toast({ title: "Please select at least one room", variant: "destructive" })
      return
    }
    createMutation.mutate(values)
  }

  const toggleRoom = (id: string) => {
    setSelectedRoomIds((prev) =>
      prev.includes(id) ? prev.filter((r) => r !== id) : [...prev, id]
    )
  }

  return (
    <div className="space-y-6 max-w-3xl mx-auto">
      <PageHeader
        title="Create New Reservation"
        description="Manually register an incoming guest or walk-in customer"
        breadcrumbs={[{ label: "Bookings", href: "/bookings" }, { label: "New Booking" }]}
      />

      <form onSubmit={handleSubmit(onSubmit)}>
        <Card className="shadow-sm">
          <CardHeader>
            <CardTitle>Guest & Stay Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="guestName">Guest Full Name *</Label>
                <Input id="guestName" placeholder="e.g. Ramesh Patel" {...register("guestName")} />
                {errors.guestName && (
                  <p className="text-xs text-destructive">{errors.guestName.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="mobile">Mobile Number *</Label>
                <Input id="mobile" placeholder="9876543210" {...register("mobile")} />
                {errors.mobile && (
                  <p className="text-xs text-destructive">{errors.mobile.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="email">Email Address</Label>
                <Input id="email" type="email" placeholder="ramesh@example.com" {...register("email")} />
              </div>

              <div className="space-y-2">
                <Label htmlFor="specialRequests">Special Requests</Label>
                <Input
                  id="specialRequests"
                  placeholder="e.g. Early check-in, ground floor"
                  {...register("specialRequests")}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="checkInDate">Check-In Date *</Label>
                <Input id="checkInDate" type="date" {...register("checkInDate")} />
                {errors.checkInDate && (
                  <p className="text-xs text-destructive">{errors.checkInDate.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="checkOutDate">Check-Out Date *</Label>
                <Input id="checkOutDate" type="date" {...register("checkOutDate")} />
                {errors.checkOutDate && (
                  <p className="text-xs text-destructive">{errors.checkOutDate.message}</p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="numberOfAdults">Adults</Label>
                <Input
                  id="numberOfAdults"
                  type="number"
                  min="1"
                  {...register("numberOfAdults", { valueAsNumber: true })}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="numberOfChildren">Children</Label>
                <Input
                  id="numberOfChildren"
                  type="number"
                  min="0"
                  {...register("numberOfChildren", { valueAsNumber: true })}
                />
              </div>
            </div>

            {/* Room selection */}
            <div className="space-y-3 pt-4 border-t">
              <Label className="text-base font-semibold">Assign Available Rooms *</Label>
              {availableRooms.length === 0 ? (
                <p className="text-sm text-amber-600">
                  No rooms currently marked 'Available'. You can assign a room later or update room statuses.
                </p>
              ) : (
                <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
                  {availableRooms.map((r) => {
                    const isSelected = selectedRoomIds.includes(r.id)
                    return (
                      <button
                        type="button"
                        key={r.id}
                        onClick={() => toggleRoom(r.id)}
                        className={`p-3 rounded-md border text-left transition-colors ${
                          isSelected
                            ? "border-primary bg-primary/10 text-primary font-bold"
                            : "border-border hover:bg-muted"
                        }`}
                      >
                        <p className="text-sm font-semibold">Room {r.roomNumber}</p>
                        <p className="text-xs text-muted-foreground">{r.roomTypeName || r.roomType?.name || "Standard"}</p>
                        <p className="text-xs font-medium text-primary mt-0.5">₹{(r.pricePerNight ?? 0).toLocaleString("en-IN")}/night</p>
                      </button>
                    )
                  })}
                </div>
              )}
            </div>
          </CardContent>
          <CardFooter className="flex justify-between">
            <Button type="button" variant="outline" onClick={() => navigate("/bookings")}>
              Cancel
            </Button>
            <Button type="submit" disabled={createMutation.isPending}>
              {createMutation.isPending ? "Creating Booking..." : "Create Reservation"}
            </Button>
          </CardFooter>
        </Card>
      </form>
    </div>
  )
}
