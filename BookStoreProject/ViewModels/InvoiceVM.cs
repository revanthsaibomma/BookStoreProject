namespace BookStoreProject.ViewModels
{
    public class InvoiceVM
    {
        public int OrderId { get; set; }

        public string CustomerName { get; set; } = "";

        public string Address { get; set; } = "";

        public string PhoneNumber { get; set; } = "";

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public List<InvoiceItemVM> Items { get; set; } = new();
    }

    public class InvoiceItemVM
    {
        public string BookName { get; set; } = "";

        public int Quantity { get; set; }

        public decimal Price { get; set; }
    }
}