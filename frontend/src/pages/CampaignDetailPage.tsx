import { useParams, useNavigate } from "react-router-dom"
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { Send, CheckCircle2, XCircle } from "lucide-react"
import { campaignService } from "@/services/campaign.service"
import { Button } from "@/components/ui/button"
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { StatusBadge } from "@/components/shared/StatusBadge"
import { LoadingSpinner } from "@/components/shared/LoadingSpinner"
import { useToast } from "@/components/ui/use-toast"

export function CampaignDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { toast } = useToast()
  const queryClient = useQueryClient()

  const { data: res, isLoading } = useQuery({
    queryKey: ["campaign", id],
    queryFn: () => campaignService.getCampaignById(id!),
    enabled: !!id,
  })

  const { data: recipientsRes } = useQuery({
    queryKey: ["campaign-recipients", id],
    queryFn: () => campaignService.getRecipients(id!),
    enabled: !!id,
  })

  const current = res?.data
  const recipients = recipientsRes?.data || []

  const sendNowMutation = useMutation({
    mutationFn: () => campaignService.sendCampaign(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["campaign", id] })
      queryClient.invalidateQueries({ queryKey: ["campaign-recipients", id] })
      queryClient.invalidateQueries({ queryKey: ["campaigns"] })
      toast({ title: "Campaign dispatched successfully" })
    },
    onError: (err: unknown) => {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
      toast({ title: msg || "Failed to dispatch campaign", variant: "destructive" })
    },
  })

  if (isLoading) {
    return <LoadingSpinner size="lg" className="h-64" />
  }

  if (!current) {
    return (
      <div className="text-center py-16 space-y-4">
        <p className="text-lg font-semibold">Campaign not found</p>
        <Button variant="outline" onClick={() => navigate("/campaigns")}>
          Back to Campaigns
        </Button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={current.name}
        description="Channel: WhatsApp"
        breadcrumbs={[{ label: "Campaigns", href: "/campaigns" }, { label: current.name }]}
      >
        {current.status === "Draft" || current.status === "Scheduled" ? (
          <Button onClick={() => sendNowMutation.mutate()} disabled={sendNowMutation.isPending}>
            <Send className="h-4 w-4 mr-1" /> {sendNowMutation.isPending ? "Sending..." : "Send Now"}
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
                <p className="text-xs text-muted-foreground">Occasion / Subject</p>
                <p className="text-sm font-semibold">{current.occasion}</p>
              </div>
              <div>
                <p className="text-xs text-muted-foreground">Message Copy</p>
                <div className="mt-2 p-4 rounded-lg bg-muted text-sm whitespace-pre-wrap leading-relaxed">
                  {current.messageTemplate}
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
              {recipients.length === 0 ? (
                <p className="text-xs text-muted-foreground py-4">No recipients queued for this campaign.</p>
              ) : (
                <div className="divide-y text-sm">
                  {recipients.map((rec) => (
                    <div key={rec.id} className="py-3 flex items-center justify-between">
                      <div>
                        <p className="font-medium">{rec.guestName || "Guest"}</p>
                        <p className="text-xs text-muted-foreground">{rec.mobile}</p>
                        {rec.errorReason && (
                          <p className="text-xs text-destructive mt-0.5">{rec.errorReason}</p>
                        )}
                      </div>
                      <div>
                        {rec.status === "Sent" ? (
                          <span className="flex items-center text-xs font-semibold text-emerald-600 gap-1">
                            <CheckCircle2 className="h-3.5 w-3.5" /> Delivered
                          </span>
                        ) : rec.status === "Failed" ? (
                          <span className="flex items-center text-xs font-semibold text-destructive gap-1">
                            <XCircle className="h-3.5 w-3.5" /> Failed
                          </span>
                        ) : (
                          <span className="text-xs text-muted-foreground font-medium">{rec.status}</span>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
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
                <span>{current.sentCount}</span>
              </div>
              <div className="flex justify-between py-2 border-b text-destructive font-semibold">
                <span>Failed Deliveries</span>
                <span>{current.failedCount}</span>
              </div>
              <div className="flex justify-between py-2 border-b">
                <span className="text-muted-foreground">Delivery Rate</span>
                <span className="font-bold">
                  {current.totalRecipients > 0
                    ? `${Math.round((current.sentCount / current.totalRecipients) * 100)}%`
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
