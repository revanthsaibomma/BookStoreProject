namespace BookStoreProject.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime CreateDate { get; set; }

        public string? Name { get; set; }

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        public string? PaymentMode { get; set; }

        public int OrderStatusId { get; set; }

        public string? StatusName { get; set; }

        public List<OrderDetailDto> OrderDetails { get; set; } = new();
    }
}