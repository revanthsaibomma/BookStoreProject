using BookStoreProject.Models;
using BookStoreProject.ViewModels;

namespace BookStoreProject.Repository
{
    public interface IAdminOrderRepo
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();

        Task UpdateOrderStatusAsync(int orderId, int statusId);

        Task<AdminDashboardVM> GetDashboardDataAsync();
    }
}