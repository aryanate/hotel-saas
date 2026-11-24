import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { Copy, Download, Check, ExternalLink, Printer, Database, Sparkles, Wifi, Smartphone, Monitor } from "lucide-react"
import { useState } from "react"
import { hotelService } from "@/services/hotel.service"
import { useCurrentHotel } from "@/hooks/useCurrentHotel"
import { Button } from "@/components/ui/button"
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"
import { useToast } from "@/components/ui/use-toast"

export function QRCodePage() {
  const { toast } = useToast()
  const { hotel, refetch: refetchHotel } = useCurrentHotel()
  const queryClient = useQueryClient()
  const [copied, setCopied] = useState(false)
  const [customHost, setCustomHost] = useState<string | null>(null)

  // 1. Fetch QR info from server
  const { data: res, isLoading } = useQuery({
    queryKey: ["hotel-qrcode", customHost],
    queryFn: () => hotelService.getQrCode(customHost || undefined),
  })

  // 2. Discover local IP dynamically
  const detectedIp = res?.data?.localIp && res.data.localIp !== "localhost" ? res.data.localIp : ""
  const isDirectIp = typeof window !== "undefined" && window.location.hostname !== "localhost" && window.location.hostname !== "127.0.0.1"

  const defaultWifiHost = isDirectIp
    ? window.location.origin
    : (detectedIp ? `http://${detectedIp}:5173` : window.location.origin)

  const activeHost = customHost || defaultWifiHost
  const publicToken = hotel?.publicToken || res?.data?.publicToken || ""
  const checkInUrl = publicToken ? `${activeHost.trim().replace(/\/$/, "")}/checkin/${publicToken}` : (res?.data?.checkInUrl || "")
  const qrImageSrc = res?.data?.qrCodeBase64 || ""

  const seedMutation = useMutation({
    mutationFn: () => hotelService.seedDemoData(),
    onSuccess: () => {
      toast({
        title: "Demo Data Seeded!",
        description: "25 rooms across 4 floors, 8 Indian guests, 10 bookings, and invoices have been populated.",
      })
      queryClient.invalidateQueries()
      refetchHotel()
    },
    onError: () => {
      toast({
        title: "Seed Failed",
        description: "Could not seed demo data. Please try again.",
        variant: "destructive",
      })
    },
  })

  const copyToClipboard = () => {
    if (!checkInUrl) return
    navigator.clipboard.writeText(checkInUrl)
    setCopied(true)
    toast({ title: "Check-in URL copied to clipboard" })
    setTimeout(() => setCopied(false), 2000)
  }

  const downloadQr = () => {
    if (!qrImageSrc) return
    const a = document.createElement("a")
    a.href = qrImageSrc
    a.download = `${(hotel?.name || "hotel").toLowerCase().replace(/\s+/g, "-")}-qr.png`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    toast({ title: "QR Code image downloaded" })
  }

  if (isLoading) {
    return <LoadingSpinner size="lg" className="h-64" />
  }

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      {/* Print-specific style tags */}
      <style>{`
        @media print {
          body {
            background: white !important;
            margin: 0 !important;
            padding: 0 !important;
          }
          aside, header, nav, footer, .no-print, button, input {
            display: none !important;
          }
          #printable-standee {
            display: block !important;
            position: absolute !important;
            left: 50% !important;
            top: 50% !important;
            transform: translate(-50%, -50%) !important;
            width: 90% !important;
            max-width: 480px !important;
            margin: 0 auto !important;
            padding: 32px !important;
            border: 3px solid #0f172a !important;
            border-radius: 20px !important;
            box-shadow: none !important;
            background: white !important;
            text-align: center !important;
          }
          #printable-standee img {
            width: 260px !important;
            height: 260px !important;
            margin: 16px auto !important;
            display: block !important;
          }
        }
      `}</style>

      <div className="no-print">
        <PageHeader
          title="Front-Desk QR Code"
          description="Print and place this QR code at your reception counter for instant contactless guest check-ins"
        >
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => seedMutation.mutate()}
              disabled={seedMutation.isPending}
              className="gap-2 border-primary/30 hover:bg-primary/5 text-primary"
            >
              <Database className="h-4 w-4" />
              {seedMutation.isPending ? "Seeding..." : "Seed Demo Rooms & Data"}
            </Button>
          </div>
        </PageHeader>
      </div>

      {/* Mobile Wi-Fi Notice Banner */}
      <div className="no-print p-4 rounded-xl bg-blue-50 border border-blue-200 text-blue-900 flex items-start gap-3 shadow-sm">
        <Wifi className="h-5 w-5 text-blue-600 shrink-0 mt-0.5" />
        <div className="text-sm space-y-1">
          <p className="font-semibold text-blue-950">
            For Mobile Phone Scanning (Same Wi-Fi Network):
          </p>
          <p className="text-xs text-blue-800 leading-relaxed">
            Ensure your mobile phone is connected to your local Wi-Fi. The QR code below is configured to open{" "}
            <code className="bg-blue-100/80 px-1.5 py-0.5 rounded font-mono font-semibold text-blue-900">
              {checkInUrl}
            </code>
            .
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-8 items-start">
        {/* Printable Counter Standee */}
        <Card
          id="printable-standee"
          className="shadow-lg border-2 border-primary/20 text-center p-6 bg-white rounded-2xl relative overflow-hidden"
        >
          <div className="space-y-4">
            <div className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-primary/10 text-primary text-xs font-semibold tracking-wide uppercase">
              <Sparkles className="h-3.5 w-3.5" /> Contactless Check-In
            </div>

            <div>
              <h3 className="text-2xl font-bold text-slate-900 tracking-tight">
                {hotel?.name || "Grand Palace Hotel"}
              </h3>
              <p className="text-xs text-slate-500 font-medium mt-1">
                {hotel?.city ? `${hotel.city}, ${hotel.state || "India"}` : "Welcome to our Hotel"}
              </p>
            </div>

            {/* QR Code Container */}
            <div className="mx-auto w-64 h-64 border-4 border-slate-900 p-3 rounded-2xl bg-white flex flex-col items-center justify-center shadow-inner">
              {qrImageSrc ? (
                <img
                  src={qrImageSrc}
                  alt="Contactless Registration QR Code"
                  className="w-full h-full object-contain"
                />
              ) : (
                <div className="flex flex-col items-center justify-center text-slate-400 gap-2">
                  <LoadingSpinner size="md" />
                  <span className="text-xs font-mono">Generating QR...</span>
                </div>
              )}
            </div>

            <div className="space-y-1">
              <p className="text-sm font-semibold text-slate-800">
                Scan with any Smartphone Camera
              </p>
              <p className="text-xs text-slate-500">
                Fill details & upload Photo ID directly from your mobile
              </p>
            </div>

            <div className="pt-2 no-print flex items-center justify-center gap-3">
              <Button onClick={() => window.print()} className="gap-2 shadow-sm">
                <Printer className="h-4 w-4" /> Print Standee
              </Button>
              <Button variant="outline" onClick={downloadQr} className="gap-2">
                <Download className="h-4 w-4" /> Download QR
              </Button>
            </div>
          </div>
        </Card>

        {/* Link sharing and network configuration */}
        <div className="space-y-6 no-print">
          {/* Target Host Switcher */}
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-base font-semibold">QR Code Target Network</CardTitle>
              <CardDescription>
                Choose which address is encoded into the QR code.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="grid grid-cols-2 gap-2">
                <button
                  type="button"
                  onClick={() => setCustomHost(defaultWifiHost)}
                  className={`flex flex-col items-center justify-center p-3 rounded-xl border text-center transition-all ${
                    !customHost || customHost === defaultWifiHost
                      ? "border-primary bg-primary/10 text-primary font-semibold ring-2 ring-primary/20"
                      : "border-slate-200 hover:border-slate-300 text-slate-600"
                  }`}
                >
                  <Smartphone className="h-5 w-5 mb-1 text-primary" />
                  <span className="text-xs font-semibold">Wi-Fi (Mobile Scan)</span>
                  <span className="text-[10px] text-muted-foreground mt-0.5 truncate max-w-[130px]">
                    {defaultWifiHost.replace(/^https?:\/\//, "")}
                  </span>
                </button>

                <button
                  type="button"
                  onClick={() => setCustomHost("http://localhost:5173")}
                  className={`flex flex-col items-center justify-center p-3 rounded-xl border text-center transition-all ${
                    customHost === "http://localhost:5173"
                      ? "border-primary bg-primary/10 text-primary font-semibold ring-2 ring-primary/20"
                      : "border-slate-200 hover:border-slate-300 text-slate-600"
                  }`}
                >
                  <Monitor className="h-5 w-5 mb-1" />
                  <span className="text-xs font-semibold">Localhost (This PC)</span>
                  <span className="text-[10px] text-muted-foreground mt-0.5">localhost:5173</span>
                </button>
              </div>

              <div className="pt-2">
                <label className="text-xs font-medium text-slate-600 block mb-1">
                  Custom Host / Domain:
                </label>
                <input
                  type="text"
                  value={activeHost}
                  onChange={(e) => setCustomHost(e.target.value)}
                  className="w-full text-xs font-mono bg-muted p-2 rounded-md border text-foreground"
                  placeholder="e.g. http://192.168.1.109:5173"
                />
              </div>
            </CardContent>
          </Card>

          {/* Direct URL Sharing */}
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-base font-semibold">Direct Registration URL</CardTitle>
              <CardDescription>
                Share directly with incoming guests via WhatsApp or SMS prior to their arrival.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center gap-2">
                <input
                  readOnly
                  value={checkInUrl}
                  className="w-full text-xs font-mono bg-muted p-2.5 rounded-md border text-muted-foreground select-all"
                />
                <Button size="icon" variant="outline" onClick={copyToClipboard}>
                  {copied ? <Check className="h-4 w-4 text-emerald-600" /> : <Copy className="h-4 w-4" />}
                </Button>
              </div>

              <div className="pt-2 flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  className="w-full"
                  onClick={() => window.open(checkInUrl, "_blank")}
                >
                  <ExternalLink className="h-4 w-4 mr-2" /> Open Mobile Form in New Tab
                </Button>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-base font-semibold">How Digital Check-In Works</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3 text-sm text-muted-foreground">
              <div className="flex items-start gap-3">
                <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">
                  1
                </span>
                <p>Guest scans the counter standee QR with their smartphone camera.</p>
              </div>
              <div className="flex items-start gap-3">
                <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">
                  2
                </span>
                <p>They fill their name, mobile, address, and Aadhaar/Govt ID number on mobile.</p>
              </div>
              <div className="flex items-start gap-3">
                <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">
                  3
                </span>
                <p>Receptionist sees the entry instantly in the <strong>Check-in Queue</strong> and clicks Approve to assign a key.</p>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
