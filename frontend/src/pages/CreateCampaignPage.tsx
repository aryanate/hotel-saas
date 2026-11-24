import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { useMutation } from "@tanstack/react-query"
import { campaignService } from "@/services/campaign.service"
import type { MessageChannel } from "@/types"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardHeader, CardTitle, CardContent, CardFooter } from "@/components/ui/card"
import { PageHeader } from "@/components/shared/PageHeader"
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from "@/components/ui/select"
import { useToast } from "@/components/ui/use-toast"

export function CreateCampaignPage() {
  const navigate = useNavigate()
  const { toast } = useToast()

  const [title, setTitle] = useState("")
  const [subject, setSubject] = useState("")
  const [channel, setChannel] = useState<MessageChannel>("WhatsApp")
  const [messageContent, setMessageContent] = useState("")
  const [scheduledFor, setScheduledFor] = useState("")
  const [onlyFavorites, setOnlyFavorites] = useState(false)

  const templates: Record<string, { title: string; subject: string; content: string }> = {
    diwali: {
      title: "Diwali Special Offer",
      subject: "Celebrate the Festival of Lights with Us!",
      content:
        "Dear {{GuestName}},\nWarm Diwali wishes from HotelSaaS! Enjoy 20% off plus free dining on your next stay. Book directly with code FESTIVE20.",
    },
    birthday: {
      title: "Birthday Treat",
      subject: "Happy Birthday from all of us!",
      content:
        "Dear {{GuestName}},\nWishing you a joyful birthday! As our privileged guest, enjoy a complimentary dessert & room upgrade on your birthday week.",
    },
    weekend: {
      title: "Weekend Getaway Deal",
      subject: "Unwind this weekend with special rates",
      content:
        "Dear {{GuestName}},\nTake a break this weekend! Book a 2-night stay and receive complimentary breakfast and late check-out till 2 PM.",
    },
  }

  const applyTemplate = (key: string) => {
    const t = templates[key]
    if (t) {
      setTitle(t.title)
      setSubject(t.subject)
      setMessageContent(t.content)
    }
  }

  const createMutation = useMutation({
    mutationFn: () =>
      campaignService.createCampaign({
        title,
        subject,
        channel,
        messageContent,
        scheduledFor: scheduledFor || undefined,
        recipientFilters: {
          onlyFavorites,
        },
      }),
    onSuccess: (res) => {
      toast({ title: "Campaign created successfully" })
      if (res.data?.id) {
        navigate(`/campaigns/${res.data.id}`)
      } else {
        navigate("/campaigns")
      }
    },
    onError: () => {
      toast({ title: "Failed to create campaign", variant: "destructive" })
    },
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!title || !messageContent) {
      toast({ title: "Please fill required fields", variant: "destructive" })
      return
    }
    createMutation.mutate()
  }

  return (
    <div className="space-y-6 max-w-3xl mx-auto">
      <PageHeader
        title="Compose New Campaign"
        description="Craft targeted guest outreach and broadcast communications"
        breadcrumbs={[{ label: "Campaigns", href: "/campaigns" }, { label: "New Campaign" }]}
      />

      <form onSubmit={handleSubmit}>
        <Card className="shadow-sm">
          <CardHeader>
            <CardTitle>Campaign Configuration</CardTitle>
          </CardHeader>
          <CardContent className="space-y-6">
            {/* Quick Templates */}
            <div className="space-y-2">
              <Label className="text-xs text-muted-foreground">Load Preset Template</Label>
              <div className="flex flex-wrap gap-2">
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => applyTemplate("diwali")}
                >
                  Diwali Festive
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => applyTemplate("birthday")}
                >
                  Birthday Greeting
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => applyTemplate("weekend")}
                >
                  Weekend Promotion
                </Button>
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="title">Campaign Internal Name *</Label>
                <Input
                  id="title"
                  placeholder="e.g. Summer Weekend Flash Sale"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  required
                />
              </div>

              <div className="space-y-2">
                <Label>Communication Channel *</Label>
                <Select value={channel} onValueChange={(val) => setChannel(val as MessageChannel)}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="WhatsApp">WhatsApp Message</SelectItem>
                    <SelectItem value="Email">Email Newsletter</SelectItem>
                    <SelectItem value="SMS">SMS Notification</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="subject">Subject / Title Header *</Label>
              <Input
                id="subject"
                placeholder="e.g. Enjoy 20% off your next weekend escape!"
                value={subject}
                onChange={(e) => setSubject(e.target.value)}
                required
              />
            </div>

            <div className="space-y-2">
              <div className="flex justify-between items-center">
                <Label htmlFor="content">Message Body *</Label>
                <span className="text-xs text-muted-foreground">Variables: &#123;&#123;GuestName&#125;&#125;</span>
              </div>
              <textarea
                id="content"
                rows={5}
                className="w-full rounded-md border border-input bg-background p-3 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                placeholder="Type your message here..."
                value={messageContent}
                onChange={(e) => setMessageContent(e.target.value)}
                required
              />
            </div>

            {/* Target Audience Filters */}
            <div className="pt-4 border-t space-y-4">
              <Label className="text-sm font-semibold">Audience Segmentation</Label>
              <div className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  id="favorites"
                  className="rounded border-gray-300 h-4 w-4 text-primary"
                  checked={onlyFavorites}
                  onChange={(e) => setOnlyFavorites(e.target.checked)}
                />
                <label htmlFor="favorites" className="text-sm font-medium cursor-pointer">
                  Send only to VIP / Favorite Guests (Higher engagement)
                </label>
              </div>

              <div className="space-y-2">
                <Label htmlFor="schedule">Schedule Dispatch (Optional)</Label>
                <Input
                  id="schedule"
                  type="datetime-local"
                  value={scheduledFor}
                  onChange={(e) => setScheduledFor(e.target.value)}
                />
                <p className="text-xs text-muted-foreground">
                  Leave blank to queue and send immediately after creation.
                </p>
              </div>
            </div>
          </CardContent>
          <CardFooter className="flex justify-between">
            <Button type="button" variant="outline" onClick={() => navigate("/campaigns")}>
              Cancel
            </Button>
            <Button type="submit" disabled={createMutation.isPending}>
              {createMutation.isPending ? "Creating..." : "Launch Campaign"}
            </Button>
          </CardFooter>
        </Card>
      </form>
    </div>
  )
}
