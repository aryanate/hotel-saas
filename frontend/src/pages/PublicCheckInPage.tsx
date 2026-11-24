import { useState, useEffect } from "react"
import { useParams } from "react-router-dom"
import { useQuery } from "@tanstack/react-query"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import {
  Camera,
  CheckCircle2,
  ShieldCheck,
  Hotel as HotelIcon,
  AlertCircle,
  Calendar,
  BookmarkCheck,
  Sparkles,
  ScanLine,
} from "lucide-react"
import { get, apiClient } from "@/services/api"
import { extractDetailsFromIdImage } from "@/lib/idOcrExtractor"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter } from "@/components/ui/card"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"

interface PublicCheckInInfoDto {
  hotelName: string
  hotelLogoUrl?: string | null
  guestName: string
  guestMobile: string
  bookingReference: string
  checkInDate: string
  checkOutDate: string
  status: string
  isExpired: boolean
}

const checkInSchema = z.object({
  fullName: z.string().min(2, "Full name is required"),
  mobile: z.string().min(10, "Valid 10-digit mobile number is required"),
  email: z.string().email("Valid email required").optional().or(z.literal("")),
  address: z.string().min(5, "Residential address is required"),
  idType: z.string().min(1, "Please select ID Type"),
  idNumber: z.string().min(4, "ID Number is required"),
  declaration: z.boolean().refine((val) => val === true, {
    message: "You must accept the terms and declaration",
  }),
  marketingConsent: z.boolean().optional(),
})

type CheckInFormValues = z.infer<typeof checkInSchema>

export function PublicCheckInPage() {
  const { token } = useParams<{ token: string }>()
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [previewUrl, setPreviewUrl] = useState<string | null>(null)
  const [isScanning, setIsScanning] = useState(false)
  const [scanProgress, setScanProgress] = useState(0)
  const [extractedSummary, setExtractedSummary] = useState<string[]>([])
  const [scanNotice, setScanNotice] = useState<string | null>(null)
  const [isSubmitted, setIsSubmitted] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)

  const { data: res, isLoading, error } = useQuery({
    queryKey: ["public-checkin", token],
    queryFn: () => get<PublicCheckInInfoDto>(`/api/public/checkin/${token}`),
    enabled: !!token,
    retry: 1,
  })

  const checkInData = res?.data

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    getValues,
    formState: { errors },
  } = useForm<CheckInFormValues>({
    resolver: zodResolver(checkInSchema),
    defaultValues: {
      fullName: "",
      mobile: "",
      email: "",
      address: "",
      idType: "MaskedAadhaar",
      idNumber: "",
      declaration: false,
      marketingConsent: true,
    },
  })

  const currentIdType = watch("idType")

  useEffect(() => {
    return () => {
      if (previewUrl) URL.revokeObjectURL(previewUrl)
    }
  }, [previewUrl])

  const runIdOcr = async (file: File, mergeOnly: boolean) => {
    if (!file.type.startsWith("image/")) {
      setScanNotice("PDF attached — please verify your details below.")
      return
    }

    setIsScanning(true)
    setScanProgress(5)
    setScanNotice(null)
    try {
      const extracted = await extractDetailsFromIdImage(file, (pct) => setScanProgress(pct))
      const filled: string[] = []

      if (extracted.idType && (!mergeOnly || !getValues("idNumber"))) {
        setValue("idType", extracted.idType, { shouldValidate: true })
        filled.push("ID Type")
      }
      if (extracted.idNumber && (!mergeOnly || !getValues("idNumber"))) {
        setValue("idNumber", extracted.idNumber, { shouldValidate: true })
        filled.push("ID Number")
      }
      if (extracted.fullName && (!mergeOnly || !getValues("fullName"))) {
        setValue("fullName", extracted.fullName, { shouldValidate: true })
        filled.push("Full Name")
      }
      if (extracted.mobile && (!mergeOnly || !getValues("mobile"))) {
        setValue("mobile", extracted.mobile, { shouldValidate: true })
        filled.push("Mobile")
      }
      if (extracted.address && (!mergeOnly || !getValues("address"))) {
        setValue("address", extracted.address, { shouldValidate: true })
        filled.push("Address")
      }

      if (mergeOnly) {
        setExtractedSummary((prev) => Array.from(new Set([...prev, ...filled])))
      } else {
        setExtractedSummary(filled)
      }

      if (filled.length === 0) {
        setScanNotice("Photo attached! Could not read text clearly — please fill or verify details below.")
      }
    } catch {
      setScanNotice("Photo attached! Please fill or verify details below.")
    } finally {
      setIsScanning(false)
      setScanProgress(0)
    }
  }

  const handleFrontIdSelect = async (file: File | null) => {
    if (!file) return
    setSelectedFile(file)
    if (previewUrl) URL.revokeObjectURL(previewUrl)
    if (file.type.startsWith("image/")) {
      setPreviewUrl(URL.createObjectURL(file))
    } else {
      setPreviewUrl(null)
    }
    await runIdOcr(file, false)
  }

  const handleBackIdSelect = async (file: File | null) => {
    if (!file) return
    await runIdOcr(file, true)
  }

  useEffect(() => {
    if (checkInData) {
      if (checkInData.guestName) {
        setValue("fullName", checkInData.guestName)
      }
      if (checkInData.guestMobile) {
        setValue("mobile", checkInData.guestMobile)
      }
    }
  }, [checkInData, setValue])

  const onSubmit = async (values: CheckInFormValues) => {
    setIsSubmitting(true)
    setSubmitError(null)
    try {
      const formData = new FormData()
      formData.append("fullName", values.fullName)
      formData.append("mobile", values.mobile)
      if (values.email) formData.append("email", values.email)
      if (values.address) formData.append("address", values.address)
      formData.append("documentType", values.idType)
      formData.append("documentNumber", values.idNumber)
      formData.append("marketingConsent", String(!!values.marketingConsent))
      if (selectedFile) formData.append("documentFile", selectedFile)

      await apiClient.post(`/api/public/checkin/${token}/upload`, formData, {
        headers: { "Content-Type": "multipart/form-data" },
      })
      setIsSubmitted(true)
    } catch (err: unknown) {
      const errObj = err as { response?: { data?: { message?: string } } }
      const msg = errObj?.response?.data?.message || "Check-in submission failed. Please try again or visit reception."
      setSubmitError(msg)
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isLoading) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center p-4 bg-slate-50">
        <LoadingSpinner size="lg" className="h-16 w-16 mb-4 text-primary" />
        <p className="text-sm font-medium text-muted-foreground animate-pulse">Loading check-in details...</p>
      </div>
    )
  }

  if (error || (checkInData && checkInData.isExpired)) {
    const axiosErr = error as { response?: { data?: { message?: string } }; message?: string }
    const apiMsg = axiosErr?.response?.data?.message
    const networkMsg = axiosErr?.message

    const displayMsg = checkInData?.isExpired
      ? "This digital registration link has expired. Please request a new link or complete check-in at the hotel front desk."
      : apiMsg
        ? apiMsg
        : networkMsg && !networkMsg.includes("Network Error")
          ? networkMsg
          : "We could not find an active check-in session for this link."

    return (
      <div className="min-h-screen flex items-center justify-center p-4 bg-slate-50">
        <div className="max-w-md w-full p-8 bg-white rounded-2xl shadow-lg border text-center">
          <div className="h-16 w-16 bg-red-100 text-red-600 rounded-full flex items-center justify-center mx-auto mb-4">
            <AlertCircle className="h-8 w-8" />
          </div>
          <h2 className="text-2xl font-bold text-foreground">
            {checkInData?.isExpired ? "Check-In Link Expired" : "Invalid Check-In Link"}
          </h2>
          <p className="mt-2 text-sm text-muted-foreground">{displayMsg}</p>
          <div className="mt-4 flex flex-col gap-2">
            <Button variant="default" onClick={() => window.location.reload()} className="w-full">
              Retry
            </Button>
          </div>
        </div>
      </div>
    )
  }

  if (isSubmitted) {
    return (
      <div className="min-h-screen flex items-center justify-center p-4 bg-slate-50">
        <div className="max-w-md w-full text-center p-8 bg-white rounded-2xl shadow-xl border">
          <div className="h-20 w-20 bg-emerald-100 text-emerald-600 rounded-full flex items-center justify-center mx-auto mb-5 shadow-sm">
            <CheckCircle2 className="h-12 w-12" />
          </div>
          <h2 className="text-2xl font-bold text-foreground">Registration Submitted!</h2>
          <p className="mt-2 text-sm text-muted-foreground">
            Thank you, <span className="font-semibold text-foreground">{checkInData?.guestName || "Guest"}</span>.
            Your registration and identity details have been sent to the front desk.
          </p>
          <div className="mt-6 p-4 rounded-xl bg-slate-50 border text-xs text-muted-foreground text-left space-y-2">
            <p className="font-semibold text-foreground flex items-center gap-1.5">
              <BookmarkCheck className="h-4 w-4 text-emerald-600" /> Next Step at Reception:
            </p>
            <p>&bull; Mention your name or mobile number at the counter.</p>
            <p>&bull; The receptionist will hand you your room key right away!</p>
            <p>&bull; Check-out time is 11:00 AM.</p>
          </div>
        </div>
      </div>
    )
  }

  const hotelName = checkInData?.hotelName || "Hotel Reception"

  return (
    <div className="min-h-screen bg-slate-50/50 py-8 px-4 sm:px-6">
      <div className="max-w-xl mx-auto">
        <Card className="shadow-lg border-slate-200">
          <CardHeader className="text-center pb-4 border-b bg-white rounded-t-xl">
            <div className="inline-flex h-14 w-14 items-center justify-center rounded-2xl bg-primary/10 text-primary mx-auto mb-2 shadow-inner">
              <HotelIcon className="h-7 w-7" />
            </div>
            <CardTitle className="text-2xl font-bold tracking-tight">{hotelName}</CardTitle>
            <CardDescription className="text-sm font-medium text-slate-500">
              Guest Registration & Digital KYC Check-In
            </CardDescription>

            {checkInData?.bookingReference && (
              <div className="mt-3 inline-flex items-center gap-2 px-3 py-1 rounded-full bg-slate-100 text-slate-700 text-xs font-semibold mx-auto border">
                <Calendar className="h-3.5 w-3.5 text-primary" />
                <span>Booking: {checkInData.bookingReference}</span>
              </div>
            )}
          </CardHeader>

          <form onSubmit={handleSubmit(onSubmit)}>
            <CardContent className="space-y-4 pt-6 bg-white">
              {submitError && (
                <div className="p-3 rounded-lg bg-red-50 border border-red-200 text-xs text-red-700 flex items-center gap-2">
                  <AlertCircle className="h-4 w-4 shrink-0 text-red-600" />
                  <span>{submitError}</span>
                </div>
              )}

              {/* Step 1: Smart ID Upload & Auto-Fill */}
              <div className="rounded-xl border-2 border-dashed border-primary/40 bg-primary/5 p-4 space-y-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <Sparkles className="h-4 w-4 text-primary" />
                    <span className="text-sm font-bold text-foreground">
                      Quick Auto-Fill via Govt ID Photo
                    </span>
                  </div>
                  <span className="text-[11px] font-medium text-primary bg-white px-2 py-0.5 rounded-full border">
                    Aadhaar / PAN / DL / Passport
                  </span>
                </div>

                <input
                  type="file"
                  id="idFile"
                  className="hidden"
                  onChange={(e) => handleFrontIdSelect(e.target.files?.[0] ?? null)}
                  accept="image/*,application/pdf"
                />

                <label
                  htmlFor="idFile"
                  className="block cursor-pointer rounded-lg border bg-white p-3 hover:border-primary transition-colors"
                >
                  {isScanning ? (
                    <div className="space-y-2 py-2 text-center">
                      <div className="flex items-center justify-center gap-2 text-primary text-sm font-semibold">
                        <ScanLine className="h-4 w-4 animate-pulse" />
                        <span>Reading details from ID... {scanProgress}%</span>
                      </div>
                      <div className="w-full bg-slate-200 h-1.5 rounded-full overflow-hidden">
                        <div
                          className="bg-primary h-full transition-all duration-200"
                          style={{ width: `${Math.max(10, scanProgress)}%` }}
                        />
                      </div>
                    </div>
                  ) : selectedFile ? (
                    <div className="flex items-center gap-3">
                      {previewUrl ? (
                        <img
                          src={previewUrl}
                          alt="ID Preview"
                          className="h-14 w-20 object-cover rounded border shrink-0"
                        />
                      ) : (
                        <CheckCircle2 className="h-10 w-10 text-emerald-600 shrink-0" />
                      )}
                      <div className="min-w-0 flex-1 text-left">
                        <p className="text-xs font-semibold text-emerald-700 flex items-center gap-1">
                          <CheckCircle2 className="h-3.5 w-3.5" /> ID Photo Attached
                        </p>
                        <p className="text-xs text-muted-foreground truncate">{selectedFile.name}</p>
                        <p className="text-[11px] text-primary mt-0.5 font-medium">
                          Tap to retake / change photo
                        </p>
                      </div>
                    </div>
                  ) : (
                    <div className="flex flex-col items-center text-center py-2 text-muted-foreground">
                      <Camera className="h-8 w-8 mb-1.5 text-primary" />
                      <span className="text-sm font-semibold text-foreground">
                        Snap or Upload Govt ID Front
                      </span>
                      <span className="text-xs">
                        Auto-fills your name, ID number &amp; address — no manual typing!
                      </span>
                    </div>
                  )}
                </label>

                {extractedSummary.length > 0 && !isScanning && (
                  <div className="flex flex-wrap items-center justify-between gap-2 text-xs bg-emerald-50 border border-emerald-200 text-emerald-800 px-3 py-2 rounded-lg">
                    <span>
                      <strong>Auto-filled:</strong> {extractedSummary.join(", ")} (verify below)
                    </span>
                    {!getValues("address") && (
                      <label className="text-primary font-semibold underline cursor-pointer shrink-0">
                        Scan Back Side for Address
                        <input
                          type="file"
                          className="hidden"
                          accept="image/*"
                          onChange={(e) => handleBackIdSelect(e.target.files?.[0] ?? null)}
                        />
                      </label>
                    )}
                  </div>
                )}

                {scanNotice && !isScanning && (
                  <p className="text-xs text-amber-700 bg-amber-50 border border-amber-200 px-3 py-1.5 rounded-lg">
                    {scanNotice}
                  </p>
                )}
              </div>

              <div className="space-y-2">
                <Label htmlFor="fullName">Full Name (as per Govt ID) *</Label>
                <Input
                  id="fullName"
                  placeholder="e.g. Rahul Sharma"
                  {...register("fullName")}
                />
                {errors.fullName && <p className="text-xs text-destructive">{errors.fullName.message}</p>}
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="mobile">Mobile Number *</Label>
                  <Input
                    id="mobile"
                    placeholder="9876543210"
                    {...register("mobile")}
                  />
                  {errors.mobile && <p className="text-xs text-destructive">{errors.mobile.message}</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="email">Email Address</Label>
                  <Input
                    id="email"
                    type="email"
                    placeholder="rahul@example.com"
                    {...register("email")}
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="address">Residential Address *</Label>
                <Input
                  id="address"
                  placeholder="Flat / House No., Area, City, State"
                  {...register("address")}
                />
                {errors.address && <p className="text-xs text-destructive">{errors.address.message}</p>}
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="idType">Govt ID Type *</Label>
                  <select
                    id="idType"
                    value={currentIdType}
                    onChange={(e) => setValue("idType", e.target.value, { shouldValidate: true })}
                    className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2"
                  >
                    <option value="MaskedAadhaar">Aadhaar Card</option>
                    <option value="Passport">Passport</option>
                    <option value="DrivingLicense">Driving License</option>
                    <option value="VoterID">Voter ID</option>
                    <option value="PAN">PAN Card</option>
                  </select>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="idNumber">Govt ID Number *</Label>
                  <Input
                    id="idNumber"
                    placeholder="e.g. XXXX-XXXX-1234"
                    {...register("idNumber")}
                  />
                  {errors.idNumber && <p className="text-xs text-destructive">{errors.idNumber.message}</p>}
                </div>
              </div>

              {/* Legal Declarations */}
              <div className="space-y-3 pt-3 border-t">
                <label className="flex items-start gap-2.5 cursor-pointer">
                  <input
                    type="checkbox"
                    className="mt-1 h-4 w-4 rounded border-gray-300 text-primary"
                    {...register("declaration")}
                  />
                  <span className="text-xs text-muted-foreground leading-relaxed">
                    I hereby declare that all information provided is accurate and authentic under applicable Indian hotel regulations.
                  </span>
                </label>
                {errors.declaration && (
                  <p className="text-xs text-destructive">{errors.declaration.message}</p>
                )}

                <label className="flex items-start gap-2.5 cursor-pointer">
                  <input
                    type="checkbox"
                    className="mt-1 h-4 w-4 rounded border-gray-300 text-primary"
                    {...register("marketingConsent")}
                  />
                  <span className="text-xs text-muted-foreground leading-relaxed">
                    I agree to receive booking confirmation and Wi-Fi access details on WhatsApp/SMS.
                  </span>
                </label>
              </div>
            </CardContent>

            <CardFooter className="flex flex-col gap-3 bg-slate-50 border-t rounded-b-xl p-6">
              <Button
                type="submit"
                className="w-full text-base py-6 font-semibold shadow-md"
                disabled={isSubmitting}
              >
                {isSubmitting ? "Submitting Registration..." : "Complete Digital Check-In"}
              </Button>
              <div className="flex items-center justify-center gap-1.5 text-xs text-muted-foreground">
                <ShieldCheck className="h-4 w-4 text-emerald-600" />
                <span>End-to-end encrypted & official KYC compliant</span>
              </div>
            </CardFooter>
          </form>
        </Card>
      </div>
    </div>
  )
}
