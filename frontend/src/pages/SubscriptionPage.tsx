import { useQuery } from "@tanstack/react-query"
import { Check, CreditCard, Sparkles } from "lucide-react"
import { roomService } from "@/services/room.service"
import { staffService } from "@/services/staff.service"
import { Button } from "@/components/ui/button"
import { Card, CardHeader, CardTitle, CardContent, CardFooter } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { useToast } from "@/components/ui/use-toast"

export function SubscriptionPage() {
  const { toast } = useToast()

  const { data: roomsRes } = useQuery({
    queryKey: ["rooms"],
    queryFn: () => roomService.getRooms(),
  })

  const { data: staffRes } = useQuery({
    queryKey: ["staff-members"],
    queryFn: () => staffService.getStaff(),
  })

  const roomsUsed = roomsRes?.data?.length ?? 0
  const staffUsed = staffRes?.data?.length ?? 1

  const currentPlan = {
    name: "Growth Professional",
    status: "Active",
    renewsOn: "15 Dec 2026",
    priceMonthly: 2499,
    roomLimit: 50,
    roomsUsed,
    staffLimit: 10,
    staffUsed,
  }

  const plans = [
    {
      name: "Starter",
      price: 999,
      rooms: 20,
      users: 3,
      features: [
        "Up to 20 rooms",
        "Front-desk digital check-in",
        "GST Invoicing",
        "Basic Reports",
      ],
      current: false,
    },
    {
      name: "Growth Professional",
      price: 2499,
      rooms: 50,
      users: 10,
      features: [
        "Up to 50 rooms",
        "WhatsApp Automation & Invoices",
        "Full Marketing Campaigns",
        "VIP & Guest Retention Engine",
        "Priority Support",
      ],
      current: true,
    },
    {
      name: "Enterprise Multi-Property",
      price: 5999,
      rooms: "Unlimited",
      users: "Unlimited",
      features: [
        "Unlimited rooms & staff",
        "Custom domain branding",
        "Dedicated account manager",
        "Custom API Integrations",
        "SLA 99.9% guarantee",
      ],
      current: false,
    },
  ]

  const handleUpgrade = (planName: string) => {
    toast({ title: `Requested upgrade to ${planName}. Payment gateway opened.` })
  }

  return (
    <div className="space-y-6 max-w-5xl mx-auto">
      <PageHeader
        title="Subscription & Billing"
        description="Review your active cloud tier, resource quotas, and upgrade options"
      />

      {/* Current plan banner */}
      <Card className="shadow-sm border-primary/30 bg-primary/5">
        <CardContent className="p-6">
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
              <div className="flex items-center gap-2">
                <h3 className="text-xl font-bold text-foreground">{currentPlan.name} Plan</h3>
                <StatusBadge status={currentPlan.status} />
              </div>
              <p className="text-xs text-muted-foreground mt-1">
                Renews automatically on {currentPlan.renewsOn} at ₹{currentPlan.priceMonthly}/month
              </p>
            </div>
            <Button variant="outline" className="bg-white">
              <CreditCard className="h-4 w-4 mr-2" /> Manage Payment Method
            </Button>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 mt-6 pt-6 border-t border-primary/10">
            <div>
              <div className="flex justify-between text-xs font-semibold mb-1">
                <span>Room Quota</span>
                <span>
                  {currentPlan.roomsUsed} / {currentPlan.roomLimit} Rooms
                </span>
              </div>
              <div className="w-full bg-slate-200 h-2 rounded-full overflow-hidden">
                <div
                  className="bg-primary h-full rounded-full"
                  style={{
                    width: `${(currentPlan.roomsUsed / currentPlan.roomLimit) * 100}%`,
                  }}
                />
              </div>
            </div>

            <div>
              <div className="flex justify-between text-xs font-semibold mb-1">
                <span>Staff Seats</span>
                <span>
                  {currentPlan.staffUsed} / {currentPlan.staffLimit} Users
                </span>
              </div>
              <div className="w-full bg-slate-200 h-2 rounded-full overflow-hidden">
                <div
                  className="bg-primary h-full rounded-full"
                  style={{
                    width: `${(currentPlan.staffUsed / currentPlan.staffLimit) * 100}%`,
                  }}
                />
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Pricing options */}
      <div>
        <h3 className="text-lg font-bold mb-4">Available Plans</h3>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {plans.map((p) => (
            <Card
              key={p.name}
              className={`flex flex-col justify-between shadow-sm relative ${
                p.current ? "border-2 border-primary" : ""
              }`}
            >
              {p.current && (
                <span className="absolute -top-3 right-4 bg-primary text-primary-foreground text-[10px] font-bold px-2 py-0.5 rounded-full uppercase tracking-wider">
                  Current Tier
                </span>
              )}
              <CardHeader>
                <CardTitle className="text-lg">{p.name}</CardTitle>
                <div className="pt-2">
                  <span className="text-3xl font-extrabold">₹{p.price.toLocaleString("en-IN")}</span>
                  <span className="text-xs text-muted-foreground"> / month</span>
                </div>
              </CardHeader>
              <CardContent className="space-y-3">
                <ul className="space-y-2 text-xs">
                  {p.features.map((feat) => (
                    <li key={feat} className="flex items-center gap-2">
                      <Check className="h-4 w-4 text-emerald-600 shrink-0" />
                      <span>{feat}</span>
                    </li>
                  ))}
                </ul>
              </CardContent>
              <CardFooter className="pt-4 border-t">
                {p.current ? (
                  <Button variant="outline" className="w-full" disabled>
                    Current Plan
                  </Button>
                ) : (
                  <Button className="w-full" onClick={() => handleUpgrade(p.name)}>
                    <Sparkles className="h-4 w-4 mr-1" /> Upgrade Now
                  </Button>
                )}
              </CardFooter>
            </Card>
          ))}
        </div>
      </div>
    </div>
  )
}
