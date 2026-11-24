import { Routes, Route, Navigate } from "react-router-dom"
import { useAuth } from "@/hooks/useAuth"
import { DashboardLayout } from "@/layouts/DashboardLayout"
import { PublicLayout } from "@/layouts/PublicLayout"
import { Toaster } from "@/components/ui/toaster"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"

// Auth Pages
import { LoginPage } from "@/pages/LoginPage"
import { RegisterPage } from "@/pages/RegisterPage"

// Dashboard
import { DashboardPage } from "@/pages/DashboardPage"

// Rooms
import { RoomsPage } from "@/pages/RoomsPage"
import { RoomDetailPage } from "@/pages/RoomDetailPage"
import { RoomTypesPage } from "@/pages/RoomTypesPage"

// Bookings
import { BookingsPage } from "@/pages/BookingsPage"
import { BookingDetailPage } from "@/pages/BookingDetailPage"
import { CreateBookingPage } from "@/pages/CreateBookingPage"

// Guests
import { GuestManagementPage } from "@/pages/GuestManagementPage"
import { GuestDetailPage } from "@/pages/GuestDetailPage"
import { CheckInPendingPage } from "@/pages/CheckInPendingPage"

// Digital Check-in
import { QRCodePage } from "@/pages/QRCodePage"
import { PublicCheckInPage } from "@/pages/PublicCheckInPage"

// Billing
import { InvoicesPage } from "@/pages/InvoicesPage"
import { InvoiceDetailPage } from "@/pages/InvoiceDetailPage"
import { CreateInvoicePage } from "@/pages/CreateInvoicePage"

// Favorites & Campaigns
import { FavoriteGuestsPage } from "@/pages/FavoriteGuestsPage"
import { CampaignsPage } from "@/pages/CampaignsPage"
import { CreateCampaignPage } from "@/pages/CreateCampaignPage"
import { CampaignDetailPage } from "@/pages/CampaignDetailPage"

// Reports, Staff, Settings, Subscription
import { ReportsPage } from "@/pages/ReportsPage"
import { StaffPage } from "@/pages/StaffPage"
import { HotelSettingsPage } from "@/pages/HotelSettingsPage"
import { SubscriptionPage } from "@/pages/SubscriptionPage"

// Super Admin
import { AdminDashboardPage } from "@/pages/AdminDashboardPage"
import { AdminHotelsPage } from "@/pages/AdminHotelsPage"
import { AdminSubscriptionPlansPage } from "@/pages/AdminSubscriptionPlansPage"

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth()

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <LoadingSpinner size="lg" />
      </div>
    )
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  return <>{children}</>
}

export default function App() {
  return (
    <>
      <Routes>
        {/* Public Guest Routes */}
        <Route element={<PublicLayout />}>
          <Route path="/checkin/:token" element={<PublicCheckInPage />} />
        </Route>

        {/* Public Auth Routes */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        {/* Protected Dashboard Routes */}
        <Route
          element={
            <ProtectedRoute>
              <DashboardLayout />
            </ProtectedRoute>
          }
        >
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="/dashboard" element={<DashboardPage />} />

          {/* Rooms */}
          <Route path="/rooms" element={<RoomsPage />} />
          <Route path="/rooms/types" element={<RoomTypesPage />} />
          <Route path="/rooms/:id" element={<RoomDetailPage />} />

          {/* Bookings */}
          <Route path="/bookings" element={<BookingsPage />} />
          <Route path="/bookings/new" element={<CreateBookingPage />} />
          <Route path="/bookings/:id" element={<BookingDetailPage />} />

          {/* Guests & Digital Queue */}
          <Route path="/guests" element={<GuestManagementPage />} />
          <Route path="/guests/:id" element={<GuestDetailPage />} />
          <Route path="/checkin" element={<CheckInPendingPage />} />

          {/* Billing & Invoices */}
          <Route path="/billing" element={<InvoicesPage />} />
          <Route path="/billing/new" element={<CreateInvoicePage />} />
          <Route path="/billing/:id" element={<InvoiceDetailPage />} />

          {/* VIP Favorites & Campaigns */}
          <Route path="/favorites" element={<FavoriteGuestsPage />} />
          <Route path="/campaigns" element={<CampaignsPage />} />
          <Route path="/campaigns/new" element={<CreateCampaignPage />} />
          <Route path="/campaigns/:id" element={<CampaignDetailPage />} />

          {/* Analytics, Staff, Settings */}
          <Route path="/reports" element={<ReportsPage />} />
          <Route path="/qr-code" element={<QRCodePage />} />
          <Route path="/staff" element={<StaffPage />} />
          <Route path="/settings" element={<HotelSettingsPage />} />
          <Route path="/subscription" element={<SubscriptionPage />} />

          {/* Admin panel routes */}
          <Route path="/admin/dashboard" element={<AdminDashboardPage />} />
          <Route path="/admin/hotels" element={<AdminHotelsPage />} />
          <Route path="/admin/plans" element={<AdminSubscriptionPlansPage />} />
        </Route>

        {/* Catch-all fallback */}
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>

      <Toaster />
    </>
  )
}
