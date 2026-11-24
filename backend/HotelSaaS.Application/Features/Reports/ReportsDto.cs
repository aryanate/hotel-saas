namespace HotelSaaS.Application.Features.Reports;

public class DashboardStatsDto
{
    public int TotalRooms { get; set; }
    public int OccupiedRooms { get; set; }
    public int AvailableRooms { get; set; }
    public int CleaningRooms { get; set; }
    public int MaintenanceRooms { get; set; }
    public double OccupancyRate { get; set; }
    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal PendingPayments { get; set; }
    public int TotalActiveGuests { get; set; }
    public int PendingCheckInSessions { get; set; }
    public List<DashboardWeeklyRevenueDto> WeeklyRevenue { get; set; } = new();
    public List<DashboardRecentBookingDto> RecentBookings { get; set; } = new();
    public List<DashboardRecentGuestDto> RecentRegistrations { get; set; } = new();
}

public class DashboardWeeklyRevenueDto
{
    public string Date { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class DashboardRecentBookingDto
{
    public string Id { get; set; } = string.Empty;
    public string BookingReference { get; set; } = string.Empty;
    public DashboardGuestBriefDto Guest { get; set; } = new();
    public string CheckInDate { get; set; } = string.Empty;
    public string CheckOutDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalNights { get; set; }
}

public class DashboardGuestBriefDto
{
    public string FullName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
}

public class DashboardRecentGuestDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RevenueReportDto
{
    public decimal TotalRevenue { get; set; }
    public decimal RoomRevenue { get; set; }
    public decimal OtherRevenue { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalDiscounts { get; set; }
    public List<DailyRevenueDto> DailyBreakdown { get; set; } = new();
}

public class DailyRevenueDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int InvoiceCount { get; set; }
}

public class OccupancyReportDto
{
    public double AverageOccupancyRate { get; set; }
    public int TotalRooms { get; set; }
    public List<DailyOccupancyDto> DailyBreakdown { get; set; } = new();
}

public class DailyOccupancyDto
{
    public DateTime Date { get; set; }
    public int OccupiedRooms { get; set; }
    public double OccupancyRate { get; set; }
}

public class GstReportDto
{
    public string HotelGstIn { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal TotalTaxableAmount { get; set; }
    public decimal TotalCgst { get; set; }
    public decimal TotalSgst { get; set; }
    public decimal TotalIgst { get; set; }
    public decimal TotalTax { get; set; }
    public int TotalInvoices { get; set; }
    public List<GstInvoiceSummaryDto> Invoices { get; set; } = new();
}

public class GstInvoiceSummaryDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string? GuestGstin { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class GuestReportDto
{
    public int TotalGuests { get; set; }
    public int RepeatGuests { get; set; }
    public int NewGuests { get; set; }
    public decimal AverageSpendingPerGuest { get; set; }
    public int FavoriteGuestsCount { get; set; }
}

public class InvoiceReportDto
{
    public int TotalInvoices { get; set; }
    public decimal TotalBilled { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int PaidInvoicesCount { get; set; }
    public int UnpaidInvoicesCount { get; set; }
}
