namespace BookStoreProject.DTOs
{
    public class BookDisplayDto
    {
        public int BookId { get; set; }

        public string? BookName { get; set; }

        public string? AuthorName { get; set; }

        public double Price { get; set; }

        public string? Image { get; set; }

        public int GenreId { get; set; }

        public string? GenreName { get; set; }

        public string? Description { get; set; }

        public int Stock { get; set; }

        public int Quantity { get; set; }
    }
}