namespace BookStoreProject.DTOs
{
    public class AdminDashboardDto
    {
        public double TotalRevenue { get; set; }

        public int TotalOrders { get; set; }

        public int TotalBooksSold { get; set; }

        public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();

        public List<BookSalesDto> BookSales { get; set; } = new();
    }
}