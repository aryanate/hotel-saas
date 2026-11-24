import { Badge } from "@/components/ui/badge"
import { cn } from "@/lib/utils"

interface StatusBadgeProps {
  status: string
  className?: string
}

export function StatusBadge({ status, className }: StatusBadgeProps) {
  const getVariant = (s: string) => {
    switch (s.toLowerCase()) {
      case "available":
      case "confirmed":
      case "checkedin":
      case "paid":
      case "active":
      case "sent":
      case "approved":
        return "bg-emerald-100 text-emerald-800 border-emerald-200 dark:bg-emerald-900/30 dark:text-emerald-400"
      case "occupied":
      case "reserved":
      case "partiallypaid":
      case "scheduled":
      case "opened":
        return "bg-blue-100 text-blue-800 border-blue-200 dark:bg-blue-900/30 dark:text-blue-400"
      case "pending":
      case "cleaning":
      case "trial":
      case "draft":
        return "bg-amber-100 text-amber-800 border-amber-200 dark:bg-amber-900/30 dark:text-amber-400"
      case "maintenance":
      case "outoforder":
      case "cancelled":
      case "noshow":
      case "unpaid":
      case "failed":
      case "rejected":
      case "expired":
      case "suspended":
        return "bg-red-100 text-red-800 border-red-200 dark:bg-red-900/30 dark:text-red-400"
      case "checkedout":
      case "refunded":
        return "bg-slate-100 text-slate-800 border-slate-200 dark:bg-slate-800 dark:text-slate-300"
      default:
        return "bg-gray-100 text-gray-800 border-gray-200"
    }
  }

  return (
    <Badge
      variant="outline"
      className={cn("font-medium tracking-wide border px-2 py-0.5 capitalize", getVariant(status), className)}
    >
      {status}
    </Badge>
  )
}
