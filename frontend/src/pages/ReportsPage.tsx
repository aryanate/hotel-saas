import { useState } from "react"
import { useQuery } from "@tanstack/react-query"
import { Download, FileSpreadsheet } from "lucide-react"
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  LineChart,
  Line,
} from "recharts"
import { reportService } from "@/services/report.service"
import { Button } from "@/components/ui/button"
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"
import { useToast } from "@/components/ui/use-toast"

export function ReportsPage() {
  const { toast } = useToast()
  const now = new Date()
  const [selectedMonth, setSelectedMonth] = useState(String(now.getMonth() + 1))
  const [selectedYear, setSelectedYear] = useState(String(now.getFullYear()))

  const { data: revRes, isLoading: loadingRev } = useQuery({
    queryKey: ["reports-revenue"],
    queryFn: () => reportService.getRevenue(),
  })

  const { data: occRes, isLoading: loadingOcc } = useQuery({
    queryKey: ["reports-occupancy"],
    queryFn: () => reportService.getOccupancy(),
  })

  const { data: gstRes } = useQuery({
    queryKey: ["reports-gst", selectedMonth, selectedYear],
    queryFn: () =>
      reportService.getGstReport({
        month: Number(selectedMonth),
        year: Number(selectedYear),
      }),
  })

  const revenueData = (revRes?.data?.dailyBreakdown || []).map((d) => ({
    name: new Date(d.date).toLocaleDateString("en-IN", { day: "2-digit", month: "short" }),
    revenue: d.amount,
  }))

  const occupancyData = (occRes?.data?.dailyBreakdown || []).slice(-14).map((d) => ({
    date: new Date(d.date).toLocaleDateString("en-IN", { day: "2-digit", month: "short" }),
    occupancyRate: d.occupancyRate,
  }))

  const gstReport = gstRes?.data || {
    month: Number(selectedMonth),
    year: Number(selectedYear),
    totalTaxableAmount: 0,
    totalCgst: 0,
    totalSgst: 0,
    totalIgst: 0,
    totalTax: 0,
    totalInvoices: 0,
    invoices: [],
  }

  const handleDownloadGST = () => {
    const headers = ["Invoice Number", "Date", "Guest Name", "GSTIN", "Taxable Value", "CGST", "SGST", "Total GST", "Total Amount"]
    const rows = gstReport.invoices.map((it) => [
      it.invoiceNumber,
      new Date(it.invoiceDate).toISOString().split("T")[0],
      `"${(it.guestName || "").replace(/"/g, '""')}"`,
      it.guestGstin || "-",
      it.taxableAmount.toFixed(2),
      (it.taxAmount / 2).toFixed(2),
      (it.taxAmount / 2).toFixed(2),
      it.taxAmount.toFixed(2),
      it.totalAmount.toFixed(2),
    ])
    const csv = [headers.join(","), ...rows.map((r) => r.join(","))].join("\n")
    const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" })
    const url = URL.createObjectURL(blob)
    const a = document.createElement("a")
    a.href = url
    a.download = `GST-Report-${selectedYear}-${selectedMonth.padStart(2, "0")}.csv`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
    toast({ title: `GST Report exported (${gstReport.invoices.length} invoices)` })
  }

  if (loadingRev || loadingOcc) {
    return <LoadingSpinner size="lg" className="h-64" />
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Business Reports & Analytics"
        description="Track financial performance, occupancy trends, and generate GST tax summaries"
      >
        <Button onClick={handleDownloadGST}>
          <Download className="h-4 w-4 mr-1" /> Export GST Summary
        </Button>
      </PageHeader>

      {/* Analytics Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card className="shadow-sm">
          <CardHeader>
            <CardTitle className="text-base font-semibold">Daily Revenue Trend (Last 30 Days)</CardTitle>
            <CardDescription>
              Total billed revenue: ₹{(revRes?.data?.totalRevenue ?? 0).toLocaleString("en-IN")}
            </CardDescription>
          </CardHeader>
          <CardContent className="h-[280px]">
            {revenueData.length === 0 ? (
              <div className="h-full flex items-center justify-center text-sm text-muted-foreground">
                No revenue transactions recorded in the last 30 days.
              </div>
            ) : (
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={revenueData}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} />
                  <XAxis dataKey="name" />
                  <YAxis tickFormatter={(val) => `₹${val >= 1000 ? `${(val / 1000).toFixed(0)}k` : val}`} />
                  <Tooltip formatter={(val: unknown) => [`₹${Number(val).toLocaleString("en-IN")}`, "Revenue"]} />
                  <Bar dataKey="revenue" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            )}
          </CardContent>
        </Card>

        <Card className="shadow-sm">
          <CardHeader>
            <CardTitle className="text-base font-semibold">Occupancy Rate (%)</CardTitle>
            <CardDescription>
              Average occupancy: {occRes?.data?.averageOccupancyRate ?? 0}% across {occRes?.data?.totalRooms ?? 0} rooms
            </CardDescription>
          </CardHeader>
          <CardContent className="h-[280px]">
            {occupancyData.length === 0 ? (
              <div className="h-full flex items-center justify-center text-sm text-muted-foreground">
                No occupancy history yet.
              </div>
            ) : (
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={occupancyData}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} />
                  <XAxis dataKey="date" />
                  <YAxis domain={[0, 100]} tickFormatter={(val) => `${val}%`} />
                  <Tooltip formatter={(val: unknown) => [`${val}%`, "Occupancy"]} />
                  <Line
                    type="monotone"
                    dataKey="occupancyRate"
                    stroke="#10b981"
                    strokeWidth={2}
                    dot={{ r: 4 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            )}
          </CardContent>
        </Card>
      </div>

      {/* GST Compliance Section */}
      <Card className="shadow-sm">
        <CardHeader className="flex flex-col sm:flex-row sm:items-center sm:justify-between pb-4 border-b">
          <div>
            <CardTitle className="text-base font-semibold flex items-center gap-2">
              <FileSpreadsheet className="h-5 w-5 text-primary" /> GST Compliance Report (GSTR-1 Ready)
            </CardTitle>
            <CardDescription>Tax breakdowns for monthly filing</CardDescription>
          </div>
          <div className="flex items-center gap-2 mt-3 sm:mt-0">
            <select
              value={selectedMonth}
              onChange={(e) => setSelectedMonth(e.target.value)}
              className="text-xs p-2 rounded-md border bg-background"
            >
              {["January","February","March","April","May","June","July","August","September","October","November","December"].map((m, idx) => (
                <option key={m} value={String(idx + 1)}>{m}</option>
              ))}
            </select>
            <select
              value={selectedYear}
              onChange={(e) => setSelectedYear(e.target.value)}
              className="text-xs p-2 rounded-md border bg-background"
            >
              <option value="2026">2026</option>
              <option value="2025">2025</option>
            </select>
            <Button size="sm" variant="outline" onClick={handleDownloadGST}>
              Download CSV
            </Button>
          </div>
        </CardHeader>
        <CardContent className="pt-6 space-y-6">
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
            <div className="p-3 rounded-lg border bg-slate-50">
              <span className="text-xs text-muted-foreground">Total Taxable Value</span>
              <p className="text-lg font-bold">₹{(gstReport.totalTaxableAmount ?? 0).toLocaleString("en-IN")}</p>
            </div>
            <div className="p-3 rounded-lg border bg-slate-50">
              <span className="text-xs text-muted-foreground">CGST (50%)</span>
              <p className="text-lg font-bold">₹{(gstReport.totalCgst ?? 0).toLocaleString("en-IN")}</p>
            </div>
            <div className="p-3 rounded-lg border bg-slate-50">
              <span className="text-xs text-muted-foreground">SGST (50%)</span>
              <p className="text-lg font-bold">₹{(gstReport.totalSgst ?? 0).toLocaleString("en-IN")}</p>
            </div>
            <div className="p-3 rounded-lg border bg-slate-50">
              <span className="text-xs text-muted-foreground">Total GST Collected</span>
              <p className="text-lg font-bold text-emerald-600">
                ₹{(gstReport.totalTax ?? 0).toLocaleString("en-IN")}
              </p>
            </div>
          </div>

          <div className="overflow-x-auto">
            {gstReport.invoices.length === 0 ? (
              <p className="text-sm text-muted-foreground text-center py-6">
                No GST invoices recorded for {selectedMonth}/{selectedYear}.
              </p>
            ) : (
              <table className="w-full text-xs">
                <thead>
                  <tr className="border-b text-muted-foreground text-left">
                    <th className="py-2">Invoice #</th>
                    <th className="py-2">Date</th>
                    <th className="py-2">Customer</th>
                    <th className="py-2">Customer GSTIN</th>
                    <th className="py-2 text-right">Taxable</th>
                    <th className="py-2 text-right">CGST</th>
                    <th className="py-2 text-right">SGST</th>
                    <th className="py-2 text-right">Total Invoice</th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {gstReport.invoices.map((it) => {
                    const halfTax = Math.round((it.taxAmount / 2) * 100) / 100
                    return (
                      <tr key={it.invoiceNumber}>
                        <td className="py-2.5 font-semibold text-primary">{it.invoiceNumber}</td>
                        <td className="py-2.5">{new Date(it.invoiceDate).toLocaleDateString("en-IN")}</td>
                        <td className="py-2.5">{it.guestName}</td>
                        <td className="py-2.5 font-mono">{it.guestGstin || "-"}</td>
                        <td className="py-2.5 text-right">₹{it.taxableAmount.toLocaleString("en-IN")}</td>
                        <td className="py-2.5 text-right">₹{halfTax.toLocaleString("en-IN")}</td>
                        <td className="py-2.5 text-right">₹{halfTax.toLocaleString("en-IN")}</td>
                        <td className="py-2.5 text-right font-bold">
                          ₹{it.totalAmount.toLocaleString("en-IN")}
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
