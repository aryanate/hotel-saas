import { Outlet } from "react-router-dom"
import { Building2 } from "lucide-react"

export function PublicLayout() {
  return (
    <div className="min-h-screen bg-slate-50 flex flex-col justify-between">
      <header className="border-b bg-white py-4 px-6 shadow-sm">
        <div className="max-w-4xl mx-auto flex items-center justify-between">
          <div className="flex items-center gap-2">
            <div className="h-8 w-8 rounded-md bg-primary text-primary-foreground flex items-center justify-center font-bold">
              <Building2 className="h-5 w-5" />
            </div>
            <span className="font-semibold text-lg tracking-tight text-foreground">
              Hotel Self Check-In
            </span>
          </div>
          <span className="text-xs text-muted-foreground hidden sm:inline">
            Secure Digital Registration
          </span>
        </div>
      </header>

      <main className="flex-1 max-w-4xl w-full mx-auto p-4 sm:p-6 md:p-8">
        <Outlet />
      </main>

      <footer className="border-t bg-white py-4 text-center text-xs text-muted-foreground">
        Powered by HotelSaaS &bull; Privacy Protected &bull; ISO 27001 Compliant
      </footer>
    </div>
  )
}
