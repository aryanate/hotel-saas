import { useQuery } from "@tanstack/react-query"
import { Building2, Users, CreditCard, CalendarCheck2, ArrowRight } from "lucide-react"
import { Link } from "react-router-dom"
import { get } from "@/services/api"
import type { Hotel } from "@/types"
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"

interface PlatformStats {
  totalHotels: number
  activeHotels: number
  suspendedHotels: number
  totalBookings: number
  totalRevenue: number
  totalActiveUsers: number
  totalGuests: number
}

export function AdminDashboardPage() {
  const { data: hotelsRes, isLoading: loadingHotels } = useQuery({
    queryKey: ["admin-hotels"],
    queryFn: () => get<Hotel[]>("/api/admin/hotels"),
  })

  const { data: statsRes, isLoading: loadingStats } = useQuery({
    queryKey: ["admin-stats"],
    queryFn: () => get<PlatformStats>("/api/admin/stats"),
  })

  const hotels = hotelsRes?.data || []
  const platformStats = statsRes?.data

  const stats = [
    {
      title: "Registered Properties",
      value: String(platformStats?.totalHotels ?? hotels.length),
      subtitle: `${platformStats?.activeHotels ?? 0} active, ${platformStats?.suspendedHotels ?? 0} suspended`,
      icon: Building2,
      color: "text-blue-600 bg-blue-50",
    },
    {
      title: "Total Bookings",
      value: String(platformStats?.totalBookings ?? 0),
      subtitle: `${platformStats?.totalGuests ?? 0} registered guests`,
      icon: CalendarCheck2,
      color: "text-emerald-600 bg-emerald-50",
    },
    {
      title: "Total Billed Volume",
      value: `₹${(platformStats?.totalRevenue ?? 0).toLocaleString("en-IN")}`,
      subtitle: "Across all tenant hotels",
      icon: CreditCard,
      color: "text-purple-600 bg-purple-50",
    },
    {
      title: "Active Users",
      value: String(platformStats?.totalActiveUsers ?? 0),
      subtitle: "Hotel managers & staff",
      icon: Users,
      color: "text-amber-600 bg-amber-50",
    },
  ]

  if (loadingHotels || loadingStats) {
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
