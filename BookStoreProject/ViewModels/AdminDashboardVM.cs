namespace BookStoreProject.ViewModels
{
    public class AdminDashboardVM
    {
        public double TotalRevenue { get; set; }

        public int TotalOrders { get; set; }

        public int TotalBooksSold { get; set; }

        public List<MonthlyRevenueVM> MonthlyRevenue { get; set; } = new();

        public List<BookSalesVM> BookSales { get; set; } = new();
    }
}