namespace BookStoreProject.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        // User Checkout Info
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string PaymentMode { get; set; } // e.g., "COD" or "Online"

        // Foreign Key for Status
        public int OrderStatusId { get; set; }
        public OrderStatus OrderStatus { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
