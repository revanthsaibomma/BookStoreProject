using BookStoreProject.Models;
using System.Text;
using System.Text.Json;

namespace BookStoreProject.Services
{
    public class GeminiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public GeminiService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }


        // ==========================================================
        // GET SEARCH INTENT FROM GEMINI
        // ==========================================================

        public async Task<BookSearchIntent> GetSearchIntent(
            string userMessage)
        {
            // ======================================================
            // 1. INPUT VALIDATION
            // ======================================================

            if (string.IsNullOrWhiteSpace(userMessage))
            {
                throw new ArgumentException(
                    "Please enter what you are looking for.");
            }

            userMessage = userMessage.Trim();


            // Minimum length
            if (userMessage.Length < 2)
            {
                throw new ArgumentException(
                    "Please enter at least 2 characters.");
            }


            // Maximum length
            if (userMessage.Length > 150)
            {
                throw new ArgumentException(
                    "Your request cannot exceed 150 characters.");
            }


            // ======================================================
            // 2. GET GEMINI CONFIGURATION
            // ======================================================

            var apiKey =
                _configuration["Gemini:ApiKey"];

            var model =
                _configuration["Gemini:Model"];


            // Validate API key
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Gemini API key is missing.");
            }


            // Validate model
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidOperationException(
                    "Gemini model is missing.");
            }


            // Remove accidental spaces
            apiKey = apiKey.Trim();
            model = model.Trim();


            // ======================================================
            // 3. CREATE PROMPT
            // ======================================================

            var prompt = """
You are BookNest AI, an intelligent assistant
for an online bookstore.

Your job is ONLY to understand what the customer
is asking for.

Do NOT recommend or invent book titles.

The application will search the bookstore database
after you identify the customer's request.

Customer request:
"""
            + userMessage +
            """

Return ONLY valid JSON.

The JSON must contain exactly these fields:

{
    "type": "",
    "value": ""
}

Possible request types:

1. genre
2. top-selling
3. popular
4. new
5. price
6. author
7. available
8. general


==================================================
GENRE EXAMPLES
==================================================

Customer:
fiction

Return:
{
    "type": "genre",
    "value": "fiction"
}


Customer:
I want fiction books

Return:
{
    "type": "genre",
    "value": "fiction"
}


Customer:
show me romance books

Return:
{
    "type": "genre",
    "value": "romance"
}


Customer:
I want science fiction

Return:
{
    "type": "genre",
    "value": "science fiction"
}


Customer:
I want mystery books

Return:
{
    "type": "genre",
    "value": "mystery"
}


==================================================
TOP SELLING EXAMPLES
==================================================

Customer:
show me top selling books

Return:
{
    "type": "top-selling",
    "value": ""
}


Customer:
best sellers

Return:
{
    "type": "top-selling",
    "value": ""
}


Customer:
most sold books

Return:
{
    "type": "top-selling",
    "value": ""
}


Customer:
books that sell the most

Return:
{
    "type": "top-selling",
    "value": ""
}


==================================================
POPULAR EXAMPLES
==================================================

Customer:
popular books

Return:
{
    "type": "popular",
    "value": ""
}


Customer:
show me popular books

Return:
{
    "type": "popular",
    "value": ""
}


Customer:
what books are popular?

Return:
{
    "type": "popular",
    "value": ""
}


==================================================
NEW BOOK EXAMPLES
==================================================

Customer:
show me new books

Return:
{
    "type": "new",
    "value": ""
}


Customer:
latest books

Return:
{
    "type": "new",
    "value": ""
}


Customer:
show me recently added books

Return:
{
    "type": "new",
    "value": ""
}


==================================================
PRICE EXAMPLES
==================================================

Customer:
books under 500

Return:
{
    "type": "price",
    "value": "500"
}


Customer:
books below 1000

Return:
{
    "type": "price",
    "value": "1000"
}


Customer:
show books below 300 rupees

Return:
{
    "type": "price",
    "value": "300"
}


Customer:
I want books under ₹700

Return:
{
    "type": "price",
    "value": "700"
}


==================================================
AUTHOR EXAMPLES
==================================================

Customer:
books by James Clear

Return:
{
    "type": "author",
    "value": "James Clear"
}


Customer:
show books written by J.K. Rowling

Return:
{
    "type": "author",
    "value": "J.K. Rowling"
}


Customer:
I want books by George Orwell

Return:
{
    "type": "author",
    "value": "George Orwell"
}


==================================================
AVAILABLE BOOK EXAMPLES
==================================================

Customer:
books available in stock

Return:
{
    "type": "available",
    "value": ""
}


Customer:
what books can I buy right now?

Return:
{
    "type": "available",
    "value": ""
}


Customer:
show me books that are in stock

Return:
{
    "type": "available",
    "value": ""
}


==================================================
GENERAL EXAMPLES
==================================================

Customer:
I want something interesting to read

Return:
{
    "type": "general",
    "value": ""
}


Customer:
recommend something

Return:
{
    "type": "general",
    "value": ""
}


Customer:
I don't know what to read

Return:
{
    "type": "general",
    "value": ""
}


==================================================
IMPORTANT RULES
==================================================

1. Return ONLY JSON.

2. Do not return markdown.

3. Do not return ```json.

4. Do not return explanations.

5. Do not recommend book titles.

6. Do not invent book names.

7. Do not invent authors.

8. Do not invent genres.

9. Only identify what the customer is asking for.

10. For genre requests, put the genre in "value".

11. For author requests, put the author name in "value".

12. For price requests, put only the maximum numeric
    price in "value".

13. For top-selling, popular, new and available,
    use an empty string for "value".

14. If the request does not clearly match another
    category, use "general".

15. The application will search the bookstore
    database for the actual books.
""";


            // ======================================================
            // 4. CREATE GEMINI REQUEST
            // ======================================================

            var request = new GeminiRequest
            {
                contents = new List<GeminiContent>
                {
                    new GeminiContent
                    {
                        parts = new List<GeminiPart>
                        {
                            new GeminiPart
                            {
                                text = prompt
                            }
                        }
                    }
                }
            };


            // ======================================================
            // 5. CREATE HTTP CLIENT
            // ======================================================

            var client =
                _httpClientFactory.CreateClient();

            client.Timeout =
                TimeSpan.FromSeconds(30);


            // ======================================================
            // 6. SERIALIZE REQUEST
            // ======================================================

            var requestJson =
                JsonSerializer.Serialize(
                    request,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy =
                            JsonNamingPolicy.CamelCase
                    });


            // ======================================================
            // 7. CREATE GEMINI API URL
            // ======================================================

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";


            using var httpRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    url);


            httpRequest.Content =
                new StringContent(
                    requestJson,
                    Encoding.UTF8,
                    "application/json");


            // ======================================================
            // 8. CALL GEMINI API
            // ======================================================

            HttpResponseMessage response;

            try
            {
                response =
                    await client.SendAsync(
                        httpRequest);
            }
            catch (TaskCanceledException)
            {
                throw new Exception(
                    "The Gemini request timed out. Please try again.");
            }
            catch (HttpRequestException)
            {
                throw new Exception(
                    "Unable to connect to the Gemini API.");
            }


            // ======================================================
            // 9. READ RESPONSE
            // ======================================================

            var responseJson =
                await response.Content
                    .ReadAsStringAsync();


            // ======================================================
            // 10. CHECK API STATUS
            // ======================================================

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    "====================================");

                Console.WriteLine(
                    "GEMINI API ERROR");

                Console.WriteLine(
                    $"Status Code: {(int)response.StatusCode}");

                Console.WriteLine(
                    $"Status: {response.StatusCode}");

                Console.WriteLine(
                    responseJson);

                Console.WriteLine(
                    "====================================");


                throw new Exception(
                    $"Gemini API Error: {(int)response.StatusCode} {response.StatusCode}");
            }


            // ======================================================
            // 11. VALIDATE EMPTY RESPONSE
            // ======================================================

            if (string.IsNullOrWhiteSpace(responseJson))
            {
                throw new Exception(
                    "Gemini returned an empty API response.");
            }


            // ======================================================
            // 12. DESERIALIZE GEMINI RESPONSE
            // ======================================================

            GeminiResponse? gemini;

            try
            {
                gemini =
                    JsonSerializer.Deserialize<GeminiResponse>(
                        responseJson,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
            }
            catch (JsonException)
            {
                Console.WriteLine(
                    "Invalid Gemini response JSON:");

                Console.WriteLine(
                    responseJson);

                throw new Exception(
                    "Gemini returned an invalid response.");
            }


            // ======================================================
            // 13. VALIDATE GEMINI RESPONSE
            // ======================================================

            if (gemini == null)
            {
                throw new Exception(
                    "Gemini returned an empty response object.");
            }


            if (gemini.candidates == null ||
                !gemini.candidates.Any())
            {
                throw new Exception(
                    "Gemini did not return any candidates.");
            }


            var candidate =
                gemini.candidates
                    .FirstOrDefault();


            if (candidate == null)
            {
                throw new Exception(
                    "Gemini returned an invalid candidate.");
            }


            if (candidate.content == null)
            {
                throw new Exception(
                    "Gemini response content is missing.");
            }


            if (candidate.content.parts == null ||
                !candidate.content.parts.Any())
            {
                throw new Exception(
                    "Gemini response parts are missing.");
            }


            var text =
                candidate.content.parts
                    .FirstOrDefault()?
                    .text;


            // ======================================================
            // 14. VALIDATE RESPONSE TEXT
            // ======================================================

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new Exception(
                    "Gemini returned an empty response.");
            }


            text = text.Trim();


            // ======================================================
            // 15. REMOVE MARKDOWN CODE BLOCK
            // ======================================================

            if (text.StartsWith("```json",
                StringComparison.OrdinalIgnoreCase))
            {
                text =
                    text.Substring(7);
            }

            else if (text.StartsWith("```"))
            {
                text =
                    text.Substring(3);
            }


            if (text.EndsWith("```"))
            {
                text =
                    text.Substring(
                        0,
                        text.Length - 3);
            }


            text = text.Trim();


            // ======================================================
            // 16. VALIDATE JSON TEXT
            // ======================================================

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new Exception(
                    "Gemini returned an empty JSON response.");
            }


            // ======================================================
            // 17. CONVERT JSON TO SEARCH INTENT
            // ======================================================

            BookSearchIntent? intent;

            try
            {
                intent =
                    JsonSerializer.Deserialize<BookSearchIntent>(
                        text,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
            }
            catch (JsonException)
            {
                Console.WriteLine(
                    "====================================");

                Console.WriteLine(
                    "GEMINI INVALID INTENT JSON");

                Console.WriteLine(
                    text);

                Console.WriteLine(
                    "====================================");


                throw new Exception(
                    "Gemini returned an invalid search format.");
            }


            // ======================================================
            // 18. VALIDATE INTENT OBJECT
            // ======================================================

            if (intent == null)
            {
                throw new Exception(
                    "Gemini returned an invalid search intent.");
            }


            // ======================================================
            // 19. VALIDATE INTENT TYPE
            // ======================================================

            if (string.IsNullOrWhiteSpace(intent.Type))
            {
                throw new Exception(
                    "Gemini did not specify a search type.");
            }


            var type =
                intent.Type
                    .Trim()
                    .ToLowerInvariant();


            // ======================================================
            // 20. VALIDATE INTENT VALUE
            // ======================================================

            var value =
                intent.Value?
                    .Trim() ?? "";


            // ======================================================
            // 21. ALLOWED SEARCH TYPES
            // ======================================================

            var allowedTypes =
                new[]
                {
                    "genre",
                    "top-selling",
                    "popular",
                    "new",
                    "price",
                    "author",
                    "available",
                    "general"
                };


            if (!allowedTypes.Contains(type))
            {
                throw new Exception(
                    "Gemini returned an unsupported search type.");
            }


            // ======================================================
            // 22. GENRE VALIDATION
            // ======================================================

            if (type == "genre")
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception(
                        "Gemini did not provide a genre.");
                }


                if (value.Length > 50)
                {
                    throw new Exception(
                        "The detected genre is too long.");
                }
            }


            // ======================================================
            // 23. AUTHOR VALIDATION
            // ======================================================

            if (type == "author")
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception(
                        "Gemini did not provide an author.");
                }


                if (value.Length > 100)
                {
                    throw new Exception(
                        "The detected author name is too long.");
                }
            }


            // ======================================================
            // 24. PRICE VALIDATION
            // ======================================================

            if (type == "price")
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception(
                        "Gemini did not provide a price.");
                }


                if (!double.TryParse(
                    value,
                    out double price))
                {
                    throw new Exception(
                        "Gemini returned an invalid price.");
                }


                if (price <= 0)
                {
                    throw new Exception(
                        "The price must be greater than zero.");
                }


                if (price > 100000)
                {
                    throw new Exception(
                        "The maximum price is ₹100000.");
                }


                // Normalize price
                intent.Value =
                    price.ToString(
                        "0.##",
                        System.Globalization.CultureInfo.InvariantCulture);
            }


            // ======================================================
            // 25. NORMALIZE INTENT
            // ======================================================

            intent.Type =
                type;

            intent.Value =
                value;


            // ======================================================
            // 26. DEBUG INFORMATION
            // ======================================================

            Console.WriteLine(
                "====================================");

            Console.WriteLine(
                "BOOKNEST AI INTENT");

            Console.WriteLine(
                $"User Request: {userMessage}");

            Console.WriteLine(
                $"Type: {intent.Type}");

            Console.WriteLine(
                $"Value: {intent.Value}");

            Console.WriteLine(
                "====================================");


            // ======================================================
            // 27. RETURN INTENT
            // ======================================================

            return intent;
        }
    }
}