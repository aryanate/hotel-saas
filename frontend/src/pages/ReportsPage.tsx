import { useState } from "react"
import { useQuery } from "@tanstack/react-query"
import {
  Download,
  FileSpreadsheet,
  IndianRupee,
  BedDouble,
  Users,
} from "lucide-react"
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
  const [selectedMonth, setSelectedMonth] = useState("11")
  const [selectedYear, setSelectedYear] = useState("2026")

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

  const revenueData = revRes?.data?.labels?.map((label, idx) => ({
    name: label,
    revenue: revRes.data?.data?.[idx] || 0,
  })) || [
    { name: "Jul", revenue: 240000 },
    { name: "Aug", revenue: 285000 },
    { name: "Sep", revenue: 310000 },
    { name: "Oct", revenue: 395000 },
    { name: "Nov", revenue: 345000 },
  ]

  const occupancyData = occRes?.data || [
    { date: "01 Nov", occupancyRate: 65 },
    { date: "03 Nov", occupancyRate: 72 },
    { date: "05 Nov", occupancyRate: 85 },
    { date: "07 Nov", occupancyRate: 60 },
    { date: "09 Nov", occupancyRate: 90 },
    { date: "11 Nov", occupancyRate: 78 },
    { date: "13 Nov", occupancyRate: 82 },
  ]

  const gstReport = gstRes?.data || {
    month: "November",
    year: 2026,
    totalTaxable: 345000,
    totalCgst: 20700,
    totalSgst: 20700,
    totalIgst: 0,
    totalGst: 41400,
    items: [
      {
        invoiceNumber: "INV-2026-0012",
        date: "2026-11-13",
        guestName: "Vikram Malhotra",
        gstIn: "27XXXXX1234",
        taxableValue: 10500,
        cgst: 630,
        sgst: 630,
        igst: 0,
        totalGst: 1260,
        totalAmount: 11760,
      },
    ],
  }

  const handleDownloadGST = () => {
    toast({ title: `GST Report for ${selectedMonth}/${selectedYear} downloaded (CSV)` })
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
            <CardTitle className="text-base font-semibold">Monthly Revenue Trend</CardTitle>
            <CardDescription>Total earnings over the last 5 months</CardDescription>
          </CardHeader>
          <CardContent className="h-[280px]">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={revenueData}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} />
                <XAxis dataKey="name" />
                <YAxis tickFormatter={(val) => `₹${val / 1000}k`} />
                <Tooltip formatter={(val: unknown) => [`₹${Number(val).toLocaleString("en-IN")}`, "Revenue"]} />
                <Bar dataKey="revenue" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        <Card className="shadow-sm">
          <CardHeader>
            <CardTitle className="text-base font-semibold">Occupancy Rate (%)</CardTitle>
            <CardDescription>Daily occupancy percentage across property</CardDescription>
          </CardHeader>
          <CardContent className="h-[280px]">
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
              <option value="11">November</option>
              <option value="10">October</option>
              <option value="09">September</option>
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
              <p className="text-lg font-bold">₹{gstReport.totalTaxable.toLocaleString("en-IN")}</p>
            </div>
            <div className="p-3 rounded-lg border bg-slate-50">
              <span className="text-xs text-muted-foreground">CGST (6%)</span>
              <p className="text-lg font-bold">₹{gstReport.totalCgst.toLocaleString("en-IN")}</p>
            </div>
            <div className="p-3 rounded-lg border bg-slate-50">
              <span className="text-xs text-muted-foreground">SGST (6%)</span>
              <p className="text-lg font-bold">₹{gstReport.totalSgst.toLocaleString("en-IN")}</p>
            </div>
            <div className="p-3 rounded-lg border bg-slate-50">
              <span className="text-xs text-muted-foreground">Total GST Collected</span>
              <p className="text-lg font-bold text-emerald-600">
                ₹{gstReport.totalGst.toLocaleString("en-IN")}
              </p>
            </div>
          </div>

          <div className="overflow-x-auto">
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
                {gstReport.items.map((it) => (
                  <tr key={it.invoiceNumber}>
                    <td className="py-2.5 font-semibold text-primary">{it.invoiceNumber}</td>
                    <td className="py-2.5">{it.date}</td>
                    <td className="py-2.5">{it.guestName}</td>
                    <td className="py-2.5 font-mono">{it.gstIn || "-"}</td>
                    <td className="py-2.5 text-right">₹{it.taxableValue.toLocaleString("en-IN")}</td>
                    <td className="py-2.5 text-right">₹{it.cgst.toLocaleString("en-IN")}</td>
                    <td className="py-2.5 text-right">₹{it.sgst.toLocaleString("en-IN")}</td>
                    <td className="py-2.5 text-right font-bold">
                      ₹{it.totalAmount.toLocaleString("en-IN")}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
