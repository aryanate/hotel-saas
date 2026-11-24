import { useState } from "react"
import { useQuery } from "@tanstack/react-query"
import { useNavigate } from "react-router-dom"
import { Plus, Send, MessageSquare, Mail } from "lucide-react"
import { campaignService } from "@/services/campaign.service"
import type { Campaign } from "@/types"
import { Button } from "@/components/ui/button"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, type Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"

export function CampaignsPage() {
  const navigate = useNavigate()
  const [page, setPage] = useState(1)

  const { data: res, isLoading } = useQuery({
    queryKey: ["campaigns", page],
    queryFn: () => campaignService.getCampaigns({ page, pageSize: 10 }),
  })

  const campaigns: Campaign[] = (res?.data?.items || [
    {
      id: "c1",
      hotelId: "h1",
      createdAt: "2026-11-10",
      updatedAt: "2026-11-10",
      title: "Diwali Festive Greeting & Discount",
      subject: "Exclusive 25% Off on your Diwali Getaway",
      channel: "WhatsApp",
      messageContent: "Diwali wishes",
      status: "Sent",
      totalRecipients: 420,
      successfulCount: 412,
      failedCount: 8,
      sentAt: "2026-11-10T09:00:00Z",
    },
    {
      id: "c2",
      hotelId: "h1",
      createdAt: "2026-11-10",
      updatedAt: "2026-11-10",
      title: "Weekend Getaway Promotion",
      subject: "Complimentary breakfast & spa access this weekend",
      channel: "WhatsApp",
      messageContent: "Weekend deals",
      status: "Scheduled",
      totalRecipients: 150,
      successfulCount: 0,
      failedCount: 0,
      scheduledFor: "2026-11-20T10:00:00Z",
    },
    {
      id: "c3",
      hotelId: "h1",
      createdAt: "2026-11-13",
      updatedAt: "2026-11-13",
      title: "Birthday & Anniversary Wishes (Automated)",
      subject: "Happy Birthday from all of us at HotelSaaS!",
      channel: "WhatsApp",
      messageContent: "Birthday wishes",
      status: "Sending",
      totalRecipients: 12,
      successfulCount: 10,
      failedCount: 0,
      sentAt: "2026-11-13T08:00:00Z",
    },
  ]) as Campaign[]

  const totalPages = res?.data?.totalPages || 1

  const columns: Column<Campaign>[] = [
    {
      header: "Campaign Name",
      accessorKey: "title",
      cell: (item) => (
        <div>
          <p className="font-semibold text-foreground">{item.title}</p>
          <p className="text-xs text-muted-foreground">{item.subject}</p>
        </div>
      ),
    },
    {
      header: "Channel",
      cell: (item) => (
        <span className="flex items-center gap-1.5 text-xs font-medium">
          {item.channel === "WhatsApp" ? (
            <MessageSquare className="h-3.5 w-3.5 text-emerald-600" />
          ) : (
            <Mail className="h-3.5 w-3.5 text-blue-600" />
          )}
          {item.channel}
        </span>
      ),
    },
    {
      header: "Audience",
      cell: (item) => (
        <span className="text-xs font-semibold">{item.totalRecipients} guests</span>
      ),
    },
    {
      header: "Delivery Stats",
      cell: (item) => (
        <div className="text-xs">
          <span className="text-emerald-600 font-semibold">{item.successfulCount} delivered</span>
          {item.failedCount > 0 && (
            <span className="text-destructive ml-2">({item.failedCount} failed)</span>
          )}
        </div>
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
        title="Marketing Campaigns"
        description="Engage guests with personalized WhatsApp & Email promotions, greetings, and offers"
      >
        <Button onClick={() => navigate("/campaigns/new")}>
          <Plus className="h-4 w-4 mr-1" /> Create Campaign
        </Button>
      </PageHeader>

      <DataTable
        data={campaigns}
        columns={columns}
        isLoading={isLoading}
        pageNumber={page}
        totalPages={totalPages}
        onPageChange={setPage}
        onRowClick={(item) => navigate(`/campaigns/${item.id}`)}
      />
    </div>
  )
}
