import { useParams } from "react-router-dom"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { Send, CheckCircle2, XCircle, Clock } from "lucide-react"
import { campaignService } from "@/services/campaign.service"
import { Button } from "@/components/ui/button"
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"
import { useToast } from "@/components/ui/use-toast"

export function CampaignDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { toast } = useToast()
  const queryClient = useQueryClient()

  const { data: res, isLoading } = useQuery({
    queryKey: ["campaign", id],
    queryFn: () => campaignService.getCampaigns(),
  })

  const { data: recipientsRes } = useQuery({
    queryKey: ["campaign-recipients", id],
    queryFn: () => campaignService.getRecipients(id!),
  })

  const current = res?.data?.items?.find((c) => c.id === id) || {
    id: id || "c1",
    title: "Diwali Festive Greeting & Discount",
    subject: "Exclusive 25% Off on your Diwali Getaway",
    channel: "WhatsApp",
    status: "Sent",
    totalRecipients: 420,
    successfulCount: 412,
    failedCount: 8,
    sentAt: "2026-11-10T09:00:00Z",
    messageContent:
      "Dear Guest,\nWarm Diwali wishes from HotelSaaS! Enjoy 20% off plus free dining on your next stay. Book directly with code FESTIVE20.",
  }

  const recipients = recipientsRes?.data || [
    {
      id: "r1",
      guest: { fullName: "Vikram Malhotra" },
      recipientAddress: "+91 9876543210",
      status: "Sent",
      sentAt: "2026-11-10T09:01:00Z",
    },
    {
      id: "r2",
      guest: { fullName: "Ananya Roy" },
      recipientAddress: "+91 9123456789",
      status: "Sent",
      sentAt: "2026-11-10T09:01:05Z",
    },
    {
      id: "r3",
      guest: { fullName: "Rohan Verma" },
      recipientAddress: "+91 9988112233",
      status: "Failed",
      errorMessage: "WhatsApp account does not exist or blocked",
    },
  ]

  const sendNowMutation = useMutation({
    mutationFn: () => campaignService.sendCampaign(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["campaign", id] })
      toast({ title: "Campaign dispatch triggered" })
    },
  })

  if (isLoading) {
    return <LoadingSpinner size="lg" className="h-64" />
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={current.title}
        description={`Channel: ${current.channel}`}
        breadcrumbs={[{ label: "Campaigns", href: "/campaigns" }, { label: current.title }]}
      >
        {current.status === "Draft" || current.status === "Scheduled" ? (
          <Button onClick={() => sendNowMutation.mutate()} disabled={sendNowMutation.isPending}>
            <Send className="h-4 w-4 mr-1" /> Send Now
          </Button>
        ) : null}
      </PageHeader>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="md:col-span-2 space-y-6">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-base font-semibold">Message Preview</CardTitle>
              <StatusBadge status={current.status} />
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <p className="text-xs text-muted-foreground">Subject</p>
                <p className="text-sm font-semibold">{current.subject}</p>
              </div>
              <div>
                <p className="text-xs text-muted-foreground">Message Copy</p>
                <div className="mt-2 p-4 rounded-lg bg-muted text-sm whitespace-pre-wrap leading-relaxed">
                  {current.messageContent}
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Recipient delivery table */}
          <Card>
            <CardHeader>
              <CardTitle className="text-base font-semibold">Recipient Delivery Log</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="divide-y text-sm">
                {recipients.map((rec) => (
                  <div key={rec.id} className="py-3 flex items-center justify-between">
                    <div>
                      <p className="font-medium">{rec.guest?.fullName || "Guest"}</p>
                      <p className="text-xs text-muted-foreground">{rec.recipientAddress}</p>
                      {rec.errorMessage && (
                        <p className="text-xs text-destructive mt-0.5">{rec.errorMessage}</p>
                      )}
                    </div>
                    <div>
                      {rec.status === "Sent" ? (
                        <span className="flex items-center text-xs font-semibold text-emerald-600 gap-1">
                          <CheckCircle2 className="h-3.5 w-3.5" /> Delivered
                        </span>
                      ) : (
                        <span className="flex items-center text-xs font-semibold text-destructive gap-1">
                          <XCircle className="h-3.5 w-3.5" /> Failed
                        </span>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Stats card */}
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="text-base font-semibold">Performance Metrics</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4 text-sm">
              <div className="flex justify-between py-2 border-b">
                <span className="text-muted-foreground">Total Targeted</span>
                <span className="font-bold">{current.totalRecipients}</span>
              </div>
              <div className="flex justify-between py-2 border-b text-emerald-600 font-semibold">
                <span>Successfully Delivered</span>
                <span>{current.successfulCount}</span>
              </div>
              <div className="flex justify-between py-2 border-b text-destructive font-semibold">
                <span>Failed Deliveries</span>
                <span>{current.failedCount}</span>
              </div>
              <div className="flex justify-between py-2 border-b">
                <span className="text-muted-foreground">Delivery Rate</span>
                <span className="font-bold">
                  {current.totalRecipients > 0
                    ? `${Math.round((current.successfulCount / current.totalRecipients) * 100)}%`
                    : "0%"}
                </span>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
