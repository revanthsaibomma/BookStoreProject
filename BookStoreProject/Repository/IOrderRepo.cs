using BookStoreProject.Models;

namespace BookStoreProject.Repository
{
    public interface IOrderRepo
    {
        Task<int> PlaceOrderAsync(int userId, string name, string address, string phone, string paymentMode);
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<IEnumerable<Order>> GetAllOrdersAsync(); // For the Admin Panel
        Task UpdateOrderStatusAsync(int orderId, int statusId);
    }
}
