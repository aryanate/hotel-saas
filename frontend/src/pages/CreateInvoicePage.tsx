import { useState } from "react"
import { useNavigate, useSearchParams } from "react-router-dom"
import { useQuery, useMutation } from "@tanstack/react-query"
import { Plus, Trash2 } from "lucide-react"
import { billingService } from "@/services/billing.service"
import { bookingService } from "@/services/booking.service"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardHeader, CardTitle, CardContent, CardFooter } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select"
import { useToast } from "@/components/ui/use-toast"

interface LineItem {
  id: string
  description: string
  category: string
  quantity: number
  unitPrice: number
  taxRate: number
}

export function CreateInvoicePage() {
  const navigate = useNavigate()
  const { toast } = useToast()
  const [searchParams] = useSearchParams()
  const bookingIdParam = searchParams.get("bookingId") || ""

  const [bookingId, setBookingId] = useState(bookingIdParam)
  const [discountAmount, setDiscountAmount] = useState(0)
  const [notes, setNotes] = useState("")

  const [items, setItems] = useState<LineItem[]>([
    {
      id: "1",
      description: "Room Tariff",
      category: "Room",
      quantity: 1,
      unitPrice: 3500,
      taxRate: 12,
    },
  ])

  const { data: bookingsRes } = useQuery({
    queryKey: ["active-bookings"],
    queryFn: () => bookingService.getBookings({ pageSize: 50 }),
  })

  const bookings = bookingsRes?.data?.items || []

  const handleBookingChange = (selectedId: string) => {
    setBookingId(selectedId)
    const selected = bookings.find((b) => b.id === selectedId)
    if (selected) {
      const nights = Math.max(1, selected.totalNights || 1)
      const assignedRooms = selected.rooms || selected.bookingRooms || []
      if (assignedRooms.length > 0) {
        setItems(
          assignedRooms.map((r, idx) => ({
            id: String(Date.now() + idx),
            description: `Room ${r.roomNumber || r.room?.roomNumber || ""} (${r.roomTypeName || "Stay"})`,
            category: "Room",
            quantity: nights,
            unitPrice: r.pricePerNight ?? r.ratePerNight ?? 2500,
            taxRate: 12,
          }))
        )
      }
    }
  }

  const addItem = () => {
    setItems((prev) => [
      ...prev,
      {
        id: String(Date.now()),
        description: "",
        category: "Food",
        quantity: 1,
        unitPrice: 0,
        taxRate: 5,
      },
    ])
  }

  const removeItem = (id: string) => {
    if (items.length > 1) {
      setItems((prev) => prev.filter((i) => i.id !== id))
    }
  }

  const updateItem = (id: string, field: keyof LineItem, val: string | number) => {
    setItems((prev) =>
      prev.map((i) => (i.id === id ? { ...i, [field]: val } : i))
    )
  }

  const subTotal = items.reduce((acc, i) => acc + i.quantity * i.unitPrice, 0)
  const totalTax = items.reduce(
    (acc, i) => acc + (i.quantity * i.unitPrice * i.taxRate) / 100,
    0
  )
  const grandTotal = Math.max(0, subTotal + totalTax - discountAmount)

  const createInvoiceMutation = useMutation({
    mutationFn: () =>
      billingService.createInvoice({
        bookingId,
        items,
        discountAmount,
        notes,
      }),
    onSuccess: (res) => {
      toast({ title: "Invoice generated successfully" })
      if (res.data?.id) {
        navigate(`/billing/${res.data.id}`)
      } else {
        navigate("/billing")
      }
    },
    onError: () => {
      toast({ title: "Failed to generate invoice", variant: "destructive" })
    },
  })

  const handleGenerate = (e: React.FormEvent) => {
    e.preventDefault()
    if (!bookingId) {
      toast({ title: "Please select a booking to bill", variant: "destructive" })
      return
    }
    createInvoiceMutation.mutate()
  }

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <PageHeader
        title="Generate Tax Invoice"
        description="Add room rates, restaurant bills, extra services, taxes, and finalize guest checkout bill"
        breadcrumbs={[{ label: "Billing", href: "/billing" }, { label: "New Invoice" }]}
      />

      <form onSubmit={handleGenerate}>
        <Card className="shadow-sm">
          <CardHeader>
            <CardTitle>Invoice Setup</CardTitle>
          </CardHeader>
          <CardContent className="space-y-6">
            <div className="space-y-2">
              <Label>Associated Booking / Stay *</Label>
              <Select value={bookingId} onValueChange={handleBookingChange}>
                <SelectTrigger>
                  <SelectValue placeholder="Select active booking" />
                </SelectTrigger>
                <SelectContent>
                  {bookings.map((b) => (
                    <SelectItem key={b.id} value={b.id}>
                      {b.bookingReference} - {b.guestName || b.guest?.fullName || "Guest"} (
                      {new Date(b.checkInDate).toLocaleDateString("en-IN")} to{" "}
                      {new Date(b.checkOutDate).toLocaleDateString("en-IN")})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Line items table */}
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <Label className="text-sm font-semibold">Charges & Line Items</Label>
                <Button type="button" variant="outline" size="sm" onClick={addItem}>
                  <Plus className="h-4 w-4 mr-1" /> Add Charge Item
                </Button>
              </div>

              <div className="space-y-2">
                {items.map((item) => (
                  <div
                    key={item.id}
                    className="grid grid-cols-12 gap-2 items-center p-3 rounded-lg border bg-slate-50/50"
                  >
                    <div className="col-span-4">
                      <Input
                        placeholder="Item Description"
                        value={item.description}
                        onChange={(e) => updateItem(item.id, "description", e.target.value)}
                        required
                      />
                    </div>
                    <div className="col-span-2">
                      <Select
                        value={item.category}
                        onValueChange={(val) => updateItem(item.id, "category", val)}
                      >
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="Room">Room</SelectItem>
                          <SelectItem value="Food">Food / Dine</SelectItem>
                          <SelectItem value="Miscellaneous">Service</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                    <div className="col-span-2">
                      <Input
                        type="number"
                        min="1"
                        placeholder="Qty"
                        value={item.quantity}
                        onChange={(e) => updateItem(item.id, "quantity", Number(e.target.value))}
                      />
                    </div>
                    <div className="col-span-2">
                      <Input
                        type="number"
                        min="0"
                        placeholder="Rate (₹)"
                        value={item.unitPrice}
                        onChange={(e) => updateItem(item.id, "unitPrice", Number(e.target.value))}
                      />
                    </div>
                    <div className="col-span-1">
                      <span className="text-xs text-muted-foreground block text-center">
                        {item.taxRate}% GST
                      </span>
                    </div>
                    <div className="col-span-1 flex justify-end">
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        className="text-destructive h-8 w-8"
                        onClick={() => removeItem(item.id)}
                        disabled={items.length <= 1}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Calculations & Discounts */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 pt-4 border-t">
              <div className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="discount">Special Discount (₹)</Label>
                  <Input
                    id="discount"
                    type="number"
                    min="0"
                    value={discountAmount}
                    onChange={(e) => setDiscountAmount(Number(e.target.value))}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="notes">Remarks / Payment Terms</Label>
                  <Input
                    id="notes"
                    placeholder="e.g. Paid in full upon check-out"
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                  />
                </div>
              </div>

              <div className="p-4 rounded-lg bg-muted/60 space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Subtotal</span>
                  <span className="font-medium">₹{subTotal.toLocaleString("en-IN")}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Estimated GST (12% / 5%)</span>
                  <span className="font-medium">₹{totalTax.toLocaleString("en-IN")}</span>
                </div>
                {discountAmount > 0 && (
                  <div className="flex justify-between text-emerald-600">
                    <span>Discount</span>
                    <span>-₹{discountAmount.toLocaleString("en-IN")}</span>
                  </div>
                )}
                <div className="flex justify-between text-lg font-bold border-t pt-2 text-foreground">
                  <span>Grand Total</span>
                  <span>₹{grandTotal.toLocaleString("en-IN")}</span>
                </div>
              </div>
            </div>
          </CardContent>
          <CardFooter className="flex justify-between">
            <Button type="button" variant="outline" onClick={() => navigate("/billing")}>
              Cancel
            </Button>
            <Button type="submit" disabled={createInvoiceMutation.isPending}>
              {createInvoiceMutation.isPending ? "Generating..." : "Generate Official Invoice"}
            </Button>
          </CardFooter>
        </Card>
      </form>
    </div>
  )
}
