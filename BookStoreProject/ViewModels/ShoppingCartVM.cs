namespace BookStoreProject.ViewModels
{
    public class ShoppingCartVM
    {
        public int CartDetailId { get; set; }

        public int BookId { get; set; }

        public string BookName { get; set; }

        public string Image { get; set; }

        public double Price { get; set; }

        public int Stock { get; set; }

        public int Quantity { get; set; }

        public double TotalPrice => Price * Quantity;
    }
}
