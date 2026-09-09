using BookStoreProject.DAL;
using BookStoreProject.Repository;
using BookStoreProject.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using BookStoreProject.Models;

namespace BookStoreProject.Repository
{
    public class AdminOrderRepo : IAdminOrderRepo
    {
        private readonly ApplicationDbContext _context;

        public AdminOrderRepo(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderStatus)                   // Gets the text name of the status (e.g., "Shipped")
                .Include(o => o.OrderDetails)                  // Prevents the null reference crash
                    .ThenInclude(od => od.Book)                // Gets the book info so you can display the Book Name
                .OrderByDescending(o => o.CreateDate)          // Shows the newest orders at the top of the table
                .ToListAsync();
        }

        public async Task UpdateOrderStatusAsync(int orderId, int statusId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order != null)
            {
                order.OrderStatusId = statusId;
                await _context.SaveChangesAsync();
            }
        }


        public async Task<AdminDashboardVM> GetDashboardDataAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Book)
                .ToListAsync();

            AdminDashboardVM dashboard = new();

            dashboard.TotalOrders = orders.Count;

            dashboard.TotalRevenue = orders
                .SelectMany(o => o.OrderDetails)
                .Sum(x => x.Quantity * x.UnitPrice);

            dashboard.TotalBooksSold = orders
                .SelectMany(o => o.OrderDetails)
                .Sum(x => x.Quantity);

            dashboard.MonthlyRevenue = orders
                .GroupBy(o => new
                {
                    o.CreateDate.Year,
                    o.CreateDate.Month
                })
                .Select(g => new MonthlyRevenueVM
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1)
                            .ToString("MMMM yyyy"),

                    Revenue = g.SelectMany(x => x.OrderDetails)
                               .Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderBy(x => DateTime.ParseExact(x.Month,
                            "MMMM yyyy",
                            CultureInfo.InvariantCulture))
                .ToList();

            dashboard.BookSales = orders
                .SelectMany(o => o.OrderDetails)
                .GroupBy(x => x.Book.BookName)
                .Select(g => new BookSalesVM
                {
                    BookName = g.Key,

                    CopiesSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.CopiesSold)
                .ToList();

            return dashboard;
        }
    }
}