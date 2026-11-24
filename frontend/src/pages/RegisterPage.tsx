import { useState } from "react"
import { useNavigate, Link } from "react-router-dom"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { Building2, AlertCircle } from "lucide-react"
import { useAuth } from "@/hooks/useAuth"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter } from "@/components/ui/card"
import { Alert, AlertDescription } from "@/components/ui/alert"

const registerSchema = z
  .object({
    hotelName: z.string().min(2, "Hotel name must be at least 2 characters"),
    ownerName: z.string().min(2, "Owner name must be at least 2 characters"),
    email: z.string().email("Please enter a valid email address"),
    mobile: z.string().min(10, "Please enter a valid 10-digit mobile number"),
    address: z.string().min(5, "Address must be at least 5 characters"),
    city: z.string().min(2, "City is required"),
    state: z.string().min(2, "State is required"),
    pinCode: z.string().min(4, "Valid PIN/Postal code is required"),
    numberOfRooms: z.number().min(1, "Minimum 1 room required"),
    password: z.string().min(8, "Password must be at least 8 characters"),
    confirmPassword: z.string(),
    gstIn: z.string().optional(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  })

type RegisterFormValues = z.infer<typeof registerSchema>

export function RegisterPage() {
  const { register: registerHotel } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      numberOfRooms: 10,
    },
  })

  const onSubmit = async (values: RegisterFormValues) => {
    setError(null)
    setIsLoading(true)
    try {
      await registerHotel(values)
      navigate("/dashboard")
    } catch (err: unknown) {
      const errObj = err as { response?: { data?: { message?: string } } }
      setError(errObj.response?.data?.message || "Registration failed. Please check your details and try again.")
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-slate-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
      <div className="sm:mx-auto sm:w-full sm:max-w-2xl text-center mb-6">
        <div className="inline-flex h-12 w-12 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-md mb-3">
          <Building2 className="h-6 w-6" />
        </div>
        <h2 className="text-2xl font-bold tracking-tight text-foreground">Register Your Hotel</h2>
        <p className="mt-1 text-sm text-muted-foreground">Start running your property efficiently with HotelSaaS</p>
      </div>

      <div className="sm:mx-auto sm:w-full sm:max-w-2xl px-4">
        <Card className="shadow-md">
          <CardHeader>
            <CardTitle className="text-xl">Property & Owner Registration</CardTitle>
            <CardDescription>Fill in your business details to setup your tenant account</CardDescription>
          </CardHeader>
          <form onSubmit={handleSubmit(onSubmit)}>
            <CardContent className="space-y-4">
              {error && (
                <Alert variant="destructive">
                  <AlertCircle className="h-4 w-4" />
                  <AlertDescription>{error}</AlertDescription>
                </Alert>
              )}

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="hotelName">Hotel Name *</Label>
                  <Input id="hotelName" placeholder="Grand Royal Palace" {...register("hotelName")} />
                  {errors.hotelName && <p className="text-xs text-destructive">{errors.hotelName.message}</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="ownerName">Owner / Manager Name *</Label>
                  <Input id="ownerName" placeholder="John Doe" {...register("ownerName")} />
                  {errors.ownerName && <p className="text-xs text-destructive">{errors.ownerName.message}</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="email">Official Email *</Label>
                  <Input id="email" type="email" placeholder="contact@grandroyal.com" {...register("email")} />
                  {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="mobile">Mobile / WhatsApp *</Label>
                  <Input id="mobile" placeholder="9876543210" {...register("mobile")} />
                  {errors.mobile && <p className="text-xs text-destructive">{errors.mobile.message}</p>}
                </div>

                <div className="space-y-2 md:col-span-2">
                  <Label htmlFor="address">Address *</Label>
                  <Input id="address" placeholder="123 Luxury Avenue, Near City Centre" {...register("address")} />
                  {errors.address && <p className="text-xs text-destructive">{errors.address.message}</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="city">City *</Label>
                  <Input id="city" placeholder="Mumbai" {...register("city")} />
                  {errors.city && <p className="text-xs text-destructive">{errors.city.message}</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="state">State *</Label>
                  <Input id="state" placeholder="Maharashtra" {...register("state")} />
                  {errors.state && <p className="text-xs text-destructive">{errors.state.message}</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="pinCode">PIN Code *</Label>
                  <Input id="pinCode" placeholder="400001" {...register("pinCode")} />
                  {errors.pinCode && <p className="text-xs text-destructive">{errors.pinCode.message}</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="numberOfRooms">Number of Rooms *</Label>
                  <Input
                    id="numberOfRooms"
                    type="number"
                    min="1"
                    {...register("numberOfRooms", { valueAsNumber: true })}
                  />
                  {errors.numberOfRooms && <p className="text-xs text-destructive">{errors.numberOfRooms.message}</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="gstIn">GSTIN (Optional)</Label>
                  <Input id="gstIn" placeholder="27AAAAA0000A1Z5" {...register("gstIn")} />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="password">Password *</Label>
                  <Input id="password" type="password" placeholder="••••••••" {...register("password")} />
                  {errors.password && <p className="text-xs text-destructive">{errors.password.message}</p>}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="confirmPassword">Confirm Password *</Label>
                  <Input id="confirmPassword" type="password" placeholder="••••••••" {...register("confirmPassword")} />
                  {errors.confirmPassword && (
                    <p className="text-xs text-destructive">{errors.confirmPassword.message}</p>
                  )}
                </div>
              </div>
            </CardContent>
            <CardFooter className="flex flex-col space-y-4">
              <Button type="submit" className="w-full" disabled={isLoading}>
                {isLoading ? "Creating Property Account..." : "Register Property"}
              </Button>
              <div className="text-center text-xs text-muted-foreground">
                Already have an account?{" "}
                <Link to="/login" className="font-semibold text-primary hover:underline">
                  Sign In
                </Link>
              </div>
            </CardFooter>
          </form>
        </Card>
      </div>
    </div>
  )
}
