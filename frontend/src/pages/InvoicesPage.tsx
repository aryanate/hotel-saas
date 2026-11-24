import { useState } from "react"
import { useQuery } from "@tanstack/react-query"
import { useNavigate } from "react-router-dom"
import { Plus, FileText, Download } from "lucide-react"
import { billingService } from "@/services/billing.service"
import type { Invoice, InvoiceStatus } from "@/types"
import { Button } from "@/components/ui/button"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, type Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { SearchInput } from "@/components/shared/SearchInput"
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select"

export function InvoicesPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState("")
  const [status, setStatus] = useState<string>("all")
  const [page, setPage] = useState(1)

  const { data: res, isLoading } = useQuery({
    queryKey: ["invoices", search, status, page],
    queryFn: () =>
      billingService.getInvoices({
        search: search || undefined,
        status: status === "all" ? undefined : (status as InvoiceStatus),
        page,
        pageSize: 10,
      }),
  })

  const invoices: Invoice[] = (res?.data?.items || [
    {
      id: "inv-1",
      hotelId: "h1",
      bookingId: "b1",
      createdAt: "2026-11-13",
      updatedAt: "2026-11-13",
      invoiceNumber: "INV-2026-0012",
      booking: {
        bookingReference: "BK-9021",
        guest: { fullName: "Vikram Malhotra" },
      },
      subTotal: 10500,
      taxAmount: 1260,
      discountAmount: 500,
      totalAmount: 11260,
      paidAmount: 11260,
      balanceAmount: 0,
      status: "Paid",
      issueDate: "2026-11-13",
    },
    {
      id: "inv-2",
      hotelId: "h1",
      bookingId: "b2",
      createdAt: "2026-11-13",
      updatedAt: "2026-11-13",
      invoiceNumber: "INV-2026-0013",
      booking: {
        bookingReference: "BK-9022",
        guest: { fullName: "Ananya Roy" },
      },
      subTotal: 4500,
      taxAmount: 540,
      discountAmount: 0,
      totalAmount: 5040,
      paidAmount: 2000,
      balanceAmount: 3040,
      status: "PartiallyPaid",
      issueDate: "2026-11-13",
    },
  ]) as Invoice[]

  const totalPages = res?.data?.totalPages || 1

  const columns: Column<Invoice>[] = [
    {
      header: "Invoice #",
      accessorKey: "invoiceNumber",
      cell: (item) => (
        <span className="font-semibold text-primary">{item.invoiceNumber}</span>
      ),
    },
    {
      header: "Guest / Booking",
      cell: (item) => (
        <div>
          <p className="font-medium">{item.booking?.guest?.fullName || "Guest"}</p>
          <p className="text-xs text-muted-foreground">{item.booking?.bookingReference}</p>
        </div>
      ),
    },
    {
      header: "Issue Date",
      accessorKey: "issueDate",
    },
    {
      header: "Total",
      cell: (item) => <span className="font-bold">₹{item.totalAmount.toLocaleString("en-IN")}</span>,
    },
    {
      header: "Balance Due",
      cell: (item) => (
        <span className={item.balanceAmount > 0 ? "font-bold text-destructive" : "text-emerald-600"}>
          ₹{item.balanceAmount.toLocaleString("en-IN")}
        </span>
      ),
    },
    {
      header: "Status",
      cell: (item) => <StatusBadge status={item.status} />,
    },
  ]

  return (
    <div className="space-y-6">
      <PageHeader
        title="Billing & Tax Invoices"
        description="Generate GST invoices, track receipts, and manage customer accounts"
      >
        <Button onClick={() => navigate("/billing/new")}>
          <Plus className="h-4 w-4 mr-1" /> Create Invoice
        </Button>
      </PageHeader>

      <div className="flex flex-col sm:flex-row items-center justify-between gap-4">
        <SearchInput
          value={search}
          onChange={setSearch}
          placeholder="Search by invoice number or guest..."
        />
        <div className="w-full sm:w-auto">
          <Select value={status} onValueChange={setStatus}>
            <SelectTrigger className="w-40">
              <SelectValue placeholder="All Statuses" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Statuses</SelectItem>
              <SelectItem value="Paid">Paid</SelectItem>
              <SelectItem value="PartiallyPaid">Partially Paid</SelectItem>
              <SelectItem value="Unpaid">Unpaid</SelectItem>
              <SelectItem value="Refunded">Refunded</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      <DataTable
        data={invoices}
        columns={columns}
        isLoading={isLoading}
        pageNumber={page}
        totalPages={totalPages}
        onPageChange={setPage}
        onRowClick={(item) => navigate(`/billing/${item.id}`)}
      />
    </div>
  )
}
