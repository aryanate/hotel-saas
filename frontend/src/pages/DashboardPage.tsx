import { useQuery } from "@tanstack/react-query"
import {
  BedDouble,
  CalendarCheck,
  CalendarX,
  IndianRupee,
  ArrowUpRight,
} from "lucide-react"
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from "recharts"
import { reportService } from "@/services/report.service"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"
import { Link } from "react-router-dom"

export function DashboardPage() {
  const { data: reportResponse, isLoading } = useQuery({
    queryKey: ["dashboard-stats"],
    queryFn: () => reportService.getDashboard(),
  })

  const raw = reportResponse?.data

  const stats = {
    totalRooms: raw?.totalRooms ?? 0,
    occupiedRooms: raw?.occupiedRooms ?? 0,
    availableRooms: raw?.availableRooms ?? 0,
    maintenanceRooms: raw?.maintenanceRooms ?? raw?.cleaningRooms ?? 0,
    todayCheckIns: raw?.todayCheckIns ?? 0,
    todayCheckOuts: raw?.todayCheckOuts ?? 0,
    monthlyRevenue: Number(raw?.monthlyRevenue ?? 0),
    pendingPayments: Number(raw?.pendingPayments ?? 0),
    weeklyRevenue: (raw?.weeklyRevenue && raw.weeklyRevenue.length > 0)
      ? raw.weeklyRevenue
      : [
          { date: "Mon", amount: 0 },
          { date: "Tue", amount: 0 },
          { date: "Wed", amount: 0 },
          { date: "Thu", amount: 0 },
          { date: "Fri", amount: 0 },
          { date: "Sat", amount: 0 },
          { date: "Sun", amount: 0 },
        ],
    recentBookings: raw?.recentBookings ?? [],
    recentRegistrations: raw?.recentRegistrations ?? [],
  }

  if (isLoading) {
    return <LoadingSpinner size="lg" className="h-64" />
  }

  const kpis = [
    {
      title: "Occupied Rooms",
      value: `${stats.occupiedRooms} / ${stats.totalRooms}`,
      subtitle: `${stats.availableRooms} rooms available`,
      icon: BedDouble,
      color: "text-blue-600 bg-blue-50",
    },
    {
      title: "Today's Check-Ins",
      value: stats.todayCheckIns,
      subtitle: "Arrivals expected today",
      icon: CalendarCheck,
      color: "text-emerald-600 bg-emerald-50",
    },
    {
      title: "Today's Check-Outs",
      value: stats.todayCheckOuts,
      subtitle: "Departures scheduled",
      icon: CalendarX,
      color: "text-amber-600 bg-amber-50",
    },
    {
      title: "Monthly Revenue",
      value: `₹${stats.monthlyRevenue.toLocaleString("en-IN")}`,
      subtitle: `₹${stats.pendingPayments.toLocaleString("en-IN")} pending collection`,
      icon: IndianRupee,
      color: "text-purple-600 bg-purple-50",
    },
  ]

  return (
    <div className="space-y-6">
      <PageHeader
        title="Operations Overview"
        description="Real-time occupancy, revenue, and daily front-desk movements"
      />

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {kpis.map((kpi) => (
          <Card key={kpi.title} className="shadow-sm">
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                {kpi.title}
              </CardTitle>
              <div className={`p-2 rounded-lg ${kpi.color}`}>
                <kpi.icon className="h-5 w-5" />
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{kpi.value}</div>
              <p className="text-xs text-muted-foreground mt-1">{kpi.subtitle}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Charts & Quick Stats */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <Card className="lg:col-span-2 shadow-sm">
          <CardHeader>
            <CardTitle className="text-base font-semibold">Weekly Revenue Trend</CardTitle>
            <CardDescription>Daily gross receipts for the current week</CardDescription>
          </CardHeader>
          <CardContent className="h-[280px]">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={stats.weeklyRevenue}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} />
                <XAxis dataKey="date" />
                <YAxis
                  tickFormatter={(val) => `₹${val >= 1000 ? `${(val / 1000).toFixed(0)}k` : val}`}
                  width={60}
                />
                <Tooltip
                  formatter={(val: unknown) => [
                    `₹${Number(val).toLocaleString("en-IN")}`,
                    "Revenue",
                  ]}
                />
                <Bar dataKey="amount" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        {/* Quick Check-in Queue and Status summary */}
        <Card className="shadow-sm flex flex-col justify-between">
          <CardHeader>
            <CardTitle className="text-base font-semibold">Room Status Breakdown</CardTitle>
            <CardDescription>Current property availability</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between text-sm py-2 border-b">
              <span className="flex items-center gap-2">
                <span className="h-3 w-3 rounded-full bg-emerald-500" />
                Available Clean
              </span>
              <span className="font-semibold">{stats.availableRooms}</span>
            </div>
            <div className="flex items-center justify-between text-sm py-2 border-b">
              <span className="flex items-center gap-2">
                <span className="h-3 w-3 rounded-full bg-blue-500" />
                Occupied
              </span>
              <span className="font-semibold">{stats.occupiedRooms}</span>
            </div>
            <div className="flex items-center justify-between text-sm py-2 border-b">
              <span className="flex items-center gap-2">
                <span className="h-3 w-3 rounded-full bg-amber-500" />
                Maintenance / Cleaning
              </span>
              <span className="font-semibold">{stats.maintenanceRooms}</span>
            </div>
            <div className="pt-2">
              <Link
                to="/checkin"
                className="flex items-center justify-between p-3 rounded-lg bg-primary/5 hover:bg-primary/10 text-primary transition-colors text-sm font-medium"
              >
                <span>View Digital Check-in Queue</span>
                <ArrowUpRight className="h-4 w-4" />
              </Link>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Recent Bookings and Registrations Tables */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Bookings */}
        <Card className="shadow-sm">
          <CardHeader className="flex flex-row items-center justify-between">
            <div>
              <CardTitle className="text-base font-semibold">Recent Bookings</CardTitle>
              <CardDescription>Latest reservations placed across channels</CardDescription>
            </div>
            <Link to="/bookings" className="text-xs text-primary font-medium hover:underline">
              View All
            </Link>
          </CardHeader>
          <CardContent>
            {stats.recentBookings.length === 0 ? (
              <p className="text-sm text-muted-foreground py-6 text-center">
                No recent bookings recorded yet.
              </p>
            ) : (
              <div className="divide-y">
                {stats.recentBookings.map((b) => (
                  <div key={b.id} className="py-3 flex items-center justify-between text-sm">
                    <div>
                      <p className="font-medium">{b.guest?.fullName || "Guest"}</p>
                      <p className="text-xs text-muted-foreground">
                        Ref: {b.bookingReference || b.id} &bull; {b.checkInDate || "-"} ({b.totalNights || 1} nights)
                      </p>
                    </div>
                    <StatusBadge status={b.status} />
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Registrations */}
        <Card className="shadow-sm">
          <CardHeader className="flex flex-row items-center justify-between">
            <div>
              <CardTitle className="text-base font-semibold">New Guest Registrations</CardTitle>
              <CardDescription>Recently registered guests via QR or front-desk</CardDescription>
            </div>
            <Link to="/guests" className="text-xs text-primary font-medium hover:underline">
              View All
            </Link>
          </CardHeader>
          <CardContent>
            {stats.recentRegistrations.length === 0 ? (
              <p className="text-sm text-muted-foreground py-6 text-center">
                No new guest registrations yet.
              </p>
            ) : (
              <div className="divide-y">
                {stats.recentRegistrations.map((g) => (
                  <div key={g.id} className="py-3 flex items-center justify-between text-sm">
                    <div>
                      <p className="font-medium">{g.fullName}</p>
                      <p className="text-xs text-muted-foreground">
                        {g.mobile} &bull; {g.email || "No email"}
                      </p>
                    </div>
                    <span className="text-xs text-muted-foreground">
                      {g.createdAt ? new Date(g.createdAt).toLocaleDateString() : "-"}
                    </span>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
