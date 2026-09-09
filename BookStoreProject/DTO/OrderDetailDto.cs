namespace BookStoreProject.DTOs
{
    public class OrderDetailDto
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        public string? BookName { get; set; }

        public int Quantity { get; set; }

        public double UnitPrice { get; set; }

        public double TotalPrice => Quantity * UnitPrice;
    }
}