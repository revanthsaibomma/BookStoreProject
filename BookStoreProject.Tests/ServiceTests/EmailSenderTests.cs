using BookStoreProject.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BookStoreProject.Tests.ServiceTests
{
    [TestClass]
    public class EmailSenderTests
    {
        private EmailSender _emailSender = null!;

        [TestInitialize]
        public void Setup()
        {
            _emailSender = new EmailSender();
        }

        // ==========================================================
        // SUCCESSFUL EMAIL
        // ==========================================================

        [TestMethod]
        public async Task SendEmailAsync_ShouldCompleteSuccessfully()
        {
            await _emailSender.SendEmailAsync(
                "test@example.com",
                "Test Subject",
                "<h1>Test Email</h1>");

            Assert.IsTrue(true);
        }

        // ==========================================================
        // VALID EMAIL DATA
        // ==========================================================

        [TestMethod]
        public async Task SendEmailAsync_ShouldReturnCompletedTask()
        {
            Task result = _emailSender.SendEmailAsync(
                "test@example.com",
                "Test Subject",
                "<p>Hello</p>");

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsCompleted);

            await result;
        }

        // ==========================================================
        // EMPTY EMAIL
        // ==========================================================

        [TestMethod]
        public async Task SendEmailAsync_ShouldComplete_WhenEmailIsEmpty()
        {
            await _emailSender.SendEmailAsync(
                "",
                "Test Subject",
                "<p>Test</p>");

            Assert.IsTrue(true);
        }

        // ==========================================================
        // EMPTY SUBJECT
        // ==========================================================

        [TestMethod]
        public async Task SendEmailAsync_ShouldComplete_WhenSubjectIsEmpty()
        {
            await _emailSender.SendEmailAsync(
                "test@example.com",
                "",
                "<p>Test</p>");

            Assert.IsTrue(true);
        }

        // ==========================================================
        // EMPTY MESSAGE
        // ==========================================================

        [TestMethod]
        public async Task SendEmailAsync_ShouldComplete_WhenMessageIsEmpty()
        {
            await _emailSender.SendEmailAsync(
                "test@example.com",
                "Test Subject",
                "");

            Assert.IsTrue(true);
        }

        // ==========================================================
        // HTML MESSAGE
        // ==========================================================

        [TestMethod]
        public async Task SendEmailAsync_ShouldComplete_WithHtmlMessage()
        {
            string htmlMessage = """
                <html>
                    <body>
                        <h1>Book Store</h1>
                        <p>Your order has been placed successfully.</p>
                    </body>
                </html>
                """;

            await _emailSender.SendEmailAsync(
                "customer@example.com",
                "Order Confirmation",
                htmlMessage);

            Assert.IsTrue(true);
        }
    }
}