using BookStoreProject.Models;
using BookStoreProject.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace BookStoreProject.Tests.ServiceTests
{
    [TestClass]
    public class GeminiServiceTests
    {
        private Mock<IHttpClientFactory> _httpClientFactoryMock = null!;
        private Mock<IConfiguration> _configurationMock = null!;
        private GeminiService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configurationMock = new Mock<IConfiguration>();

            _configurationMock
                .Setup(x => x["Gemini:ApiKey"])
                .Returns("test-api-key");

            _configurationMock
                .Setup(x => x["Gemini:Model"])
                .Returns("gemini-test-model");

            _service = new GeminiService(
                _httpClientFactoryMock.Object,
                _configurationMock.Object);
        }

        // ==========================================================
        // INPUT VALIDATION
        // ==========================================================

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenMessageIsNull()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetSearchIntent(null!));

            Assert.AreEqual(
                "Please enter what you are looking for.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenMessageIsEmpty()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetSearchIntent(""));

            Assert.AreEqual(
                "Please enter what you are looking for.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenMessageIsWhitespace()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetSearchIntent("   "));

            Assert.AreEqual(
                "Please enter what you are looking for.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenMessageHasLessThanTwoCharacters()
        {
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetSearchIntent("a"));

            Assert.AreEqual(
                "Please enter at least 2 characters.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenMessageExceeds150Characters()
        {
            string longMessage = new string('a', 151);

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetSearchIntent(longMessage));

            Assert.AreEqual(
                "Your request cannot exceed 150 characters.",
                exception.Message);
        }

        // ==========================================================
        // CONFIGURATION VALIDATION
        // ==========================================================

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenApiKeyIsMissing()
        {
            _configurationMock
                .Setup(x => x["Gemini:ApiKey"])
                .Returns((string?)null);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "Gemini API key is missing.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenApiKeyIsEmpty()
        {
            _configurationMock
                .Setup(x => x["Gemini:ApiKey"])
                .Returns("");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "Gemini API key is missing.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenModelIsMissing()
        {
            _configurationMock
                .Setup(x => x["Gemini:Model"])
                .Returns((string?)null);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "Gemini model is missing.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenModelIsEmpty()
        {
            _configurationMock
                .Setup(x => x["Gemini:Model"])
                .Returns("");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "Gemini model is missing.",
                exception.Message);
        }

        // ==========================================================
        // SUCCESSFUL SEARCH TYPES
        // ==========================================================

        [TestMethod]
        public async Task GetSearchIntent_ShouldReturnGenreIntent()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"genre\",\"value\":\"fiction\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var result =
                await _service.GetSearchIntent("I want fiction books");

            Assert.IsNotNull(result);
            Assert.AreEqual("genre", result.Type);
            Assert.AreEqual("fiction", result.Value);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldReturnAuthorIntent()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"author\",\"value\":\"James Clear\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var result =
                await _service.GetSearchIntent("books by James Clear");

            Assert.IsNotNull(result);
            Assert.AreEqual("author", result.Type);
            Assert.AreEqual("James Clear", result.Value);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldReturnPriceIntent()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"price\",\"value\":\"500\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var result =
                await _service.GetSearchIntent("books under 500");

            Assert.IsNotNull(result);
            Assert.AreEqual("price", result.Type);
            Assert.AreEqual("500", result.Value);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldReturnTopSellingIntent()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"top-selling\",\"value\":\"\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var result =
                await _service.GetSearchIntent("show me top selling books");

            Assert.IsNotNull(result);
            Assert.AreEqual("top-selling", result.Type);
            Assert.AreEqual("", result.Value);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldReturnPopularIntent()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"popular\",\"value\":\"\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var result =
                await _service.GetSearchIntent("popular books");

            Assert.IsNotNull(result);
            Assert.AreEqual("popular", result.Type);
            Assert.AreEqual("", result.Value);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldReturnNewIntent()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"new\",\"value\":\"\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var result =
                await _service.GetSearchIntent("new books");

            Assert.IsNotNull(result);
            Assert.AreEqual("new", result.Type);
            Assert.AreEqual("", result.Value);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldReturnAvailableIntent()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"available\",\"value\":\"\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var result =
                await _service.GetSearchIntent("books available in stock");

            Assert.IsNotNull(result);
            Assert.AreEqual("available", result.Type);
            Assert.AreEqual("", result.Value);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldReturnGeneralIntent()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"general\",\"value\":\"\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var result =
                await _service.GetSearchIntent("I want something interesting");

            Assert.IsNotNull(result);
            Assert.AreEqual("general", result.Type);
            Assert.AreEqual("", result.Value);
        }

        // ==========================================================
        // CASE NORMALIZATION
        // ==========================================================

        [TestMethod]
        public async Task GetSearchIntent_ShouldConvertTypeToLowerCase()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"GENRE\",\"value\":\"fiction\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var result =
                await _service.GetSearchIntent("fiction books");

            Assert.AreEqual("genre", result.Type);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldTrimTypeAndValue()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"  genre  \",\"value\":\"  fiction  \"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var result =
                await _service.GetSearchIntent("fiction books");

            Assert.AreEqual("genre", result.Type);
            Assert.AreEqual("fiction", result.Value);
        }

        // ==========================================================
        // MARKDOWN JSON RESPONSE
        // ==========================================================

        [TestMethod]
        public async Task GetSearchIntent_ShouldHandleJsonCodeBlock()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "```json\n{\"type\":\"genre\",\"value\":\"fiction\"}\n```"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var result =
                await _service.GetSearchIntent("fiction books");

            Assert.IsNotNull(result);
            Assert.AreEqual("genre", result.Type);
            Assert.AreEqual("fiction", result.Value);
        }

        // ==========================================================
        // HTTP ERRORS
        // ==========================================================

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenHttpRequestFails()
        {
            SetupHttpException(new HttpRequestException());

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "Unable to connect to the Gemini API.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenRequestTimesOut()
        {
            SetupHttpException(new TaskCanceledException());

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "The Gemini request timed out. Please try again.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenApiReturnsBadRequest()
        {
            SetupHttpResponse(
                "{\"error\":\"Bad Request\"}",
                HttpStatusCode.BadRequest);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "Gemini API Error: 400 BadRequest",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenApiReturnsUnauthorized()
        {
            SetupHttpResponse(
                "{\"error\":\"Unauthorized\"}",
                HttpStatusCode.Unauthorized);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "Gemini API Error: 401 Unauthorized",
                exception.Message);
        }

        // ==========================================================
        // EMPTY / INVALID GEMINI RESPONSE
        // ==========================================================

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenResponseIsEmpty()
        {
            SetupHttpResponse("");

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "Gemini returned an empty API response.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenResponseJsonIsInvalid()
        {
            SetupHttpResponse(
                "This is not valid JSON");

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "Gemini returned an invalid response.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenCandidatesAreMissing()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": []
                }
                """);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "Gemini did not return any candidates.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenContentIsMissing()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": null
                        }
                    ]
                }
                """);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "Gemini response content is missing.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenPartsAreMissing()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": []
                            }
                        }
                    ]
                }
                """);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "Gemini response parts are missing.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenResponseTextIsEmpty()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": ""
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "Gemini returned an empty response.",
                exception.Message);
        }

        // ==========================================================
        // INVALID SEARCH INTENT
        // ==========================================================

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenSearchJsonIsInvalid()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "invalid json"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "Gemini returned an invalid search format.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenTypeIsMissing()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"value\":\"fiction\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "Gemini did not specify a search type.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenTypeIsUnsupported()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"book-title\",\"value\":\"Harry Potter\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("Harry Potter"));

            Assert.AreEqual(
                "Gemini returned an unsupported search type.",
                exception.Message);
        }

        // ==========================================================
        // GENRE VALIDATION
        // ==========================================================

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenGenreValueIsMissing()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"genre\",\"value\":\"\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "Gemini did not provide a genre.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenGenreIsTooLong()
        {
            string longGenre = new string('a', 51);

            SetupHttpResponse(
                $$"""
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"genre\",\"value\":\"{{longGenre}}\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("fiction"));

            Assert.AreEqual(
                "The detected genre is too long.",
                exception.Message);
        }

        // ==========================================================
        // AUTHOR VALIDATION
        // ==========================================================

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenAuthorValueIsMissing()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"author\",\"value\":\"\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("books by author"));

            Assert.AreEqual(
                "Gemini did not provide an author.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenAuthorIsTooLong()
        {
            string longAuthor = new string('a', 101);

            SetupHttpResponse(
                $$"""
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"author\",\"value\":\"{{longAuthor}}\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("books by author"));

            Assert.AreEqual(
                "The detected author name is too long.",
                exception.Message);
        }

        // ==========================================================
        // PRICE VALIDATION
        // ==========================================================

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenPriceIsMissing()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"price\",\"value\":\"\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("cheap books"));

            Assert.AreEqual(
                "Gemini did not provide a price.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenPriceIsInvalid()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"price\",\"value\":\"abc\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("books under abc"));

            Assert.AreEqual(
                "Gemini returned an invalid price.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenPriceIsZero()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"price\",\"value\":\"0\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("books under 0"));

            Assert.AreEqual(
                "The price must be greater than zero.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenPriceIsNegative()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"price\",\"value\":\"-100\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("books under -100"));

            Assert.AreEqual(
                "The price must be greater than zero.",
                exception.Message);
        }

        [TestMethod]
        public async Task GetSearchIntent_ShouldThrowException_WhenPriceExceedsMaximum()
        {
            SetupHttpResponse(
                """
                {
                    "candidates": [
                        {
                            "content": {
                                "parts": [
                                    {
                                        "text": "{\"type\":\"price\",\"value\":\"100001\"}"
                                    }
                                ]
                            }
                        }
                    ]
                }
                """);

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetSearchIntent("books under 100001"));

            Assert.AreEqual(
                "The maximum price is ₹100000.",
                exception.Message);
        }

        // ==========================================================
        // HELPER METHODS
        // ==========================================================

        private void SetupHttpResponse(
            string responseContent,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var handler = new FakeHttpMessageHandler
            {
                Response = new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(
                        responseContent,
                        Encoding.UTF8,
                        "application/json")
                }
            };

            var httpClient = new HttpClient(handler);

            // IMPORTANT:
            // GeminiService calls _httpClientFactory.CreateClient()
            // which is an extension method that internally calls
            // CreateClient(string.Empty).
            _httpClientFactoryMock
                .Setup(x => x.CreateClient(string.Empty))
                .Returns(httpClient);
        }

        private void SetupHttpException(Exception exception)
        {
            var handler = new FakeHttpMessageHandler
            {
                ExceptionToThrow = exception
            };

            var httpClient = new HttpClient(handler);

            _httpClientFactoryMock
                .Setup(x => x.CreateClient(string.Empty))
                .Returns(httpClient);
        }
    }

    // ==============================================================
    // FAKE HTTP MESSAGE HANDLER
    // ==============================================================

    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        public HttpResponseMessage? Response { get; set; }

        public Exception? ExceptionToThrow { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (ExceptionToThrow != null)
            {
                return Task.FromException<HttpResponseMessage>(
                    ExceptionToThrow);
            }

            return Task.FromResult(
                Response ?? new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}