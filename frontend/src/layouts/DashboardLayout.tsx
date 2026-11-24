import * as React from "react"
import { NavLink, Outlet, useLocation } from "react-router-dom"
import {
  LayoutDashboard,
  BedDouble,
  CalendarCheck2,
  Users,
  QrCode,
  FileText,
  Star,
  Send,
  BarChart3,
  UserCheck,
  UserCog,
  Settings,
  CreditCard,
  Menu,
  X,
  Bell,
  LogOut,
  Building2,
  ShieldAlert,
} from "lucide-react"
import { useAuth } from "@/hooks/useAuth"
import { useCurrentHotel } from "@/hooks/useCurrentHotel"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"

const navigation = [
  { name: "Dashboard", href: "/dashboard", icon: LayoutDashboard },
  { name: "Rooms", href: "/rooms", icon: BedDouble },
  { name: "Bookings", href: "/bookings", icon: CalendarCheck2 },
  { name: "Guests", href: "/guests", icon: Users },
  { name: "Check-in Queue", href: "/checkin", icon: UserCheck },
  { name: "Billing & Invoices", href: "/billing", icon: FileText },
  { name: "Favorite Guests", href: "/favorites", icon: Star },
  { name: "Campaigns", href: "/campaigns", icon: Send },
  { name: "Reports", href: "/reports", icon: BarChart3 },
  { name: "QR Code", href: "/qr-code", icon: QrCode },
  { name: "Staff", href: "/staff", icon: UserCog },
  { name: "Settings", href: "/settings", icon: Settings },
  { name: "Subscription", href: "/subscription", icon: CreditCard },
]

export function DashboardLayout() {
  const { user, logout } = useAuth()
  const { hotel } = useCurrentHotel()
  const [sidebarOpen, setSidebarOpen] = React.useState(false)
  const location = useLocation()

  // Close mobile sidebar on route change
  React.useEffect(() => {
    setSidebarOpen(false)
  }, [location.pathname])

  const userInitials =
    user?.firstName
      ? `${user.firstName[0]}${user.lastName?.[0] || ""}`.toUpperCase()
      : user?.email
      ? user.email.slice(0, 2).toUpperCase()
      : "HT"

  return (
    <div className="min-h-screen bg-slate-50/50 flex flex-col md:flex-row">
      {/* Mobile backdrop */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 z-40 bg-black/40 backdrop-blur-sm md:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      <aside
        className={cn(
          "fixed top-0 bottom-0 left-0 z-50 w-64 bg-card border-r flex flex-col transition-transform duration-200 ease-in-out md:static md:translate-x-0",
          sidebarOpen ? "translate-x-0" : "-translate-x-full"
        )}
      >
        {/* Brand header */}
        <div className="h-16 flex items-center justify-between px-6 border-b">
          <div className="flex items-center gap-3">
            {hotel?.logoUrl ? (
              <img
                src={hotel.logoUrl}
                alt="Logo"
                className="h-8 w-8 rounded-md object-contain border"
              />
            ) : (
              <div className="h-8 w-8 rounded-md bg-primary text-primary-foreground flex items-center justify-center font-bold">
                <Building2 className="h-5 w-5" />
              </div>
            )}
            <div className="overflow-hidden">
              <h2 className="text-sm font-semibold truncate text-foreground leading-tight">
                {hotel?.name || "HotelSaaS"}
              </h2>
              <p className="text-xs text-muted-foreground truncate">
                {hotel?.city ? `${hotel.city}${hotel.state ? `, ${hotel.state}` : ""}` : "Management Portal"}
              </p>
            </div>
          </div>
          <button
            className="md:hidden text-muted-foreground hover:text-foreground"
            onClick={() => setSidebarOpen(false)}
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        {/* Navigation list */}
        <div className="flex-1 overflow-y-auto py-4 px-3 space-y-1">
          {navigation.map((item) => (
            <NavLink
              key={item.name}
              to={item.href}
              className={({ isActive }) =>
                cn(
                  "flex items-center gap-3 px-3 py-2 text-sm font-medium rounded-md transition-colors",
                  isActive
                    ? "bg-primary text-primary-foreground shadow-sm"
                    : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                )
              }
            >
              <item.icon className="h-4 w-4 shrink-0" />
              <span>{item.name}</span>
            </NavLink>
          ))}

          {/* Super Admin Section if role */}
          <div className="pt-4 mt-4 border-t px-1">
            <p className="px-2 mb-2 text-xs font-semibold text-muted-foreground tracking-wider uppercase">
              Super Admin
            </p>
            <NavLink
              to="/admin/dashboard"
              className={({ isActive }) =>
                cn(
                  "flex items-center gap-3 px-3 py-2 text-sm font-medium rounded-md transition-colors",
                  isActive
                    ? "bg-primary text-primary-foreground shadow-sm"
                    : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                )
              }
            >
              <ShieldAlert className="h-4 w-4 shrink-0" />
              <span>Admin Panel</span>
            </NavLink>
          </div>
        </div>

        {/* User bar at bottom */}
        <div className="p-4 border-t flex items-center justify-between">
          <div className="flex items-center gap-3 min-w-0">
            <Avatar className="h-9 w-9">
              <AvatarFallback>{userInitials}</AvatarFallback>
            </Avatar>
            <div className="min-w-0">
              <p className="text-sm font-medium truncate leading-tight">
                {user ? `${user.firstName || ""} ${user.lastName || ""}`.trim() || "Staff Member" : "Staff Member"}
              </p>
              <p className="text-xs text-muted-foreground truncate">
                {user?.email || "staff@hotel.com"}
              </p>
            </div>
          </div>
          <Button
            variant="ghost"
            size="icon"
            onClick={logout}
            title="Log out"
            className="text-muted-foreground hover:text-destructive"
          >
            <LogOut className="h-4 w-4" />
          </Button>
        </div>
      </aside>

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col min-w-0 overflow-hidden">
        {/* Top Navbar */}
        <header className="h-16 bg-card border-b flex items-center justify-between px-4 sm:px-6 z-30 sticky top-0">
          <div className="flex items-center gap-4">
            <Button
              variant="ghost"
              size="icon"
              className="md:hidden"
              onClick={() => setSidebarOpen(true)}
            >
              <Menu className="h-5 w-5" />
            </Button>
            <div className="hidden sm:block">
              <span className="text-sm font-semibold text-foreground">
                {hotel?.name || "Hotel Management Cloud"}
              </span>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <Button variant="ghost" size="icon" className="relative text-muted-foreground">
              <Bell className="h-5 w-5" />
              <span className="absolute top-2 right-2 h-2 w-2 rounded-full bg-primary" />
            </Button>

            <DropdownMenu>
              <DropdownMenuTrigger className="flex items-center gap-2 outline-none">
                <Avatar className="h-8 w-8 cursor-pointer">
                  <AvatarImage src="" />
                  <AvatarFallback className="text-xs">{userInitials}</AvatarFallback>
                </Avatar>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-56">
                <DropdownMenuLabel>
                  <p className="text-sm font-medium">
                    {user ? `${user.firstName || ""} ${user.lastName || ""}`.trim() || "User" : "User"}
                  </p>
                  <p className="text-xs text-muted-foreground truncate">{user?.email}</p>
                </DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={() => (window.location.href = "/settings")}>
                  Settings
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => (window.location.href = "/subscription")}>
                  Subscription
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={logout} className="text-destructive font-medium">
                  Log out
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </header>

        {/* Outlet for dynamic page content */}
        <main className="flex-1 overflow-y-auto p-4 sm:p-6 lg:p-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
