import { get, post } from "./api"
import type { Invoice, Payment, PagedResponse, InvoiceStatus, PaymentMethod } from "@/types"

export interface CreateInvoicePayload {
  bookingId: string
  items: Array<{
    description: string
    category: string
    quantity: number
    unitPrice: number
    taxRate: number
    discountAmount?: number
  }>
  discountAmount?: number
  notes?: string
}

export interface RecordPaymentPayload {
  amount: number
  paymentMethod: PaymentMethod
  transactionReference?: string
  notes?: string
}

export const billingService = {
  async createInvoice(payload: CreateInvoicePayload) {
    return post<Invoice>("/api/invoices", payload)
  },

  async getInvoices(params?: { search?: string; status?: InvoiceStatus; page?: number; pageSize?: number }) {
    return get<PagedResponse<Invoice>>("/api/invoices", { params })
  },

  async getInvoiceById(id: string) {
    return get<Invoice>(`/api/invoices/${id}`)
  },

  async getInvoicePdf(id: string) {
    return get<{ pdfUrl: string }>(`/api/invoices/${id}/pdf`)
  },

  async sendWhatsApp(id: string) {
    return post<{ success: boolean; messageId: string }>(`/api/invoices/${id}/send-whatsapp`)
  },

  async sendEmail(id: string) {
    return post<{ success: boolean }>(`/api/invoices/${id}/send-email`)
  },

  async recordPayment(invoiceId: string, payload: RecordPaymentPayload) {
    return post<Payment>(`/api/invoices/${invoiceId}/payments`, payload)
  },
}
