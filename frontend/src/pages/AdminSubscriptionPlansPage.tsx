import { useState } from "react"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { Plus, Edit2 } from "lucide-react"
import { get, post, put } from "@/services/api"
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
  const [editingId, setEditingId] = useState<string | null>(null)
  const [name, setName] = useState("")
  const [description, setDescription] = useState("")
  const [price, setPrice] = useState(1999)
  const [billingCycle, setBillingCycle] = useState("Monthly")
  const [maxBookingsPerMonth, setMaxBookingsPerMonth] = useState(500)

  const { data: res, isLoading } = useQuery({
    queryKey: ["admin-plans"],
    queryFn: () => get<SubscriptionPlan[]>("/api/admin/plans"),
  })

  const plans: SubscriptionPlan[] = Array.isArray(res?.data) ? res.data : []

  const openCreateModal = () => {
    setEditingId(null)
    setName("")
    setDescription("")
    setPrice(1999)
    setBillingCycle("Monthly")
    setMaxBookingsPerMonth(500)
    setModalOpen(true)
  }

  const openEditModal = (plan: SubscriptionPlan) => {
    setEditingId(plan.id)
    setName(plan.name)
    setDescription(plan.description || "")
    setPrice(plan.price)
    setBillingCycle(plan.billingCycle || "Monthly")
    setMaxBookingsPerMonth(plan.maxBookingsPerMonth || 500)
    setModalOpen(true)
  }

  const savePlanMutation = useMutation({
    mutationFn: () => {
      const payload = {
        name,
        description,
        price: Number(price),
        billingCycle,
        maxBookingsPerMonth: Number(maxBookingsPerMonth),
        featuresJson: "[]",
        isActive: true,
      }
      return editingId
        ? put(`/api/admin/plans/${editingId}`, payload)
        : post("/api/admin/plans", payload)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-plans"] })
      toast({ title: editingId ? "Subscription plan updated" : "New plan tier created" })
      setModalOpen(false)
    },
    onError: (err: unknown) => {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
      toast({ title: msg || "Failed to save plan", variant: "destructive" })
    },
  })

  const handleSave = (e: React.FormEvent) => {
    e.preventDefault()
    if (!name) return
    savePlanMutation.mutate()
  }

  if (isLoading) {
    return <LoadingSpinner size="lg" className="h-64" />
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Subscription Plans Management"
        description="Configure pricing, feature allowances, and booking quotas for the SaaS platform"
        breadcrumbs={[{ label: "Admin", href: "/admin/dashboard" }, { label: "Subscription Plans" }]}
      >
        <Button onClick={openCreateModal}>
          <Plus className="h-4 w-4 mr-1" /> Add Plan Tier
        </Button>
      </PageHeader>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {plans.map((p) => {
          let features: string[] = []
          try {
            const parsed = JSON.parse(p.featuresJson || "[]")
            if (Array.isArray(parsed)) features = parsed
          } catch {
            features = []
          }
          return (
            <Card key={p.id} className="shadow-sm flex flex-col justify-between">
              <CardHeader>
                <div className="flex justify-between items-start">
                  <CardTitle className="text-lg">{p.name}</CardTitle>
                  <span className="text-xs font-mono font-semibold bg-muted px-2 py-0.5 rounded">
                    {p.billingCycle}
                  </span>
                </div>
                <div className="pt-2">
                  <span className="text-2xl font-bold">₹{(p.price ?? 0).toLocaleString("en-IN")}</span>
                  <span className="text-xs text-muted-foreground"> / {p.billingCycle?.toLowerCase() || "month"}</span>
                </div>
              </CardHeader>
              <CardContent className="space-y-3 text-xs">
                <p className="text-muted-foreground">{p.description}</p>
                <div className="pt-2 border-t space-y-1">
                  <p>Max Bookings / Month: <span className="font-semibold">{p.maxBookingsPerMonth}</span></p>
                  {features.map((f) => (
                    <p key={f} className="text-muted-foreground">&bull; {f}</p>
                  ))}
                </div>
              </CardContent>
              <CardFooter className="pt-3 border-t">
                <Button variant="outline" size="sm" className="w-full" onClick={() => openEditModal(p)}>
                  <Edit2 className="h-3.5 w-3.5 mr-1" /> Edit Tier
                </Button>
              </CardFooter>
            </Card>
          )
        })}
      </div>

      <Dialog open={modalOpen} onOpenChange={setModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editingId ? "Edit Subscription Plan" : "Create Subscription Plan"}</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSave} className="space-y-4">
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
              <Label>Description</Label>
              <Input
                placeholder="Plan summary"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />
            </div>
            <div className="grid grid-cols-3 gap-2">
              <div className="space-y-2">
                <Label>Price (₹)</Label>
                <Input
                  type="number"
                  value={price}
                  onChange={(e) => setPrice(Number(e.target.value))}
                />
              </div>
              <div className="space-y-2">
                <Label>Cycle</Label>
                <Input
                  value={billingCycle}
                  onChange={(e) => setBillingCycle(e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <Label>Max Bookings</Label>
                <Input
                  type="number"
                  value={maxBookingsPerMonth}
                  onChange={(e) => setMaxBookingsPerMonth(Number(e.target.value))}
                />
              </div>
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setModalOpen(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={savePlanMutation.isPending}>
                {savePlanMutation.isPending ? "Saving..." : "Save Plan"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
