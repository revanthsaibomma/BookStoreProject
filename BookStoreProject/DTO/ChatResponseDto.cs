namespace BookStoreProject.DTOs
{
    public class ChatResponseDto
    {
        public string Message { get; set; } = "";
        public List<string> Books { get; set; } = new();
    }
}