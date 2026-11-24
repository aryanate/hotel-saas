import { useState, useEffect } from "react"
import { useCurrentHotel } from "@/hooks/useCurrentHotel"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"
import { useToast } from "@/components/ui/use-toast"

export function HotelSettingsPage() {
  const { toast } = useToast()
  const { hotel, isLoading, updateHotel, isUpdating } = useCurrentHotel()

  const [name, setName] = useState("")
  const [ownerName, setOwnerName] = useState("")
  const [email, setEmail] = useState("")
  const [mobile, setMobile] = useState("")
  const [address, setAddress] = useState("")
  const [city, setCity] = useState("")
  const [state, setState] = useState("")
  const [pinCode, setPinCode] = useState("")
  const [gstIn, setGstIn] = useState("")
  const [invoicePrefix, setInvoicePrefix] = useState("INV")
  const [checkInTime, setCheckInTime] = useState("14:00")
  const [checkOutTime, setCheckOutTime] = useState("11:00")

  useEffect(() => {
    if (hotel) {
      setName(hotel.name || "")
      setOwnerName(hotel.ownerName || "")
      setEmail(hotel.email || "")
      setMobile(hotel.mobile || "")
      setAddress(hotel.address || "")
      setCity(hotel.city || "")
      setState(hotel.state || "")
      setPinCode(hotel.pinCode || "")
      setGstIn(hotel.gstIn || "")
      setInvoicePrefix(hotel.invoicePrefix || "INV")
      setCheckInTime(hotel.checkInTime || "14:00")
      setCheckOutTime(hotel.checkOutTime || "11:00")
    }
  }, [hotel])

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await updateHotel({
        name,
        ownerName,
        email,
        mobile,
        address,
        city,
        state,
        pinCode,
        gstIn,
        invoicePrefix,
        checkInTime,
        checkOutTime,
      })
      toast({ title: "Hotel preferences saved successfully" })
    } catch {
      toast({ title: "Failed to update hotel settings", variant: "destructive" })
    }
  }

  if (isLoading) {
    return <LoadingSpinner size="lg" className="h-64" />
  }

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <PageHeader
        title="Hotel Settings & Preferences"
        description="Configure property identity, check-in policy, invoice series, and GST details"
      />

      <form onSubmit={handleSave}>
        <div className="space-y-6">
          {/* General Information */}
          <Card>
            <CardHeader>
              <CardTitle className="text-base font-semibold">Property Profile</CardTitle>
              <CardDescription>Displayed on guest invoices and public check-in forms</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="hName">Hotel Name *</Label>
                  <Input id="hName" value={name} onChange={(e) => setName(e.target.value)} required />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="oName">Owner / Primary Contact *</Label>
                  <Input id="oName" value={ownerName} onChange={(e) => setOwnerName(e.target.value)} required />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="hEmail">Official Email *</Label>
                  <Input id="hEmail" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="hPhone">Contact Mobile / Phone *</Label>
                  <Input id="hPhone" value={mobile} onChange={(e) => setMobile(e.target.value)} required />
                </div>
                <div className="space-y-2 md:col-span-2">
                  <Label htmlFor="hAddr">Address *</Label>
                  <Input id="hAddr" value={address} onChange={(e) => setAddress(e.target.value)} required />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="hCity">City</Label>
                  <Input id="hCity" value={city} onChange={(e) => setCity(e.target.value)} />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="hState">State</Label>
                  <Input id="hState" value={state} onChange={(e) => setState(e.target.value)} />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="hPin">PIN Code</Label>
                  <Input id="hPin" value={pinCode} onChange={(e) => setPinCode(e.target.value)} />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="hGst">GSTIN Number</Label>
                  <Input id="hGst" value={gstIn} onChange={(e) => setGstIn(e.target.value)} placeholder="27AAAAA0000A1Z5" />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Policy & Invoicing */}
          <Card>
            <CardHeader>
              <CardTitle className="text-base font-semibold">Policy & Invoicing Configuration</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="cin">Standard Check-In Time</Label>
                  <Input id="cin" type="time" value={checkInTime} onChange={(e) => setCheckInTime(e.target.value)} />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="cout">Standard Check-Out Time</Label>
                  <Input id="cout" type="time" value={checkOutTime} onChange={(e) => setCheckOutTime(e.target.value)} />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="prefix">Invoice Prefix</Label>
                  <Input id="prefix" value={invoicePrefix} onChange={(e) => setInvoicePrefix(e.target.value)} placeholder="INV" />
                </div>
              </div>
            </CardContent>
            <CardFooter className="flex justify-end pt-4 border-t">
              <Button type="submit" disabled={isUpdating}>
                {isUpdating ? "Saving Settings..." : "Save Preferences"}
              </Button>
            </CardFooter>
          </Card>
        </div>
      </form>
    </div>
  )
}
