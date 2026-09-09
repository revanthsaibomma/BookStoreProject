using BookStoreProject.DAL;
using BookStoreProject.Models;
using BookStoreProject.Repository;
using Microsoft.EntityFrameworkCore;

namespace BookStoreProject.Repository
{
    public class OrderRepo : IOrderRepo
    {
        private readonly ApplicationDbContext _context;

        public OrderRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> PlaceOrderAsync(int userId, string name, string address, string phone, string paymentMode)
        {
            // 1. Find the user's active cart and its details
            var cart = await _context.Carts
                                     .Include(c => c.CartDetails)
                                     .ThenInclude(cd => cd.Book)
                                     .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

            if (cart == null || !cart.CartDetails.Any())
                throw new Exception("Cart is empty.");

            // 2. Create the Order
            var order = new Order
            {
                UserId = userId,
                Name = name,
                Address = address,
                PhoneNumber = phone,
                PaymentMode = paymentMode,
                OrderStatusId = 1, // Assuming '1' is the ID for "Pending" in your OrderStatus table
                CreateDate = DateTime.UtcNow
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Save to get the OrderId

            // 3. Move CartDetails to OrderDetails
            foreach (var item in cart.CartDetails)
            {
                // Check whether enough stock is available
                if (item.Book.Stock < item.Quantity)
                {
                    throw new Exception(
                        $"Not enough stock available for {item.Book.BookName}."
                    );
                }

                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Book.Price
                };

                _context.OrderDetails.Add(orderDetail);

                // Reduce the available stock
                item.Book.Stock -= item.Quantity;
            }

            // 4. Clear the Cart (Soft delete as per your IsDeleted property)
            cart.IsDeleted = true;

            await _context.SaveChangesAsync();
            return order.Id;
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await _context.Orders
                                 .Include(o => o.OrderStatus)
                                 .Include(o => o.OrderDetails)
                                 .ThenInclude(od => od.Book)
                                 .Where(o => o.UserId == userId)
                                 .OrderByDescending(o => o.CreateDate)
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                                 .Include(o => o.OrderStatus)
                                 .Include(o => o.OrderDetails)
                                 .ThenInclude(od => od.Book)
                                 .OrderByDescending(o => o.CreateDate)
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
    }
}
