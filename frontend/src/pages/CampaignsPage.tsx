import { useQuery } from "@tanstack/react-query"
import { useNavigate } from "react-router-dom"
import { Plus, MessageSquare } from "lucide-react"
import { campaignService } from "@/services/campaign.service"
import type { Campaign } from "@/types"
import { Button } from "@/components/ui/button"
import { PageHeader } from "@/components/shared/PageHeader"
import { DataTable, type Column } from "@/components/shared/DataTable"
import { StatusBadge } from "@/components/shared/StatusBadge"

export function CampaignsPage() {
  const navigate = useNavigate()

  const { data: res, isLoading } = useQuery({
    queryKey: ["campaigns"],
    queryFn: () => campaignService.getCampaigns(),
  })

  const campaigns: Campaign[] = Array.isArray(res?.data) ? res.data : []

  const columns: Column<Campaign>[] = [
    {
      header: "Campaign Name",
      accessorKey: "name",
      cell: (item) => (
        <div>
          <p className="font-semibold text-foreground">{item.name}</p>
          <p className="text-xs text-muted-foreground">{item.occasion}</p>
        </div>
      ),
    },
    {
      header: "Channel",
      cell: () => (
        <span className="flex items-center gap-1.5 text-xs font-medium">
          <MessageSquare className="h-3.5 w-3.5 text-emerald-600" />
          WhatsApp
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
          <span className="text-emerald-600 font-semibold">{item.sentCount} delivered</span>
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
        onRowClick={(item) => navigate(`/campaigns/${item.id}`)}
      />
    </div>
  )
}
