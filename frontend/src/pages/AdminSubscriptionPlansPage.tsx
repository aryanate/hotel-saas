import { useState } from "react"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { Plus, Check, Edit2 } from "lucide-react"
import { get, post } from "@/services/api"
import type { SubscriptionPlan } from "@/types"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardHeader, CardTitle, CardContent, CardFooter } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog"
import { useToast } from "@/components/ui/use-toast"

export function AdminSubscriptionPlansPage() {
  const { toast } = useToast()
  const queryClient = useQueryClient()
  const [modalOpen, setModalOpen] = useState(false)
  const [name, setName] = useState("")
  const [code, setCode] = useState("")
  const [priceMonthly, setPriceMonthly] = useState(1999)
  const [maxRooms, setMaxRooms] = useState(30)
  const [maxUsers, setMaxUsers] = useState(5)

  const { data: res, isLoading } = useQuery({
    queryKey: ["admin-plans"],
    queryFn: () => get<SubscriptionPlan[]>("/api/admin/plans"),
  })

  const plans: SubscriptionPlan[] = res?.data || [
    {
      id: "p1",
      createdAt: "2026-01-01",
      updatedAt: "2026-01-01",
      name: "Starter",
      code: "STARTER",
      description: "For small motels and guest houses",
      priceMonthly: 999,
      priceYearly: 9990,
      maxRooms: 20,
      maxUsers: 3,
      isActive: true,
      features: ["Up to 20 rooms", "Front-desk check-in", "GST Invoicing"],
    },
    {
      id: "p2",
      createdAt: "2026-01-01",
      updatedAt: "2026-01-01",
      name: "Growth Professional",
      code: "GROWTH",
      description: "For busy boutique hotels and resorts",
      priceMonthly: 2499,
      priceYearly: 24990,
      maxRooms: 50,
      maxUsers: 10,
      isActive: true,
      features: [
        "Up to 50 rooms",
        "WhatsApp Automation",
        "Campaigns",
        "VIP Retention",
      ],
    },
    {
      id: "p3",
      createdAt: "2026-01-01",
      updatedAt: "2026-01-01",
      name: "Enterprise Multi-Property",
      code: "ENTERPRISE",
      description: "For hotel chains with extensive requirements",
      priceMonthly: 5999,
      priceYearly: 59990,
      maxRooms: 999,
      maxUsers: 999,
      isActive: true,
      features: [
        "Unlimited rooms & staff",
        "Custom branding",
        "API access",
        "SLA guarantee",
      ],
    },
  ]

  const createPlanMutation = useMutation({
    mutationFn: () =>
      post("/api/admin/plans", {
        name,
        code,
        priceMonthly,
        priceYearly: priceMonthly * 10,
        maxRooms,
        maxUsers,
        isActive: true,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-plans"] })
      toast({ title: "New plan tier created" })
      setModalOpen(false)
      setName("")
      setCode("")
    },
  })

  const handleCreate = (e: React.FormEvent) => {
    e.preventDefault()
    if (!name || !code) return
    createPlanMutation.mutate()
  }

  if (isLoading) {
    return <LoadingSpinner size="lg" className="h-64" />
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Subscription Plans Management"
        description="Configure pricing, feature allowances, and room quotas for the SaaS platform"
        breadcrumbs={[{ label: "Admin", href: "/admin/dashboard" }, { label: "Subscription Plans" }]}
      >
        <Button onClick={() => setModalOpen(true)}>
          <Plus className="h-4 w-4 mr-1" /> Add Plan Tier
        </Button>
      </PageHeader>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {plans.map((p) => (
          <Card key={p.id} className="shadow-sm flex flex-col justify-between">
            <CardHeader>
              <div className="flex justify-between items-start">
                <CardTitle className="text-lg">{p.name}</CardTitle>
                <span className="text-xs font-mono font-semibold bg-muted px-2 py-0.5 rounded">
                  {p.code}
                </span>
              </div>
              <div className="pt-2">
                <span className="text-2xl font-bold">₹{p.priceMonthly.toLocaleString("en-IN")}</span>
                <span className="text-xs text-muted-foreground"> / month</span>
              </div>
            </CardHeader>
            <CardContent className="space-y-3 text-xs">
              <p className="text-muted-foreground">{p.description}</p>
              <div className="pt-2 border-t space-y-1">
                <p>Max Rooms: <span className="font-semibold">{p.maxRooms}</span></p>
                <p>Max Staff Accounts: <span className="font-semibold">{p.maxUsers}</span></p>
              </div>
            </CardContent>
            <CardFooter className="pt-3 border-t">
              <Button variant="outline" size="sm" className="w-full">
                <Edit2 className="h-3.5 w-3.5 mr-1" /> Edit Tier
              </Button>
            </CardFooter>
          </Card>
        ))}
      </div>

      <Dialog open={modalOpen} onOpenChange={setModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Create Subscription Plan</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleCreate} className="space-y-4">
            <div className="space-y-2">
              <Label>Plan Name *</Label>
              <Input
                placeholder="e.g. Boutique Standard"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
              />
            </div>
            <div className="space-y-2">
              <Label>Code Identifier *</Label>
              <Input
                placeholder="BOUTIQUE"
                value={code}
                onChange={(e) => setCode(e.target.value)}
                required
              />
            </div>
            <div className="grid grid-cols-3 gap-2">
              <div className="space-y-2">
                <Label>Monthly (₹)</Label>
                <Input
                  type="number"
                  value={priceMonthly}
                  onChange={(e) => setPriceMonthly(Number(e.target.value))}
                />
              </div>
              <div className="space-y-2">
                <Label>Max Rooms</Label>
                <Input
                  type="number"
                  value={maxRooms}
                  onChange={(e) => setMaxRooms(Number(e.target.value))}
                />
              </div>
              <div className="space-y-2">
                <Label>Max Users</Label>
                <Input
                  type="number"
                  value={maxUsers}
                  onChange={(e) => setMaxUsers(Number(e.target.value))}
                />
              </div>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setModalOpen(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={createPlanMutation.isPending}>
                Save Plan
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
