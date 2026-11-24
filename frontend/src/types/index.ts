// Enums
export type RoomStatus =
  | 'Available'
  | 'Occupied'
  | 'Reserved'
  | 'Cleaning'
  | 'Maintenance'
  | 'OutOfOrder'

export type BookingStatus =
  | 'Pending'
  | 'Confirmed'
  | 'CheckedIn'
  | 'CheckedOut'
  | 'Cancelled'
  | 'NoShow'

export type BookingSource = 'QR' | 'Manual' | 'Online'

export type SubscriptionStatus =
  | 'Trial'
  | 'Active'
  | 'PastDue'
  | 'Suspended'
  | 'Cancelled'

export type DocumentType =
  | 'Passport'
  | 'DrivingLicense'
  | 'VoterID'
  | 'MaskedAadhaar'
  | 'PAN'
  | 'Other'

export type InvoiceStatus = 'Unpaid' | 'Paid' | 'PartiallyPaid' | 'Refunded'

export type PaymentMethod = 'Cash' | 'Card' | 'UPI' | 'BankTransfer' | 'Online'

export type ItemCategory = 'Room' | 'Food' | 'Miscellaneous' | 'Tax' | 'Discount'

export type MessageChannel = 'WhatsApp' | 'Email' | 'SMS'

export type MessageStatus =
  | 'Pending'
  | 'Sent'
  | 'Delivered'
  | 'Read'
  | 'Failed'

export type MessageType =
  | 'BookingConfirmation'
  | 'CheckInLink'
  | 'Invoice'
  | 'OTP'
  | 'Campaign'
  | 'Custom'

export type CampaignStatus =
  | 'Draft'
  | 'Scheduled'
  | 'Sending'
  | 'Sent'
  | 'Failed'
  | 'Cancelled'

export type RecipientStatus = 'Pending' | 'Sent' | 'Failed' | 'Skipped'

export type CheckInSessionStatus =
  | 'Sent'
  | 'Opened'
  | 'Submitted'
  | 'Approved'
  | 'Rejected'
  | 'Expired'

export type ConsentType = 'CheckIn' | 'Marketing'

export type RetentionRecordType =
  | 'GuestProfile'
  | 'GuestDocument'
  | 'BookingRecord'
  | 'InvoiceRecord'
  | 'ConsentRecord'
  | 'AuditRecord'

// API responses
export interface ApiResponse<T = unknown> {
  success: boolean
  data?: T
  message?: string
  errors?: string[]
}

export interface PagedResponse<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface AuthTokens {
  accessToken: string
  refreshToken: string
  expiresIn?: number
}

// Entities
export interface BaseEntity {
  id: string
  createdAt: string
  updatedAt: string
}

export interface TenantEntity extends BaseEntity {
  hotelId: string
}

export interface Hotel extends BaseEntity {
  name: string
  ownerName: string
  email: string
  mobile: string
  address: string
  city: string
  state: string
  pinCode: string
  numberOfRooms: number
  publicToken: string
  logoUrl?: string | null
  gstIn?: string | null
  contactEmail?: string | null
  contactPhone?: string | null
  invoicePrefix: string
  invoiceSequence: number
  checkInTime: string
  checkOutTime: string
  currency: string
  timeZone: string
  isActive: boolean
  isDeleted: boolean
}

export interface User extends BaseEntity {
  hotelId?: string | null
  email: string
  firstName: string
  lastName: string
  mobile?: string | null
  isActive: boolean
  isSuperAdmin?: boolean
  roles?: string[]
}

export interface Role extends BaseEntity {
  name: string
  description?: string
  isSystemRole: boolean
}

export interface RoomType extends TenantEntity {
  name: string
  description?: string | null
  basePrice: number
  maxCapacity: number
  totalRooms?: number
}

export interface Room extends TenantEntity {
  roomTypeId: string
  roomTypeName?: string | null
  roomType?: RoomType
  roomNumber: string
  floor: number
  capacity: number
  pricePerNight: number
  description?: string | null
  status: RoomStatus
  notes?: string | null
  isActive?: boolean
}

export interface GuestDocument extends TenantEntity {
  guestId?: string
  documentType: DocumentType
  documentNumber?: string | null
  fileName: string
  storageUrl?: string | null
  fileSizeBytes?: number
  isVerified?: boolean
  verifiedAt?: string | null
  verifiedBy?: string | null
}

export interface GuestNote extends TenantEntity {
  guestId?: string
  note: string
  noteText?: string
  createdBy?: string
  createdAt: string
}

export interface GuestConsent extends TenantEntity {
  guestId: string
  consentType: ConsentType
  isGranted: boolean
  grantedAt: string
  ipAddress?: string
}

export interface Guest extends TenantEntity {
  fullName: string
  mobile: string
  email?: string | null
  address?: string | null
  totalVisits: number
  totalSpending: number
  lastVisitAt?: string | null
  preferences?: string | null
  isFavorite: boolean
  marketingConsent: boolean
  marketingConsentAt?: string | null
  documents?: GuestDocument[]
  notes?: GuestNote[]
  consents?: GuestConsent[]
}

export interface BookingRoom {
  id?: string
  bookingId?: string
  roomId: string
  roomNumber: string
  roomTypeName?: string | null
  pricePerNight: number
  checkInDate?: string
  checkOutDate?: string
  room?: Room
  ratePerNight?: number
}

export interface BookingGuest {
  id: string
  bookingId: string
  guestId: string
  guest?: Guest
  isPrimary: boolean
}

export interface Booking extends TenantEntity {
  guestId: string
  guestName?: string
  guestMobile?: string
  guestEmail?: string | null
  guest?: Guest
  bookingReference: string
  source: BookingSource
  status: BookingStatus
  numberOfAdults: number
  numberOfChildren: number
  checkInDate: string
  checkOutDate: string
  specialRequests?: string | null
  totalNights: number
  totalAmount?: number
  rooms?: BookingRoom[]
  bookingRooms?: BookingRoom[]
  bookingGuests?: BookingGuest[]
  invoices?: Invoice[]
  checkInSessions?: CheckInSession[]
}

export interface CheckInSession extends TenantEntity {
  bookingId: string
  token: string
  status: CheckInSessionStatus
  expiresAt?: string
  openedAt?: string | null
  submittedAt?: string | null
  booking?: Booking
  guestName?: string
  guestMobile?: string
  guestEmail?: string
  guestAddress?: string
  documentType?: string
  documentNumber?: string
  documentUrl?: string
  documentUrls?: string[]
  bookingReference?: string
  checkInDate?: string
  checkOutDate?: string
  roomNumbers?: string[]
}

export interface InvoiceItem {
  id: string
  invoiceId: string
  description: string
  category: ItemCategory
  quantity: number
  unitPrice: number
  taxRate: number
  taxAmount: number
  discountAmount: number
  totalAmount: number
}

export interface Payment extends TenantEntity {
  invoiceId: string
  amount: number
  paymentMethod: PaymentMethod
  reference?: string | null
  transactionReference?: string | null
  paymentDate: string
  notes?: string | null
}

export interface Invoice extends TenantEntity {
  bookingId: string
  bookingReference?: string | null
  booking?: Booking
  invoiceNumber: string
  invoiceDate: string
  guestName: string
  guestMobile: string
  guestGstin?: string | null
  status: InvoiceStatus
  subTotal: number
  taxAmount: number
  discountAmount: number
  totalAmount: number
  amountPaid: number
  outstandingBalance: number
  paidAmount?: number
  balanceAmount?: number
  issueDate?: string
  dueDate?: string | null
  notes?: string | null
  items?: InvoiceItem[]
  payments?: Payment[]
}

export interface CampaignRecipient {
  id: string
  campaignId: string
  guestId: string
  guestName?: string | null
  mobile: string
  channel: MessageChannel
  status: RecipientStatus
  sentAt?: string | null
  externalMessageId?: string | null
  errorReason?: string | null
}

export interface Campaign extends TenantEntity {
  name: string
  occasion: string
  messageTemplate: string
  status: CampaignStatus
  scheduledAt?: string | null
  sentAt?: string | null
  totalRecipients: number
  sentCount: number
  failedCount: number
  recipients?: CampaignRecipient[]
}

export interface MessageLog extends TenantEntity {
  channel: MessageChannel
  messageType: MessageType
  recipient: string
  content: string
  status: MessageStatus
  sentAt: string
  errorMessage?: string | null
}

export interface RetentionPolicy extends BaseEntity {
  recordType: RetentionRecordType
  retentionDays: number
  description?: string
  isEnabled: boolean
}

export interface SubscriptionPlan extends BaseEntity {
  name: string
  description?: string
  price: number
  billingCycle: string
  maxBookingsPerMonth: number
  featuresJson: string
  isActive: boolean
}

export interface HotelSubscription extends BaseEntity {
  hotelId: string
  hotel?: Hotel
  planId: string
  plan?: SubscriptionPlan
  status: SubscriptionStatus
  startDate: string
  endDate: string
  autoRenew: boolean
}

// Dashboard and Report Types
export interface DashboardStats {
  totalRooms: number
  occupiedRooms: number
  availableRooms: number
  cleaningRooms?: number
  maintenanceRooms: number
  occupancyRate?: number
  todayCheckIns: number
  todayCheckOuts: number
  monthlyRevenue: number
  pendingPayments: number
  weeklyRevenue: { date: string; amount: number }[]
  recentBookings: Booking[]
  recentRegistrations: Guest[]
}

export interface GSTInvoiceSummary {
  invoiceNumber: string
  invoiceDate: string
  guestName: string
  guestGstin?: string | null
  taxableAmount: number
  taxAmount: number
  totalAmount: number
}

export interface GSTReport {
  hotelGstIn?: string
  month: number
  year: number
  totalTaxableAmount: number
  totalCgst: number
  totalSgst: number
  totalIgst: number
  totalTax: number
  totalInvoices: number
  invoices: GSTInvoiceSummary[]
}

export interface DailyRevenue {
  date: string
  amount: number
  invoiceCount: number
}

export interface RevenueReport {
  totalRevenue: number
  roomRevenue: number
  otherRevenue: number
  totalTax: number
  totalDiscounts: number
  dailyBreakdown: DailyRevenue[]
}

export interface DailyOccupancy {
  date: string
  occupiedRooms: number
  occupancyRate: number
}

export interface OccupancyReport {
  averageOccupancyRate: number
  totalRooms: number
  dailyBreakdown: DailyOccupancy[]
}
