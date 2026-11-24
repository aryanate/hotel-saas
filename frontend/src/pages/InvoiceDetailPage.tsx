import { useState } from "react"
import { useParams, useNavigate } from "react-router-dom"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { Download, Send, CreditCard, Building2, MessageSquare } from "lucide-react"
import { billingService } from "@/services/billing.service"
import { useCurrentHotel } from "@/hooks/useCurrentHotel"
import type { PaymentMethod } from "@/types"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog"
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select"
import { useToast } from "@/components/ui/use-toast"

export function InvoiceDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { toast } = useToast()
  const queryClient = useQueryClient()
  const { hotel } = useCurrentHotel()

  const [paymentModalOpen, setPaymentModalOpen] = useState(false)
  const [paymentAmount, setPaymentAmount] = useState<number>(0)
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>("UPI")
  const [paymentRef, setPaymentRef] = useState("")

  const { data: res, isLoading } = useQuery({
    queryKey: ["invoice", id],
    queryFn: () => billingService.getInvoiceById(id!),
    enabled: !!id,
  })

  const invoice = res?.data

  const recordPaymentMutation = useMutation({
    mutationFn: () =>
      billingService.recordPayment(id!, {
        amount: paymentAmount,
        paymentMethod,
        transactionReference: paymentRef,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["invoice", id] })
      queryClient.invalidateQueries({ queryKey: ["invoices"] })
      queryClient.invalidateQueries({ queryKey: ["dashboard-stats"] })
      toast({ title: "Payment recorded successfully" })
      setPaymentModalOpen(false)
      setPaymentRef("")
    },
    onError: (err: unknown) => {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
      toast({ title: msg || "Failed to record payment", variant: "destructive" })
    },
  })

  const sendWhatsAppMutation = useMutation({
    mutationFn: () => billingService.sendWhatsApp(id!),
    onSuccess: () => {
      toast({ title: "Invoice PDF sent via WhatsApp" })
    },
  })

  const sendEmailMutation = useMutation({
    mutationFn: () => billingService.sendEmail(id!),
    onSuccess: () => {
      toast({ title: "Invoice emailed to guest" })
    },
  })

  if (isLoading) {
    return <LoadingSpinner size="lg" className="h-64" />
  }

  if (!invoice) {
    return (
      <div className="text-center py-16 space-y-4">
        <p className="text-lg font-semibold">Invoice not found</p>
        <Button variant="outline" onClick={() => navigate("/billing")}>
          Back to Billing
        </Button>
      </div>
    )
  }

  const paid = invoice.amountPaid ?? invoice.paidAmount ?? 0
  const balance = invoice.outstandingBalance ?? Math.max(0, (invoice.totalAmount ?? 0) - paid)
  const issueDateStr = invoice.invoiceDate
    ? new Date(invoice.invoiceDate).toLocaleDateString("en-IN", { day: "2-digit", month: "short", year: "numeric" })
    : invoice.issueDate || "-"
  const displayGuestName = invoice.guestName || invoice.booking?.guestName || "Guest"
  const displayGuestMobile = invoice.guestMobile || invoice.booking?.guestMobile || "-"

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <PageHeader
        title={`Invoice #${invoice.invoiceNumber}`}
        breadcrumbs={[
          { label: "Billing", href: "/billing" },
          { label: invoice.invoiceNumber },
        ]}
      >
        <div className="flex flex-wrap items-center gap-2">
          {balance > 0 && (
            <Button
              onClick={() => {
                setPaymentAmount(balance)
                setPaymentModalOpen(true)
              }}
            >
              <CreditCard className="h-4 w-4 mr-1" /> Record Payment
            </Button>
          )}

          <Button
            variant="outline"
            onClick={() => sendWhatsAppMutation.mutate()}
            disabled={sendWhatsAppMutation.isPending}
          >
            <MessageSquare className="h-4 w-4 mr-1 text-emerald-600" /> WhatsApp PDF
          </Button>

          <Button
            variant="outline"
            onClick={() => sendEmailMutation.mutate()}
            disabled={sendEmailMutation.isPending}
          >
            <Send className="h-4 w-4 mr-1" /> Email
          </Button>

          <Button variant="outline" onClick={() => window.print()}>
            <Download className="h-4 w-4 mr-1" /> Print / Download
          </Button>
        </div>
      </PageHeader>

      {/* Invoice Document Card */}
      <Card className="shadow-md bg-white p-8">
        <div className="flex flex-col sm:flex-row justify-between pb-8 border-b gap-4">
          <div>
            <div className="flex items-center gap-2 text-primary font-bold text-xl mb-1">
              <Building2 className="h-6 w-6" /> {hotel?.name || "HotelSaaS Property"}
            </div>
            <p className="text-xs text-muted-foreground">GSTIN: {hotel?.gstIn || "Not Configured"}</p>
            <p className="text-xs text-muted-foreground">
              {hotel?.address ? `${hotel.address}, ${hotel.city}, ${hotel.state}` : "India"}
            </p>
          </div>

          <div className="text-left sm:text-right">
            <h2 className="text-2xl font-black text-foreground">{invoice.invoiceNumber}</h2>
            <p className="text-xs text-muted-foreground">Issue Date: {issueDateStr}</p>
            <div className="mt-2">
              <StatusBadge status={invoice.status} />
            </div>
          </div>
        </div>

        {/* Bill To */}
        <div className="py-6 border-b flex justify-between">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
              Billed To
            </p>
            <p className="font-bold text-base mt-1">{displayGuestName}</p>
            <p className="text-xs text-muted-foreground">{displayGuestMobile}</p>
            {invoice.guestGstin && (
              <p className="text-xs text-muted-foreground font-mono">GSTIN: {invoice.guestGstin}</p>
            )}
          </div>
          <div className="text-right">
            <p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
              Booking Ref
            </p>
            <p className="font-semibold text-primary text-sm mt-1">
              {invoice.bookingReference || invoice.booking?.bookingReference || "-"}
            </p>
          </div>
        </div>

        {/* Items Table */}
        <div className="py-6">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b text-muted-foreground text-xs font-semibold uppercase">
                <th className="text-left py-2">Item Description</th>
                <th className="text-center py-2">Qty</th>
                <th className="text-right py-2">Unit Price</th>
                <th className="text-right py-2">Tax</th>
                <th className="text-right py-2">Total</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {invoice.items?.map((item) => (
                <tr key={item.id}>
                  <td className="py-3 font-medium">{item.description}</td>
                  <td className="py-3 text-center">{item.quantity}</td>
                  <td className="py-3 text-right">₹{item.unitPrice.toLocaleString("en-IN")}</td>
                  <td className="py-3 text-right">₹{item.taxAmount.toLocaleString("en-IN")} ({item.taxRate}%)</td>
                  <td className="py-3 text-right font-semibold">
                    ₹{item.totalAmount.toLocaleString("en-IN")}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Totals Summary */}
        <div className="pt-4 border-t flex justify-end">
          <div className="w-64 space-y-2 text-sm">
            <div className="flex justify-between text-muted-foreground">
              <span>Subtotal</span>
              <span>₹{(invoice.subTotal ?? 0).toLocaleString("en-IN")}</span>
            </div>
            <div className="flex justify-between text-muted-foreground">
              <span>GST (CGST + SGST)</span>
              <span>₹{(invoice.taxAmount ?? 0).toLocaleString("en-IN")}</span>
            </div>
            {invoice.discountAmount > 0 && (
              <div className="flex justify-between text-emerald-600 font-medium">
                <span>Discount</span>
                <span>-₹{invoice.discountAmount.toLocaleString("en-IN")}</span>
              </div>
            )}
            <div className="flex justify-between font-bold text-base pt-2 border-t text-foreground">
              <span>Total Amount</span>
              <span>₹{(invoice.totalAmount ?? 0).toLocaleString("en-IN")}</span>
            </div>
            <div className="flex justify-between text-emerald-600 font-semibold">
              <span>Amount Paid</span>
              <span>₹{paid.toLocaleString("en-IN")}</span>
            </div>
            <div className="flex justify-between font-extrabold text-destructive border-t pt-2">
              <span>Balance Due</span>
              <span>₹{balance.toLocaleString("en-IN")}</span>
            </div>
          </div>
        </div>

        {/* Payments History */}
        {invoice.payments && invoice.payments.length > 0 && (
          <div className="mt-8 pt-6 border-t">
            <h4 className="text-sm font-bold mb-3">Recorded Payments</h4>
            <div className="divide-y text-xs">
              {invoice.payments.map((p) => (
                <div key={p.id} className="py-2 flex justify-between items-center text-muted-foreground">
                  <div>
                    <span className="font-semibold text-foreground">{p.paymentMethod}</span> &bull;{" "}
                    {new Date(p.paymentDate).toLocaleString()} &bull; Ref: {p.reference || p.transactionReference || "N/A"}
                  </div>
                  <span className="font-bold text-emerald-600">
                    ₹{p.amount.toLocaleString("en-IN")}
                  </span>
                </div>
              ))}
            </div>
          </div>
        )}
      </Card>

      {/* Record Payment Dialog */}
      <Dialog open={paymentModalOpen} onOpenChange={setPaymentModalOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Record Payment</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div className="space-y-2">
              <Label>Payment Amount (₹)</Label>
              <Input
                type="number"
                value={paymentAmount}
                onChange={(e) => setPaymentAmount(Number(e.target.value))}
              />
            </div>

            <div className="space-y-2">
              <Label>Payment Method</Label>
              <Select value={paymentMethod} onValueChange={(val) => setPaymentMethod(val as PaymentMethod)}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="UPI">UPI / QR</SelectItem>
                  <SelectItem value="Cash">Cash</SelectItem>
                  <SelectItem value="Card">Credit / Debit Card</SelectItem>
                  <SelectItem value="BankTransfer">Bank Transfer</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Transaction Reference / UTR</Label>
              <Input
                placeholder="e.g. UPI-9128301923"
                value={paymentRef}
                onChange={(e) => setPaymentRef(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setPaymentModalOpen(false)}>
              Cancel
            </Button>
            <Button
              onClick={() => recordPaymentMutation.mutate()}
              disabled={recordPaymentMutation.isPending}
            >
              Save Receipt
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
