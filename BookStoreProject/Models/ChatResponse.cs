using System.ComponentModel.DataAnnotations;

namespace BookStoreProject.Models
{
    public class ChatResponse
    {
        [Required]
        public string Message { get; set; } = "";

        public List<string> Books { get; set; } = new();
    }
}