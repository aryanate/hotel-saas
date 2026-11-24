import { useQuery } from "@tanstack/react-query"
import { Building2, Users, CreditCard, ShieldCheck, ArrowRight } from "lucide-react"
import { Link } from "react-router-dom"
import { get } from "@/services/api"
import type { Hotel } from "@/types"
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"

export function AdminDashboardPage() {
  const { data: res, isLoading } = useQuery({
    queryKey: ["admin-hotels"],
    queryFn: () => get<Hotel[]>("/api/admin/hotels"),
  })

  const hotels = res?.data || [
    {
      id: "h1",
      name: "Grand Royal Palace",
      ownerName: "Rajeev Kapoor",
      email: "contact@grandroyal.com",
      mobile: "9876543210",
      city: "Mumbai",
      numberOfRooms: 40,
      isActive: true,
      createdAt: "2026-10-01",
    },
    {
      id: "h2",
      name: "Seaside Heritage Resort",
      ownerName: "Anita Rao",
      email: "info@seaside.com",
      mobile: "9123456780",
      city: "Goa",
      numberOfRooms: 65,
      isActive: true,
      createdAt: "2026-10-15",
    },
    {
      id: "h3",
      name: "Mountain Mist Haven",
      ownerName: "Devendra Negi",
      email: "mist@mountain.com",
      mobile: "9988771122",
      city: "Manali",
      numberOfRooms: 25,
      isActive: false,
      createdAt: "2026-11-01",
    },
  ]

  const stats = [
    {
      title: "Registered Properties",
      value: "142",
      subtitle: "+8 this month",
      icon: Building2,
      color: "text-blue-600 bg-blue-50",
    },
    {
      title: "Active Rooms Cloud",
      value: "4,820",
      subtitle: "Across 24 states",
      icon: Users,
      color: "text-emerald-600 bg-emerald-50",
    },
    {
      title: "Monthly SaaS ARR",
      value: "₹3,54,000",
      subtitle: "Active subscriptions",
      icon: CreditCard,
      color: "text-purple-600 bg-purple-50",
    },
    {
      title: "System Compliance",
      value: "100%",
      subtitle: "All tenants masked KYC",
      icon: ShieldCheck,
      color: "text-amber-600 bg-amber-50",
    },
  ]

  if (isLoading) {
    return <LoadingSpinner size="lg" className="h-64" />
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Super Admin Control Hub"
        description="Global platform metrics, multi-tenant overview, and cloud infrastructure status"
      />

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {stats.map((stat) => (
          <Card key={stat.title} className="shadow-sm">
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {stat.title}
              </CardTitle>
              <div className={`p-2 rounded-lg ${stat.color}`}>
                <stat.icon className="h-5 w-5" />
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{stat.value}</div>
              <p className="text-xs text-muted-foreground mt-1">{stat.subtitle}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid grid-cols-1 gap-6">
        <Card className="shadow-sm">
          <CardHeader className="flex flex-row items-center justify-between">
            <div>
              <CardTitle className="text-base font-semibold">Tenant Properties</CardTitle>
              <CardDescription>Hotel businesses running on HotelSaaS</CardDescription>
            </div>
            <Link
              to="/admin/hotels"
              className="text-xs text-primary font-semibold flex items-center gap-1 hover:underline"
            >
              Manage All Hotels <ArrowRight className="h-3 w-3" />
            </Link>
          </CardHeader>
          <CardContent>
            <div className="divide-y text-sm">
              {hotels.map((h) => (
                <div key={h.id} className="py-3 flex items-center justify-between">
                  <div>
                    <p className="font-bold text-foreground">{h.name}</p>
                    <p className="text-xs text-muted-foreground">
                      {h.city} &bull; Owner: {h.ownerName} &bull; {h.numberOfRooms} Rooms
                    </p>
                  </div>
                  <StatusBadge status={h.isActive ? "Active" : "Suspended"} />
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
