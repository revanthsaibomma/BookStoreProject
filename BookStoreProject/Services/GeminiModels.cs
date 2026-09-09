namespace BookStoreProject.Services
{
    // Request Models

    public class GeminiRequest
    {
        public List<GeminiContent> contents { get; set; } = new();

        // This tells Gemini to return JSON instead of markdown
        public GenerationConfig generationConfig { get; set; } = new()
        {
            responseMimeType = "application/json"
        };
    }

    public class GenerationConfig
    {
        public string responseMimeType { get; set; } = "application/json";
    }

    public class GeminiContent
    {
        public List<GeminiPart> parts { get; set; } = new();
    }

    public class GeminiPart
    {
        public string text { get; set; } = "";
    }

    // Response Models

    public class GeminiResponse
    {
        public List<Candidate>? candidates { get; set; }
    }

    public class Candidate
    {
        public GeminiContent? content { get; set; }
    }
}